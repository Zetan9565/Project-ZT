using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class HideIfAttribute : PropertyAttribute
{
    public readonly string path;
    public readonly object value;
    public readonly bool readOnly;

    public HideIfAttribute(string path, object value, bool readOnly = false)
    {
        this.path = path;
        this.value = value;
        this.readOnly = readOnly;
    }
}