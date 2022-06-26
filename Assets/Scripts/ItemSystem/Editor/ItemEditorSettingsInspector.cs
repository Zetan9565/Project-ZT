using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomEditor(typeof(ItemEditorSettings))]
    public class ItemEditorSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty treeUxml;
        private SerializedProperty treeUss;
        private SerializedProperty minWindowSize;
        private SerializedProperty scriptTemplate;
        private SerializedProperty newScriptFolder;
        private bool useDatabase;
        private SerializedProperty language;
        private string path;


        private void OnEnable()
        {
            treeUxml = serializedObject.FindProperty("treeUxml");
            treeUss = serializedObject.FindProperty("treeUss");
            minWindowSize = serializedObject.FindProperty("minWindowSize");
            scriptTemplate = serializedObject.FindProperty("scriptTemplate");
            newScriptFolder = serializedObject.FindProperty("newScriptFolder");
            language = serializedObject.FindProperty("language");
            path = ZetanUtility.Editor.GetAssetPathWhere<MonoScript>(x => x.GetClass() == typeof(Item));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(treeUxml, new GUIContent(Tr("编辑器UXML")));
            EditorGUILayout.PropertyField(treeUss, new GUIContent(Tr("编辑器USS")));
            EditorGUILayout.PropertyField(minWindowSize, new GUIContent(Tr("编辑器最小尺寸")));
            EditorGUILayout.PropertyField(scriptTemplate, new GUIContent(Tr("模块脚本模板")));
            EditorGUILayout.PropertyField(newScriptFolder, new GUIContent(Tr("新模块脚本路径")));
            bool useBef = Item.UseDatabase;
            useDatabase = EditorGUILayout.Toggle(new GUIContent(Tr("使用数据库"), Tr("若否，则为每个道具新建一个独立的'*.asset'文件")), Item.UseDatabase);
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height), new GUIContent(Tr("资源保存位置")));
            float buttonWidth = GUI.skin.button.CalcSize(new GUIContent(Tr("选择"))).x;
            var assetsFolder = EditorGUI.TextField(new Rect(rect.x + EditorGUIUtility.labelWidth + 2, rect.y, rect.width - EditorGUIUtility.labelWidth - buttonWidth - 4, rect.height),
                                                   Item.assetsFolder);
            Rect buttonRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, rect.height);
            if (GUI.Button(buttonRect, Tr("选择")))
            {
                string path;
                path = assetsFolder.StartsWith("Assets/Resources") && AssetDatabase.IsValidFolder(assetsFolder) ? assetsFolder : "Assets/Resources";
                while (true)
                {
                    path = EditorUtility.SaveFolderPanel(Tr("选择道具相关资源的位置"), path, null);
                    path = ZetanUtility.Editor.ConvertToAssetsPath(path);
                    if (!string.IsNullOrEmpty(path) && !path.StartsWith("Assets/Resources"))
                        if (!EditorUtility.DisplayDialog(Tr("路径错误"), Tr("请选择{0}范围内的路径", "Assets/Resources"), Tr("确定"), Tr("取消")))
                        {
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else continue;
                    if (!string.IsNullOrEmpty(path))
                    {
                        assetsFolder = path;
                        if (assetsFolder != Item.assetsFolder)
                        {
                            try
                            {
                                File.WriteAllText(this.path, File.ReadAllText(this.path).Replace($"public const string assetsFolder = \"{Item.assetsFolder}\"",
                                    $"public const string assetsFolder = \"{assetsFolder}\""));
                                AssetDatabase.ImportAsset(this.path);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                    break;
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.PropertyField(language, new GUIContent(Tr("编辑器语言")));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (useDatabase != useBef)
            {
                try
                {
                    if (useDatabase)
                    {
                        File.WriteAllText(path, File.ReadAllText(path).Replace("private const bool useDatabase = false", "private const bool useDatabase = true"));
                    }
                    else
                    {
                        File.WriteAllText(path, File.ReadAllText(path).Replace("private const bool useDatabase = true", "private const bool useDatabase = false"));
                    }
                    AssetDatabase.ImportAsset(path);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private string Tr(string text)
        {
            return L.Tr(language.objectReferenceValue as LanguageMap, text);
        }
        private string Tr(string text, params object[] args)
        {
            return L.Tr(language.objectReferenceValue as LanguageMap, text, args);
        }
    }
}
