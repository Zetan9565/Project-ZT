using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("材料")]
    public class MaterialModule : ItemModule
    {
        [field: SerializeField, Enum(typeof(MaterialType))]
        private int type;
        public MaterialType Type => MaterialTypeEnum.Instance[type];

        public override bool IsValid => type >= 0;

        public static bool Compare(Item item, MaterialType materialType)
        {
            if (!item) return false;
            return item.GetModule<MaterialModule>() is MaterialModule material && material.Type == materialType;
        }
    }
}