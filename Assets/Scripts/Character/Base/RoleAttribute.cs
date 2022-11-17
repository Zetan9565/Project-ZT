using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZetanStudio.CharacterSystem
{
    public interface IRoleValue
    {
        public string ID { get; }

        public string Name { get; }

        public ValueType Value { get; }

        public bool HasIntValue { get; }
        public bool HasFloatValue { get; }
        public bool HasBoolValue { get; }

        public int IntValue { get; }
        public float FloatValue { get; }
        public bool BoolValue { get; }

        public RoleValueType ValueType { get; }

        public GenericData GetSaveData()
        {
            var data = new GenericData();
            data["ID"] = ID;
            data["value"] = Value;
            return data;
        }
    }

    public interface IRoleValue<T> : IRoleValue where T : ScriptableObjectEnumItem
    {
        public T Type { get; }
    }

    [Serializable]
    public class RoleAttribute : IRoleValue<RoleAttributeType>
    {
        public string Name => string.IsNullOrEmpty(Type.Alias) ? Type.Name : Type.Alias;
        public string ID => Type.ID;

        public event Action<RoleAttribute, ValueType> OnValueChanged;

        public RoleAttribute(RoleAttributeType type)
        {
            Type = type;
#if UNITY_EDITOR
            this.type = RoleAttributeEnum.IndexOf(type);
#endif
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

#if UNITY_EDITOR
        [SerializeField, ReadOnly, Enum(typeof(RoleAttributeType))]
#pragma warning disable IDE0052 // 删除未读的私有成员
        private int type;
#pragma warning restore IDE0052 // 删除未读的私有成员
#endif
        public RoleAttributeType Type { get; private set; }

        public RoleValueType ValueType => Type.ValueType;

        public bool HasIntValue => ValueType == RoleValueType.Integer;
        public bool HasFloatValue => ValueType == RoleValueType.Float;
        public bool HasBoolValue => ValueType == RoleValueType.Boolean;

        [SerializeField, ShowIf("ValueType", RoleValueType.Integer), ReadOnly]
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

        [SerializeField, ShowIf("ValueType", RoleValueType.Float), ReadOnly]
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

        [SerializeField, ShowIf("ValueType", RoleValueType.Boolean), ReadOnly]
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

        public void Plus(ValueType value)
        {
            try
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        IntValue += (int)value;
                        break;
                    case RoleValueType.Float:
                        FloatValue += (float)value;
                        break;
                    case RoleValueType.Boolean:
                        BoolValue = !(BoolValue ^ (bool)value);
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }
        public void Minus(ValueType value)
        {
            try
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        IntValue -= (int)value;
                        break;
                    case RoleValueType.Float:
                        FloatValue -= (float)value;
                        break;
                    case RoleValueType.Boolean:
                        BoolValue = BoolValue || !(bool)value;
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }

        public override string ToString()
        {
            return $"{Name}\t{ValueString}";
        }

        public static implicit operator bool(RoleAttribute self)
        {
            return self != null;
        }

        public class Comparer : IComparer<RoleAttribute>
        {
            public static Comparer Default => new Comparer();

            public int Compare(RoleAttribute x, RoleAttribute y)
            {
                if (x.Type.Priority < y.Type.Priority) return -1;
                else if (x.Type.Priority > y.Type.Priority) return 1;
                else return 0;
            }
        }
    }
}

namespace ZetanStudio.CharacterSystem
{
    [Serializable]
    public class RoleProperty : IRoleValue<RolePropertyType>
    {
        public string Name => string.IsNullOrEmpty(Type.Alias) ? Type.Name : Type.Alias;

        public string ID => Type.ID;

        public event Action<RoleProperty, ValueType> OnValueChanged;

        public RoleProperty(RolePropertyType type)
        {
            Type = type;
#if UNITY_EDITOR
            this.type = RolePropertyEnum.IndexOf(type);
#endif
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

#if UNITY_EDITOR
        [SerializeField, ReadOnly, Enum(typeof(RolePropertyType))]
#pragma warning disable IDE0052 // 删除未读的私有成员
        private int type;
#pragma warning restore IDE0052 // 删除未读的私有成员
#endif
        public RolePropertyType Type { get; }

        public RoleValueType ValueType => Type.ValueType;

        public bool HasIntValue => ValueType == RoleValueType.Integer;
        public bool HasFloatValue => ValueType == RoleValueType.Float;
        public bool HasBoolValue => ValueType == RoleValueType.Boolean;

        [SerializeField, ShowIf("ValueType", RoleValueType.Integer), ReadOnly]
        private int intValue;
        public int IntValue
        {
            get
            {
                if (!HasIntValue) Debug.LogWarning($"[{Name}] 属性使用的不是整型数值");
                return intValue;
            }
            private set
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

        [SerializeField, ShowIf("ValueType", RoleValueType.Float), ReadOnly]
        private float floatValue;
        public float FloatValue
        {
            get
            {
                if (!HasFloatValue) Debug.LogWarning($"[{Name}] 属性使用的不是浮点型数值");
                return floatValue;
            }
            private set
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

        [SerializeField, ShowIf("ValueType", RoleValueType.Boolean), ReadOnly]
        private bool boolValue;
        public bool BoolValue
        {
            get
            {
                if (!HasBoolValue) Debug.LogWarning($"[{Name}] 属性使用的不是布尔型数值");
                return boolValue;
            }
            private set
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


        private readonly List<IEnumerable<IRoleValue>> operandSets = new List<IEnumerable<IRoleValue>>();

        public void AddOperandSet(IEnumerable<IRoleValue> operands)
        {
            if (operands == null || operandSets.Contains(operands)) return;
            operandSets.Add(operands);
            CalculateValue();
        }
        public void RemoveOperandSet(IEnumerable<IRoleValue> operands)
        {
            if (operands == null) return;
            operandSets.Remove(operands);
            CalculateValue();
        }

        public void CalculateValue()
        {
            string formula = Regex.Replace(Type.Expression, @"[ \t\n]", "");
            if (HasBoolValue)
            {
                string[] operands = formula.Split(Expression.ValidBoolOperators, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, bool> operandMap = new Dictionary<string, bool>();
                foreach (var set in operandSets)
                {
                    foreach (var operand in set)
                    {
                        if (operand.HasBoolValue) operandMap[operand.ID] = operand.BoolValue;
                    }
                }
                List<bool> args = new List<bool>();
                foreach (var op in operands)
                {
                    var match = Regex.Match(op, @"^\[(\w+\?\w+)\]$");
                    if (match.Success)
                    {
                        formula = formula.Replace(op, $"{{{args.Count}}}");
                        string[] values = match.Groups[1].Value.Split('?');
                        if (operandMap.TryGetValue(values[0], out var value)) args.Add(value);
                        else if (bool.TryParse(values[1], out value)) args.Add(value);
                        else throw new InvalidCastException($"找不到属性[{values[0]}]且无预设值");
                    }
                }
                BoolValue = Expression.ToBool(formula, args.ToArray());
            }
            else
            {
                string[] operands = formula.Split(Expression.ValidDigitOperators, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, float> operandMap = new Dictionary<string, float>();
                foreach (var set in operandSets)
                {
                    foreach (var operand in set)
                    {
                        float fvalue = 0;
                        if (operand.HasIntValue) fvalue = operand.IntValue;
                        else if (operand.HasFloatValue) fvalue = operand.FloatValue;
                        if (!operand.HasBoolValue)
                            if (operandMap.ContainsKey(operand.ID)) operandMap[operand.ID] += fvalue;
                            else operandMap[operand.ID] = fvalue;
                    }
                }
                List<float> args = new List<float>();
                foreach (var op in operands)
                {
                    var match = Regex.Match(op, @"^\[(\w+\?\w+)\]$");
                    if (match.Success)
                    {
                        formula = formula.Replace(op, $"{{{args.Count}}}");
                        string[] values = match.Groups[1].Value.Split('?');
                        if (operandMap.TryGetValue(values[0], out var value)) args.Add(value);
                        else if (float.TryParse(values[1], out value)) args.Add(value);
                        else throw new InvalidCastException($"找不到属性[{values[0]}]且无预设值");
                    }
                }
                if (HasIntValue) IntValue = Expression.ToInt(formula, args.ToArray());
                else if (HasFloatValue) FloatValue = Expression.ToFloat(formula, args.ToArray());
            }
        }

        public void Plus(ValueType value)
        {
            try
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        IntValue += (int)value;
                        break;
                    case RoleValueType.Float:
                        FloatValue += (float)value;
                        break;
                    case RoleValueType.Boolean:
                        BoolValue = !(BoolValue ^ (bool)value);
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }
        public void Minus(ValueType value)
        {
            try
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        IntValue -= (int)value;
                        break;
                    case RoleValueType.Float:
                        FloatValue -= (float)value;
                        break;
                    case RoleValueType.Boolean:
                        BoolValue = BoolValue || !(bool)value;
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }

        public void Plus(RoleProperty property)
        {
            if (!property) return;
            try
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        IntValue += property.intValue;
                        break;
                    case RoleValueType.Float:
                        FloatValue += property.floatValue;
                        break;
                    case RoleValueType.Boolean:
                        BoolValue = !(BoolValue ^ property.boolValue);
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }
        public void Minus(RoleProperty property)
        {
            if (!property) return;
            try
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        IntValue -= property.intValue;
                        break;
                    case RoleValueType.Float:
                        FloatValue -= property.floatValue;
                        break;
                    case RoleValueType.Boolean:
                        BoolValue = BoolValue || !property.boolValue;
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }

        public override string ToString()
        {
            return $"{Name}\t{ValueString}";
        }

        public static implicit operator bool(RoleProperty self)
        {
            return self != null;
        }

        public class Comparer : IComparer<RoleProperty>
        {
            public static Comparer Default => new Comparer();

            public int Compare(RoleProperty x, RoleProperty y)
            {
                if (x.Type.Priority < y.Type.Priority) return -1;
                else if (x.Type.Priority > y.Type.Priority) return 1;
                else return 0;
            }
        }
    }
}