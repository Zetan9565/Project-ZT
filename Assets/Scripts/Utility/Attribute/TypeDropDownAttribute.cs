using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TypeDropDownAttribute : PropertyAttribute
{
    public readonly Type baseType;
    public readonly bool includeAbstract;
    public readonly bool groupByNamespace;

    public TypeDropDownAttribute(bool groupByNamespace = false, bool includeAbstract = false)
    {
        this.groupByNamespace = groupByNamespace;
        this.includeAbstract = includeAbstract;
    }
    public TypeDropDownAttribute(Type baseType, bool groupByNamespace = false, bool includeAbstract = false)
    {
        this.baseType = baseType;
        this.groupByNamespace = groupByNamespace;
        this.includeAbstract = includeAbstract;
    }
}