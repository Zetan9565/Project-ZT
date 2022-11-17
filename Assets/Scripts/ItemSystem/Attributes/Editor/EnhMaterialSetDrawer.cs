using System.Collections;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(EnhMaterialSet))]
    public class EnhMaterialSetDrawer : PropertyDrawer
    {
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();
        private readonly float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType)) return lineHeightSpace + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("materials"));
            else return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
            {
                int index = property.GetArrayIndex();
                SerializedProperty rate = property.FindAutoProperty("SuccessRate");
                SerializedProperty materials = property.FindPropertyRelative("materials");
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), rate, new GUIContent(Tr("[{0}组] 成功率", index + 1)));
                EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight), materials, new GUIContent(Tr("材料")));
            }
            else EditorGUI.PropertyField(position, property, label, true);
        }

        private string Tr(string text, params object[] args)
        {
            return L.Tr(settings.language, text, args);
        }
    }
}