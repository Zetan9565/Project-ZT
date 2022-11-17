using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(EnhConsumable))]
    public class EnhConsumableDrawer : PropertyDrawer
    {
        private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var oldLW = EditorGUIUtility.labelWidth;
            SerializedProperty item = property.FindAutoProperty("Item");
            SerializedProperty amount = property.FindAutoProperty("Amount");
            SerializedProperty rate = property.FindAutoProperty("SuccessRate");
            var t = new GUIContent(L.Tr(settings.language, "道具"));
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(t).x;
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width / 3 - 1, lineHeight), item, t);
            t = new GUIContent(L.Tr(settings.language, "数量"));
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(t).x;
            EditorGUI.PropertyField(new Rect(position.x + position.width / 3 + 1, position.y, position.width / 3 - 1, lineHeight), amount, t);
            if (amount.intValue < 1) amount.intValue = 1;
            t = new GUIContent(L.Tr(settings.language, "成功率"));
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(t).x;
            EditorGUI.PropertyField(new Rect(position.x + position.width * 2 / 3 + 2, position.y, position.width / 3 - 1, lineHeight), rate, t);
            rate.floatValue = Mathf.Clamp(rate.floatValue, 0.0001f, 1f);
            EditorGUIUtility.labelWidth = oldLW;
        }
    }
}