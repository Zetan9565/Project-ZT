using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using InventorySystem;
    using ItemSystem;

    [Name("道具条件")]
    [Description("持有指定道具时才可进入从本结点开始的分支。")]
    public class ItemBlocker : BlockerNode
    {
        [field: SerializeField]
        public Item Item { get; private set; }

        [field: SerializeField]
        public bool Have { get; private set; } = true;

        public override bool IsValid => Item;

        protected override bool CheckCondition() => !(BackpackManager.Instance.HasItem(Item) ^ Have);

        protected override string GetNotification(bool result)
        {
            if (!result && Have) return $"未持有[{ItemFactory.GetColorName(Item)}]时无法继续";
            else if (!result && !Have) return $"持有时[{ItemFactory.GetColorName(Item)}]无法继续";
            else return string.Empty;
        }
    }
}