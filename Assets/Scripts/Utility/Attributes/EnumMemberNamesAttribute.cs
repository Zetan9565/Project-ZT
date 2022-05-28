using System.Collections.Generic;
using UnityEngine;

public class EnumMemberNamesAttribute : PropertyAttribute
{
    public readonly GUIContent[] memberNames;

    public EnumMemberNamesAttribute(string memberName, params string[] memberNames)
    {
        List<GUIContent> names = new List<GUIContent>() { new GUIContent(memberName) };
        foreach (var name in memberNames)
        {
            names.Add(new GUIContent(name));
        }
        this.memberNames = names.ToArray();
    }
}