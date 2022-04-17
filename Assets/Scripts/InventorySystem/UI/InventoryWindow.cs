using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InventoryWindow : Window, IHideable
{
    [SerializeField]
    protected GridView<ItemSlotBase, ItemSlotData> grid;
    public GridView<ItemSlotBase, ItemSlotData> Grid => grid;

    [SerializeField]
    protected Dropdown pageSelector;

    [SerializeField]
    protected Text money;
    [SerializeField]
    protected Text weight;
    [SerializeField]
    protected Text size;

    [SerializeField]
    protected Button sortButton;
    [SerializeField]
    protected DiscardButton discardButton;
    [SerializeField]
    protected InputField searchInput;
    [SerializeField]
    protected Button searchButton;

    public bool IsHidden { get; private set; }

    public abstract IInventoryHandler Handler { get; }

    protected abstract string InventoryMoneyChangedMsgKey { get; }
    protected abstract string InventorySpaceChangedMsgKey { get; }
    protected abstract string InventoryWeightChangedMsgKey { get; }
    protected abstract string ItemAmountChangedMsgKey { get; }
    protected abstract string SlotStateChangedMsgKey { get; }

    protected override bool OnOpen(params object[] args)
    {
        if (IsHidden) return false;
        Refresh();
        return true;
    }

    protected override bool OnClose(params object[] args)
    {
        if (NewWindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selector) && selector.IsSelectFor(grid as ISlotContainer))
            NewWindowsManager.CloseWindow<ItemSelectionWindow>();
        pageSelector.value = 0;
        IsHidden = false;
        return true;
    }

    public void Hide(bool hide, params object[] args)
    {
        if (!IsOpen) return;
        content.blocksRaycasts = !hide;
        content.alpha = hide ? 0 : 1;
        IsHidden = hide;
    }

    public virtual void Refresh()
    {
        grid.Refresh(Handler.Inventory.Slots);
        RefreshMoney();
        RefreshSpace();
        RefreshWeight();
        SetPage(currentPage);
    }

    protected virtual void ModifiySlot(ItemSlotBase item)
    {
        if (item is ItemSlot s)
            s.SetCallbacks(GetSlotButtons, OnSlotRightClick, OnSlotEndDrag);
    }

    public virtual void RefreshMoney()
    {
        money.text = Handler.Inventory.Money.ToString();
    }

    public virtual void RefreshWeight()
    {
        weight.text = $"{Handler.Inventory.WeightCost:F2}/{Handler.Inventory.WeightLimit}";
    }

    public virtual void RefreshSpace()
    {
        size.text = $"{Handler.Inventory.SpaceCost}/{Handler.Inventory.SpaceLimit}";
    }

    #region 道具页相关
    protected int currentPage;
    public void SetPage(int index)
    {
        currentPage = index;
        switch (index)
        {
            case 1: ShowEquipments(); break;
            case 2: ShowConsumables(); break;
            case 3: ShowMaterials(); break;
            default: ShowAll(); break;
        }
    }

    protected void ShowAll()
    {
        grid.ForEach(ia => ia.Show());
    }

    protected void ShowEquipments()
    {
        grid.ForEach(ia =>
        {
            if (ia.IsEmpty || ia.Item.Model.IsEquipment) ia.Show();
            else ia.Hide();
        });
    }

    protected void ShowConsumables()
    {
        grid.ForEach(ia =>
        {
            if (ia.IsEmpty || ia.Item.Model.IsConsumable) ia.Show();
            else ia.Hide();
        });
    }

    protected void ShowMaterials()
    {
        grid.ForEach(ia =>
        {
            if (ia.IsEmpty || ia.Item.Model.IsMaterial) ia.Show();
            else ia.Hide();
        });
    }
    #endregion

    protected abstract ButtonWithTextData[] GetSlotButtons(ItemSlot slot);
    protected abstract void OnSlotEndDrag(GameObject go, ItemSlot slot);
    protected abstract void OnSlotRightClick(ItemSlot slot);

    #region 消息相关
    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(InventoryMoneyChangedMsgKey, OnInventoryMoneyChanged, this);
        NotifyCenter.AddListener(InventorySpaceChangedMsgKey, OnInventorySpaceChanged, this);
        NotifyCenter.AddListener(InventoryWeightChangedMsgKey, OnInventoryWeightChanged, this);
        NotifyCenter.AddListener(ItemAmountChangedMsgKey, OnItemAmountChanged, this);
        NotifyCenter.AddListener(SlotStateChangedMsgKey, OnSlotStateChanged, this);
    }

    protected virtual void OnInventoryWeightChanged(object[] msg)
    {
        if (IsOpen) RefreshWeight();
    }
    protected virtual void OnInventorySpaceChanged(object[] msg)
    {
        if (IsOpen)
        {
            grid.Refresh();
            RefreshSpace();
        }
    }
    protected virtual void OnInventoryMoneyChanged(params object[] msg)
    {
        if (IsOpen) RefreshMoney();
    }
    protected virtual void OnItemAmountChanged(params object[] msg)
    {
        if (IsOpen)
        {
            grid.Refresh();
            RefreshSpace();
            RefreshWeight();
        }
    }
    protected virtual void OnSlotStateChanged(params object[] msg)
    {
        if (IsOpen && msg.Length > 0 && msg[0] is ItemSlotData slot && slot.index >= 0 && slot.index < grid.Count)
        {
            grid.RefreshItem(slot.index);
        }
    }
    #endregion

    protected override void OnAwake()
    {
        discardButton.SetWindow(this);
        sortButton.onClick.AddListener(Arrange);
        pageSelector.onValueChanged.AddListener(SetPage);
        searchButton.onClick.AddListener(Search);
        grid.SetItemModifier(ModifiySlot);
    }

    public void Search()
    {
        if (string.IsNullOrEmpty(searchInput.text))
        {
            SetPage(currentPage);
            return;
        }
        grid.ForEach(ia =>
        {
            if (!(!ia.IsEmpty && ia.Info.ItemName.Contains(searchInput.text))) ia.Hide();
        });
        searchInput.text = string.Empty;
    }

    public void Arrange()
    {
        Handler.Inventory.Arrange();
        Refresh();
        NewWindowsManager.CloseWindow<ItemWindow>();
    }

    public static ItemSelectionWindow OpenSelectionWindow<T>(ItemSelectionType selectionType, Action<List<ItemWithAmount>> confirm, string title = null, string confirmDialog = null,
        int? typeLimit = null, int? amountLimit = null, Predicate<ItemSlotBase> selectCondition = null, Action cancel = null, params object[] args) where T : InventoryWindow
    {
        var window = NewWindowsManager.FindWindow<T>();
        if (window)
        {
            bool openBef = window.IsOpen, hiddenBef = window.IsHidden;
            if (window.IsHidden) NewWindowsManager.HideWindow<T>(false);
            if (!window.IsOpen) window.Open(args);
            var selection = ItemSelectionWindow.StartSelection(selectionType, window.grid as ISlotContainer, window.Handler, confirm, title, confirmDialog, typeLimit, amountLimit, selectCondition, cancel);
            selection.onClose += () =>
            {
                if (!openBef) window.Close(typeof(ItemSelectionWindow));
                else if (hiddenBef) NewWindowsManager.HideWindow<T>(true);
            };
            return selection;
        }
        return null;
    }
}