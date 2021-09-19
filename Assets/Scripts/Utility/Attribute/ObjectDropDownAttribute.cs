using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ObjectDropDownAttribute : PropertyAttribute
{
    public Type type;
    public string fieldAsName;
    public string resPath;
    public string nameNull;

    public ObjectDropDownAttribute(Type type, string fieldAsName = "", string resPath = "", string nameNull = "未选择")
    {
        this.type = type;
        this.fieldAsName = fieldAsName;
        this.resPath = resPath;
        this.nameNull = nameNull;
    }
}