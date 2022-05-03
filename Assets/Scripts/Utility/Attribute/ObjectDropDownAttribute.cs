using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ObjectDropDownAttribute : PropertyAttribute
{
    public readonly Type type;
    public readonly string fieldAsName;
    public readonly string resPath;
    public readonly string nameNull;

    public ObjectDropDownAttribute(Type type, string fieldAsName = "", string resPath = "", string nameNull = "未选择")
    {
        this.type = type;
        this.fieldAsName = fieldAsName;
        this.resPath = resPath;
        this.nameNull = nameNull;
    }
}