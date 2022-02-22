using System;
using System.Collections;
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
    /// 获取SerializedProperty关联字段的值
    /// </summary>
    public static bool TryGetValue(SerializedProperty property, out object value)
    {
        return TryGetValue(property, out value, out _);
    }

    /// <summary>
    /// 获取SerializedProperty关联字段的值
    /// </summary>
    /// <param name="property">SerializedProperty</param>
    /// <param name="fieldInfo">字段信息，找不到关联字段时是null</param>
    /// <returns>获取到的字段值</returns>
    public static bool TryGetValue(SerializedProperty property, out object value, out FieldInfo fieldInfo)
    {
        value = default;
        fieldInfo = null;
        if (property.serializedObject.targetObject)
        {
            try
            {
                string[] paths = property.propertyPath.Split('.');
                value = property.serializedObject.targetObject;
                for (int i = 0; i < paths.Length; i++)
                {
                    if (i + 1 < paths.Length - 1 && i + 2 < paths.Length)
                    {
                        if (paths[i + 1] == "Array" && paths[i + 2].StartsWith("data"))
                        {
                            if (int.TryParse(paths[i + 2].Replace("data[", "").Replace("]", ""), out var index))
                            {
                                fieldInfo = value.GetType().GetField(paths[i], ZetanUtility.CommonBindingFlags);
                                value = (fieldInfo.GetValue(value) as IList)[index];
                                i += 2;
                            }
                        }
                    }
                    else
                    {
                        fieldInfo = value.GetType().GetField(paths[i], ZetanUtility.CommonBindingFlags);
                        value = fieldInfo.GetValue(value);
                    }
                }
                return fieldInfo != null;
            }
            catch
            {
                value = default;
                fieldInfo = null;
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 设置SerializedProperty关联字段的值
    /// </summary>
    /// <returns>是否成功</returns>
    public static bool TrySetValue(SerializedProperty property, object value)
    {
        object temp = property.serializedObject.targetObject;
        FieldInfo fieldInfo = null;
        if (temp != null)
        {
            try
            {
                string[] paths = property.propertyPath.Split('.');
                for (int i = 0; i < paths.Length; i++)
                {
                    if (i + 1 < paths.Length - 1 && i + 2 < paths.Length)
                    {
                        if (paths[i + 1] == "Array" && paths[i + 2].StartsWith("data"))
                        {
                            if (int.TryParse(paths[i + 2].Replace("data[", "").Replace("]", ""), out var index))
                            {
                                fieldInfo = temp.GetType().GetField(paths[i], ZetanUtility.CommonBindingFlags);
                                temp = (fieldInfo.GetValue(temp) as IList)[index];
                                i += 2;
                            }
                        }
                    }
                    else
                    {
                        fieldInfo = temp.GetType().GetField(paths[i], ZetanUtility.CommonBindingFlags);
                        if (fieldInfo != null)
                        {
                            if (i < paths.Length - 1)
                                temp = fieldInfo.GetValue(temp);
                        }
                        else break;
                    }
                }
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(temp, value);
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }
        return false;
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
    /// <param name="folder">以Assets开头的指定加载文件夹路径</param>
    /// <returns>找到的资源</returns>
    public static List<T> LoadAssets<T>(string folder = "") where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", string.IsNullOrEmpty(folder) ? null : new string[] { folder });
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

    public static T SaveFilePanel<T>(Func<T> instantiate, string assetName, bool ping = false, bool select = false) where T : Object
    {
        return SaveFilePanel(instantiate, "选择保存位置", assetName, ping, select);
    }
    public static T SaveFilePanel<T>(Func<T> instantiate, string title, string assetName, bool ping = false, bool select = false) where T : Object
    {
        return SaveFilePanel(instantiate, title, assetName, "asset", ping, select);
    }
    public static T SaveFilePanel<T>(Func<T> instantiate, string title, string assetName, string extension, bool ping = false, bool select = false) where T : Object
    {
        while (true)
        {
            string path = EditorUtility.SaveFilePanel(title, Application.dataPath, assetName, extension);
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
                    if (!EditorUtility.DisplayDialog("提示", "请选择Assets目录或子文件夹。", "确定", "取消"))
                        return null;
                }
            }
            return null;
        }
    }

    public static void MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        MinMaxSlider(EditorGUILayout.GetControlRect(), label, ref minValue, ref maxValue, minLimit, maxLimit);
    }
    public static void MinMaxSlider(GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        MinMaxSlider(EditorGUILayout.GetControlRect(), label, ref minValue, ref maxValue, minLimit, maxLimit);
    }
    public static void MinMaxSlider(Rect rect, string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        EditorGUI.LabelField(new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height), label);
        minValue = EditorGUI.FloatField(new Rect(rect.x + EditorGUIUtility.labelWidth + 2, rect.y, 40, rect.height), minValue);
        if (minValue < minLimit) minValue = minLimit;
        maxValue = EditorGUI.FloatField(new Rect(rect.x + rect.width - 40, rect.y, 40, rect.height), maxValue);
        if (maxValue > maxLimit) maxValue = maxLimit;
        EditorGUI.MinMaxSlider(new Rect(rect.x + EditorGUIUtility.labelWidth + 45, rect.y, rect.width - EditorGUIUtility.labelWidth - 88, rect.height), ref minValue, ref maxValue, minLimit, maxLimit);
    }
    public static void MinMaxSlider(Rect rect, GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        EditorGUI.LabelField(new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height), label);
        minValue = EditorGUI.FloatField(new Rect(rect.x + EditorGUIUtility.labelWidth + 2, rect.y, 40, rect.height), minValue);
        if (minValue < minLimit) minValue = minLimit;
        maxValue = EditorGUI.FloatField(new Rect(rect.x + rect.width - 40, rect.y, 40, rect.height), maxValue);
        if (maxValue > maxLimit) maxValue = maxLimit;
        EditorGUI.MinMaxSlider(new Rect(rect.x + EditorGUIUtility.labelWidth + 45, rect.y, rect.width - EditorGUIUtility.labelWidth - 88, rect.height), ref minValue, ref maxValue, minLimit, maxLimit);
    }
}