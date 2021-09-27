using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class DisplayNameAttribute : PropertyAttribute
{
    public readonly string name;

    public readonly bool readOnly;

    public DisplayNameAttribute(string name, bool readOnly = false)
    {
        this.name = name;
        this.readOnly = readOnly;
    }
}