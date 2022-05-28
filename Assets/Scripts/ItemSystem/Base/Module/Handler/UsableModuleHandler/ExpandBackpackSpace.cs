using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [CreateAssetMenu(fileName = "expand backpack space", menuName = "Zetan Studio/道具/用途/扩张背包空间")]
    public sealed class ExpandBackpackSpace : ItemUsage
    {
        public override string Name => "扩张背包空间";

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<SpaceExpandModule>() is not SpaceExpandModule module) return false;
            return BackpackManager.Instance.ExpandSpace(module.Space);
        }
    }
}