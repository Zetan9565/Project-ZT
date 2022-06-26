using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ZetanStudio
{
    public class ReferencesFixing : EditorWindow
    {
        [MenuItem("Window/Zetan Studio/工具/引用丢失修复")]
        public static void CreateWindow()
        {
            var wnd = GetWindow<ReferencesFixing>("引用修复");
            wnd.minSize = new Vector2(400, 160);
            wnd.Show();
        }
        public static void CreateWindow(Action<int> finishCallback)
        {
            var wnd = GetWindow<ReferencesFixing>("引用修复");
            wnd.minSize = new Vector2(400, 160);
            wnd.FinishCallback += finishCallback;
            wnd.Show();
        }

        private string searchTypeName = "ScriptableObject";
        private string oldTypeName = "类名, 命名空间(如果有), Assembly-CSharp";
        private string newTypeName = "类名, 命名空间(如果有), Assembly-CSharp";
        public event Action<int> FinishCallback;

        private void OnGUI()
        {
            if (error(oldTypeName) || error(newTypeName))
                EditorGUILayout.HelpBox("类名格式不正确，应为：\"类名, 命名空间(如果有), 程序集\")", MessageType.Error);
            else EditorGUILayout.HelpBox("无错误", MessageType.Info);
            searchTypeName = EditorGUILayout.TextField("检索类名", searchTypeName);
            EditorGUILayout.LabelField("把");
            oldTypeName = EditorGUILayout.TextField(oldTypeName);
            EditorGUILayout.LabelField("重命名为");
            newTypeName = EditorGUILayout.TextField(newTypeName);
            if (GUILayout.Button("修复"))
            {
                var temp = oldTypeName.Replace(" ", "").Split(',');
                var oldName = temp[0];
                var oldNs = temp[1];
                var oldAsm = temp[2];
                temp = newTypeName.Replace(" ", "").Split(',');
                var newName = temp[0];
                newName = string.IsNullOrEmpty(newName) ? oldName : newName;
                var newNs = temp[1];
                var newAsm = temp[2];
                List<string> paths = new List<string>();
                string[] guids = AssetDatabase.FindAssets($"t:{searchTypeName}");
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path)) paths.Add(path);
                }
                int count = 0;
                for (int i = 0; i < paths.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("查找替换中", $"当前路径: {paths[i]}", i / paths.Count);
                    string pattern = $"type: *{{class: *{oldName}, *ns: *{oldNs}, *asm: *{oldAsm}}}";
                    string text = File.ReadAllText(paths[i]);
                    if (Regex.IsMatch(text, pattern))
                    {
                        File.WriteAllText(paths[i], Regex.Replace(text, pattern, $"type: {{class: {newName}, ns: {newNs}, asm: {newAsm}}}"));
                        AssetDatabase.ImportAsset(paths[i]);
                        count++;
                    }
                }
                EditorUtility.ClearProgressBar();
                Debug.Log($"共替换了 {count} 个资源");
                FinishCallback?.Invoke(count);
            }

            static bool error(string typeName)
            {
                return !Regex.IsMatch(typeName.Replace(" ", ""), @"^[a-zA-Z_]\w*, *([a-zA-Z_][\w]*(\.[\w])*)*, *[a-zA-Z_][\w-]*$");
            }
        }
    }
}
