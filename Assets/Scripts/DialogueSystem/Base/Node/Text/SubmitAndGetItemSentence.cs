using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using InventorySystem;
    using InventorySystem.UI;
    using ItemSystem;
    using ItemSystem.UI;
    using UI;

    [Serializable, Name("交换道具")]
    [Description("提交道具以获得其它道具，交换成功后说出本句。")]
    public sealed class SubmitAndGetItemSentence : SubmitItemSentence
    {
        [SerializeField]
        private ItemInfo[] itemsCanGet = { };
        public ReadOnlyCollection<ItemInfo> ItemsCanGet => new ReadOnlyCollection<ItemInfo>(itemsCanGet);

        public override bool IsValid => base.IsValid && itemsCanGet.Length > 0 && itemsCanGet.All(x => x.IsValid);

        public SubmitAndGetItemSentence() { }

        public SubmitAndGetItemSentence(string talker, string content, ItemInfo[] itemsToSubmit, ItemInfo[] itemsCanGet) : base(talker, content, itemsToSubmit)
        {
            Talker = talker;
            Text = content;
            this.itemsCanGet = itemsCanGet;
        }

        public override void DoManual(DialogueWindow window)
        {
            if (InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, cancel: cancel, amountLimit: amount, selectCondition: condition))
                WindowsManager.HideWindow(window, true);

            bool confirm(IEnumerable<CountedItem> items)
            {
                foreach (var info in ItemsToSubmit)
                {
                    if (items.FirstOrDefault(x => info.Item == x.source.Model) is not CountedItem find || info.Amount != find.amount)
                    {
                        MessageManager.Instance.New(L.Tr(typeof(Dialogue).Name, "数量不正确"));
                        return false;
                    }
                }
                if (BackpackManager.Instance.Lose(items, CountedItem.Convert(itemsCanGet))) window.ContinueWith(this);
                WindowsManager.HideWindow(window, false);
                return true;
            }
            bool condition(ItemData item) => ItemsToSubmit.Any(x => x.Item == item?.Model);
            int amount(ItemData item) => ItemsToSubmit.FirstOrDefault(x => x.Item == item.Model)?.Amount ?? 0;
            void cancel() => WindowsManager.HideWindow(window, false);
        }
    }
}