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
    }
}