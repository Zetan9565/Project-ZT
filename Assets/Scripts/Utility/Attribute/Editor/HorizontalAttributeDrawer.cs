using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HorizontalAttribute))]
public class HorizontalAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HorizontalAttribute horizontal = (HorizontalAttribute)attribute;
        if (horizontal.position == horizontal.count - 1 || NextCanHorizontal(property))
        {
            Rect rect = new Rect(position.x + position.width / horizontal.count * horizontal.position, position.y, position.width / horizontal.count, EditorGUI.GetPropertyHeight(property, label));
            EditorGUI.PropertyField(rect, property, label);
        }
        else EditorGUI.PropertyField(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HorizontalAttribute horizontal = (HorizontalAttribute)attribute;
        if (horizontal.position >= horizontal.count - 1 || !NextCanHorizontal(property)) return EditorGUI.GetPropertyHeight(property, label);
        else return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool NextCanHorizontal(SerializedProperty property)
    {
        SerializedProperty next = property.GetEndProperty();
        if (next != null && EditorGUI.GetPropertyHeight(next) < 0) return false;
        if (next != null && ZetanUtility.Editor.TryGetValue(next, out _, out var fieldInfo))
            return fieldInfo.GetCustomAttribute<HorizontalAttribute>() != null;
        return false;
    }
}