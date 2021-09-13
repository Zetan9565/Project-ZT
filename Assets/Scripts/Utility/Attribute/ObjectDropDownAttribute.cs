using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ObjectDropDownAttribute : PropertyAttribute
{
    public Type type;
    public string fieldAsName;
    public string path;
    public string nameNull;

    public ObjectDropDownAttribute(Type type, string fieldAsName = "", string path = "", string nameNull = "未选择")
    {
        this.type = type;
        this.fieldAsName = fieldAsName;
        this.path = path;
        this.nameNull = nameNull;
    }
}