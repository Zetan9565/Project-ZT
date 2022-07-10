using System;
using UnityEngine;
using ZetanStudio.Character;

namespace ZetanStudio
{
    [CreateAssetMenu(menuName = "Zetan Studio/道具/枚举/道具属性枚举")]
    public class ItemAttributeEnum : ScriptableObjectEnum<ItemAttributeEnum, ItemAttributeType>
    {
        public override ItemAttributeType this[string ID] => Array.Find(_enum, i => i.ID == ID) ?? new ItemAttributeType();
    }

    [Serializable]
    public sealed class ItemAttributeType : ScriptableObjectEnumItem
    {
        [field: SerializeField]
        public string ID { get; private set; }

        [field: SerializeField]
        public string Alias { get; private set; }

        [field: SerializeField]
        public int Priority { get; private set; }

        [field: SerializeField]
        public string Format { get; private set; } = "{0}";

        [field: SerializeField]
        public RoleValueType ValueType { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is ItemAttributeType type && GetType() == obj.GetType() && type.Name == Name && type.ID == ID) return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ID);
        }
    }
}
