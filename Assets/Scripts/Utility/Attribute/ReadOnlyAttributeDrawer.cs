using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class ReadOnlyAttribute : PropertyAttribute
{
    public readonly bool onlyRuntime;

    public ReadOnlyAttribute(bool onlyRuntime = false)
    {
        this.onlyRuntime = onlyRuntime;
    }
}

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
        if (readOnly.onlyRuntime && Application.isPlaying && component && !string.IsNullOrEmpty(component.gameObject.scene.name) || !readOnly.onlyRuntime) 
            GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        if (readOnly.onlyRuntime && Application.isPlaying && component && !string.IsNullOrEmpty(component.gameObject.scene.name) || !readOnly.onlyRuntime) 
            GUI.enabled = true;
    }
}
#endif