using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisplayNameAttribute))]
public class DisplayNameAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DisplayNameAttribute attribute = this.attribute as DisplayNameAttribute;
        if (!attribute.ReadOnly)
        {
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = attribute.Name }, true);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(false);
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = attribute.Name }, true);
            EditorGUI.EndDisabledGroup();
        }
    }
}