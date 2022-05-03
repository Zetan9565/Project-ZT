using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;

[Serializable]
public class RoleAttribute
{
    [SerializeField]
    private RoleAttributeType type;
    public RoleAttributeType Type => type;

    [SerializeField]
    private int intValue;
    public int IntValue => intValue;

    [SerializeField]
    private float floatValue;
    public float FloatValue => floatValue;

    [SerializeField]
    private bool boolValue;
    public bool BoolValue => boolValue;

    public string name
    {
        get
        {
            switch (type)
            {
                case RoleAttributeType.HP:
                    return "血";
                case RoleAttributeType.MP:
                    return "蓝";
                case RoleAttributeType.SP:
                    return "耐";
                case RoleAttributeType.CutATK:
                    return "切攻";
                case RoleAttributeType.PunATK:
                    return "穿攻";
                case RoleAttributeType.BluATK:
                    return "钝攻";
                case RoleAttributeType.DEF:
                    return "防";
                case RoleAttributeType.ATKSpeed:
                    return "攻速";
                case RoleAttributeType.MoveSpeed:
                    return "移速";
                default:
                    return "未知属性";
            }
        }
    }

    public ValueType Value
    {
        get
        {
            switch (type)
            {
                case RoleAttributeType.HP:
                case RoleAttributeType.MP:
                case RoleAttributeType.SP:
                case RoleAttributeType.CutATK:
                case RoleAttributeType.PunATK:
                case RoleAttributeType.BluATK:
                case RoleAttributeType.DEF:
                    return intValue;
                case RoleAttributeType.Hit:
                case RoleAttributeType.Crit:
                case RoleAttributeType.ATKSpeed:
                case RoleAttributeType.MoveSpeed:
                    return floatValue;
                case RoleAttributeType.TestBool:
                    return boolValue;
                default:
                    return intValue;
            }
        }
        set
        {
            switch (type)
            {
                case RoleAttributeType.HP:
                case RoleAttributeType.MP:
                case RoleAttributeType.SP:
                case RoleAttributeType.CutATK:
                case RoleAttributeType.PunATK:
                case RoleAttributeType.BluATK:
                case RoleAttributeType.DEF:
                    intValue = (int)value;
                    break;
                case RoleAttributeType.Hit:
                case RoleAttributeType.Crit:
                case RoleAttributeType.ATKSpeed:
                case RoleAttributeType.MoveSpeed:
                    floatValue = (float)value;
                    break;
                case RoleAttributeType.TestBool:
                    boolValue = (bool)value;
                    break;
                default:
                    break;
            }
        }
    }

    public RoleAttribute()
    {
        type = RoleAttributeType.HP;
    }

    public RoleAttribute(RoleAttributeType type)
    {
        this.type = type;
    }

    public RoleAttribute(RoleAttributeType type, ValueType value)
    {
        this.type = type;
        Value = value;
    }

    public static Type AttributeType2ValueType(RoleAttributeType type)
    {
        switch (type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                return typeof(int);
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                return typeof(float);
            default:
                return typeof(int);
        }
    }

    public static bool IsUsingIntValue(RoleAttributeType type)
    {
        switch (type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                return true;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
            case RoleAttributeType.TestBool:
            default:
                return false;
        }
    }
    public static bool IsUsingIntValue(RoleAttribute attr)
    {
        switch (attr.type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                return true;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
            case RoleAttributeType.TestBool:
            default:
                return false;
        }
    }

    public static bool IsUsingFloatValue(RoleAttributeType type)
    {
        switch (type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                return false;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                return true;
            case RoleAttributeType.TestBool:
            default:
                return false;
        }
    }
    public static bool IsUsingFloatValue(RoleAttribute attr)
    {
        switch (attr.type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                return false;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                return true;
            case RoleAttributeType.TestBool:
            default:
                return false;
        }
    }

    public static bool IsUsingBoolValue(RoleAttributeType type)
    {
        switch (type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                return false;
            case RoleAttributeType.TestBool:
                return true;
            default:
                return false;
        }
    }
    public static bool IsUsingBoolValue(RoleAttribute attr)
    {
        switch (attr.type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                return false;
            case RoleAttributeType.TestBool:
                return true;
            default:
                return false;
        }
    }

    public static implicit operator bool(RoleAttribute self)
    {
        return self != null;
    }
}

public enum RoleAttributeType
{
    [InspectorName("体力")]
    HP,
    [InspectorName("内力")]
    MP,
    [InspectorName("耐力")]
    SP,
    [InspectorName("切割力")]
    CutATK,
    [InspectorName("穿刺力")]
    PunATK,
    [InspectorName("钝击力")]
    BluATK,
    [InspectorName("防御力")]
    DEF,
    [InspectorName("命中率")]
    Hit,
    [InspectorName("闪避率")]
    Flee,
    [InspectorName("暴击率")]
    Crit,
    [InspectorName("攻击速度")]
    ATKSpeed,
    [InspectorName("移动速度")]
    MoveSpeed,
    [InspectorName("布尔测试")]
    TestBool,
}

[Serializable]
public class RoleAttributeGroup
{
    [SerializeField, NonReorderable]
    private List<RoleAttribute> attributes = new List<RoleAttribute>();
    public List<RoleAttribute> Attributes => attributes;

    public ValueType this[RoleAttributeType type]
    {
        get => GetValueByType(type);
        set => SetValueByType(type, value);
    }

    public ValueType GetValueByType(RoleAttributeType type)
    {
        if (RoleAttribute.IsUsingIntValue(type))
        {
            int tempInt = 0;
            foreach (RoleAttribute attr in attributes)
            {
                if (attr.Type == type)
                {
                    tempInt += attr.IntValue;
                }
            }
            return tempInt;
        }
        else if (RoleAttribute.IsUsingFloatValue(type))
        {
            float tempFloat = 0;
            foreach (RoleAttribute attr in attributes)
            {
                if (attr.Type == type)
                {
                    tempFloat += attr.FloatValue;
                }
            }
            return tempFloat;
        }
        else if (RoleAttribute.IsUsingBoolValue(type))
        {
            bool tempBool = true;
            foreach (RoleAttribute attr in attributes)
            {
                if (attr.Type == type)
                {
                    tempBool |= attr.BoolValue;
                }
            }
            return tempBool;
        }
        return 0;
    }

    public void SetValueByType(RoleAttributeType type, ValueType value)
    {
        if (RoleAttribute.IsUsingBoolValue(type))
        {
            RoleAttribute find = attributes.Last(x => x.Type == type);
            if (find) find.Value = value;
            else attributes.Add(new RoleAttribute(type, value));
        }
        else if (RoleAttribute.IsUsingIntValue(type))
        {
            RoleAttribute find = attributes.Last(x => x.Type == type);
            if (find) find.Value = (int)value - ((int)this[type] - (int)find.Value);
            else attributes.Add(new RoleAttribute(type, value));
        }
        else if (RoleAttribute.IsUsingFloatValue(type))
        {
            RoleAttribute find = attributes.Last(x => x.Type == type);
            if (find) find.Value = (float)value - ((float)this[type] - (float)find.Value);
            else attributes.Add(new RoleAttribute(type, value));
        }
    }

    public void PlusAttribute(RoleAttribute attr)
    {
        if (!attr) return;
        switch (attr.Type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                //即便索引器是获取该类属性多个条目的总值，但在置数时会覆盖该类型第一个条目，所以无需担心加错
                this[attr.Type] = (int)this[attr.Type] + attr.IntValue;
                break;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                this[attr.Type] = (float)this[attr.Type] + attr.FloatValue;
                break;
            case RoleAttributeType.TestBool:
                this[attr.Type] = !((bool)this[attr.Type] ^ attr.BoolValue);
                break;
            default:
                this[attr.Type] = attr.Value;
                break;
        }
    }

    public void PlusAttributes(IEnumerable<RoleAttribute> attrs)
    {
        foreach (var attr in attrs)
        {
            PlusAttribute(attr);
        }
    }

    public void SubAttribute(RoleAttribute attr)
    {
        if (!attr) return;
        switch (attr.Type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                //即便索引器是获取该类属性多个条目的总值，但在置数时会覆盖该类型第一个条目，所以无需担心减错
                this[attr.Type] = (int)this[attr.Type] - attr.IntValue;
                break;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                this[attr.Type] = (float)this[attr.Type] - attr.FloatValue;
                break;
            case RoleAttributeType.TestBool:
                this[attr.Type] = (bool)this[attr.Type] || !attr.BoolValue;
                break;
            default:
                break;
        }
    }

    public void SubAttributes(IEnumerable<RoleAttribute> attrs)
    {
        foreach (var attr in attrs)
        {
            SubAttribute(attr);
        }
    }

    public override string ToString()
    {
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        RoleAttribute temp;
        for (int i = 0; i < attributes.Count; i++)
        {
            temp = attributes[i];
            if (RoleAttribute.IsUsingIntValue(temp.Type) && temp.IntValue <= 0 || RoleAttribute.IsUsingFloatValue(temp.Type) && temp.FloatValue <= 0.0f)
                continue;
            str.Append(temp.name);
            str.Append(" ");
            str.Append(temp.Value);
            if (i != attributes.Count - 1)
                str.Append("\n");
        }
        return str.ToString();
    }

    #region 运算符重载
    /// <summary>
    /// 两个属性组相加，返回一个全新的属性组。其中，对于布尔类型的属性值，会进行同或运算
    /// </summary>
    /// <param name="left">左侧属性组</param>
    /// <param name="right">右侧属性组</param>
    /// <returns>全新的属性组</returns>
    public static RoleAttributeGroup operator +(RoleAttributeGroup left, RoleAttributeGroup right)
    {
        RoleAttributeGroup temp = new RoleAttributeGroup();
        temp.PlusAttributes(left.Attributes);
        temp.PlusAttributes(right.Attributes);
        return temp;
    }
    /// <summary>
    /// 属性组和属性列表相加，返回一个全新的属性组。其中，对于布尔类型的属性值，会进行同或运算
    /// </summary>
    /// <param name="left">左侧属性组</param>
    /// <param name="right">右侧属性列表</param>
    /// <returns>全新的属性组</returns>
    public static RoleAttributeGroup operator +(RoleAttributeGroup left, IEnumerable<RoleAttribute> right)
    {
        RoleAttributeGroup temp = new RoleAttributeGroup();
        temp.PlusAttributes(left.Attributes);
        temp.PlusAttributes(right);
        return temp;
    }

    /// <summary>
    /// 两个属性组相减，返回一个全新的属性组。其中，对于数字类型的属性值，可能产生负值；
    /// 对于布尔类型的属性值，会进行异或运算
    /// </summary>
    /// <param name="left">左侧属性组</param>
    /// <param name="right">右侧属性组</param>
    /// <returns>全新的属性组</returns>
    public static RoleAttributeGroup operator -(RoleAttributeGroup left, RoleAttributeGroup right)
    {
        RoleAttributeGroup temp = new RoleAttributeGroup();
        temp.PlusAttributes(left.Attributes);
        temp.SubAttributes(right.Attributes);
        return temp;
    }
    /// <summary>
    /// 属性组和属性列表相减，返回一个全新的属性组。其中，对于数字类型的属性值，可能产生负值；
    /// 对于布尔类型的属性值，会进行异或运算
    /// </summary>
    /// <param name="left">左侧属性组</param>
    /// <param name="right">右侧属性列表</param>
    /// <returns>全新的属性组</returns>
    public static RoleAttributeGroup operator -(RoleAttributeGroup left, IEnumerable<RoleAttribute> right)
    {
        RoleAttributeGroup temp = new RoleAttributeGroup();
        temp.PlusAttributes(left.Attributes);
        temp.SubAttributes(right);
        return temp;
    }

    public static implicit operator bool(RoleAttributeGroup self)
    {
        return self != null;
    }
    #endregion
}

namespace ZetanStudio.Character
{
    public sealed class RoleAttributeType
    {
        [field: SerializeField]
        public string Name { get; private set; }

        [field: SerializeField]
        public int Priority { get; private set; }

        [field: SerializeField]
        public RoleAttributeValueType ValueType { get; private set; }

        public class Comparer : IComparer<RoleAttributeType>
        {
            public static Comparer Default => new Comparer();

            public int Compare(RoleAttributeType x, RoleAttributeType y)
            {
                if (x.Priority < y.Priority) return -1;
                else if (x.Priority > y.Priority) return 1;
                else return 0;
            }
        }

        public static implicit operator string(RoleAttributeType self)
        {
            return self.Name;
        }
    }

    public enum RoleAttributeValueType
    {
        Integer,
        Float,
        Boolean
    }

    public class RoleAttributeGroup
    {
        private Dictionary<string, RoleAttribute> attributes = new Dictionary<string, RoleAttribute>();

        public RoleAttribute this[string name]
        {
            get => attributes[name];
        }

        public bool TryGetAttribute(string name, out RoleAttribute attribute)
        {
            return attributes.TryGetValue(name, out attribute);
        }

        public void PlusAttribute(RoleAttribute attr)
        {
            if (!attr) return;
            if (attributes.TryGetValue(attr.Type, out var find)) find.Plus(attr.Value);
            else attributes.Add(attr.Type, find.Clone());
        }

        public void PlusAttributes(IEnumerable<RoleAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                PlusAttribute(attr);
            }
        }

        public void SubAttribute(RoleAttribute attr)
        {
            if (!attr) return;
            if (attributes.TryGetValue(attr.Type, out var find))
            {
                find.Minus(attr.Value);
            }
        }

        public void SubAttributes(IEnumerable<RoleAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                SubAttribute(attr);
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            int i = 0;
            foreach (var kvp in attributes.Values.OrderBy(x => x.Type.Priority))
            {
                str.Append(kvp.Value);
                if (i != attributes.Count - 1)
                    str.Append("\n");
                i++;
            }
            return str.ToString();
        }

        #region 运算符重载
        /// <summary>
        /// 两个属性组相加，返回一个全新的属性组。其中，对于布尔类型的属性值，会进行同或运算
        /// </summary>
        /// <param name="left">左侧属性组</param>
        /// <param name="right">右侧属性组</param>
        /// <returns>全新的属性组</returns>
        public static RoleAttributeGroup operator +(RoleAttributeGroup left, RoleAttributeGroup right)
        {
            RoleAttributeGroup temp = new RoleAttributeGroup();
            temp.PlusAttributes(left.attributes.Values);
            temp.PlusAttributes(right.attributes.Values);
            return temp;
        }
        /// <summary>
        /// 属性组和属性列表相加，返回一个全新的属性组。其中，对于布尔类型的属性值，会进行同或运算
        /// </summary>
        /// <param name="left">左侧属性组</param>
        /// <param name="right">右侧属性列表</param>
        /// <returns>全新的属性组</returns>
        public static RoleAttributeGroup operator +(RoleAttributeGroup left, IEnumerable<RoleAttribute> right)
        {
            RoleAttributeGroup temp = new RoleAttributeGroup();
            temp.PlusAttributes(left.attributes.Values);
            temp.PlusAttributes(right);
            return temp;
        }

        /// <summary>
        /// 两个属性组相减，返回一个全新的属性组。其中，对于数字类型的属性值，可能产生负值；
        /// 对于布尔类型的属性值，会进行异或运算
        /// </summary>
        /// <param name="left">左侧属性组</param>
        /// <param name="right">右侧属性组</param>
        /// <returns>全新的属性组</returns>
        public static RoleAttributeGroup operator -(RoleAttributeGroup left, RoleAttributeGroup right)
        {
            RoleAttributeGroup temp = new RoleAttributeGroup();
            temp.PlusAttributes(left.attributes.Values);
            temp.SubAttributes(right.attributes.Values);
            return temp;
        }
        /// <summary>
        /// 属性组和属性列表相减，返回一个全新的属性组。其中，对于数字类型的属性值，可能产生负值；
        /// 对于布尔类型的属性值，会进行异或运算
        /// </summary>
        /// <param name="left">左侧属性组</param>
        /// <param name="right">右侧属性列表</param>
        /// <returns>全新的属性组</returns>
        public static RoleAttributeGroup operator -(RoleAttributeGroup left, IEnumerable<RoleAttribute> right)
        {
            RoleAttributeGroup temp = new RoleAttributeGroup();
            temp.PlusAttributes(left.attributes.Values);
            temp.SubAttributes(right);
            return temp;
        }
        #endregion

        public static implicit operator bool(RoleAttributeGroup self)
        {
            return self != null;
        }
    }

    public class RoleAttribute
    {
        public string Name => Type.Name;

        public event Action<RoleAttribute, ValueType> OnValueChanged;

        public ValueType Value
        {
            get
            {
                switch (Type.ValueType)
                {
                    case RoleAttributeValueType.Integer:
                        return IntValue;
                    case RoleAttributeValueType.Float:
                        return FloatValue;
                    case RoleAttributeValueType.Boolean:
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
                        case RoleAttributeValueType.Integer:
                            IntValue = (int)value;
                            break;
                        case RoleAttributeValueType.Float:
                            FloatValue = (float)value;
                            break;
                        case RoleAttributeValueType.Boolean:
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

        [field: SerializeField]
        public RoleAttributeType Type { get; private set; }

        public bool HasIntValue => Type.ValueType == RoleAttributeValueType.Integer;
        public bool HasFloatValue => Type.ValueType == RoleAttributeValueType.Float;
        public bool HasBoolValue => Type.ValueType == RoleAttributeValueType.Boolean;

        [SerializeField]
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

        [SerializeField]
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

        [SerializeField]
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
                    case RoleAttributeValueType.Integer:
                        IntValue += (int)value;
                        break;
                    case RoleAttributeValueType.Float:
                        FloatValue += (float)value;
                        break;
                    case RoleAttributeValueType.Boolean:
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
                    case RoleAttributeValueType.Integer:
                        IntValue -= (int)value;
                        break;
                    case RoleAttributeValueType.Float:
                        FloatValue -= (float)value;
                        break;
                    case RoleAttributeValueType.Boolean:
                        BoolValue = BoolValue || !(bool)value;
                        break;
                }
            }
            catch
            {
                Debug.LogError($"尝试给 [{Name}] 属性设置类型不匹配的值");
            }
        }

        public RoleAttribute Clone()
        {
            return new RoleAttribute() { Type = Type, intValue = intValue, floatValue = floatValue, boolValue = boolValue };
        }

        public override string ToString()
        {
            return $"{Name}\t{Value}";
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