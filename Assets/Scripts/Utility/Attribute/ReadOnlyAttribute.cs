using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyAttribute : PropertyAttribute
{
    public readonly bool onlyRuntime;

    public ReadOnlyAttribute(bool onlyRuntime = false)
    {
        this.onlyRuntime = onlyRuntime;
    }
}