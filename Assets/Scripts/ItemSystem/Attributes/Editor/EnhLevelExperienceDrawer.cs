using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(EnhLevelExperience))]
    public class EnhLevelExperienceDrawer : PropertyDrawer
    {
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width / 2 - 1, EditorGUIUtility.singleLineHeight), property.FindAutoPropertyRelative("Experience"), 
                new GUIContent(L.Tr(settings.language, "[{0}级] 经验值", property.GetArrayIndex()+1)));
            SerializedProperty rate = property.FindAutoPropertyRelative("SuccessRate");
            var oldLW = EditorGUIUtility.labelWidth;
            var sL = new GUIContent(Tr("成功率"));
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(sL).x;
            EditorGUI.PropertyField(new Rect(position.x + position.width / 2 + 1, position.y, position.width / 2 - 1, EditorGUIUtility.singleLineHeight), rate, sL);
            EditorGUIUtility.labelWidth = oldLW;
            rate.floatValue = Mathf.Clamp(rate.floatValue, 0.0001f, 1);
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
    }
}