using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class EnumMemberNamesAttribute : PropertyAttribute
{
    public string[] memberNames;

    public EnumMemberNamesAttribute(params string[] memberNames)
    {
        this.memberNames = memberNames;
    }
}

[CustomPropertyDrawer(typeof(EnumMemberNamesAttribute))]
public class EnumMemberNamesAttributeDrawer: PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumMemberNamesAttribute namesAttribute = (EnumMemberNamesAttribute)attribute;
        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, namesAttribute.memberNames);
    }
}
#endif