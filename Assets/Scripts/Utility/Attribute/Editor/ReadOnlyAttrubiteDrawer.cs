using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ReadOnlyAttribute readOnly = (ReadOnlyAttribute)attribute;
        Component component = property.serializedObject.targetObject as Component;
        EditorGUI.BeginDisabledGroup(readOnly.onlyRuntime && Application.isPlaying && component && !string.IsNullOrEmpty(component.gameObject.scene.name) || !readOnly.onlyRuntime);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
    }
}