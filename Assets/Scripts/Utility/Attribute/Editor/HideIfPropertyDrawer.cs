using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        bool hide = GetConditionalHideAttributeResult(hideAttr, property);
        if (!hide || hide && hideAttr.readOnly)
        {
            EditorGUI.BeginDisabledGroup(hide && hideAttr.readOnly);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        bool hide = GetConditionalHideAttributeResult(hideAttr, property);

        if (hideAttr.readOnly || !hide)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionalHideAttributeResult(HideIfAttribute hideAttr, SerializedProperty property)
    {
        if (ZetanEditorUtility.GetFieldValue(hideAttr.path, property.serializedObject.targetObject, out var value, out _))
        {
            if (Equals(value, hideAttr.value)) return true;
            else return false;
        }
        else
        {
            Debug.LogWarning("找不到字段路径：" + hideAttr.path);
            return false;
        }
    }
}