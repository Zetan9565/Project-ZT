using UnityEngine;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    [CreateAssetMenu(fileName = "learn to make item", menuName = "Zetan Studio/道具/用途/学习制作道具")]
    public sealed class LearnToMakeItem : ItemUsage
    {
        public override string Name => "学习制作道具";

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<MakingBlueprintModule>() is not MakingBlueprintModule making) return false;
            return MakingManager.Instance.Learn(making.Product);
        }
    }
}