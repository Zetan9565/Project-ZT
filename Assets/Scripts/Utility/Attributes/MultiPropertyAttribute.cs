using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class MultiPropertyAttribute : PropertyAttribute
{
    public readonly string[] labels;

    public MultiPropertyAttribute(string label, params string[] labels)
    {
        List<string> temp = new List<string>() { label };
        temp.AddRange(labels);
        this.labels = temp.ToArray();
    }
}