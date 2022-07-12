using UnityEditor;

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
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
