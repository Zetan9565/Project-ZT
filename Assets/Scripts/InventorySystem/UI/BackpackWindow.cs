using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.ItemSystem.UI;
using ZetanStudio.ItemSystem.Module;

[DisallowMultipleComponent]
public class BackpackWindow : InventoryWindow
{
    [SerializeField]
    private CraftTool handworkButton;

    public override InventoryHandler Handler => BackpackManager.Instance;

    protected override string InventoryMoneyChangedMsgKey => BackpackManager.BackpackMoneyChanged;
    protected override string InventorySpaceChangedMsgKey => BackpackManager.BackpackSpaceChanged;
    protected override string InventoryWeightChangedMsgKey => BackpackManager.BackpackWeightChanged;
    protected override string ItemAmountChangedMsgKey => BackpackManager.BackpackItemAmountChanged;
    protected override string SlotStateChangedMsgKey => BackpackManager.BackpackSlotStateChanged;

    protected override ButtonWithTextData[] GetSlotButtons(ItemSlotEx slot)
    {
        if (!slot || slot.IsEmpty) return null;

        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
        if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && selecter.SourceHandler == Handler)
        {
            if (!slot.IsDark)
                buttons.Add(new ButtonWithTextData("选取", () =>
                {
                    selecter.Place(slot);
                }));
        }
        else if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Type == WarehouseWindow.OpenType.Store && warehouse.OtherWindow == this)
        {
            buttons.Add(new ButtonWithTextData("存入", () =>
            {
                warehouse.StoreItem(slot.Item);
            }));
            if (slot.Data.amount > 1)
                buttons.Add(new ButtonWithTextData("全部存入", () =>
                {
                    warehouse.StoreItem(slot.Item, true);
                }));
        }
        else if (WindowsManager.IsWindowOpen<ShopWindow>(out var shop))
        {
            if (slot.Item.GetModule<SellableModule>())
                buttons.Add(new ButtonWithTextData("出售", () =>
                {
                    shop.PurchaseItem(slot.Item, Handler.GetAmount(slot.Item));
                }));
        }
        else if (WindowsManager.IsWindowOpen<EnhancementWindow>(out var enhancement))
        {
            if (slot.Item.GetModule<EnhancementModule>())
                buttons.Add(new ButtonWithTextData("强化", () =>
                {
                    enhancement.SetItem(slot.Item);
                }));
            if (slot.Item.GetModule<EnhConsumableModule>())
                buttons.Add(new ButtonWithTextData("放入", () =>
                {
                    enhancement.SetConsumable(slot.Item);
                }));
        }
        else
        {
            if (slot.Item.TryGetModule<UsableModule>(out var usable))
            {
                string btn = usable.UseActionName;
                buttons.Add(new ButtonWithTextData(btn, () =>
                {
                    usable.Usage.Handle(slot.Item);
                }));
            }
            if (slot.Item.Discardable)
                buttons.Add(new ButtonWithTextData("丢弃", () =>
                {
                    InventoryUtility.DiscardItem(Handler, slot.Data.item, slot.transform.position);
                }));
        }
        return buttons.ToArray();
    }
    protected override void OnSlotEndDrag(GameObject go, ItemSlotEx slot)
    {
        ItemSlotEx target = go.GetComponentInParent<ItemSlotEx>();
        if (target && slot != target && grid.Contains(target)) slot.Swap(target);
        else if (go.GetComponentInParent<DiscardButton>() == discardButton)
        {
            InventoryUtility.DiscardItem(Handler, slot.Item, discardButton.transform.position);
        }
        else if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && selecter.IsSelectFor(grid as ISlotContainer)
            && (target && selecter.ContainsSlot(target) || go == selecter.PlacementArea))
            selecter.Place(slot);
        else if (target is EquipmentSlot eSlot && EquipableModule.SameType(eSlot.SlotType, slot.Item) && WindowsManager.IsWindowOpen<EquipmentWindow>(out var equipment) && equipment.ContainsSlot(eSlot))
            ItemUsage.UseItem(slot.Item);
    }
    protected override void OnSlotRightClick(ItemSlotEx slot)
    {
        if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Type == WarehouseWindow.OpenType.Store && warehouse.OtherWindow == this)
            warehouse.StoreItem(slot.Item, true);
        else if (WindowsManager.IsWindowOpen<ShopWindow>(out var shop))
            shop.PurchaseItem(slot.Item, Handler.GetAmount(slot.Item));
        else if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && selecter.SourceHandler == Handler)
            selecter.Place(slot);
        else if (WindowsManager.IsWindowOpen<EnhancementWindow>(out var enhancement))
        {
            if (slot.Item.GetModule<EnhancementModule>()) enhancement.SetItem(slot.Item);
            else if (slot.Item.GetModule<EnhConsumableModule>()) enhancement.SetConsumable(slot.Item);
        }
        else
        {
            if (slot.Item.TryGetModule<UsableModule>(out var usable))
            {
                usable.Usage.Handle(slot.Item);
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
            WindowsManager.OpenWindowBy<CraftWindow>(ZetanStudio.ItemSystem.Craft.CraftToolInformation.Handwork, Handler);
        });
    }
}