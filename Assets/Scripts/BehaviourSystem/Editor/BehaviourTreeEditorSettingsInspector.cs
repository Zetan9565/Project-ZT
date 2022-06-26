using UnityEditor;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Editor
{
    [CustomEditor(typeof(BehaviourTreeEditorSettings))]
    public class BehaviourTreeEditorSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty treeUxml;
        private SerializedProperty treeUss;
        private SerializedProperty minWindowSize;
        private SerializedProperty nodeUxml;
        private SerializedProperty scriptTemplateAction;
        private SerializedProperty scriptTemplateConditional;
        private SerializedProperty scriptTemplateComposite;
        private SerializedProperty scriptTemplateDecorator;
        private SerializedProperty scriptTemplateVariable;
        private SerializedProperty newNodeScriptFolder;
        private SerializedProperty newVarScriptFolder;
        private SerializedProperty newAssetFolder;
        private SerializedProperty changeOnSelected;
        private SerializedProperty language;

        private void OnEnable()
        {
            treeUxml = serializedObject.FindProperty("treeUxml");
            treeUss = serializedObject.FindProperty("treeUss");
            minWindowSize = serializedObject.FindProperty("minWindowSize");
            nodeUxml = serializedObject.FindProperty("nodeUxml");
            scriptTemplateAction = serializedObject.FindProperty("scriptTemplateAction");
            scriptTemplateConditional = serializedObject.FindProperty("scriptTemplateConditional");
            scriptTemplateComposite = serializedObject.FindProperty("scriptTemplateComposite");
            scriptTemplateDecorator = serializedObject.FindProperty("scriptTemplateDecorator");
            scriptTemplateVariable = serializedObject.FindProperty("scriptTemplateVariable");
            newNodeScriptFolder = serializedObject.FindProperty("newNodeScriptFolder");
            newVarScriptFolder = serializedObject.FindProperty("newVarScriptFolder");
            newAssetFolder = serializedObject.FindProperty("newAssetFolder");
            changeOnSelected = serializedObject.FindProperty("changeOnSelected");
            language = serializedObject.FindProperty("language");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(treeUxml, new GUIContent(Tr("编辑器UXML")));
            EditorGUILayout.PropertyField(treeUss, new GUIContent(Tr("编辑器USS")));
            EditorGUILayout.PropertyField(minWindowSize, new GUIContent(Tr("编辑器最小尺寸")));
            EditorGUILayout.PropertyField(nodeUxml, new GUIContent(Tr("结点UXML")));
            EditorGUILayout.PropertyField(scriptTemplateAction, new GUIContent(Tr("行为结点脚本模板")));
            EditorGUILayout.PropertyField(scriptTemplateConditional, new GUIContent(Tr("条件结点脚本模板")));
            EditorGUILayout.PropertyField(scriptTemplateComposite, new GUIContent(Tr("复合结点脚本模板")));
            EditorGUILayout.PropertyField(scriptTemplateDecorator, new GUIContent(Tr("修饰结点脚本模板")));
            EditorGUILayout.PropertyField(scriptTemplateVariable, new GUIContent(Tr("共享变量脚本模板")));
            EditorGUILayout.PropertyField(newNodeScriptFolder, new GUIContent(Tr("新结点脚本默认路径")));
            EditorGUILayout.PropertyField(newVarScriptFolder, new GUIContent(Tr("新变量脚本默认路径")));
            EditorGUILayout.PropertyField(newAssetFolder, new GUIContent(Tr("新建资源默认路径")));
            EditorGUILayout.PropertyField(changeOnSelected, new GUIContent(Tr("选中行为树时更新视图")));
            EditorGUILayout.PropertyField(language, new GUIContent(Tr("编辑器语言")));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        public string Tr(string text)
        {
            return L.Tr(language.objectReferenceValue as LanguageMap, text);
        }
    }
}
