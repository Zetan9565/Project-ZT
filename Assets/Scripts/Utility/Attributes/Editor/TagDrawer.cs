using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagAttribute))]
public class TagDrawer : EnhancedAttributeDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String) Draw(position, property, label);
        else PropertyField(position, property, label);
    }

    public static void Draw(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        property.stringValue = EditorGUI.TagField(position, label, string.IsNullOrEmpty(property.stringValue) ? UnityEditorInternal.InternalEditorUtility.tags[0] : property.stringValue);
        EditorGUI.EndProperty();
    }
}