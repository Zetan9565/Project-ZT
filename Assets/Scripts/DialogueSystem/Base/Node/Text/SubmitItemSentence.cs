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

    [Serializable, Name("提交道具")]
    [Description("提交道具后说出本句。")]
    public class SubmitItemSentence : SentenceNode
    {
        [SerializeField]
        private ItemInfo[] itemsToSubmit = { };
        public ReadOnlyCollection<ItemInfo> ItemsToSubmit => new ReadOnlyCollection<ItemInfo>(itemsToSubmit);

        public override bool IsValid => base.IsValid && itemsToSubmit.Length > 0 && itemsToSubmit.All(x => x.IsValid);

        public SubmitItemSentence() { }

        public SubmitItemSentence(string talker, string content, params ItemInfo[] items)
        {
            Talker = talker;
            Text = content;
            itemsToSubmit = items;
        }

        public override bool IsManual() => true;

        public override void DoManual(DialogueWindow window)
        {
            if (InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, cancel: cancel, amountLimit: amount, selectCondition: condition))
                WindowsManager.HideWindow(window, true);

            bool confirm(IEnumerable<CountedItem> items)
            {
                foreach (var info in itemsToSubmit)
                {
                    if (items.FirstOrDefault(x => info.Item == x.source.Model) is not CountedItem find || info.Amount != find.amount)
                    {
                        MessageManager.Instance.New(L.Tr(typeof(Dialogue).Name, "数量不正确"));
                        return false;
                    }
                }
                if (BackpackManager.Instance.Lose(items)) window.ContinueWith(this);
                WindowsManager.HideWindow(window, false);
                return true;
            }
            bool condition(ItemData item) => itemsToSubmit.Any(x => x.Item == item?.Model);
            int amount(ItemData item) => itemsToSubmit.FirstOrDefault(x => x.Item == item.Model)?.Amount ?? 0;
            void cancel() => WindowsManager.HideWindow(window, false);
        }
    }
}