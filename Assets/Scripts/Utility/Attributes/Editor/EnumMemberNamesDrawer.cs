using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumMemberNamesAttribute))]
public class EnumMemberNamesDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumMemberNamesAttribute namesAttribute = (EnumMemberNamesAttribute)attribute;
        label = EditorGUI.BeginProperty(position, label, property);
        property.enumValueIndex = EditorGUI.Popup(position, label, property.enumValueIndex, namesAttribute.memberNames);
        EditorGUI.EndProperty();
    }
}