using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Item
{
    [CustomPropertyDrawer(typeof(ItemQualityAttribute))]
    public class ItemQualityAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                var qualities = ItemQualityEnum.Instance.Enum.Select(x => new GUIContent(x.Name));
                GUIStyle style = new GUIStyle(EditorStyles.popup);
                style.normal.textColor = style.hover.textColor = style.focused.textColor = style.active.textColor = ItemQualityEnum.Instance.Enum[property.intValue].Color;
                property.intValue = EditorGUI.Popup(position, label, property.intValue, qualities.ToArray(), style);
            }
            else EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}