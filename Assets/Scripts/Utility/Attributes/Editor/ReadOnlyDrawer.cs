using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    private PropertyDrawer custom;
    private bool shouldCheckCustom = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ReadOnlyAttribute readOnly = (ReadOnlyAttribute)attribute;
        Component component = property.serializedObject.targetObject as Component;
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginDisabledGroup(readOnly.onlyRuntime && Application.isPlaying && component && !string.IsNullOrEmpty(component.gameObject.scene.name) || !readOnly.onlyRuntime);
        if (shouldCheckCustom)
        {
            custom = this.GetCustomDrawer();
            shouldCheckCustom = false;
        }
        if (custom != null) custom.OnGUI(position, property, label);
        else EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return custom?.GetPropertyHeight(property, label) ?? EditorGUI.GetPropertyHeight(property, label, true);
    }
}