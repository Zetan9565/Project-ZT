using UnityEditor;

namespace ZetanStudio.Editor
{
    [CustomEditor(typeof(LanguageMap), true)]
    public class LanguageMapInspector : UnityEditor.Editor
    {
        SerializedProperty _name;
        SerializedProperty items;
        PaginatedReorderableList list;

        private void OnEnable()
        {
            _name = serializedObject.FindProperty("_name");
            items = serializedObject.FindProperty("items");
            list = new PaginatedReorderableList("映射", items, 30, true);
        }

        private void OnDisable()
        {
            list.Dispose();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_name);
            list.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
