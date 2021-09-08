using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class DisplayNameAttribute : PropertyAttribute
{
    public string Name;

    public bool ReadOnly;

    public DisplayNameAttribute(string name, bool readOnly = false)
    {
        Name = name;
        ReadOnly = readOnly;
    }
}