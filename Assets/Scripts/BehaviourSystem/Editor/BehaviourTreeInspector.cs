using UnityEditor;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Editor
{
    [CustomEditor(typeof(BehaviourTree))]
    public class BehaviourTreeInspector : UnityEditor.Editor
    {
        SerializedProperty _name;
        SerializedProperty description;

        SerializedProperty variables;
        SharedVariableListDrawer variableList;

        private void OnEnable()
        {
            _name = serializedObject.FindProperty("_name");
            description = serializedObject.FindProperty("description");
            variables = serializedObject.FindProperty("variables");
            variableList = new SharedVariableListDrawer(variables, true);
        }

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            if (GUILayout.Button("编辑脚本"))
            {
                AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(target as BehaviourTree));
            }
            EditorGUILayout.Space(10);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_name);
            EditorGUILayout.PropertyField(description);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
            variableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}