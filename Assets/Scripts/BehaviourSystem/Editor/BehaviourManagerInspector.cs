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
            if (!globalVariables.objectReferenceValue) return;
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
            if (FindObjectsOfType<BehaviourManager>().Length > 1)
            {
                EditorGUILayout.HelpBox("存在多个激活的BehaviourManager，请删除或失活其它", MessageType.Error);
                return;
            }
            serializedObject.UpdateIfRequiredOrScript();
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
                serializedObject.UpdateIfRequiredOrScript();

                showGlobal.target = EditorGUILayout.Foldout(serializedVariables.isExpanded, "全局变量列表", true); ;
                if (EditorGUILayout.BeginFadeGroup(showGlobal.faded))
                    variableList.DoLayoutList();
                EditorGUILayout.EndFadeGroup();
                if (!Application.isPlaying && !ZetanUtility.IsPrefab((target as BehaviourManager).gameObject))
                {
                    showPreset.target = EditorGUILayout.Foldout(presetVariables.isExpanded, "变量预设列表", true);
                    if (EditorGUILayout.BeginFadeGroup(showPreset.faded))
                        presetVariableList.DoLayoutList();
                    EditorGUILayout.EndFadeGroup();
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}