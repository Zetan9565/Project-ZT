using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class MultiPropertyAttribute : PropertyAttribute
{
    public readonly string[] labels;

    public MultiPropertyAttribute(string[] labels)
    {
        this.labels = labels;
    }
}