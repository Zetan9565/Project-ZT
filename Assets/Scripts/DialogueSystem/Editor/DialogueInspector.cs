using UnityEditor;
using UnityEngine;

namespace ZetanStudio.DialogueSystem.Editor
{
    [CustomEditor(typeof(Dialogue))]
    public class DialogueInspector : UnityEditor.Editor
    {
        SerializedProperty description;

        private void OnEnable()
        {
            description = serializedObject.FindProperty("description");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.LabelField("预览");
            var style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(Dialogue.Editor.Preview(target as Dialogue));
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
