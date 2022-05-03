using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MultiPropertyAttribute))]
public class MultiPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (MultiPropertyAttribute)attribute;
        GUIContent[] sub = new GUIContent[attr.labels.Length];
        for (int i = 0; i < attr.labels.Length; i++)
        {
            sub[i] = new GUIContent(attr.labels[i]);
        }
        using var copy = property.Copy();
        copy.NextVisible(true);
        EditorGUI.MultiPropertyField(position, sub, copy, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 2;
    }
}