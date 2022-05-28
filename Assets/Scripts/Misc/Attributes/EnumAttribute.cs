using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class EnumAttribute : PropertyAttribute
{
    public readonly Type type;

    public EnumAttribute(Type type)
    {
        this.type = type;
    }
}