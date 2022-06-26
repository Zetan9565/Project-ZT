using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Craft;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.ItemSystem.UI;

[DisallowMultipleComponent]
public class CraftWindow : Window, IHideable
{
    [SerializeField] private ItemTypeDropDown pageSelector;

    [SerializeField] private CraftList list;

    [SerializeField] private CanvasGroup descriptionWindow;

    [SerializeField] private Text nameText;
    [SerializeField] private ItemSlotEx icon;
    [SerializeField] private Text haveLabel;
    [SerializeField] private Text haveText;
    [SerializeField] private Text timeLabel;
    [SerializeField] private Text timeText;
    [SerializeField] private Text matLabel;
    [SerializeField] private MaterialList matList;
    [SerializeField] private Button craftButton;

    [SerializeField] private Toggle loopToggle;
    [SerializeField] private Button manualButton;
    [SerializeField] private InputField searchInput;
    [SerializeField] private Button searchButton;
    [SerializeField] private Dropdown inventorySelector;

    private Item currentItem;

    private InventoryHandler handler;
    private IWarehouseKeeper warehouse;
    private WarehouseSelectionWindow wsWindow;

    private int invTypeBef;
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
        craftButton.onClick.AddListener(CraftCurrent);
        manualButton.onClick.AddListener(OnManualClick);
        pageSelector.container = list;
        icon.SetCallbacks((i) =>
        {
            return new ButtonWithTextData[] { new ButtonWithTextData(Tr("制作"), CraftCurrent) };
        });
        list.Selectable = true;
        list.SetSelectCallback(OnListItemClick);
        matList.Clickable = true;
        matList.SetItemModifier(x => x.handler = handler, true);
        inventorySelector.onValueChanged.AddListener(SelectInventory);
    }

    private void OnListItemClick(CraftAgent agent)
    {
        if (agent) ShowDescription(agent.Data);
        else HideDescription();
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
            invTypeBef = type;
            warehouse = null;
            handler = BackpackManager.Instance;
            inventorySelector.ClearOptions();
            inventorySelector.AddOptions(new List<string> { Tr("从{0}中制作", BackpackManager.Instance.Name), Tr("从{0}中制作", WarehouseManager.Instance.Name) });
            ShowDescription(currentItem);
        }
        else if (type == 1 || type == 2)
        {
            Vector3 point = CurrentTool ? CurrentTool.transform.position : PlayerManager.Instance.Player.Position;
            wsWindow = WarehouseSelectionWindow.StartSelection(OnSetWarehouse, () => inventorySelector.SetValueWithoutNotify(invTypeBef), (k) => k == warehouse, false, true, point);
            if (!wsWindow) inventorySelector.value = 0;
            else
            {
                if (type != 2) invTypeBef = type;
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
        inventorySelector.AddOptions(new List<string> { Tr("从{0}中制作", BackpackManager.Instance.Name), $"{warehouse.WarehouseName}", Tr("选择其它仓库") });
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

    private void CraftCurrent()
    {
        if (!ToolInfo || !currentItem) return;
        if (IsCrafting)
        {
            MessageManager.Instance.New(Tr("正在制作中"));
            return;
        }
        var craftable = currentItem.GetModule<CraftableModule>();
        int amountCanCraft = handler.GetAmountCanCraft(craftable.Materials);
        if (amountCanCraft < 1)
        {
            MessageManager.Instance.New(Tr("材料不足"));
            return;
        }
        if (!Manualy(craftable))
        {
            if (amountCanCraft > 0 && amountCanCraft < 2)
            {
                ConfirmWindow.StartConfirm(Tr("确定制作1次 [{0}] 吗？", currentItem.Name), delegate
                {
                    IsCrafting = true;
                    WindowsManager.HideWindow(this, true);
                    ProgressBar.Instance.New(ToolInfo.MakingTime, delegate
                    {
                        IsCrafting = false;
                        WindowsManager.HideWindow(this, false);
                        ProductItem(currentItem);
                    },
                    OnCancel, Tr("制作中"), true);
                });
            }
            else
            {
                AmountWindow.StartInput(delegate (long amount)
                {
                    ConfirmWindow.StartConfirm(Tr("确定制作{0}次 [{1}] 吗？", (int)amount, currentItem.Name), delegate
                    {
                        int num = (int)amount;
                        IsCrafting = true;
                        WindowsManager.HideWindow(this, true);
                        ProgressBar.Instance.New(ToolInfo.MakingTime, num,
                            OnCancel,
                            delegate
                            {
                                ProductItem(currentItem);
                                IsCrafting = false;
                                NotifyCenter.PostNotify(CraftManager.CraftCanceled);
                                WindowsManager.HideWindow(this, false);
                                ProgressBar.Instance.Cancel();
                            },
                            OnCancel, "制作中", true);
                    });
                }, amountCanCraft, Tr("制作次数"), ZetanUtility.ScreenCenter, Vector2.zero);
            }
        }
        else
        {
            InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, CraftCurrent, Tr("放入一份材料"), selectCondition: CanSelect);
        }
    }

    private bool CanSelect(ItemSlot slot)
    {
        return slot.Item && slot.Item.Model.GetModule<MaterialModule>();
    }

    private void CraftCurrent(IEnumerable<CountedItem> materialsRaw)
    {
        //Debug.Log("Start making");
        var materials = ItemInfo.Convert(materialsRaw);
        IsCrafting = true;
        WindowsManager.HideWindow(this, true);
        if (loopToggle.isOn)
            ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                bool enough = handler.IsMaterialsEnough(currentItem.GetModule<CraftableModule>().Materials, materials);
                if (!enough) MessageManager.Instance.New(Tr("材料不足，无法继续制作"));
                return !enough && loopToggle.isOn;
            },
            OnCancel,
            delegate
            {
                if (!ProductItem(currentItem, materialsRaw))
                {
                    //Debug.Log("Failed3");
                    ProgressBar.Instance.Cancel();
                }
            },
            OnCancel, Tr("制作中"), true);
        else
            ProgressBar.Instance.New(ToolInfo.MakingTime,
                delegate
                {
                    ProductItem(currentItem, materialsRaw);
                    //Debug.Log("Finished");
                    IsCrafting = false;
                    WindowsManager.HideWindow(this, false);
                    ProgressBar.Instance.Cancel();
                },
                OnCancel, Tr("制作中"), true);
    }

    public void OnManualClick()
    {
        if (handler is BackpackManager) InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, CraftManualy, Tr("放入一份材料"), selectCondition: CanSelect);
        else if (handler is WarehouseManager) InventoryWindow.OpenSelectionWindow<WarehouseWindow>(ItemSelectionType.SelectNum, CraftManualy, Tr("放入一份材料"), selectCondition: CanSelect, args:
            new object[] { WarehouseWindow.OpenType.Craft, warehouse });
    }
    private void CraftManualy(IEnumerable<CountedItem> materialsRaw)
    {
        IsCrafting = true;
        WindowsManager.HideWindow(this, true);
        ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                ProductItem(materialsRaw);
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
            }, Tr("制作中"), true);
    }

    /// <summary>
    /// 产生道具，用于根据已知配方直接消耗材料来制作道具的玩法
    /// </summary>
    /// <param name="item">目标道具</param>
    /// <param name="amount">制作数量</param>
    private void ProductItem(Item item)
    {
        if (!item || !item.TryGetModule<CraftableModule>(out var craftable) || !craftable.IsValid) return;
        List<CountedItem> materialsRaw = handler.GetMaterials(craftable.Materials);
        if (handler.IsMaterialsEnough(craftable.Materials, ItemInfo.Convert(materialsRaw)))
        {
            int amoutBef = handler.GetAmount(item.ID);
            if (handler.Get(item, craftable.RandomAmount(), materialsRaw.ToArray()))
                MessageManager.Instance.New(Tr("制作了 {0} 个 [{1}]", handler.GetAmount(currentItem.ID) - amoutBef, currentItem.ColorName));
        }
        else MessageManager.Instance.New(Tr("材料不足，无法继续制作"));
    }
    /// <summary>
    /// 产生道具，用于根据已知配方自己放入材料来制作道具的玩法
    /// </summary>
    /// <param name="item">目标道具</param>
    /// <param name="materialsRaw">放入的材料</param>
    private bool ProductItem(Item item, IEnumerable<CountedItem> materialsRaw)
    {
        if (!item || materialsRaw == null || materialsRaw.Count() < 1 || !item.TryGetModule<CraftableModule>(out var craftable) || !craftable.IsValid)
        {
            MessageManager.Instance.New(Tr("无效的制作"));
            return false;
        }
        if (MaterialInfo.CheckMaterialsMatch(craftable.Materials, ItemInfo.Convert(materialsRaw)))
        {
            ItemData production = ItemFactory.MakeItem(item);
            int amount = craftable.RandomAmount();
            List<CountedItem> itemsToLose = new List<CountedItem>();
            foreach (var material in craftable.Materials)
            {
                if (material.CostType == MaterialCostType.SingleItem)
                {
                    CountedItem find = materialsRaw.FirstOrDefault(x => x.source.ModelID == material.ItemID);
                    if (!find || find.amount != material.Amount)
                    {
                        MessageManager.Instance.New(Tr("材料不正确，请确保只放入了一份"));
                        return false;//所提供的材料中没有这种材料或数量不符合，则无法制作
                    }
                    //否则加入消耗列表
                    else if (!handler.Inventory.PeekLose(find.source, find.amount, out _))
                    {
                        MessageManager.Instance.New(Tr("可用的[{0}]不足，无法继续制作", find.source.ColorName));
                        return false;//若任意一个相应数量的材料无法失去，则会导致总数量不符合，所以无法制作
                    }
                    else
                    {
                        //否则加入消耗列表
                        itemsToLose.Add(find);
                    }
                }
                else
                {
                    var finds = materialsRaw.Where(x => MaterialModule.SameType(material.MaterialType, x.source.Model));//找到种类相同的道具
                    if (finds.Count() > 0)
                    {
                        if (finds.Select(x => x.amount).Sum() != material.Amount)
                        {
                            MessageManager.Instance.New(Tr("材料不正确，请确保只放入了一份"));
                            return false;//若材料总数不符合，则无法制作
                        }
                        foreach (var find in finds)
                        {
                            if (!handler.Inventory.PeekLose(find.source, find.amount, out _))
                            {
                                MessageManager.Instance.New(Tr("可用的[{0}]类材料不足，无法继续制作", material.MaterialType));
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
                        MessageManager.Instance.New(Tr("可用的[{0}]类材料不足，无法继续制作", material.MaterialType));
                        return false;//材料不足
                    }
                }
            }
            if (amount > 0 && handler.Get(production, amount, itemsToLose.ToArray()))
            {
                MessageManager.Instance.New(Tr("生产了 {0} 个[{1}]", amount, production.ColorName));
                if (!CraftManager.IsLearned(item)) CraftManager.Learn(item);
                return true;
            }
            else return false;
        }
        else
        {
            MessageManager.Instance.New(Tr("材料不正确，请确保只放入了一份"));
            return false;
        }
    }
    /// <summary>
    /// 产生道具，用于自己放入材料来制作道具的玩法
    /// </summary>
    /// <param name="materials">放入的材料</param>
    private bool ProductItem(IEnumerable<CountedItem> materialsRaw)
    {
        var materials = ItemInfo.Convert(materialsRaw);
        foreach (var item in CraftManager.LearnedItems)
            if (check(materials, item, false))
                return ProductItem(item, materialsRaw);
        foreach (var item in Item.GetItems())
            if (check(materials, item, true))
                return ProductItem(item, materialsRaw);
        MessageManager.Instance.New(Tr("这样似乎什么也做不了"));
        return false;

        bool check(ItemInfo[] materials, Item item, bool tryCheck)
        {
            CraftableModule craftableModule = item.GetModule<CraftableModule>();
            if (!craftableModule) return false;
            if (tryCheck && !craftableModule.CanMakeByTry) return false;
            return ToolInfo.ToolType.Methods.Contains(craftableModule.CraftMethod) && MaterialInfo.CheckMaterialsMatch(craftableModule.Materials, materials);
        }
    }

    #region UI相关
    protected override bool OnOpen(params object[] args)
    {
        if (!PlayerManager.Instance.CheckIsIdleWithAlert()) return false;
        if (IsCrafting)
        {
            MessageManager.Instance.New(Tr("正在制作中"));
            return false;
        }
        if (IsHidden) return true;
        CurrentTool = openBy as CraftTool;
        toolInfo = openBy as CraftToolInformation;
        if (!ToolInfo) return false;
        handler = args[0] as InventoryHandler;
        if (handler != null) NotifyCenter.AddListener(handler.ItemAmountChangedMsgKey, OnItemAmountChanged);
        IsCrafting = false;
        list.Refresh(CraftManager.LearnedItems);
        invTypeBef = 0;
        pageSelector.Value = 0;
        inventorySelector.ClearOptions();
        inventorySelector.AddOptions(new List<string> { Tr("从{0}中制作", BackpackManager.Instance.Name), Tr("从{0}中制作", WarehouseManager.Instance.Name) });
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
        if (!item || !item.TryGetModule<CraftableModule>(out var craftable))
        {
            HideDescription();
            return;
        }
        currentItem = item;
        haveLabel.text = Tr("持有数量");
        haveText.text = handler.GetAmount(item).ToString();
        timeLabel.text = Tr("作业耗时");
        timeText.text = MiscFuntion.SecondsToSortTime(ToolInfo.MakingTime);
        matLabel.text = Tr("材料列表");
        matList.Refresh(craftable.Materials);
        int craftAmount = handler.GetAmountCanCraft(craftable.Materials);
        icon.SetItem(item, craftAmount.ToString());
        nameText.text = item.Name;
        craftButton.interactable = craftAmount > 0;
        descriptionWindow.alpha = 1;
        descriptionWindow.blocksRaycasts = true;
        descOpen = true;
        ZetanUtility.SetActive(loopToggle.gameObject, Manualy(craftable));
    }

    private bool Manualy(CraftableModule craftable)
    {
        return craftable.Materials.Any(x => !x.Item.StackAble || x.CostType == MaterialCostType.SameType);
    }

    public void HideDescription()
    {
        haveText.text = string.Empty;
        timeText.text = string.Empty;
        icon.Refresh(ItemSlotData.Empty);
        descriptionWindow.alpha = 0;
        descriptionWindow.blocksRaycasts = false;
        descOpen = false;
        currentItem = null;
    }

    public void Refresh()
    {
        if (!IsOpen) return;
        list.Refresh(CraftManager.LearnedItems);
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
