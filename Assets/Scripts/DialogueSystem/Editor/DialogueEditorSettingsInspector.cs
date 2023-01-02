using UnityEngine;
using UnityEditor;

namespace ZetanStudio.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueEditorSettings))]
    public class DialogueEditorSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty editorUxml;
        private SerializedProperty editorUss;
        private SerializedProperty minWindowSize;
        private SerializedProperty language;
        private bool enablePortrait = false;

        private void OnEnable()
        {
            editorUxml = serializedObject.FindProperty("editorUxml");
            editorUss = serializedObject.FindProperty("editorUss");
            minWindowSize = serializedObject.FindProperty("minWindowSize");
            language = serializedObject.FindProperty("language");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(editorUxml, new GUIContent(Tr("编辑器UXML")));
            EditorGUILayout.PropertyField(editorUss, new GUIContent(Tr("编辑器USS")));
            EditorGUILayout.PropertyField(minWindowSize, new GUIContent(Tr("编辑器最小尺寸")));
            EditorGUILayout.PropertyField(language, new GUIContent(Tr("编辑器语言包")));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
#if ZTDS_ENABLE_PORTRAIT
            enablePortrait = true;
#endif
            if (enablePortrait != EditorGUILayout.Toggle(Tr("启用对话肖像"), enablePortrait))
            {
                enablePortrait = !enablePortrait;
                RefreshDefine();
            }
        }

        private void RefreshDefine()
        {
#if !ZTDS_ENABLE_PORTRAIT
            Utility.Editor.AddScriptingDefineSymbols("ZTDS_ENABLE_PORTRAIT");
#else
            Utility.Editor.RemoveScriptingDefineSymbols("ZTDS_ENABLE_PORTRAIT");
#endif
        }

        private string Tr(string text)
        {
            if (language != null) return L.Tr(language.objectReferenceValue as LanguageSet, text);
            else return text;
        }
    }
}