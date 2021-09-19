using UnityEditor;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [CustomEditor(typeof(Node), true)]
    public class NodeInspector : Editor
    {
        SerializedProperty description;

        private void OnEnable()
        {
            description = serializedObject.FindProperty("_description");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("标识", (target as Node).guid);
            EditorGUILayout.PropertyField(description, new GUIContent("描述"));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}