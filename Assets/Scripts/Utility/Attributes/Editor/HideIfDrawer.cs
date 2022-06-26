using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfDrawer : EnhancedAttributeDrawer
{
    private bool shouldHide;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
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
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        this.TryGetOwnerValue(property, out var owner);
        shouldHide = ICheckValueAttribute.Check(owner, hideAttr);
        if (!shouldHide || hideAttr.readOnly) return base.GetPropertyHeight(property, label);
        return -EditorGUIUtility.standardVerticalSpacing;
    }
}