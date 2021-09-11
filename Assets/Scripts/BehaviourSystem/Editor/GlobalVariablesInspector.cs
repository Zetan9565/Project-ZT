using UnityEngine;
using UnityEditor;

namespace ZetanStudio.BehaviourTree
{
    [CustomEditor(typeof(GlobalVariables))]
    public class GlobalVariablesInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("脚本", MonoScript.FromScriptableObject(target as GlobalVariables), typeof(MonoScript), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("variables"), new GUIContent("变量列表"));
            EditorGUI.EndDisabledGroup();
        }
    }
}