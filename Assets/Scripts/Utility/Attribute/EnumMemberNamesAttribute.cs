using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class EnumMemberNamesAttribute : PropertyAttribute
{
    public readonly GUIContent[] memberNames;

    public EnumMemberNamesAttribute(params string[] memberNames)
    {
        List<GUIContent> names = new List<GUIContent>();
        foreach (var name in memberNames)
        {
            names.Add(new GUIContent(name));
        }
        this.memberNames = names.ToArray();
    }
}