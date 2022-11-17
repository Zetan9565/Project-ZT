using System;
using UnityEngine;

namespace ZetanStudio.CharacterSystem
{
    [CreateAssetMenu(menuName = "Zetan Studio/角色/枚举/属性枚举")]
    public sealed class RolePropertyEnum : ScriptableObjectEnum<RolePropertyEnum, RolePropertyType>
    {

    }

    /// <summary>
    /// 角色的实际属性，是在基础属性<see cref="RoleAttributeType"/>的基础上通过各种公式计算得到的角色实际属性，可通过装备、BUFF等各种途径改变
    /// </summary>
    [Serializable]
    public sealed class RolePropertyType : ScriptableObjectEnumItem
    {
        [field: SerializeField]
        public string ID { get; private set; }

        [field: SerializeField]
        public string Alias { get; private set; }

        [field: SerializeField]
        public int Priority { get; private set; }

        [field: SerializeField]
        public RoleValueType ValueType { get; private set; }

        [field: SerializeField]
        public string Description { get; private set; }

        [field: SerializeField]
        public string Format { get; private set; } = "{0}";

        [field: SerializeField, TextArea]
        public string Expression { get; private set; }

        [field: SerializeField]
        public bool SkillCost { get; private set; }

        [field: SerializeField]
        public bool ShowInPanel { get; private set; } = true;

        [field: SerializeField]
        public bool OnlyForPlayer { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is RolePropertyType type && GetType() == obj.GetType() && type.Name == Name && type.ID == ID) return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ID);
        }
    }
}