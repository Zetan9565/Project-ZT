using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    using ZetanStudio;
    using ZetanStudio.InteractionSystem.UI;
    using ZetanStudio.InventorySystem;
    using ZetanStudio.LootSystem;
    using ZetanStudio.UI;

    [DisallowMultipleComponent]
    [AddComponentMenu("Zetan Studio/管理器/拾取管理器")]
    public class LootWindow : InteractionWindow<LootAgent>
    {
        [SerializeField]
        private Button takeAllButton;

        [SerializeField]
        private int slotCount = 10;

        [SerializeField]
        private GridView<ItemSlot, ItemSlotData> productList;

        private LootAgent lootAgent;
        public override LootAgent Target => lootAgent;

        private ButtonWithTextData[] GetHandleButtons(ItemSlotEx slot)
        {
            if (!slot || slot.IsEmpty) return null;

            List<ButtonWithTextData> buttons = new List<ButtonWithTextData>
        {
            new ButtonWithTextData("取出", delegate
            {
                TakeItem(slot.Data);
            }),
        };
            if (lootAgent.lootItems.Find(x => x.source == slot.Item).amount > 1)
                buttons.Add(new ButtonWithTextData("全部取出", delegate
                {
                    TakeItem(slot.Data, true);
                }));
            return buttons.ToArray();
        }

        public void TakeItem(ItemSlotData info, bool all = false)
        {
            if (!lootAgent || info == null || !info.item) return;
            WindowsManager.CloseWindow<ItemWindow>();
            if (!all)
                if (info.amount == 1) OnTake(info, 1);
                else
                {
                    AmountWindow.StartInput(delegate (long amount)
                    {
                        OnTake(info, (int)amount);
                    }, info.amount, "拾取数量", Utility.ScreenCenter, Vector2.zero);
                }
            else OnTake(info, info.amount);
        }

        private void OnTake(ItemSlotData item, int amount)
        {
            if (!lootAgent) return;
            if (!lootAgent.lootItems.Exists(x => x.source == item.item)) return;
            int takeAmount = BackpackManager.Instance.Inventory.PeekGet(item.item, amount);
            if (BackpackManager.Instance.Get(item.item, takeAmount)) item.amount -= takeAmount;
            if (item.amount < 1) lootAgent.lootItems.RemoveAll(x => x.source == item.item);
            if (lootAgent.lootItems.Count < 1)
            {
                lootAgent.Recycle();
                Close();
            }
            else productList.Refresh(ItemSlotData.Convert(lootAgent.lootItems, slotCount));
        }

        public void TakeAll()
        {
            if (!lootAgent) return;
            foreach (var item in lootAgent.lootItems)
            {
                int takeAmount = BackpackManager.Instance.Inventory.PeekGet(item.source, item.amount);
                if (BackpackManager.Instance.Get(item.source, takeAmount)) item.amount -= takeAmount;
                productList.Refresh(ItemSlotData.Convert(lootAgent.lootItems, slotCount));
            }
            lootAgent.lootItems.RemoveAll(x => x.amount < 1);
            if (lootAgent.lootItems.Count < 1)
            {
                lootAgent.Recycle();
                Close();
            }
        }

        protected override bool OnOpen(params object[] args)
        {
            if (IsOpen)
            {
                MessageManager.Instance.New("请先拾取完上一个物品");
                return false;
            }
            LootAgent lootAgent = openBy as LootAgent;
            if (!lootAgent) return false;
            this.lootAgent = lootAgent;
            productList.Refresh(ItemSlotData.Convert(this.lootAgent.lootItems, slotCount));
            return base.OnOpen(args);
        }

        protected override bool OnClose(params object[] args)
        {
            base.OnClose(args);
            WindowsManager.CloseWindow<ItemWindow>();
            lootAgent = null;
            productList.Clear();
            return true;
        }
        protected override void OnAwake()
        {
            takeAllButton.onClick.AddListener(TakeAll);
            productList.SetItemModifier(x =>
            {
                if (x is ItemSlotEx slot)
                    slot.SetCallbacks(GetHandleButtons, x => TakeItem(x.Data, true));
            });
        }
    }
}