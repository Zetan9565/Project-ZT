using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;

    [CustomPropertyDrawer(typeof(DropItemInfo))]
    public class DropItemInfoDrawer : PropertyDrawer
    {
        private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private readonly float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Max(EditorGUI.GetPropertyHeight(property, label) - lineHeightSpace, lineHeightSpace);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty item = property.FindPropertyRelative("item");
            SerializedProperty amount = property.FindPropertyRelative("Amount");
            SerializedProperty range = amount.FindAutoPropertyRelative("Range");
            string amountStr = range.vector2IntValue.x == range.vector2IntValue.y ? $"{range.vector2IntValue.x}" : $"[{range.vector2IntValue.x}~{range.vector2IntValue.y}]";
            SerializedProperty onlyDropForQuest = property.FindPropertyRelative("onlyDropForQuest");
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width / 2f - 1, lineHeight), property,
                new GUIContent($"{(item.objectReferenceValue as Item).Name} ×{amountStr}"), false);
            EditorGUI.PropertyField(new Rect(position.x + position.width / 2f + 1, position.y, position.width / 2f - 1, lineHeight), item, new GUIContent(string.Empty));
            if (property.isExpanded)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace, position.width, EditorGUI.GetPropertyHeight(amount)),
                    amount, new GUIContent("数量"));
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace + EditorGUI.GetPropertyHeight(amount), position.width, lineHeight),
                    onlyDropForQuest, new GUIContent("只为此任务产出"));
            }
        }
    }
}