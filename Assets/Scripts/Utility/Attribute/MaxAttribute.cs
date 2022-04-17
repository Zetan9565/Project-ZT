using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class MaxAttribute : PropertyAttribute
{
    public readonly int intValue;
    public readonly long longValue;
    public readonly float floatValue;
    public readonly double doubleValue;

    public readonly ValueType valueType;

    public MaxAttribute(int value)
    {
        intValue = value;
        valueType = ValueType.Int;
    }
    public MaxAttribute(long value)
    {
        longValue = value;
        valueType = ValueType.Long;
    }
    public MaxAttribute(float value)
    {
        floatValue = value;
        valueType = ValueType.Float;
    }
    public MaxAttribute(double value)
    {
        doubleValue = value;
        valueType = ValueType.Double;
    }
    public enum ValueType
    {
        Int,
        Long,
        Float,
        Double,
    }
}