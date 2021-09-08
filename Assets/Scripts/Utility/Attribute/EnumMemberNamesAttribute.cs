using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class EnumMemberNamesAttribute : PropertyAttribute
{
    public string[] memberNames;

    public EnumMemberNamesAttribute(params string[] memberNames)
    {
        this.memberNames = memberNames;
    }
}