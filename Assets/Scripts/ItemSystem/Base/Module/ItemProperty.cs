using System;
using UnityEngine;
using ZetanStudio.CharacterSystem;

namespace ZetanStudio.ItemSystem
{
    [Serializable]
    public class ItemProperty : IRoleValue<ItemAttributeType>
    {
        public string Name => string.IsNullOrEmpty(Type.Alias) ? Type.Name : Type.Alias;

        public string ID => Type.ID;

        public event Action<ItemProperty, ValueType> OnValueChanged;

        public ItemProperty(ItemAttributeType type)
        {
            Type = type;
        }

        public ValueType Value
        {
            get
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        return IntValue;
                    case RoleValueType.Float:
                        return FloatValue;
                    case RoleValueType.Boolean:
                        return BoolValue;
                    default:
                        throw new InvalidOperationException("意料之外的错误");
                }
            }
            set
            {
                try
                {
                    switch (Type.ValueType)
                    {
                        case RoleValueType.Integer:
                            IntValue = (int)value;
                            break;
                        case RoleValueType.Float:
                            FloatValue = (float)value;
                            break;
                        case RoleValueType.Boolean:
                            BoolValue = (bool)value;
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
                }
            }
        }

        public string ValueString
        {
            get
            {
                return ValueType switch
                {
                    RoleValueType.Integer => string.Format(Type.Format, intValue).Replace(".0%", "%"),
                    RoleValueType.Float => string.Format(Type.Format, floatValue).Replace(".0%", "%"),
                    RoleValueType.Boolean => string.Format(Type.Format, boolValue),
                    _ => throw new InvalidOperationException("意料之外的错误"),
                };
            }
        }

        public ItemAttributeType Type { get; }

        public RoleValueType ValueType => Type.ValueType;

        public bool HasIntValue => ValueType == RoleValueType.Integer;
        public bool HasFloatValue => ValueType == RoleValueType.Float;
        public bool HasBoolValue => ValueType == RoleValueType.Boolean;

        [SerializeField, ShowIf("ValueType", RoleValueType.Integer)]
        private int intValue;
        public int IntValue
        {
            get
            {
                if (!HasIntValue) Debug.LogWarning($"[{Name}] 属性使用的不是整型数值");
                return intValue;
            }
            set
            {
                if (HasIntValue)
                {
                    if (intValue != value)
                    {
                        var oldValue = intValue;
                        intValue = value;
                        OnValueChanged?.Invoke(this, oldValue);
                    }
                }
                else Debug.LogWarning($"[{Name}] 属性使用的不是整型数值");
            }
        }

        [SerializeField, ShowIf("ValueType", RoleValueType.Float)]
        private float floatValue;
        public float FloatValue
        {
            get
            {
                if (!HasFloatValue) Debug.LogWarning($"[{Name}] 属性使用的不是浮点型数值");
                return floatValue;
            }
            set
            {
                if (HasFloatValue)
                {
                    if (floatValue != value)
                    {
                        var oldValue = floatValue;
                        floatValue = value;
                        OnValueChanged?.Invoke(this, oldValue);
                    }
                }
                else Debug.LogWarning($"[{Name}] 属性使用的不是浮点型数值");
            }
        }

        [SerializeField, ShowIf("ValueType", RoleValueType.Boolean)]
        private bool boolValue;
        public bool BoolValue
        {
            get
            {
                if (!HasBoolValue) Debug.LogWarning($"[{Name}] 属性使用的不是布尔型数值");
                return boolValue;
            }
            set
            {
                if (HasBoolValue)
                {
                    if (boolValue != value)
                    {
                        var oldValue = boolValue;
                        boolValue = value;
                        OnValueChanged?.Invoke(this, oldValue);
                    }
                }
                else Debug.LogWarning($"[{Name}] 属性使用的不是布尔型数值");
            }
        }

        public override string ToString()
        {
            return $"{Name}\t{ValueString}";
        }

        public void Plus(ValueType value)
        {
            switch (ValueType)
            {
                case RoleValueType.Integer:
                    if (value is int i) intValue += i;
                    else Debug.LogWarning($"[{Name}] 使用的是整型数值，而{nameof(value)}是{value?.GetType()}");
                    break;
                case RoleValueType.Float:
                    if (value is float f) floatValue += f;
                    else Debug.LogWarning($"[{Name}] 使用的是浮点型数值，而{nameof(value)}是{value?.GetType()}");
                    break;
                case RoleValueType.Boolean:
                    if (value is bool b) boolValue = b;
                    else Debug.LogWarning($"[{Name}] 使用的是布尔型数值，而{nameof(value)}是{value?.GetType()}");
                    break;
            }
        }

        public static implicit operator bool(ItemProperty obj)
        {
            return obj != null;
        }
    }
}