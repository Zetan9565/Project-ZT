using UnityEditor;

namespace ZetanStudio
{
    [CustomEditor(typeof(LanguageMap), true)]
    public class LanguageMapInspector : Editor
    {
        SerializedProperty items;
        PaginatedReorderableList list;

        private void OnEnable()
        {
            items = serializedObject.FindProperty("items");
            list = new PaginatedReorderableList("映射", items, 20, true);
        }

        private void OnDisable()
        {
            list.Dispose();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            list.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
