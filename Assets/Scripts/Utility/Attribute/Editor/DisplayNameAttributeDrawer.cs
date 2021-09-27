using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisplayNameAttribute))]
public class DisplayNameAttributeDrawer : PropertyDrawer
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
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = attribute.name }, true);
            EditorGUI.EndProperty();
        }
        else
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(false);
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = attribute.name }, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
}