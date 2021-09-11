using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class EnumMemberNamesAttribute : PropertyAttribute
{
    public string[] memberNames;

    public EnumMemberNamesAttribute(params string[] memberNames)
    {
        this.memberNames = memberNames;
    }
}