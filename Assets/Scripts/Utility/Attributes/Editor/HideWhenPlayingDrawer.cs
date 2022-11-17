using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideWhenPlayingAttribute))]
public class HideWhenPlayingDrawer : EnhancedPropertyDrawer
{
    private bool shouldHide;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideWhenPlayingAttribute hideAttr = (HideWhenPlayingAttribute)attribute;
        if (!shouldHide || hideAttr.readOnly)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(shouldHide && hideAttr.readOnly);
            PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideWhenPlayingAttribute hideAttr = (HideWhenPlayingAttribute)attribute;
        if (property.serializedObject.targetObject is Component component && AssetDatabase.Contains(component.gameObject)) shouldHide = false;
        else shouldHide = (Application.isPlaying ? !hideAttr.reverse : hideAttr.reverse);
        if (!shouldHide || hideAttr.readOnly) return base.GetPropertyHeight(property, label);
        return -EditorGUIUtility.standardVerticalSpacing;
    }
}