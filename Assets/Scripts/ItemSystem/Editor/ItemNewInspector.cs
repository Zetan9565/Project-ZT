using UnityEngine;
using UnityEditor;

namespace ZetanStudio.Item.Editor
{
    [CustomEditor(typeof(ItemNew))]
    public sealed class ItemNewInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("编辑"))
            {
                bool isSub = AssetDatabase.IsSubAsset(target);
                bool database = ItemEditorSettings.GetOrCreate().useDatabase;
                if (isSub && database || !isSub && !database) ItemEditor.CreateWindow(target as ItemNew);
                else if (database) EditorUtility.DisplayDialog("错误", "道具编辑器处于数据库模式，不支持混用！", "确定");
                else EditorUtility.DisplayDialog("错误", "道具编辑器处于非数据库模式，不支持混用！", "确定");
            }
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginDisabledGroup(true);
            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator != null)
            {
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren) && iterator.propertyPath != "modules")
                {

                    if ("m_Script" != iterator.propertyPath)
                        EditorGUILayout.PropertyField(iterator, true);
                    enterChildren = false;
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}