using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideWhenPlayingAttribute : PropertyAttribute
{
    public readonly bool readOnly;
    public readonly bool reverse;

    public HideWhenPlayingAttribute(bool readOnly = false, bool reverse=false)
    {
        this.readOnly = readOnly;
        this.reverse = reverse;
    }
}