using System.IO;
using UnityEditor;
using UnityEngine;

public static class ZetanEditorUtility
{
    public static string GetDirectoryName(Object target)
    {
        string path = AssetDatabase.GetAssetPath(target);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            return Path.GetDirectoryName(path);
        return path;
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
}