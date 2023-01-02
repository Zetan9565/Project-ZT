using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomEditor(typeof(ItemAffixInformation))]
    public class ItemAffixInfoInspector : UnityEditor.Editor
    {
        private SerializedProperty affix;
        private SerializedProperty affixCountMin;
        private SerializedProperty affixCountMax;
        private SerializedProperty affixCountDistrib;
        private SerializedProperty affixIndexDistrib;
        private SerializedProperty affixes;
        private LanguageSet language;

        private void OnEnable()
        {
            language = ItemEditorSettings.GetOrCreate().language;

            affix = serializedObject.FindProperty("affix");
            var affixCountRange = affix.FindPropertyRelative("affixCountRange");
            affixCountMin = affixCountRange.FindPropertyRelative("x");
            affixCountMax = affixCountRange.FindPropertyRelative("y");
            affixCountDistrib = affix.FindPropertyRelative("affixCountDistrib");
            affixIndexDistrib = affix.FindPropertyRelative("affixIndexDistrib");
            affixes = affix.FindPropertyRelative("affixes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            affixCountMin.intValue = EditorGUILayout.IntSlider(new GUIContent(Tr("最少词缀数")), affixCountMin.intValue, 0, affixCountMax.intValue);
            affixCountMax.intValue = EditorGUILayout.IntSlider(new GUIContent(Tr("最多词缀数")), affixCountMax.intValue, affixCountMin.intValue, affixes.arraySize);
            EditorGUILayout.PropertyField(affixCountDistrib, new GUIContent(Tr("词缀数量分布"), Tr("按此分布曲线选择词缀数量")));
            EditorGUILayout.PropertyField(affixIndexDistrib, new GUIContent(Tr("词缀分布"), Tr("按此分布曲线选择词缀")));
            EditorGUILayout.PropertyField(affixes, new GUIContent(Tr("词缀列表")));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        protected string Tr(string text)
        {
            return L.Tr(language, text);
        }
    }
}
