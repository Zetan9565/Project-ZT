using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Editor
{
    [CustomEditor(typeof(BehaviourExecutor), true)]
    public class BehaviourExecutorInspector : UnityEditor.Editor
    {
        SerializedProperty behaviour;

        SerializedProperty frequency;
        SerializedProperty interval;
        SerializedProperty startOnStart;
        SerializedProperty restartOnComplete;
        SerializedProperty resetOnRestart;
        SerializedProperty gizmos;
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
            gizmos = serializedObject.FindProperty("gizmos");
            presetVariables = serializedObject.FindProperty("presetVariables");
            InitTree();
        }

        private void InitTree()
        {
            if (!behaviour.objectReferenceValue) return;
            serializedTree?.Dispose();
            serializedTree = new SerializedObject(behaviour.objectReferenceValue);
            serializedVariables = serializedTree.FindProperty("variables");
            variableList = new SharedVariableListDrawer(serializedVariables, true);
            presetVariableList = new SharedVariablePresetListDrawer(presetVariables, serializedTree.targetObject as ISharedVariableHandler,
                                                                    (target as BehaviourExecutor).GetPresetVariableTypeAtIndex);
            showList = new AnimBool(serializedVariables.isExpanded);
            showList.valueChanged.AddListener(() => { Repaint(); if (serializedVariables != null) serializedVariables.isExpanded = showList.target; });
            showPreset = new AnimBool(presetVariables.isExpanded);
            showPreset.valueChanged.AddListener(() => { Repaint(); if (presetVariables != null) presetVariables.isExpanded = showPreset.target; });
        }

        public override void OnInspectorGUI()
        {
            if (target is RuntimeBehaviourExecutor exe)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(exe))
                {
                    EditorGUILayout.HelpBox("不能在预制件使用RuntimeBehaviourExecutor", MessageType.Error);
                    return;
                }
                else EditorGUILayout.HelpBox("只在本场景生效，若要制作预制件，请使用BehaviourExecutor", MessageType.Info);
            }
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            var hasTreeBef = behaviour.objectReferenceValue;
            if (target is not RuntimeBehaviourExecutor)
            {
                bool shouldDisable = Application.isPlaying && !PrefabUtility.IsPartOfAnyPrefab(target);
                EditorGUI.BeginDisabledGroup(shouldDisable);
                if (shouldDisable) EditorGUILayout.ObjectField(behaviour, new GUIContent("行为树"));
                else EditorGUILayout.PropertyField(behaviour, new GUIContent("行为树"));
                EditorGUI.EndDisabledGroup();
            }
            if (behaviour.objectReferenceValue != hasTreeBef) InitTree();
            if (behaviour.objectReferenceValue)
            {
                if (serializedTree == null) InitTree();
                if (GUILayout.Button("编辑")) BehaviourTreeEditor.CreateWindow(target as BehaviourExecutor);
                serializedTree.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedTree.FindProperty("_name"));
                EditorGUILayout.PropertyField(serializedTree.FindProperty("description"));
                if (EditorGUI.EndChangeCheck()) serializedTree.ApplyModifiedProperties();
            }
            else
            {
                if (GUILayout.Button("新建"))
                {
                    BehaviourTree tree = ZetanUtility.Editor.SaveFilePanel(CreateInstance<BehaviourTree>, "new behaviour tree");
                    if (tree)
                    {
                        behaviour.objectReferenceValue = tree;
                        InitTree();
                        EditorGUILayout.PropertyField(behaviour, new GUIContent("行为树"));
                        EditorApplication.delayCall += delegate { BehaviourTreeEditor.CreateWindow(target as BehaviourExecutor); };
                    }
                }
            }
            EditorGUILayout.PropertyField(frequency, new GUIContent("执行频率"));
            if (frequency.enumValueIndex == (int)BehaviourExecutor.Frequency.FixedTime)
                EditorGUILayout.PropertyField(interval, new GUIContent("间隔(秒)"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(startOnStart, new GUIContent("开始时执行"));
            EditorGUILayout.PropertyField(restartOnComplete, new GUIContent("完成时重启"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(resetOnRestart, new GUIContent("重启时重置"));
            EditorGUILayout.PropertyField(gizmos, new GUIContent("显示Gizmos"));
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (behaviour.objectReferenceValue)
            {
                serializedTree.UpdateIfRequiredOrScript();
                showList.target = EditorGUILayout.Foldout(serializedVariables.isExpanded, "行为树共享变量", true);
                if (EditorGUILayout.BeginFadeGroup(showList.faded))
                    variableList.DoLayoutList();
                EditorGUILayout.EndFadeGroup();
                if (target is not RuntimeBehaviourExecutor && !Application.isPlaying && !ZetanUtility.IsPrefab((target as BehaviourExecutor).gameObject))
                {

                    showPreset.target = EditorGUILayout.Foldout(presetVariables.isExpanded, "变量预设列表", true);
                    if (EditorGUILayout.BeginFadeGroup(showPreset.faded))
                        presetVariableList.DoLayoutList();
                    EditorGUILayout.EndFadeGroup();
                }
                serializedTree.ApplyModifiedProperties();
            }
        }
    }
}