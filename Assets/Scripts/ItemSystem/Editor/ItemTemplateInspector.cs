using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Item.Editor
{
    [CustomEditor(typeof(ItemTemplate))]
    public class ItemTemplateInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("编辑"))
            {
                ItemEditor.CreateWindow(target as ItemTemplate);
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