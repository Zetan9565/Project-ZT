using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "learn to make item", menuName = "Zetan Studio/道具/用途/学习制作道具")]
    public sealed class LearnToMakeItem : ItemUsage
    {
        public LearnToMakeItem()
        {
            _name = "学习制作道具";
        }

        protected override bool Use(ItemData item)
        {
            if (!item.TryGetModule<CraftBlueprintModule>(out var making)) return false;
            return CraftManager.Learn(making.Product);
        }
    }
}