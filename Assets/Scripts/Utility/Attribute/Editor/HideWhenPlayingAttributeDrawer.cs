using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideWhenPlayingAttribute))]
public class HideWhenPlayingAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideWhenPlayingAttribute hideAttr = (HideWhenPlayingAttribute)attribute;
        bool hide = ShouldHide(hideAttr);
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
        HideWhenPlayingAttribute hideAttr = (HideWhenPlayingAttribute)attribute;
        bool hide = ShouldHide(hideAttr);

        if (hideAttr.readOnly || !hide)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool ShouldHide(HideWhenPlayingAttribute hideAttr)
    {
        return Application.isPlaying ? !hideAttr.reverse : hideAttr.reverse;
    }
}