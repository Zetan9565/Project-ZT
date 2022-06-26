using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomPropertyDrawer(typeof(ItemQualityAttribute))]
    public class ItemQualityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                var qualities = ItemQualityEnum.Instance.Enum.Select(x => new GUIContent(x.Name));
                GUIStyle style = new GUIStyle(EditorStyles.popup);
                style.normal.textColor = style.hover.textColor = style.focused.textColor = style.active.textColor = ItemQualityEnum.Instance.Enum[property.intValue].Color;
                label = EditorGUI.BeginProperty(position, label, property);
                EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
                property.intValue = EditorGUI.Popup(position, label, property.intValue, qualities.ToArray(), style);
                EditorGUI.EndProperty();
                EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;

                void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
                {
                    if (property.propertyType != SerializedPropertyType.Integer)
                        return;

                    menu.AddItem(EditorGUIUtility.TrTextContent("Location"), false, () =>
                    {
                        EditorGUIUtility.PingObject(ItemQualityEnum.Instance);
                    });
                    menu.AddItem(EditorGUIUtility.TrTextContent("Select"), false, () =>
                    {
                        EditorGUIUtility.PingObject(ItemQualityEnum.Instance);
                        Selection.activeObject = ItemQualityEnum.Instance;
                    });
                    menu.AddItem(EditorGUIUtility.TrTextContent("Properties..."), false, () =>
                    {
                        EditorUtility.OpenPropertyEditor(ItemQualityEnum.Instance);
                    });
                }
            }
            else EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}