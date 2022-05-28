using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class IDAttribute : PropertyAttribute
{
    public readonly string prefix;
    public readonly int length;
    public readonly string path;

    public IDAttribute(string prefix = null, int length = 4, string path = null)
    {
        this.prefix = prefix;
        this.length = length;
        this.path = path;
    }
}