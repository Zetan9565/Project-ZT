using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class DisplayNameAttribute: PropertyAttribute
{
    public string Name;

    public bool ReadOnly;

    public DisplayNameAttribute(string name, bool readOnly = false)
    {
        Name = name;
        ReadOnly = readOnly;
    }
}

[CustomPropertyDrawer(typeof(DisplayNameAttribute))]
public class DisplayNameAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        DisplayNameAttribute attribute = this.attribute as DisplayNameAttribute;
        return EditorGUI.GetPropertyHeight(property, new GUIContent(label) { text = attribute.Name }, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DisplayNameAttribute attribute = this.attribute as DisplayNameAttribute;
        if (!attribute.ReadOnly)
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = attribute.Name }, true);
        else
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = attribute.Name }, true);
            GUI.enabled = true;
        }
    }
}
#endif