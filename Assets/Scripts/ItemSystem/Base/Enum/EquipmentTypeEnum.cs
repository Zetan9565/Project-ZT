using UnityEngine;

namespace ZetanStudio.Item
{
    [CreateAssetMenu(fileName = "equipment type", menuName = "Zetan Studio/道具/枚举/装备类型")]
    public sealed class EquipmentTypeEnum : ScriptableObjectEnum<EquipmentTypeEnum, EquipmentType>
    {
        public EquipmentTypeEnum()
        {
            _enum = new EquipmentType[]
            {
                new EquipmentType("主武器", 0),
                new EquipmentType("副武器", 1),
                new EquipmentType("头盔", 2),
                new EquipmentType("盔甲", 3),
                new EquipmentType("手套", 4),
                new EquipmentType("鞋子", 5),
                new EquipmentType("项链", 6),
                new EquipmentType("戒指", 7),
            };
        }
    }

    [System.Serializable]
    public sealed class EquipmentType : ScriptableObjectEnumItem
    {
        [field: SerializeField]
        public int Priority { get; private set; }

        [field: SerializeField, SpriteSelector]
        public Sprite Icon { get; private set; }

        public EquipmentType() : base() { }

        public EquipmentType(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }
    }
}