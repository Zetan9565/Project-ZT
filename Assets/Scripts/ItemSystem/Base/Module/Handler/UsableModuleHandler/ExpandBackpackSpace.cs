using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "expand backpack space", menuName = "Zetan Studio/道具/用途/扩张背包空间")]
    public sealed class ExpandBackpackSpace : ItemUsage
    {
        public ExpandBackpackSpace()
        {
            _name = "扩张背包空间";
        }

        protected override bool Use(ItemData item)
        {
            if (!item.TryGetModule<SpaceExpandModule>(out var module)) return false;
            return BackpackManager.Instance.ExpandSpace(module.Space);
        }
    }
}