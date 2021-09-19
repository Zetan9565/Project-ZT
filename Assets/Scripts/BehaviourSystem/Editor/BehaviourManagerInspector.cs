using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [CustomEditor(typeof(BehaviourManager))]
    public sealed class BehaviourManagerInspector : Editor
    {
        SerializedProperty globalVariables;
        SerializedProperty presetVariables;

        ObjectSelectionDrawer<GlobalVariables> globalDrawer;
        SharedVariableListDrawer variableList;
        SharedVariablePresetListDrawer presetVariableList;
        SerializedObject serializedGlobal;
        SerializedProperty serializedVariables;

        AnimBool showGlobal;
        AnimBool showPreset;

        private void OnEnable()
        {
            globalVariables = serializedObject.FindProperty("globalVariables");
            presetVariables = serializedObject.FindProperty("presetVariables");
            globalDrawer = new ObjectSelectionDrawer<GlobalVariables>(globalVariables, string.Empty, string.Empty, "全局变量");
            InitGlobal();
        }

        private void InitGlobal()
        {
            serializedGlobal = new SerializedObject(globalVariables.objectReferenceValue);
            serializedVariables = serializedGlobal.FindProperty("variables");
            variableList = new SharedVariableListDrawer(serializedGlobal, serializedVariables, false);
            presetVariableList = new SharedVariablePresetListDrawer(serializedObject, presetVariables,
                                                                    serializedGlobal.targetObject as ISharedVariableHandler,
                                                                    (target as BehaviourManager).GetPresetVariableTypeAtIndex);
            showGlobal = new AnimBool(serializedVariables.isExpanded);
            showGlobal.valueChanged.AddListener(() => { Repaint(); if (serializedVariables != null) serializedVariables.isExpanded = showGlobal.target; });
            showPreset = new AnimBool(presetVariables.isExpanded);
            showPreset.valueChanged.AddListener(() => { Repaint(); if (presetVariables != null) presetVariables.isExpanded = showPreset.target; });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            bool shouldDisable = Application.isPlaying && !PrefabUtility.IsPartOfAnyPrefab(target);
            EditorGUI.BeginDisabledGroup(shouldDisable);
            var globalBef = globalVariables.objectReferenceValue;
            if (shouldDisable) EditorGUILayout.PropertyField(globalVariables, new GUIContent("全局变量"));
            else globalDrawer.DoLayoutDraw();
            if (!globalVariables.objectReferenceValue && ZetanEditorUtility.LoadAsset<GlobalVariables>() == null)
                if (GUILayout.Button("新建"))
                    globalVariables.objectReferenceValue = ZetanEditorUtility.SaveFilePanel(CreateInstance<GlobalVariables>, "global variables");
            EditorGUI.EndDisabledGroup();
            if (globalVariables.objectReferenceValue != globalBef) InitGlobal();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (globalVariables.objectReferenceValue)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedVariables, new GUIContent("全局变量列表"), false);
                showGlobal.target = serializedVariables.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showGlobal.faded))
                    variableList.DoLayoutList();
                EditorGUILayout.EndFadeGroup();
                if (!Application.isPlaying)
                {
                    EditorGUILayout.PropertyField(presetVariables, new GUIContent("变量预设列表"), false);
                    showPreset.target = presetVariables.isExpanded;
                    if (EditorGUILayout.BeginFadeGroup(showPreset.faded))
                        presetVariableList.DoLayoutList();
                    EditorGUILayout.EndFadeGroup();
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}