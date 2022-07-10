using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension.Editor;

    [CustomEditor(typeof(NewDialogue))]
    public class NewDialogueInspector : UnityEditor.Editor
    {
        SerializedProperty description;

        private void OnEnable()
        {
            try
            {
                description = serializedObject.FindProperty("description");
            }
            catch { }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(description);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
