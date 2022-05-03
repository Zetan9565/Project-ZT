using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisplayNameAttribute))]
public class DisplayNameDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DisplayNameAttribute attribute = this.attribute as DisplayNameAttribute;
        if (!attribute.readOnly)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            label.text = attribute.name;
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }
        else
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(false);
            label.text = attribute.name;
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
}