using UnityEditor;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [CustomEditor(typeof(BehaviourTree))]
    public class BehaviourTreeInspector : Editor
    {
        SerializedProperty _name;
        SerializedProperty description;

        private void OnEnable()
        {
            _name = serializedObject.FindProperty("_name");
            description = serializedObject.FindProperty("description");
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
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.ObjectField("脚本", MonoScript.FromScriptableObject(target as BehaviourTree), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_name);
            EditorGUILayout.PropertyField(description);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}