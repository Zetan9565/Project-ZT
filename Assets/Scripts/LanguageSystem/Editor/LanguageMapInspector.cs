using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.LanguageSystem.Editor
{
    using ZetanStudio.Editor;

    [CustomEditor(typeof(LanguageSet))]
    public class LanguageSetInspector : UnityEditor.Editor
    {
        SerializedProperty _name;
        SerializedProperty maps;

        PaginatedReorderableList list;

        private void OnEnable()
        {
            _name = serializedObject.FindProperty("_name");
            list = new PaginatedReorderableList(maps = serializedObject.FindProperty("maps"), 40);
        }

        public override void OnInspectorGUI()
        {
            var maps = (target as LanguageSet).Maps;
            string duplicatedKey = null;
            Action callback = null;
            foreach (var map in maps)
            {
                if (maps.Any(m => m != map && m.Key == map.Key))
                {
                    duplicatedKey = map.Key;
                    break;
                }
            }
            if (duplicatedKey != null)
            {
                EditorGUILayout.HelpBox($"存在相同的键：{duplicatedKey}", MessageType.Error);
                callback = () =>
                {
                    list.Search(duplicatedKey);
                };
            }
            else if (maps.FirstOrDefault(m => string.IsNullOrEmpty(m.Key)) is LanguageMap empty)
            {
                EditorGUILayout.HelpBox("存在空键！", MessageType.Error);
                callback = () =>
                {
                    var index = maps.IndexOf(empty);
                    list.Select(index);
                    this.maps.GetArrayElementAtIndex(index).isExpanded = true;
                };
            }
            else EditorGUILayout.HelpBox("无错误", MessageType.Info);
            EditorGUI.BeginDisabledGroup(callback == null);
            if (GUILayout.Button("查看错误")) callback();
            EditorGUI.EndDisabledGroup();

            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_name);
            list?.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
