using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class RelationFinding : EditorWindow
{
    private UnityEngine.Object target;
    private int depth = 3;
    private string path = "Configuration";
    private readonly List<SearchResult> results = new List<SearchResult>();
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/Zetan Studio/工具/查找关联配置")]
    public static void CreateWindow()
    {
        RelationFinding window = GetWindow<RelationFinding>("查找关联配置");
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        target = EditorGUILayout.ObjectField(new GUIContent("查找对象"), target, typeof(UnityEngine.Object), false);
        depth = EditorGUILayout.IntField(new GUIContent("查找深度"), depth);
        path = EditorGUILayout.TextField(new GUIContent("查找路径"), path);
        if (target)
        {
            if (GUILayout.Button("开始查找"))
            {
                results.Clear();
                Search();
                Debug.Log($"查找结束，共找到个 {results.Count} 结果");
            }
            if (results.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("清空"))
                {
                    target = null;
                    results.Clear();
                }
                EditorGUILayout.LabelField("查找结果：", $"共 {results.Count} 个");
                EditorGUILayout.EndHorizontal();
                GUI.enabled = false;
                for (int i = 0; i < results.Count; i++)
                {
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.ObjectField(new GUIContent(string.Empty), results[i].linkedObject, typeof(ScriptableObject), true);
                    EditorGUILayout.LabelField($"引用链：this{results[i].linkedField}");
                    EditorGUILayout.LabelField($"路径：{AssetDatabase.GetAssetPath(results[i].linkedObject)}");
                    EditorGUILayout.EndVertical();
                }
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.LabelField("暂无查找结果");
            }
        }
        GUILayout.EndScrollView();
    }

    private void Search()
    {
        var objects = Resources.LoadAll<ScriptableObject>(path).Where(x => x != target);
        foreach (var obj in objects)
        {
            string field = string.Empty;
            if (ShouldTake(obj, depth, ref field))
            {
                if (!results.Exists(x => x.linkedObject == obj))
                    results.Add(new SearchResult(obj, field));
            }
        }
    }

    private bool ShouldTake(object scanObject, int depth, ref string fieldName)
    {
        if (depth < 0) return false;
        if (scanObject == null) return false;
        Type type = scanObject.GetType();
        if (target.GetType() == typeof(GameObject) && scanObject is Component)
        {
            if ((scanObject as Component).gameObject == target as GameObject)
                return true;
        }
        else if (type == target.GetType() || target.GetType().IsAssignableFrom(type))
            return scanObject as UnityEngine.Object == target;
        else if (type.IsValueType || type == typeof(string))
            return false;
        else if (type.IsArray || type.GetInterfaces().Contains(typeof(IList)))
        {
            var sEnum = (scanObject as IEnumerable).GetEnumerator();
            int index = 0;
            while (sEnum.MoveNext())
            {
                if (ShouldTake(sEnum.Current, depth - 1, ref fieldName))
                {
                    fieldName = $"[{index}]{fieldName}";
                    return true;
                }
                index++;
            }
        }
        else if (type.IsClass)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (ShouldTake(field.GetValue(scanObject), depth - 1, ref fieldName))
                {
                    fieldName = $".{field.Name}{fieldName}";
                    return true;
                }
            }
        }
        return false;
    }

    private class SearchResult
    {
        public readonly ScriptableObject linkedObject;
        public readonly string linkedField;

        public SearchResult(ScriptableObject obj, string field)
        {
            linkedObject = obj;
            linkedField = field;
        }
    }
}