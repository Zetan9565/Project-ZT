using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumMemberNamesAttribute))]
public class EnumMemberNamesAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumMemberNamesAttribute namesAttribute = (EnumMemberNamesAttribute)attribute;
        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, namesAttribute.memberNames);
    }
}