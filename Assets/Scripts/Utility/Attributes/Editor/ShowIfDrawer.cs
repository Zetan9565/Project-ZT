using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : EnhancedAttributeDrawer
{
    private bool shouldShow;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showAttr = attribute as ShowIfAttribute;
        if (shouldShow || showAttr.readOnly)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(!shouldShow && showAttr.readOnly);
            PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showAttr = attribute as ShowIfAttribute;
        this.TryGetOnwerValue(property, out var owner);
        shouldShow = ICheckValueAttribute.Check(owner, showAttr);
        if (shouldShow || showAttr.readOnly) return base.GetPropertyHeight(property, label);
        return -EditorGUIUtility.standardVerticalSpacing;
    }
}