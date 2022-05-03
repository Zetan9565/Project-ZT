using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MakingWindow : Window, IHideable
{
    public Dropdown pageSelector;

    public MakingList list;

    public CanvasGroup descriptionWindow;

    public Text nameText;
    public ItemSlot icon;
    public Text description;
    public Button makeButton;

    public Toggle loopToggle;
    public Button DIYButton;
    public InputField searchInput;
    public Button searchButton;
    public Dropdown inventorySelector;

    private ItemBase currentItem;

    private IInventoryHandler handler;
    private IWarehouseKeeper warehouse;
    private WarehouseSelectionWindow wsWindow;
    private ItemSlotData slot;

    private int typeBef;
    private bool descOpen;

    public MakingTool CurrentTool { get; private set; }

    private MakingToolInformation toolInfo;
    public MakingToolInformation ToolInfo
    {
        get
        {
            if (CurrentTool) return CurrentTool.ToolInfo;
            else return toolInfo;
        }
    }

    protected override void OnAwake()
    {
        makeButton.onClick.AddListener(MakeCurrent);
        DIYButton.onClick.AddListener(DIY);
        pageSelector.onValueChanged.AddListener(SetPage);
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
            wsWindow = WarehouseSelectionWindow.StartSelection(OnSetWarehouse, () => inventorySelector.value = typeBef, (d) => d == warehouse, false, true, point);
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

    private bool isMaking;
    public bool IsMaking
    {
        get => isMaking;
        private set
        {
            isMaking = value;
            if (!value) NotifyCenter.PostNotify(MakingManager.MakingCanceled);
        }
    }

    public bool IsHidden { get; private set; }

    private void MakeCurrent()
    {
        if (!ToolInfo || !currentItem) return;
        if (IsMaking)
        {
            MessageManager.Instance.New("正在制作中");
            return;
        }
        int amountCanMake = handler.GetAmountCanMake(currentItem.Formulation.Materials);
        if (amountCanMake < 1)
        {
            MessageManager.Instance.New("材料不足");
            return;
        }
        if (!currentItem.DIYAble)
        {
            if (amountCanMake > 0 && amountCanMake < 2)
            {
                ConfirmWindow.StartConfirm(string.Format("确定制作1次 [{0}] 吗？", currentItem.Name), delegate
                {
                    IsMaking = true;
                    WindowsManager.HideWindow(this, true);
                    ProgressBar.Instance.New(ToolInfo.MakingTime, delegate
                    {
                        IsMaking = false;
                        WindowsManager.HideWindow(this, false);
                        int amoutBef = handler.GetAmount(currentItem.ID);
                        if (MakeItem(currentItem))
                            MessageManager.Instance.New(string.Format("制作了 {0} 个 [{1}]", handler.GetAmount(currentItem.ID) - amoutBef,
                                ItemUtility.GetColorName(currentItem)));
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
                        IsMaking = true;
                        WindowsManager.HideWindow(this, true);
                        ProgressBar.Instance.New(ToolInfo.MakingTime, num - 1,
                            OnCancel,
                            delegate
                            {
                                int amoutBef = handler.GetAmount(currentItem.ID);
                                if (MakeItem(currentItem))
                                {
                                    IsMaking = false;
                                    MessageManager.Instance.New(string.Format("制作了 {0} 个 [{1}]", handler.GetAmount(currentItem.ID) - amoutBef, ItemUtility.GetColorName(currentItem)));
                                }
                                else IsMaking = false;
                                NotifyCenter.PostNotify(MakingManager.MakingCanceled);
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
            InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, MakeCurrent, "放入一份材料", selectCondition: (slot) =>
            {
                return slot.Item && slot.Item.Model_old.MaterialType != MaterialType.None;
            });
        }
    }

    private void MakeCurrent(IEnumerable<ItemWithAmount> materialsRaw)
    {
        //Debug.Log("Start making");
        var materials = ItemInfoBase.Convert(materialsRaw);
        IsMaking = true;
        WindowsManager.HideWindow(this, true);
        if (loopToggle.isOn)
            ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                bool enough = handler.IsMaterialsEnough(currentItem.Formulation.Materials, materials);
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
                    IsMaking = false;
                    WindowsManager.HideWindow(this, false);
                    ProgressBar.Instance.Cancel();
                },
                OnCancel, "制作中 ", true);
    }

    public void DIY()
    {
        static bool canSelect(ItemSlotBase slot)
        {
            return slot.Item && slot.Item.Model_old.MaterialType != MaterialType.None;
        }
        if (handler is BackpackManager) InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, DIYMake, "放入一份材料", selectCondition: canSelect);
        else if (handler is WarehouseManager) InventoryWindow.OpenSelectionWindow<WarehouseWindow>(ItemSelectionType.SelectNum, DIYMake, "放入一份材料", selectCondition: canSelect, args:
            new object[] { WarehouseWindow.OpenType.Making, warehouse });
    }
    public void DIYMake(IEnumerable<ItemWithAmount> materialsRaw)
    {
        IsMaking = true;
        WindowsManager.HideWindow(this, true);
        ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                MakeItem(materialsRaw);
                //Debug.Log("Finished");
                IsMaking = false;
                WindowsManager.HideWindow(this, false);
            },
            delegate
            {
                //Debug.Log("Cancel");
                IsMaking = false;
                NotifyCenter.PostNotify(MakingManager.MakingCanceled);
                WindowsManager.HideWindow(this, false);
            }, "制作中", true);
    }

    /// <summary>
    /// 制作道具，用于根据已知配方直接消耗材料来制作道具的玩法
    /// </summary>
    /// <param name="itemToMake">目标道具</param>
    /// <param name="amount">制作数量</param>
    /// <returns>是否成功制作</returns>
    private bool MakeItem(ItemBase itemToMake)
    {
        if (!itemToMake || !itemToMake.Formulation || itemToMake.Formulation.Materials.Count < 1) return false;
        if (itemToMake.DIYAble) return false;
        List<ItemWithAmount> materialsRaw = handler.GetMaterialsFromInventory(itemToMake.Formulation.Materials);
        if (handler.IsMaterialsEnough(itemToMake.Formulation.Materials, ItemInfoBase.Convert(materialsRaw)))
            return handler.GetItem(itemToMake, Random.Range(itemToMake.MinYield, itemToMake.MaxYield + 1), materialsRaw.ToArray());
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
    private bool MakeItem(ItemBase item, IEnumerable<ItemWithAmount> materialsRaw)
    {
        if (!item || !item.Formulation || materialsRaw == null || materialsRaw.Count() < 1)
        {
            MessageManager.Instance.New("无效的制作");
            return false;
        }
        if (MaterialInfo.CheckMaterialsMatch(item.Formulation.Materials, ItemInfoBase.Convert(materialsRaw)))
        {
            ItemData production = item.CreateData();
            int amount = Random.Range(item.MinYield, item.MaxYield + 1);
            List<ItemWithAmount> itemsToLose = new List<ItemWithAmount>();
            foreach (var material in item.Formulation.Materials)
            {
                if (material.MakingType == MakingType.SingleItem)
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
                    var finds = materialsRaw.Where(x => x.source.Model_old.MaterialType == material.MaterialType);//找到种类相同的道具
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
                MessageManager.Instance.New(string.Format("生产了 {0} 个[{1}]", amount, ItemUtility.GetColorName(production.Model_old)));
                if (!MakingManager.Instance.HadLearned(item)) MakingManager.Instance.Learn(item);
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
        var materials = ItemInfoBase.Convert(materialsRaw);
        foreach (var item in MakingManager.Instance.LearnedItems)
            if (item.MakingTool == ToolInfo.ToolType && MaterialInfo.CheckMaterialsMatch(item.Formulation.Materials, materials))
                return MakeItem(item, materialsRaw);
        foreach (var item in Resources.LoadAll<ItemBase>("Configuration"))
            if (item.Makable && item.MakingTool == ToolInfo.ToolType && MaterialInfo.CheckMaterialsMatch(item.Formulation.Materials, materials))
                return MakeItem(item, materialsRaw);
        MessageManager.Instance.New("这样似乎什么也做不了");
        return false;
    }

    #region UI相关
    protected override bool OnOpen(params object[] args)
    {
        if (!PlayerManager.Instance.CheckIsIdleWithAlert()) return false;
        if (IsMaking)
        {
            MessageManager.Instance.New("正在制作中");
            return false;
        }
        if (IsHidden) return true;
        CurrentTool = openBy as MakingTool;
        toolInfo = openBy as MakingToolInformation;
        if (!ToolInfo) return false;
        handler = args[0] as IInventoryHandler;
        if (handler != null) NotifyCenter.AddListener(handler.ItemAmountChangedMsgKey, OnItemAmountChanged);
        IsMaking = false;
        list.Refresh(MakingManager.Instance.LearnedItems);
        pageSelector.SetValueWithoutNotify(0);
        typeBef = 0;
        SetPage(0);
        inventorySelector.ClearOptions();
        inventorySelector.AddOptions(new List<string> { $"从{BackpackManager.Instance.Name}中制作", $"从{WarehouseManager.Instance.Name}中制作" });
        WindowsManager.CloseWindow<ItemWindow>();
        PlayerManager.Instance.Player.SetMachineState<PlayerMakingState>();
        return base.OnOpen(args);
    }

    protected override bool OnClose(params object[] args)
    {
        base.OnClose(args);
        if (IsMaking)
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

    public void ShowDescription(ItemBase item)
    {
        if (!item)
        {
            HideDescription();
            return;
        }
        currentItem = item;
        List<string> info = handler.GetMaterialsInfoString(currentItem.Formulation.Materials);
        StringBuilder materials = new StringBuilder("<b>持有数量：</b>" + handler.GetAmount(item));
        materials.Append("\n<b>制作材料：</b>");
        materials.Append(handler.IsMaterialsEnough(item.Formulation.Materials) ? "<color=green>(可制作)</color>\n" : "<color=red>(耗材不足)</color>\n");
        for (int i = 0; i < info.Count; i++)
            materials.Append(info[i] + (i == info.Count - 1 ? string.Empty : "\n"));
        description.text = materials.ToString();
        int makeAmount = handler.GetAmountCanMake(currentItem.Formulation.Materials);
        slot.item = new ItemData(item, false);
        slot.amount = makeAmount;
        icon.Refresh();
        nameText.text = item.Name;
        makeButton.interactable = makeAmount > 0;
        descriptionWindow.alpha = 1;
        descriptionWindow.blocksRaycasts = true;
        descOpen = true;
        ZetanUtility.SetActive(loopToggle.gameObject, currentItem.DIYAble);
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
        list.Refresh(MakingManager.Instance.LearnedItems);
        if (descOpen) ShowDescription(currentItem);
        SetPage(currentPage);
    }

    public void Interrupt()
    {
        if (IsMaking)
        {
            ProgressBar.Instance.CancelWithoutAction();
            OnCancel();
        }
    }
    public void OnCancel()
    {
        IsMaking = false;
        WindowsManager.HideWindow(this, false);
        NotifyCenter.PostNotify(MakingManager.MakingCanceled);
    }

    #region 道具页相关
    private int currentPage;

    public void SetPage(int index)
    {
        currentPage = index;
        switch (index)
        {
            case 1: ShowEquipments(); break;
            case 3: ShowConsumables(); break;
            case 4: ShowMaterials(); break;
            default: ShowAll(); break;
        }
    }

    private void ShowAll()
    {
        list.ForEach(ia => ZetanUtility.SetActive(ia, true));
    }

    private void ShowEquipments()
    {
        list.ForEach(ia =>
        {
            if (ia.Data.IsEquipment)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        });
    }

    private void ShowConsumables()
    {
        list.ForEach(ia =>
        {
            if (ia.Data.IsConsumable)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        });
    }

    private void ShowMaterials()
    {
        list.ForEach(ia =>
        {
            if (ia.Data.IsMaterial)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        });
    }
    #endregion
    #endregion

    #region 消息
    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(MakingManager.LearnedNewMakingItem, _ => Refresh(), this);
        NotifyCenter.AddListener(NotifyCenter.CommonKeys.PlayerStateChanged, OnPlayerStateChanged, this);
    }
    public void OnPlayerStateChanged(params object[] msg)
    {
        if (IsMaking && msg.Length > 0 && msg[0] is CharacterStates.Normal && (msg[1] is CharacterNormalStates.Walk || msg[1] is CharacterNormalStates.Run))
            Interrupt();
    }
    public void OnItemAmountChanged(params object[] msg)
    {
        if (descOpen) ShowDescription(currentItem);
    }
    #endregion
}
