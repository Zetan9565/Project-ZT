using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("材料")]
    public class MaterialModule : ItemModule
    {
        [field: SerializeField, Enum(typeof(MaterialType))]
        private int type;
        public MaterialType Type => MaterialTypeEnum.Instance[type];

        public override bool IsValid => type >= 0;

        public static bool SameType(MaterialType materialType, Item item)
        {
            if (!item) return false;
            return item.TryGetModule<MaterialModule>(out var material) && material.Type == materialType;
        }
    }
}