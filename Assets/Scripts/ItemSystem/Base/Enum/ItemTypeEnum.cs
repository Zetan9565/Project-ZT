using UnityEngine;

namespace ZetanStudio.Item
{
    [CreateAssetMenu(fileName = "item type", menuName = "Zetan Studio/道具/枚举/道具类型")]
    public sealed class ItemTypeEnum : ScriptableObjectEnum<ItemTypeEnum, ItemType>
    {
        public ItemTypeEnum()
        {
            @enum = new ItemType[]
            {
                new ItemType("一般", 999),
                new ItemType("装备", 1),
                new ItemType("消耗品", 2),
                new ItemType("镶嵌", 3),
                new ItemType("材料", 4),
                new ItemType("放置", 5),
                new ItemType("特殊", 6),
                new ItemType("文件", 7),
                new ItemType("贸易品", 8),
                new ItemType("货币", -1),
            };
        }
    }

    [System.Serializable]
    public sealed class ItemType : ScriptableObjectEnumItem
    {
        [field: SerializeField]
        public int Priority { get; private set; }

        [field: SerializeField, SpriteSelector]
        public Sprite Icon { get; private set; }

        public ItemType() : this("一般", 999) { }

        public ItemType(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }
    }
}