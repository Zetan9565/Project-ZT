using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

    /// <summary>
    /// 获取SerializedProperty关联字段的值，该字段必须是SerializedProperty.serializedObject.targetObject的顶级成员
    /// </summary>
    public static object GetValue(SerializedProperty property)
    {
        object value = default;
        if (property.serializedObject.targetObject)
        {
            var onwerType = property.serializedObject.targetObject.GetType();
            var fieldInfo = onwerType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null) value = fieldInfo.GetValue(property.serializedObject.targetObject);
        }
        return value;
    }

    /// <summary>
    /// 获取SerializedProperty关联字段的值，该字段必须是SerializedProperty.serializedObject.targetObject的顶级成员
    /// </summary>
    /// <param name="property">SerializedProperty</param>
    /// <param name="fieldInfo">字段信息，找不到关联字段时是null</param>
    /// <returns>获取到的字段值</returns>
    public static object GetValue(SerializedProperty property, out FieldInfo fieldInfo)
    {
        object value = default;
        fieldInfo = null;
        if (property.serializedObject.targetObject)
        {
            var onwerType = property.serializedObject.targetObject.GetType();
            fieldInfo = onwerType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null) value = fieldInfo.GetValue(property.serializedObject.targetObject);
        }
        return value;
    }

    public static bool GetFieldValue(string path, object target, out object value, out FieldInfo fieldInfo)
    {
        value = default;
        fieldInfo = null;
        string[] fields = path.Split('.');
        object fv = target;
        var fType = fv.GetType();
        for (int i = 0; i < fields.Length; i++)
        {
            fieldInfo = fType.GetField(fields[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null)
            {
                fv = fieldInfo.GetValue(fv);
                if (fv != null) fType = fv.GetType();
            }
            else return false;
        }
        if (fieldInfo != null)
        {
            value = fv;
            return true;
        }
        else return false;
    }

    /// <summary>
    /// 设置SerializedProperty关联字段的值，该字段必须是SerializedProperty.serializedObject.targetObject的顶级成员
    /// </summary>
    /// <returns>是否成功</returns>
    public static bool SetValue(SerializedProperty property, object value)
    {
        var onwerType = property.serializedObject.targetObject.GetType();
        var fieldInfo = onwerType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (fieldInfo != null)
        {
            try
            {
                fieldInfo.SetValue(property.serializedObject.targetObject, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        else return false;
    }

    /// <summary>
    /// 根据关键字截取给定内容中的一段
    /// </summary>
    /// <param name="input">内容</param>
    /// <param name="key">关键字</param>
    /// <param name="length">截取长度</param>
    /// <returns>截取到的内容</returns>
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

    /// <summary>
    /// 加载所有T类型的资源
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object类型</typeparam>
    /// <returns>找到的资源</returns>
    public static List<T> LoadAssets<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new List<T>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
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

    /// <summary>
    /// 加载第一个T类型的资源
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object类型</typeparam>
    /// <returns>找到的资源</returns>
    public static T LoadAsset<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new List<T>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            try
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            catch
            {
                Debug.LogWarning($"找不到路径：{path}");
            }
            if (assets.Count > 0) return assets[0];
        }
        return null;
    }

    public static T SaveFilePanel<T>(Func<T> instantiate, string assetName, bool ping = false, bool select = false, string title = "选择保存文件夹", string extension = "asset") where T : Object
    {
        while (true)
        {
            string path = EditorUtility.SaveFilePanel(title, string.Empty, assetName, extension);
            if (!string.IsNullOrEmpty(path))
            {
                if (IsValidPath(path))
                {
                    try
                    {
                        T obj = instantiate();
                        AssetDatabase.CreateAsset(obj, ConvertToAssetsPath(path));
                        AssetDatabase.SaveAssets();
                        if (select) Selection.activeObject = obj;
                        if (ping) EditorGUIUtility.PingObject(obj);
                        return obj;
                    }
                    catch
                    {
                        if (!EditorUtility.DisplayDialog("保存失败", "请检查路径或者资源的有效性。", "确定", "取消"))
                            return null;
                    }
                }
                else
                {
                    if (!EditorUtility.DisplayDialog("提示", "请选择Assets目录或以下的文件夹。", "确定", "取消"))
                        return null;
                }
            }
            return null;
        }
    }
}