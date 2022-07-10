using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.UI;

public abstract class InventoryWindow : Window, IHideable
{
    [SerializeField]
    protected GridView<ItemSlot, ItemSlotData> grid;
    public GridView<ItemSlot, ItemSlotData> Grid => grid;

    [SerializeField]
    protected ItemTypeDropDown pageSelector;

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

    protected List<string> types;

    public bool IsTyping => searchInput ? searchInput.isFocused : false;

    public bool IsHidden { get; private set; }

    public abstract InventoryHandler Handler { get; }

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
        if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selector) && selector.IsSelectFor(grid as ISlotContainer))
            WindowsManager.CloseWindow<ItemSelectionWindow>();
        if (WindowsManager.IsWindowOpen<ItemWindow>(out var item) && Handler.ContainsItem(item.Item)) item.Close();
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
        //SetPage(currentPage);
        //pageSelector.Value = pageSelector.Value;
    }

    protected virtual void ModifiySlot(ItemSlot item)
    {
        if (item is ItemSlotEx s)
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
    public void SetPage(int index)
    {
        //currentPage = index;
        pageSelector.Value = index;
        //if (index == 0)
        //    grid.ForEach(ia => ia.Show());
        //else
        //    grid.ForEach(ia =>
        //    {
        //        if (ia.IsEmpty || ia.Item.Model.Type.Name == types[index]) ia.Show();
        //        else ia.Hide();
        //    });
    }
    #endregion

    protected abstract ButtonWithTextData[] GetSlotButtons(ItemSlotEx slot);
    protected abstract void OnSlotEndDrag(GameObject go, ItemSlotEx slot);
    protected abstract void OnSlotRightClick(ItemSlotEx slot);

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
        pageSelector.container = grid as IFiltableItemContainer;
        searchButton.onClick.AddListener(Search);
        grid.SetItemModifier(ModifiySlot);
    }

    public void Search()
    {
        if (string.IsNullOrEmpty(searchInput.text))
        {
            pageSelector.Value = pageSelector.Value;
            return;
        }

        bool itemFilter(ItemSlot ia) => !ia.IsEmpty && ia.Item.Name.Contains(searchInput.text);
        grid.AddItemFilter(itemFilter, true);
        grid.RemoveItemFilter(itemFilter);
        searchInput.text = string.Empty;
    }

    public void Arrange()
    {
        Handler.Inventory.Arrange();
        Refresh();
        WindowsManager.CloseWindow<ItemWindow>();
    }

    public static ItemSelectionWindow OpenSelectionWindow<T>(ItemSelectionType selectionType, Action<List<CountedItem>> confirm, string title = null, string confirmDialog = null,
        int? typeLimit = null, Func<ItemData, int> amountLimit = null, Predicate<ItemData> selectCondition = null, Action cancel = null, params object[] args) where T : InventoryWindow
    {
        var window = WindowsManager.FindWindow<T>();
        if (window)
        {
            bool openBef = window.IsOpen, hiddenBef = window.IsHidden;
            if (window.IsHidden) WindowsManager.HideWindow<T>(false);
            if (!window.IsOpen) window.Open(args);
            var selection = ItemSelectionWindow.StartSelection(selectionType, window.grid as ISlotContainer, window.Handler, confirm, title, confirmDialog, typeLimit, amountLimit, selectCondition, cancel);
            selection.onClose += () =>
            {
                if (!openBef) window.Close(typeof(ItemSelectionWindow));
                else if (hiddenBef) WindowsManager.HideWindow<T>(true);
            };
            return selection;
        }
        return null;
    }

    public static ItemSelectionWindow OpenSelectionWindow<T>(ItemSelectionType selectionType, Func<List<CountedItem>, bool> confirm, string title = null, string confirmDialog = null,
        int? typeLimit = null, Func<ItemData, int> amountLimit = null, Predicate<ItemData> selectCondition = null, Action cancel = null, params object[] args) where T : InventoryWindow
    {
        var window = WindowsManager.FindWindow<T>();
        if (window)
        {
            bool openBef = window.IsOpen, hiddenBef = window.IsHidden;
            if (window.IsHidden) WindowsManager.HideWindow<T>(false);
            if (!window.IsOpen) window.Open(args);
            var selection = ItemSelectionWindow.StartSelection(selectionType, window.grid as ISlotContainer, window.Handler, confirm, title, confirmDialog, typeLimit, amountLimit, selectCondition, cancel);
            selection.onClose += () =>
            {
                if (!openBef) window.Close(typeof(ItemSelectionWindow));
                else if (hiddenBef) WindowsManager.HideWindow<T>(true);
            };
            return selection;
        }
        return null;
    }
}