using System;
using UnityEngine;

namespace ZetanStudio.Item
{
    [CreateAssetMenu(fileName = "item quality", menuName = "Zetan Studio/道具/枚举/道具品质")]
    public sealed class ItemQualityEnum : ScriptableObjectEnum<ItemQualityEnum, ItemQuality>
    {
        public ItemQualityEnum()
        {
            _enum = new ItemQuality[]
            {
                new ItemQuality("凡品", Color.grey, 0),
                new ItemQuality("精品", new Color(0, 0.85f, 0, 1), 1),
                new ItemQuality("珍品", new Color(0, 0.8f, 0.75f, 1), 2),
                new ItemQuality("极品", new Color(1, 0.75f, 0, 1), 3),
                new ItemQuality("绝品", new Color(0.95f, 0.4f, 0.15f, 1), 4),
            };
        }

        public static Color IndexToColor(int quality)
        {
            if (quality < 0 || quality > Instance._enum.Length) return default;
            else return Instance._enum[quality].Color;
        }
    }

    [Serializable]
    public sealed class ItemQuality : ScriptableObjectEnumItem
    {
        [field: SerializeField]
        public Color Color { get; private set; }

        [field: SerializeField]
        public int Priority { get; private set; }

        public ItemQuality() : this("普通", Color.clear, 0) { }

        public ItemQuality(string name, Color color, int priority)
        {
            Name = name;
            Color = color;
            Priority = priority;
        }
    }
}