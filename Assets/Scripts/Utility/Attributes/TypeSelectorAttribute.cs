using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TypeSelectorAttribute : PropertyAttribute
{
    public readonly Type baseType;
    public readonly bool includeAbstract;
    public readonly bool groupByNamespace;

    public TypeSelectorAttribute(bool groupByNamespace = false, bool includeAbstract = false)
    {
        this.groupByNamespace = groupByNamespace;
        this.includeAbstract = includeAbstract;
    }
    public TypeSelectorAttribute(Type baseType, bool groupByNamespace = false, bool includeAbstract = false)
    {
        this.baseType = baseType;
        this.groupByNamespace = groupByNamespace;
        this.includeAbstract = includeAbstract;
    }
}