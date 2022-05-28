using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [CreateAssetMenu(fileName = "learn to make item", menuName = "Zetan Studio/道具/用途/学习制作道具")]
    public sealed class LearnToMakeItem : ItemUsage
    {
        public override string Name => "学习制作道具";

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<CraftBlueprintModule>() is not CraftBlueprintModule making) return false;
            return CraftManager.Instance.Learn(making.Product);
        }
    }
}