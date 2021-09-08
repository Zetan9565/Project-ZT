using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ZetanEditorUtility
{
    public static string GetDirectoryName(Object target)
    {
        return GetDirectoryName(AssetDatabase.GetAssetPath(target));
    }

    public static string GetDirectoryName(string path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            return Path.GetDirectoryName(path);
        else return string.Empty;
    }

    public static bool IsValidPath(string path)
    {
        return path.Contains(Application.dataPath);
    }

    public static string ConvertToAssetsPath(string path)
    {
        return path.Replace(Application.dataPath, "Assets");
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public static object GetValue(SerializedProperty property)
    {
        object value = default;
        var onwerType = property.serializedObject.targetObject.GetType();
        var field = onwerType.GetField(property.propertyPath);
        if (field != null) value = field.GetValue(property.serializedObject.targetObject);
        return value;
    }

    public static object GetValue(SerializedProperty property, out System.Reflection.FieldInfo fieldInfo)
    {
        object value = default;
        var onwerType = property.serializedObject.targetObject.GetType();
        fieldInfo = onwerType.GetField(property.propertyPath);
        if (fieldInfo != null) value = fieldInfo.GetValue(property.serializedObject.targetObject);
        return value;
    }

    public static void SetValue(SerializedProperty property, object value)
    {
        var onwerType = property.serializedObject.targetObject.GetType();
        var fieldInfo = onwerType.GetField(property.propertyPath);
        if (fieldInfo != null) fieldInfo.SetValue(property.serializedObject.targetObject, value);
    }

    public static string TrimContentByKey(string input, string key, int length)
    {
        string output;
        int cut = (length - key.Length) / 2;
        int index = input.IndexOf(key);
        int start = index - cut;
        int end = index + key.Length + cut;
        while (start < 0)
        {
            start++;
            if (end < input.Length - 1) end++;
        }
        while (end > input.Length - 1)
        {
            end--;
            if (start > 0) start--;
        }
        start = start < 0 ? 0 : start;
        end = end > input.Length - 1 ? input.Length - 1 : end;
        int len = end - start + 1;
        output = input.Substring(start, Mathf.Min(len, input.Length - start));
        index = output.IndexOf(key);
        output = output.Insert(index, "<").Insert(index + 1 + key.Length, ">");
        return output;
    }

    public static List<T> LoadAssets<T>() where T : Object
    {
        string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new List<T>();
        foreach (var assetId in assetIds)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetId);
            try
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            catch
            {
                Debug.LogWarning($"找不到路径：{path}");
            }
        }
        return assets;
    }
}