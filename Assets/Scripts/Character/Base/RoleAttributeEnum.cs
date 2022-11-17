using System;
using UnityEngine;

namespace ZetanStudio.CharacterSystem
{
    [CreateAssetMenu(menuName = "Zetan Studio/角色/枚举/基础属性枚举")]
    public class RoleAttributeEnum : ScriptableObjectEnum<RoleAttributeEnum, RoleAttributeType>
    {
        public RoleAttributeEnum()
        {
            _enum = new RoleAttributeType[]
            {
                new RoleAttributeType("B_HP", 0, "基础体力", "角色基本体力值", RoleValueType.Integer),
                new RoleAttributeType("B_MP", 1, "基础能量", "角色基本能量值", RoleValueType.Integer),
                new RoleAttributeType("B_STM", 2, "基础耐力", "角色基本耐力值", RoleValueType.Integer),
                new RoleAttributeType("B_ATK", 3, "基础攻击", "角色基本攻击力", RoleValueType.Integer),
                new RoleAttributeType("B_DEF", 4, "基础防御", "角色基本防御力", RoleValueType.Integer),
                new RoleAttributeType("B_CRIT", 5, "基础暴击率", "角色基本暴击率", RoleValueType.Integer),
                new RoleAttributeType("B_CDMG", 6, "基础暴击伤害", "角色基本暴击伤害", RoleValueType.Integer),
                new RoleAttributeType("B_ACC", 7, "基础命中", "角色基本命中力", RoleValueType.Integer),
                new RoleAttributeType("B_EVA", 8, "基础闪避", "角色基本闪避力", RoleValueType.Integer),
                new RoleAttributeType("B_ARES", 9, "基础异常状态抵抗", "角色基本异常状态抵抗力", RoleValueType.Integer),
                new RoleAttributeType("B_HRES", 10, "基础耐饿", "角色基本饥饿忍耐力", RoleValueType.Integer),
                new RoleAttributeType("B_TRES", 11, "基础耐渴", "角色基本口渴忍耐力", RoleValueType.Integer),
                new RoleAttributeType("B_CRES", 12, "基础耐寒", "角色基本寒冷忍耐力", RoleValueType.Integer),
                new RoleAttributeType("B_HtRES", 13, "基础耐热", "角色基本炎热忍耐力", RoleValueType.Integer),
            };
        }
    }

    /// <summary>
    /// 角色生来就具有的，只能通过升级或突破来提升，无法通过其它如装备等途径改变的属性
    /// </summary>
    [System.Serializable]
    public sealed class RoleAttributeType : ScriptableObjectEnumItem
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

        [field: SerializeField]
        public string Description { get; private set; }

        public RoleAttributeType() : this("B_HP", 0, "基础体力", "", RoleValueType.Integer)
        {

        }

        public RoleAttributeType(string iD, int priority, string name, string description, RoleValueType valueType)
        {
            ID = iD;
            Name = name;
            Priority = priority;
            Description = description;
            ValueType = valueType;
        }

        public override bool Equals(object obj)
        {
            if (obj is RoleAttributeType type && GetType() == obj.GetType() && type.Name == Name && type.ID == ID) return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ID);
        }
    }

    public enum RoleValueType
    {
        Integer,
        Float,
        Boolean
    }
}