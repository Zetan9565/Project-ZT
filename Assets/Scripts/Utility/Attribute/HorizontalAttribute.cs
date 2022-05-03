using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HorizontalAttribute : PropertyAttribute
{
    public readonly int position;
    public readonly int count;

    public HorizontalAttribute(int position, int count)
    {
        this.position = position;
        this.count = count;
    }
}