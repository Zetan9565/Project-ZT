using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Editor
{
    [CustomEditor(typeof(BehaviourManager))]
    public sealed class BehaviourManagerInspector : UnityEditor.Editor
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
        BehaviourTreeEditorSettings settings;

        private void OnEnable()
        {
            settings = BehaviourTreeEditorSettings.GetOrCreate();

            globalVariables = serializedObject.FindProperty("globalVariables");
            presetVariables = serializedObject.FindProperty("presetVariables");
            globalDrawer = new ObjectSelectionDrawer<GlobalVariables>(globalVariables, string.Empty, string.Empty, Tr("全局变量"));
            InitGlobal();
        }

        private void InitGlobal()
        {
            if (!globalVariables.objectReferenceValue) return;
            serializedGlobal = new SerializedObject(globalVariables.objectReferenceValue);
            serializedVariables = serializedGlobal.FindProperty("variables");
            variableList = new SharedVariableListDrawer(serializedVariables, false);
            presetVariableList = new SharedVariablePresetListDrawer(presetVariables, serializedGlobal.targetObject as ISharedVariableHandler,
                                                                    (target as BehaviourManager).GetPresetVariableTypeAtIndex);
            showGlobal = new AnimBool(serializedVariables.isExpanded);
            showGlobal.valueChanged.AddListener(() => { Repaint(); if (serializedVariables != null) serializedVariables.isExpanded = showGlobal.target; });
            showPreset = new AnimBool(presetVariables.isExpanded);
            showPreset.valueChanged.AddListener(() => { Repaint(); if (presetVariables != null) presetVariables.isExpanded = showPreset.target; });
        }

        public override void OnInspectorGUI()
        {
            if (FindObjectsOfType<BehaviourManager>().Count(x => x.isActiveAndEnabled) > 1)
            {
                EditorGUILayout.HelpBox(Language.Tr(settings.language, "存在多个激活的{0}，请移除或失活其它", typeof(BehaviourManager).Name), MessageType.Error);
                return;
            }
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            bool shouldDisable = Application.isPlaying && !PrefabUtility.IsPartOfAnyPrefab(target);
            EditorGUI.BeginDisabledGroup(shouldDisable);
            var globalBef = globalVariables.objectReferenceValue;
            if (shouldDisable) EditorGUILayout.PropertyField(globalVariables, new GUIContent(Tr("全局变量")));
            else globalDrawer.DoLayoutDraw();
            if (!globalVariables.objectReferenceValue && ZetanUtility.Editor.LoadAsset<GlobalVariables>() == null)
                if (GUILayout.Button(Tr("新建")))
                    globalVariables.objectReferenceValue = ZetanUtility.Editor.SaveFilePanel(CreateInstance<GlobalVariables>, "global variables");
            EditorGUI.EndDisabledGroup();
            if (globalVariables.objectReferenceValue != globalBef) InitGlobal();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (globalVariables.objectReferenceValue)
            {
                serializedObject.UpdateIfRequiredOrScript();

                showGlobal.target = EditorGUILayout.Foldout(serializedVariables.isExpanded, Tr("全局变量列表"), true); ;
                if (EditorGUILayout.BeginFadeGroup(showGlobal.faded))
                    variableList.DoLayoutList();
                EditorGUILayout.EndFadeGroup();
                if (!Application.isPlaying && !ZetanUtility.IsPrefab((target as BehaviourManager).gameObject))
                {
                    showPreset.target = EditorGUILayout.Foldout(presetVariables.isExpanded, Tr("变量预设列表"), true);
                    if (EditorGUILayout.BeginFadeGroup(showPreset.faded))
                        presetVariableList.DoLayoutList();
                    EditorGUILayout.EndFadeGroup();
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private string Tr(string text)
        {
            return Language.Tr(settings.language, text);
        }
    }
}