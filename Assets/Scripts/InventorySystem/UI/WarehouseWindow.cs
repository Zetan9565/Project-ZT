using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.InventorySystem.UI
{
    using ItemSystem;
    using ItemSystem.UI;
    using ZetanStudio;
    using ZetanStudio.InventorySystem;
    using ZetanStudio.UI;

    public class WarehouseWindow : InventoryWindow
    {
        public override InventoryHandler Handler => WarehouseManager.Instance;

        protected override string InventoryMoneyChangedMsgKey => WarehouseManager.WarehouseMoneyChanged;
        protected override string InventorySpaceChangedMsgKey => WarehouseManager.WarehouseSpaceChanged;
        protected override string InventoryWeightChangedMsgKey => WarehouseManager.WarehouseWeightChanged;
        protected override string ItemAmountChangedMsgKey => WarehouseManager.WarehouseItemAmountChanged;
        protected override string SlotStateChangedMsgKey => WarehouseManager.WarehouseSlotStateChanged;

        protected override ButtonWithTextData[] GetSlotButtons(ItemSlotEx slot)
        {
            if (!slot || slot.IsEmpty) return null;

            List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
            switch (Type)
            {
                case OpenType.Store:
                    buttons.Add(new ButtonWithTextData("取出", () => TakeOutItem(slot.Item)));
                    if (slot.Data.amount > 1)
                        buttons.Add(new ButtonWithTextData("全部取出", () => TakeOutItem(slot.Item, true)));
                    break;
                case OpenType.Craft:
                    if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter))
                    {
                        if (!slot.IsDark)
                            buttons.Add(new ButtonWithTextData("选取", () => selecter.Place(slot)));
                    }
                    break;
                case OpenType.Preview:
                    buttons.Add(new ButtonWithTextData("转移", () => { Debug.Log("打开转移窗口"); }));
                    break;
                default:
                    break;
            }
            return buttons.ToArray();
        }

        public OpenType Type { get; private set; }

        public InventoryWindow OtherWindow { get; private set; }

        private Warehouse warehouse;
        private bool otherOpenBef = false;
        private bool otherHiddenBef = false;

        protected override bool OnOpen(params object[] args)
        {
            if (args == null || args.Length < 2) return false;
            Type = (OpenType)args[0];
            WarehouseManager.Instance.SetManagedWarehouse(args[1] as IWarehouseKeeper);
            switch (Type)
            {
                case OpenType.Store:
                    if (args.Length > 2)
                    {
                        OtherWindow = args[2] as InventoryWindow;
                        otherOpenBef = OtherWindow.IsOpen;
                        if (!otherOpenBef) OtherWindow.Open();
                        else otherHiddenBef = OtherWindow.IsHidden;
                        if (otherHiddenBef) WindowsManager.HideWindow(OtherWindow, false);
                        OtherWindow.onClose += () => CloseBy(OtherWindow);
                        break;
                    }
                    else return false;
                case OpenType.Craft:
                    break;
                case OpenType.Preview:
                    break;
                default:
                    return false;
            }
            return base.OnOpen(args);
        }

        protected override bool OnClose(params object[] args)
        {
            if (Type == OpenType.Store && OtherWindow && !OtherWindow.Equals(closeBy))//被自己关闭
            {
                if (!otherOpenBef) OtherWindow.Close();
                else if (otherHiddenBef) WindowsManager.HideWindow(OtherWindow, true);
                OtherWindow = null;
            }
            if (warehouse) warehouse.EndManagement();
            warehouse = null;
            otherOpenBef = false;
            otherHiddenBef = false;
            return base.OnClose(args);
        }

        public void TakeOutItem(ItemData item, bool all = false)
        {
            if (Handler == null || item == null) return;
            WindowsManager.CloseWindow<ItemWindow>();
            int have = Handler.GetAmount(item);
            if (!all)
                if (have == 1 && OnTakeOut(item, 1, 1) > 0)
                    MessageManager.Instance.New($"取出了1个 [{item.ColorName}");
                else
                {
                    AmountWindow.StartInput(delegate (long amount)
                    {
                        int take = OnTakeOut(item, (int)amount, have);
                        if (take > 0)
                            MessageManager.Instance.New($"取出了{take}个 [{item.ColorName}]");
                    }, have, "取出数量", Utility.ScreenCenter, Vector2.zero);
                }
            else
            {
                int take = OnTakeOut(item, have, have);
                if (take > 0)
                    MessageManager.Instance.New($"取出了{have + take}个 [{item.ColorName}]");
            }
        }

        private int OnTakeOut(ItemData item, int amount, int have)
        {
            if (Handler == null || item == null || amount < 1) return 0;
            int finalLose = have < amount ? have : amount;
            if (Handler.CanLose(item, finalLose) && OtherWindow.Handler.CanGet(item, finalLose))
            {
                Handler.Inventory.TransferItem(OtherWindow.Handler.Inventory, item, finalLose);
                return finalLose;
            }
            else return 0;
        }

        public void StoreItem(ItemData item, bool all = false)
        {
            if (Handler == null || OtherWindow == null || !OtherWindow.Handler.Inventory || !item) return;
            WindowsManager.CloseWindow<ItemWindow>();
            int have = OtherWindow.Handler.GetAmount(item);
            if (have < 1) return;
            if (!all)
            {
                if (have == 1 && OnStore(item, 1) > 0)
                    MessageManager.Instance.New($"存入了1个 [{item.ColorName}]");
                else
                {
                    int maxGet = Handler.Inventory.PeekGet(item, have);
                    AmountWindow.StartInput(delegate (long amount)
                    {
                        int store = OnStore(item, (int)amount);
                        if (store > 0)
                            MessageManager.Instance.New($"存入了{store}个 [{item.ColorName}]");
                    }, have > maxGet ? maxGet : have, "存入数量", Utility.ScreenCenter, Vector2.zero);
                }
            }
            else
            {
                int amountBef = Handler.GetAmount(item);
                int store = OnStore(item, have);
                if (store > 0)
                    MessageManager.Instance.New($"存入了{store}个 [{item.ColorName}]");
            }
        }

        private int OnStore(ItemData item, int amount)
        {
            if (Handler == null || item == null || amount < 1) return 0;
            int finalGet = Handler.Inventory.PeekGet(item, amount);
            if (finalGet > 0) OtherWindow.Handler.Inventory.TransferItem(Handler.Inventory, item, amount);
            return finalGet;
        }

        protected override void OnSlotEndDrag(GameObject go, ItemSlotEx slot)
        {
            ItemSlotEx target = go.GetComponentInParent<ItemSlotEx>();
            if (Type != OpenType.Preview && target && slot != target && grid.Contains(target)) slot.Swap(target);
            else if (Type != OpenType.Preview && go.GetComponentInParent<DiscardButton>() == discardButton)
            {
                InventoryUtility.OpenDiscardItemPanel(Handler, slot.Item, discardButton.transform.position);
            }
            else if (WindowsManager.IsWindowOpen<ItemSelectionWindow>(out var selecter) && (target && selecter.ContainsSlot(target) || go == selecter.PlacementArea))
                selecter.Place(slot);
            else if (Type == OpenType.Store && OtherWindow && (target && OtherWindow.Grid.Contains(target) || target == OtherWindow.Grid.Container))
                TakeOutItem(slot.Item);
        }

        protected override void OnSlotRightClick(ItemSlotEx slot)
        {
            switch (Type)
            {
                case OpenType.Store:
                    TakeOutItem(slot.Item, true);
                    break;
                case OpenType.Craft:
                    break;
                case OpenType.Preview:
                    break;
                default:
                    break;
            }
        }

        public override void RefreshWeight()
        {
            //仓库没有重量上限
            weight.text = Handler.Inventory.WeightCost.ToString("F2");
        }

        public enum OpenType
        {
            Store,
            Craft,
            Preview
        }
    }
}