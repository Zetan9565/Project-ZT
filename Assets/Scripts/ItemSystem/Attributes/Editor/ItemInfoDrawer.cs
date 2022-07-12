using System.Collections;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomPropertyDrawer(typeof(ItemInfo))]
    public class ItemInfoDrawer : PropertyDrawer
    {
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType)) return EditorGUIUtility.singleLineHeight;
            else return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
            {
                SerializedProperty item = property.FindPropertyRelative("item");
                SerializedProperty amount = property.FindPropertyRelative("amount");
                float halfWidth = (position.width) / 2 - 1;
                EditorGUI.PropertyField(new Rect(position.x, position.y, halfWidth, EditorGUIUtility.singleLineHeight), item, GUIContent.none);
                EditorGUI.PropertyField(new Rect(position.x + halfWidth + 2, position.y, halfWidth, EditorGUIUtility.singleLineHeight), amount, GUIContent.none);
            }
            else EditorGUI.PropertyField(position, property, label, true);
        }
    }
}