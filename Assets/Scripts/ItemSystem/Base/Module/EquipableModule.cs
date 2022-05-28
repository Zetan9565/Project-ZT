using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("装备"), Require(typeof(AttributeModule), typeof(UsableModule))]
    public class EquipableModule : ItemModule
    {
        [SerializeField, Enum(typeof(EquipmentType))]
        private int type;
        public EquipmentType Type => EquipmentTypeEnum.Instance[type];

        public override bool IsValid => type >= 0;

        public override ItemModuleData CreateData(ItemData item)
        {
            return new EquipmentData(item, this);
        }
    }
}