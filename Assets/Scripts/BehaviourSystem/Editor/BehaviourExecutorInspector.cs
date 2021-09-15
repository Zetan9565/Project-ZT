using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [CustomEditor(typeof(BehaviourExecutor), true)]
    public class BehaviourExecutorInspector : Editor
    {
        SerializedProperty behaviour;

        SerializedProperty frequency;
        SerializedProperty interval;
        SerializedProperty startOnStart;
        SerializedProperty restartOnComplete;
        SerializedProperty resetOnRestart;
        SerializedProperty presetVariables;

        AnimBool showList;
        AnimBool showPreset;
        SharedVariableListDrawer variableList;
        SharedVariablePresetListDrawer presetVariableList;
        SerializedObject serializedTree;
        SerializedProperty serializedVariables;

        private void OnEnable()
        {
            behaviour = serializedObject.FindProperty("behaviour");
            frequency = serializedObject.FindProperty("frequency");
            interval = serializedObject.FindProperty("interval");
            startOnStart = serializedObject.FindProperty("startOnStart");
            restartOnComplete = serializedObject.FindProperty("restartOnComplete");
            resetOnRestart = serializedObject.FindProperty("resetOnRestart");
            presetVariables = serializedObject.FindProperty("presetVariables");
            InitTree();
        }

        private void InitTree()
        {
            if (!behaviour.objectReferenceValue) return;
            serializedTree = new SerializedObject(behaviour.objectReferenceValue);
            serializedVariables = serializedTree.FindProperty("variables");
            variableList = new SharedVariableListDrawer(serializedTree, serializedVariables, true);
            presetVariableList = new SharedVariablePresetListDrawer(serializedObject, presetVariables,
                                                                    serializedTree.targetObject as ISharedVariableHandler,
                                                                    (target as BehaviourExecutor).GetVariableTypeAtIndex);
            showList = new AnimBool(serializedVariables.isExpanded);
            showList.valueChanged.AddListener(() => { Repaint(); if (serializedVariables != null) serializedVariables.isExpanded = showList.target; });
            showPreset = new AnimBool(presetVariables.isExpanded);
            showPreset.valueChanged.AddListener(() => { Repaint(); if (presetVariables != null) presetVariables.isExpanded = showPreset.target; });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(target is RuntimeBehaviourExecutor);
            bool hasTreeBef = behaviour.objectReferenceValue;
            if (target is RuntimeBehaviourExecutor) EditorGUILayout.ObjectField(behaviour, new GUIContent("行为树"));
            else EditorGUILayout.PropertyField(behaviour, new GUIContent("行为树"));
            EditorGUI.EndDisabledGroup();
            if (behaviour.objectReferenceValue != hasTreeBef) InitTree();
            if (behaviour.objectReferenceValue)
            {
                if (GUILayout.Button("编辑")) BehaviourTreeEditor.CreateWindow(target as BehaviourExecutor);
                EditorGUILayout.PropertyField(serializedTree.FindProperty("_name"));
                EditorGUILayout.PropertyField(serializedTree.FindProperty("description"));
            }
            EditorGUILayout.PropertyField(frequency, new GUIContent("执行频率"));
            if (frequency.enumValueIndex == (int)BehaviourExecutor.Frequency.FixedTime)
                EditorGUILayout.PropertyField(interval, new GUIContent("间隔"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(startOnStart, new GUIContent("开始时执行"));
            EditorGUILayout.PropertyField(restartOnComplete, new GUIContent("完成时重启"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(resetOnRestart, new GUIContent("重启时重置"));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (behaviour.objectReferenceValue)
            {
                serializedTree.Update();
                EditorGUILayout.PropertyField(serializedVariables, new GUIContent("行为树共享变量"), false);
                showList.target = serializedVariables.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showList.faded))
                    variableList.DoLayoutList();
                EditorGUILayout.EndFadeGroup();
                if (!(target is RuntimeBehaviourExecutor) && !Application.isPlaying)
                {
                    EditorGUILayout.PropertyField(presetVariables, new GUIContent("变量预设列表"), false);
                    showPreset.target = presetVariables.isExpanded;
                    if (EditorGUILayout.BeginFadeGroup(showPreset.faded))
                        presetVariableList.DoLayoutList();
                    EditorGUILayout.EndFadeGroup();
                }
                serializedTree.ApplyModifiedProperties();
            }
        }
    }
}