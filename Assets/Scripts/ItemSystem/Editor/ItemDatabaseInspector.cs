using UnityEditor;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseInspector : UnityEditor.Editor
    {
        PaginatedReorderableList list;

        private void OnEnable()
        {
            list = new PaginatedReorderableList(serializedObject.FindProperty("items"), 20, true, false, false, false) { searchFilter = SearchFilter };
        }

        private bool SearchFilter(string key, SerializedProperty property)
        {
            return (property.objectReferenceValue as Item).Name.Contains(key);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            list.DoLayoutList();
            EditorGUI.EndDisabledGroup();
        }
    }
}