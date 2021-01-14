using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using System.Linq;

[Serializable]
public class RoleAttribute
{
    [SerializeField]
    private RoleAttributeType type;
    public RoleAttributeType Type => type;

    [SerializeField]
    private int intValue;
    public int IntValue => IntValue;

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
                case RoleAttributeType.ATK:
                    return "攻";
                case RoleAttributeType.DEF:
                    return "防";
                case RoleAttributeType.ATKSpd:
                    return "攻速";
                case RoleAttributeType.MoveSpd:
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
                case RoleAttributeType.ATK:
                case RoleAttributeType.DEF:
                    return intValue;
                case RoleAttributeType.Hit:
                case RoleAttributeType.Crit:
                case RoleAttributeType.ATKSpd:
                case RoleAttributeType.MoveSpd:
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
                case RoleAttributeType.ATK:
                case RoleAttributeType.DEF:
                    intValue = (int)value;
                    break;
                case RoleAttributeType.Hit:
                case RoleAttributeType.Crit:
                case RoleAttributeType.ATKSpd:
                case RoleAttributeType.MoveSpd:
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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                return typeof(int);
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                return true;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                return false;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
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
    [InspectorName("攻击力")]
    ATK,
    [InspectorName("防御力")]
    DEF,
    [InspectorName("命中率")]
    Hit,
    [InspectorName("暴击率")]
    Crit,
    [InspectorName("攻击速度")]
    ATKSpd,
    [InspectorName("移动速度")]
    MoveSpd,
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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                //即便索引器是获取该类属性多个条目的总值，但在置数时会覆盖该类型第一个条目，所以无需担心加错
                this[attr.Type] = (int)this[attr.Type] + attr.IntValue;
                break;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                //即便索引器是获取该类属性多个条目的总值，但在置数时会覆盖该类型第一个条目，所以无需担心减错
                this[attr.Type] = (int)this[attr.Type] - attr.IntValue;
                break;
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
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