using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        bool hide = ShouldHide(hideAttr, property);
        if (!hide || hide && hideAttr.readOnly)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(hide && hideAttr.readOnly);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        bool hide = ShouldHide(hideAttr, property);

        if (hideAttr.readOnly || !hide)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool ShouldHide(HideIfAttribute hideAttr, SerializedProperty property)
    {
        if (ZetanEditorUtility.TryGetValue(property, out var target))
        {
            if (ZetanEditorUtility.TryGetMemberValue(hideAttr.path, target, out var value, out _))
            {
                if (Equals(value, hideAttr.value)) return true;
                else return false;
            }
            else
            {
                Debug.LogWarning("找不到路径：" + hideAttr.path);
                return false;
            }
        }
        else
        {
            Debug.LogWarning("无法访问：" + property.propertyPath);
            return false;
        }
    }
}