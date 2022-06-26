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
                var indexLabel = new GUIContent($"[{property.GetArrayIndex() + 1}]");
                float labelWidth = GUI.skin.label.CalcSize(indexLabel).x;
                EditorGUI.LabelField(new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight), indexLabel);
                float halfWidth = (position.width - labelWidth) / 2 - 1;
                float oldLabelWdith = EditorGUIUtility.labelWidth;
                var tL = new GUIContent(L.Tr(settings.language, "道具"));
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(tL).x;
                EditorGUI.PropertyField(new Rect(position.x + labelWidth, position.y, halfWidth, EditorGUIUtility.singleLineHeight), item, tL);
                tL = new GUIContent(L.Tr(settings.language, "数量"));
                EditorGUI.PropertyField(new Rect(position.x + labelWidth + halfWidth + 2, position.y, halfWidth, EditorGUIUtility.singleLineHeight), amount, tL);
                EditorGUIUtility.labelWidth = oldLabelWdith;
            }
            else EditorGUI.PropertyField(position, property, label, true);
        }
    }
}