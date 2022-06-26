using UnityEditor;
using UnityEngine;

namespace ZetanStudio
{
    [CustomPropertyDrawer(typeof(SliderAttribute))]
    public sealed class SliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SliderAttribute attr = attribute as SliderAttribute;
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                property.intValue = EditorGUI.IntSlider(position, label, property.intValue, (int)attr.min, (int)attr.max);
                EditorGUI.EndProperty();
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                property.floatValue = EditorGUI.Slider(position, label, property.floatValue, attr.min, attr.max);
                EditorGUI.EndProperty();
            }
            else EditorGUI.PropertyField(position, property, true);
        }
    }
}
