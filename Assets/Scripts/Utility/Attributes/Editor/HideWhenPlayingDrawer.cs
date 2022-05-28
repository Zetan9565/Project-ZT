using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;

[CustomPropertyDrawer(typeof(HideWhenPlayingAttribute))]
public class HideWhenPlayingDrawer : PropertyDrawer
{
    private PropertyDrawer custom;
    private bool shouldCheckCustom = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideWhenPlayingAttribute hideAttr = (HideWhenPlayingAttribute)attribute;
        bool hide = ShouldHide(hideAttr);
        if (!hide || hide && hideAttr.readOnly)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(hide && hideAttr.readOnly);
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
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideWhenPlayingAttribute hideAttr = (HideWhenPlayingAttribute)attribute;
        bool hide = ShouldHide(hideAttr);

        if (hideAttr.readOnly || !hide)
        {
            return custom?.GetPropertyHeight(property, label) ?? EditorGUI.GetPropertyHeight(property, label, true);
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