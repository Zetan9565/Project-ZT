using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Item;
using ZetanStudio.Item.Craft;
using ZetanStudio.Item.Module;

[DisallowMultipleComponent]
public class CraftWindow : Window, IHideable
{
    public ItemTypeDropDown pageSelector;

    public CraftList list;

    public CanvasGroup descriptionWindow;

    public Text nameText;
    public ItemSlot icon;
    public Text description;
    public Button craftButton;

    public Toggle loopToggle;
    public Button tryButton;
    public InputField searchInput;
    public Button searchButton;
    public Dropdown inventorySelector;

    private Item currentItem;

    private IInventoryHandler handler;
    private IWarehouseKeeper warehouse;
    private WarehouseSelectionWindow wsWindow;
    private ItemSlotData slot;

    private int typeBef;
    private bool descOpen;

    public CraftTool CurrentTool { get; private set; }

    private CraftToolInformation toolInfo;
    public CraftToolInformation ToolInfo
    {
        get
        {
            if (CurrentTool) return CurrentTool.ToolInfo;
            else return toolInfo;
        }
    }

    protected override void OnAwake()
    {
        craftButton.onClick.AddListener(MakeCurrent);
        tryButton.onClick.AddListener(OnTryClick);
        pageSelector.container = list;
        icon.SetCallbacks((i) =>
        {
            return new ButtonWithTextData[] { new ButtonWithTextData("制作", MakeCurrent) };
        });
        slot = new ItemSlotData();
        icon.Refresh(slot);
        list.SetItemModifier(ia => ia.SetWindow(this));
        inventorySelector.onValueChanged.AddListener(SelectInventory);
    }

    protected override void OnDestroy_()
    {
        if (handler != null) NotifyCenter.RemoveListener(handler.ItemAmountChangedMsgKey, OnItemAmountChanged);
        base.OnDestroy_();
    }

    private void SelectInventory(int type)
    {
        if (type == 0)
        {
            typeBef = type;
            warehouse = null;
            handler = BackpackManager.Instance;
            inventorySelector.ClearOptions();
            inventorySelector.AddOptions(new List<string> { $"从{BackpackManager.Instance.Name}中制作", $"从{WarehouseManager.Instance.Name}中制作" });
            ShowDescription(currentItem);
        }
        else if (type == 1 || type == 2)
        {
            Vector3 point = CurrentTool ? CurrentTool.transform.position : PlayerManager.Instance.Player.Position;
            wsWindow = WarehouseSelectionWindow.StartSelection(OnSetWarehouse, () => inventorySelector.SetValueWithoutNotify(typeBef), (d) => d == warehouse, false, true, point);
            if (!wsWindow) inventorySelector.value = 0;
            else
            {
                if (type != 2) typeBef = type;
                inventorySelector.interactable = false;
                wsWindow.onClose += () =>
                {
                    if (warehouse == null) inventorySelector.value = 0;
                    wsWindow = null;
                    inventorySelector.interactable = true;
                };
            }
        }
    }
    private void OnSetWarehouse(IWarehouseKeeper data)
    {
        warehouse = data;
        if (warehouse == null)
        {
            inventorySelector.value = 0;
            return;
        }
        WarehouseManager.Instance.SetManagedWarehouse(data);
        handler = WarehouseManager.Instance;
        inventorySelector.ClearOptions();
        inventorySelector.AddOptions(new List<string> { $"从{BackpackManager.Instance.Name}中制作", $"{warehouse.WarehouseName}", "选择其它仓库" });
        inventorySelector.SetValueWithoutNotify(1);
        ShowDescription(currentItem);
    }

    private bool isCrafting;
    public bool IsCrafting
    {
        get => isCrafting;
        private set
        {
            isCrafting = value;
            if (!value) NotifyCenter.PostNotify(CraftManager.CraftCanceled);
        }
    }

    public bool IsHidden { get; private set; }

    private void MakeCurrent()
    {
        if (!ToolInfo || !currentItem) return;
        if (IsCrafting)
        {
            MessageManager.Instance.New("正在制作中");
            return;
        }
        var makeble = currentItem.GetModule<CraftableModule>();
        int amountCanMake = handler.GetAmountCanMake(makeble.Formulation.Materials);
        if (amountCanMake < 1)
        {
            MessageManager.Instance.New("材料不足");
            return;
        }
        if (!makeble.CanMakeByTry)
        {
            if (amountCanMake > 0 && amountCanMake < 2)
            {
                ConfirmWindow.StartConfirm(string.Format("确定制作1次 [{0}] 吗？", currentItem.Name), delegate
                {
                    IsCrafting = true;
                    WindowsManager.HideWindow(this, true);
                    ProgressBar.Instance.New(ToolInfo.MakingTime, delegate
                    {
                        IsCrafting = false;
                        WindowsManager.HideWindow(this, false);
                        int amoutBef = handler.GetAmount(currentItem.ID);
                        if (MakeItem(currentItem))
                            MessageManager.Instance.New(string.Format("制作了 {0} 个 [{1}]", handler.GetAmount(currentItem.ID) - amoutBef, currentItem.ColorName));
                    },
                    OnCancel, "制作中", true);
                });
            }
            else
            {
                AmountWindow.StartInput(delegate (long amount)
                {
                    ConfirmWindow.StartConfirm(string.Format("确定制作{0}次 [{1}] 吗？", (int)amount, currentItem.Name), delegate
                    {
                        int num = (int)amount;
                        IsCrafting = true;
                        WindowsManager.HideWindow(this, true);
                        ProgressBar.Instance.New(ToolInfo.MakingTime, num - 1,
                            OnCancel,
                            delegate
                            {
                                int amoutBef = handler.GetAmount(currentItem.ID);
                                if (MakeItem(currentItem))
                                {
                                    IsCrafting = false;
                                    MessageManager.Instance.New(string.Format("制作了 {0} 个 [{1}]", handler.GetAmount(currentItem.ID) - amoutBef, currentItem.ColorName));
                                }
                                else IsCrafting = false;
                                NotifyCenter.PostNotify(CraftManager.CraftCanceled);
                                WindowsManager.HideWindow(this, false);
                                ProgressBar.Instance.Cancel();
                            },
                            OnCancel, "制作中", true);
                    });
                }, amountCanMake, "制作次数", ZetanUtility.ScreenCenter, Vector2.zero);
            }
        }
        else
        {
            InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, MakeCurrent, "放入一份材料", selectCondition: CanSelect);
        }
    }

    private bool CanSelect(ItemSlotBase slot)
    {
        return slot.Item && slot.Item.Model.GetModule<MaterialModule>();
    }

    private void MakeCurrent(IEnumerable<ItemWithAmount> materialsRaw)
    {
        //Debug.Log("Start making");
        var materials = ItemInfo.Convert(materialsRaw);
        IsCrafting = true;
        WindowsManager.HideWindow(this, true);
        if (loopToggle.isOn)
            ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                bool enough = handler.IsMaterialsEnough(currentItem.GetModule<CraftableModule>().Formulation.Materials, materials);
                if (!enough) MessageManager.Instance.New("材料不足，无法继续制作");
                return !enough && loopToggle.isOn;
            },
            OnCancel,
            delegate
            {
                if (!MakeItem(currentItem, materialsRaw))
                {
                    //Debug.Log("Failed3");
                    ProgressBar.Instance.Cancel();
                }
            },
            OnCancel, "制作中", true);
        else
            ProgressBar.Instance.New(ToolInfo.MakingTime,
                delegate
                {
                    MakeItem(currentItem, materialsRaw);
                    //Debug.Log("Finished");
                    IsCrafting = false;
                    WindowsManager.HideWindow(this, false);
                    ProgressBar.Instance.Cancel();
                },
                OnCancel, "制作中 ", true);
    }

    public void OnTryClick()
    {
        if (handler is BackpackManager) InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, TryMake, "放入一份材料", selectCondition: CanSelect);
        else if (handler is WarehouseManager) InventoryWindow.OpenSelectionWindow<WarehouseWindow>(ItemSelectionType.SelectNum, TryMake, "放入一份材料", selectCondition: CanSelect, args:
            new object[] { WarehouseWindow.OpenType.Craft, warehouse });
    }
    private void TryMake(IEnumerable<ItemWithAmount> materialsRaw)
    {
        IsCrafting = true;
        WindowsManager.HideWindow(this, true);
        ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                MakeItem(materialsRaw);
                //Debug.Log("Finished");
                IsCrafting = false;
                WindowsManager.HideWindow(this, false);
            },
            delegate
            {
                //Debug.Log("Cancel");
                IsCrafting = false;
                NotifyCenter.PostNotify(CraftManager.CraftCanceled);
                WindowsManager.HideWindow(this, false);
            }, "制作中", true);
    }

    /// <summary>
    /// 制作道具，用于根据已知配方直接消耗材料来制作道具的玩法
    /// </summary>
    /// <param name="itemToMake">目标道具</param>
    /// <param name="amount">制作数量</param>
    /// <returns>是否成功制作</returns>
    private bool MakeItem(Item itemToMake)
    {
        if (!itemToMake || itemToMake.GetModule<CraftableModule>() is not CraftableModule makable || !makable.IsValid) return false;
        if (makable.CanMakeByTry) return false;
        List<ItemWithAmount> materialsRaw = handler.GetMaterialsFromInventory(makable.Formulation.Materials);
        if (handler.IsMaterialsEnough(makable.Formulation.Materials, ItemInfo.Convert(materialsRaw)))
            return handler.GetItem(itemToMake, makable.RandomAmount(), materialsRaw.ToArray());
        else
        {
            MessageManager.Instance.New("材料不足，无法继续制作");
            return false;
        }
    }
    /// <summary>
    /// 制作道具，用于根据已知配方自己放入材料来制作道具的玩法
    /// </summary>
    /// <param name="item">目标道具</param>
    /// <param name="materialsRaw">放入的材料</param>
    private bool MakeItem(Item item, IEnumerable<ItemWithAmount> materialsRaw)
    {
        if (!item || materialsRaw == null || materialsRaw.Count() < 1 || item.GetModule<CraftableModule>() is not CraftableModule makable || !makable.Formulation)
        {
            MessageManager.Instance.New("无效的制作");
            return false;
        }
        if (MaterialInfo.CheckMaterialsMatch(makable.Formulation.Materials, ItemInfo.Convert(materialsRaw)))
        {
            ItemData production = item.CreateData();
            int amount = makable.RandomAmount();
            List<ItemWithAmount> itemsToLose = new List<ItemWithAmount>();
            foreach (var material in makable.Formulation.Materials)
            {
                if (material.MakingType == CraftType.SingleItem)
                {
                    ItemWithAmount find = materialsRaw.FirstOrDefault(x => x.source.ModelID == material.ItemID);
                    if (!find || find.amount != material.Amount)
                    {
                        MessageManager.Instance.New("材料不正确(请确保只放入了一份)");
                        return false;//所提供的材料中没有这种材料或数量不符合，则无法制作
                    }
                    //否则加入消耗列表
                    else itemsToLose.Add(find);
                }
                else
                {
                    var finds = materialsRaw.Where(x => MaterialModule.Compare(x.source.Model, material.MaterialType));//找到种类相同的道具
                    if (finds.Count() > 0)
                    {
                        if (finds.Select(x => x.amount).Sum() != material.Amount)
                        {
                            MessageManager.Instance.New("材料不正确(请确保只放入了一份)");
                            return false;//若材料总数不符合，则无法制作
                        }
                        foreach (var find in finds)
                        {
                            if (!handler.CanLose(find.source, find.amount))
                            {
                                MessageManager.Instance.New("材料不足，无法继续制作");
                                return false;//若任意一个相应数量的材料无法失去，则会导致总数量不符合，所以无法制作
                            }
                            else
                            {
                                //否则加入消耗列表
                                itemsToLose.Add(find);
                            }
                        }
                    }
                    else
                    {
                        MessageManager.Instance.New("材料不足，无法继续制作");
                        return false;//材料不足
                    }
                }
            }
            if (handler.GetItem(production, amount, itemsToLose.ToArray()))
            {
                MessageManager.Instance.New(string.Format("生产了 {0} 个[{1}]", amount, production.Model.ColorName));
                if (!CraftManager.Instance.HadLearned(item)) CraftManager.Instance.Learn(item);
                return true;
            }
            else return false;
        }
        else
        {
            MessageManager.Instance.New("材料不正确(请确保只放入了一份)");
            return false;
        }
    }
    /// <summary>
    /// 制作道具，用于自己放入材料来组合新道具的玩法
    /// </summary>
    /// <param name="materials">放入的材料</param>
    private bool MakeItem(IEnumerable<ItemWithAmount> materialsRaw)
    {
        var materials = ItemInfo.Convert(materialsRaw);
        foreach (var item in CraftManager.Instance.LearnedItems)
            if (check(materials, item))
                return MakeItem(item, materialsRaw);
        foreach (var item in Resources.LoadAll<Item>("Configuration"))
            if (check(materials, item))
                return MakeItem(item, materialsRaw);
        MessageManager.Instance.New("这样似乎什么也做不了");
        return false;

        bool check(ItemInfo[] materials, Item item)
        {
            CraftableModule craftableModule = item.GetModule<CraftableModule>();
            if (!craftableModule) return false;
            return ToolInfo.ToolType.Methods.Contains(craftableModule.CraftMethod) && MaterialInfo.CheckMaterialsMatch(craftableModule.Formulation.Materials, materials);
        }
    }

    #region UI相关
    protected override bool OnOpen(params object[] args)
    {
        if (!PlayerManager.Instance.CheckIsIdleWithAlert()) return false;
        if (IsCrafting)
        {
            MessageManager.Instance.New("正在制作中");
            return false;
        }
        if (IsHidden) return true;
        CurrentTool = openBy as CraftTool;
        toolInfo = openBy as CraftToolInformation;
        if (!ToolInfo) return false;
        handler = args[0] as IInventoryHandler;
        if (handler != null) NotifyCenter.AddListener(handler.ItemAmountChangedMsgKey, OnItemAmountChanged);
        IsCrafting = false;
        list.Refresh(CraftManager.Instance.LearnedItems);
        typeBef = 0;
        SetPage(0);
        inventorySelector.ClearOptions();
        inventorySelector.AddOptions(new List<string> { $"从{BackpackManager.Instance.Name}中制作", $"从{WarehouseManager.Instance.Name}中制作" });
        WindowsManager.CloseWindow<ItemWindow>();
        PlayerManager.Instance.Player.SetMachineState<PlayerCraftingState>();
        return base.OnOpen(args);
    }

    protected override bool OnClose(params object[] args)
    {
        base.OnClose(args);
        if (IsCrafting)
        {
            ProgressBar.Instance.CancelWithoutAction();
            OnCancel();
        }
        CurrentTool = null;
        currentItem = null;
        toolInfo = null;
        warehouse = null;
        if (handler != null) NotifyCenter.RemoveListener(handler.ItemAmountChangedMsgKey, OnItemAmountChanged);
        handler = null;
        HideDescription();
        WindowsManager.CloseWindow<AmountWindow>();
        WindowsManager.CloseWindow<ItemWindow>();
        WindowsManager.CloseWindow<ItemSelectionWindow>();
        PlayerManager.Instance.Player.SetMachineState<CharacterIdleState>();
        if (wsWindow) wsWindow.Close();
        return true;
    }

    public void Hide(bool hide, params object[] args)
    {
        if (!IsOpen) return;
        IHideable.HideHelper(content, hide);
        IsHidden = hide;
    }

    public void ShowDescription(Item item)
    {
        if (!item || item.GetModule<CraftableModule>() is not CraftableModule makable)
        {
            HideDescription();
            return;
        }
        currentItem = item;
        List<string> info = handler.GetMaterialsInfoString(makable.Formulation.Materials);
        StringBuilder materials = new StringBuilder("<b>持有数量：</b>" + handler.GetAmount(item));
        materials.Append("\n<b>制作材料：</b>");
        materials.Append(handler.IsMaterialsEnough(makable.Formulation.Materials) ? "<color=green>(可制作)</color>\n" : "<color=red>(耗材不足)</color>\n");
        for (int i = 0; i < info.Count; i++)
            materials.Append(info[i] + (i == info.Count - 1 ? string.Empty : "\n"));
        description.text = materials.ToString();
        int makeAmount = handler.GetAmountCanMake(makable.Formulation.Materials);
        slot.item = new ItemData(item, false);
        slot.amount = makeAmount;
        icon.Refresh();
        nameText.text = item.Name;
        craftButton.interactable = makeAmount > 0;
        descriptionWindow.alpha = 1;
        descriptionWindow.blocksRaycasts = true;
        descOpen = true;
        ZetanUtility.SetActive(loopToggle.gameObject, makable.CanMakeByTry);
    }

    public void HideDescription()
    {
        description.text = string.Empty;
        icon.Refresh(ItemSlotData.Empty);
        descriptionWindow.alpha = 0;
        descriptionWindow.blocksRaycasts = false;
        descOpen = false;
    }

    public void Refresh()
    {
        if (!IsOpen) return;
        list.Refresh(CraftManager.Instance.LearnedItems);
        if (descOpen) ShowDescription(currentItem);
        pageSelector.Value = pageSelector.Value;
    }

    public void Interrupt()
    {
        if (IsCrafting)
        {
            ProgressBar.Instance.CancelWithoutAction();
            OnCancel();
        }
    }
    public void OnCancel()
    {
        IsCrafting = false;
        WindowsManager.HideWindow(this, false);
        NotifyCenter.PostNotify(CraftManager.CraftCanceled);
    }

    #region 道具页相关
    public void SetPage(int index)
    {
        pageSelector.Value = index;
        //switch (index)
        //{
        //    case 1: ShowEquipments(); break;
        //    case 3: ShowConsumables(); break;
        //    case 4: ShowMaterials(); break;
        //    default: ShowAll(); break;
        //}
    }

    private void ShowAll()
    {
        list.ForEach(ia => ZetanUtility.SetActive(ia, true));
    }

    private void ShowEquipments()
    {
        list.ForEach(ia =>
        {
            ZetanUtility.SetActive(ia.gameObject, ia.Data.GetModule<EquipableModule>());
        });
    }

    private void ShowConsumables()
    {
        list.ForEach(ia =>
        {
            if (ia.Data.Type.Name == "消耗品")
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        });
    }

    private void ShowMaterials()
    {
        list.ForEach(ia =>
        {
            ZetanUtility.SetActive(ia.gameObject, ia.Data.GetModule<MaterialModule>());
        });
    }
    #endregion
    #endregion

    #region 消息
    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(CraftManager.LearnedCraftableItem, _ => Refresh(), this);
        NotifyCenter.AddListener(NotifyCenter.CommonKeys.PlayerStateChanged, OnPlayerStateChanged, this);
    }
    public void OnPlayerStateChanged(params object[] msg)
    {
        if (IsCrafting && msg.Length > 0 && msg[0] is CharacterStates.Normal && (msg[1] is CharacterNormalStates.Walk || msg[1] is CharacterNormalStates.Run))
            Interrupt();
    }
    public void OnItemAmountChanged(params object[] msg)
    {
        if (descOpen) ShowDescription(currentItem);
    }
    #endregion
}
