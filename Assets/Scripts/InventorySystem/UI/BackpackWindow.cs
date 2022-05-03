using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BackpackWindow : InventoryWindow
{
    [SerializeField]
    private MakingTool handworkButton;

    public override IInventoryHandler Handler => BackpackManager.Instance;

    protected override string InventoryMoneyChangedMsgKey => BackpackManager.BackpackMoneyChanged;
    protected override string InventorySpaceChangedMsgKey => BackpackManager.BackpackSpaceChanged;
    protected override string InventoryWeightChangedMsgKey => BackpackManager.BackpackWeightChanged;
    protected override string ItemAmountChangedMsgKey => BackpackManager.BackpackItemAmountChanged;
    protected override string SlotStateChangedMsgKey => BackpackManager.BackpackSlotStateChanged;

    protected override ButtonWithTextData[] GetSlotButtons(ItemSlot slot)
    {
        if (!slot || slot.IsEmpty) return null;

        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
        if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && selecter.SourceHandler == Handler)
        {
            if (!slot.IsDark)
                buttons.Add(new ButtonWithTextData("选取", delegate
                {
                    selecter.Place(slot);
                }));
        }
        else if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Type == WarehouseWindow.OpenType.Store && warehouse.OtherWindow == this)
        {
            buttons.Add(new ButtonWithTextData("存入", delegate
            {
                warehouse.StoreItem(slot.Item);
            }));
            if (slot.Data.amount > 1)
                buttons.Add(new ButtonWithTextData("全部存入", delegate
                {
                    warehouse.StoreItem(slot.Item, true);
                }));
        }
        else if (WindowsManager.IsWindowOpen<ShopWindow>(out var shop))
        {
            if (slot.Data.Model.SellAble)
                buttons.Add(new ButtonWithTextData("出售", delegate
                {
                    shop.PurchaseItem(slot.Item, Handler.GetAmount(slot.Item));
                }));
        }
        else
        {
            if (slot.Item.Model_old.Usable)
            {
                string btn = "使用";
                if (slot.Item.Model_old.IsEquipment)
                    btn = "装备";
                buttons.Add(new ButtonWithTextData(btn, delegate
                {
                    BackpackManager.Instance.UseItem(slot.Item);
                }));
            }
            if (slot.Item.Model_old.DiscardAble)
                buttons.Add(new ButtonWithTextData("丢弃", delegate
                {
                    InventoryUtility.DiscardItem(Handler, slot.Data.item, slot.transform.position);
                }));
        }
        return buttons.ToArray();
    }
    protected override void OnSlotEndDrag(GameObject go, ItemSlot slot)
    {
        ItemSlot target = go.GetComponentInParent<ItemSlot>();
        if (target && slot != target && grid.ContainsItem(target)) slot.Swap(target);
        else if (go.GetComponentInParent<DiscardButton>() == discardButton)
        {
            InventoryUtility.DiscardItem(Handler, slot.Item, discardButton.transform.position);
        }
        else if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && selecter.IsSelectFor(grid as ISlotContainer)
            && (target && selecter.ContainsSlot(target) || go == selecter.PlacementArea))
            selecter.Place(slot);
    }
    protected override void OnSlotRightClick(ItemSlot slot)
    {
        if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Type == WarehouseWindow.OpenType.Store && warehouse.OtherWindow == this)
        {
            warehouse.StoreItem(slot.Item, true);
        }
        else if (WindowsManager.IsWindowOpen<ShopWindow>(out var shop))
        {
            shop.PurchaseItem(slot.Item, Handler.GetAmount(slot.Item));
        }
        else if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && selecter.SourceHandler == Handler)
        {
            selecter.Place(slot);
        }
        else
        {
            if (slot.Item.Model_old.Usable)
            {
                BackpackManager.Instance.UseItem(slot.Item);
                slot.Refresh();
            }
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!handworkButton.GetComponent<Button>()) handworkButton.gameObject.AddComponent<Button>();
        handworkButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            WindowsManager.OpenWindowBy<MakingWindow>(MakingToolInformation.Handwork, Handler);
        });
    }
}