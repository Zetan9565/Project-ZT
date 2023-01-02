using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using InventorySystem;

    [Serializable, Name("获得道具")]
    [Description("说出本句后获得道具。")]
    public sealed class GetItemSentence : SentenceNode
    {
        [SerializeField]
        private ItemInfo[] itemsCanGet = { };
        public ReadOnlyCollection<ItemInfo> ItemsCanGet => new ReadOnlyCollection<ItemInfo>(itemsCanGet);

        public override bool IsValid => base.IsValid && itemsCanGet.Length > 0 && itemsCanGet.All(i => i.IsValid);

        public GetItemSentence() { }

        public GetItemSentence(string talker, string content, params ItemInfo[] items)
        {
            Talker = talker;
            Text = content;
            itemsCanGet = items;
        }

        public override bool OnEnter()
        {
            return BackpackManager.Instance.Get(itemsCanGet);
        }
    }
}