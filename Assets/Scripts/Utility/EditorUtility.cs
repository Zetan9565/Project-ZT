using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace ZetanStudio
{
    public static partial class Utility
    {
#if UNITY_EDITOR
        public static class Editor
        {
            #region 杂项
            public static Texture2D GetIconForObject(Object obj)
            {
                if (!obj) return null;
                return UnityEditorInternal.InternalEditorUtility.GetIconForFile(AssetDatabase.GetAssetPath(obj));
            }

            /// <summary>
            /// 根据关键字截取给定内容中的一段
            /// </summary>
            /// <param name="input">内容</param>
            /// <param name="key">关键字</param>
            /// <param name="length">截取长度</param>
            /// <returns>截取到的内容</returns>
            public static string TrimByKeyword(string input, string key, int length)
            {
                string output;
                if (length > input.Length) length = input.Length;
                int cut = (length - key.Length) / 2;
                int index = input.IndexOf(key);
                if (index < 0) return input;
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
                output = output.Insert(index, "<");
                end = index + 1 + key.Length;
                if (end > output.Length - 1) output += '>';
                else output = output.Insert(end, ">");
                return output;
            }
            public static string HighlightKeyword(string input, string key, int length)
            {
                string output;
                if (length > input.Length) length = input.Length;
                int cut = (length - key.Length) / 2;
                int index = input.IndexOf(key);
                if (index < 0) return input;
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
                output = output.Insert(index, "<b>");
                end = index + 3 + key.Length;
                if (end > output.Length - 1) output += "</b>";
                else output = output.Insert(end, "</b>");
                return output;
            }

            public static void AddScriptingDefineSymbols(string define)
            {
                GetScriptingDefineSymbols(out var defines);
                if (!defines.Contains(define))
                {
                    ArrayUtility.Add(ref defines, define);
                    SetScriptingDefineSymbols(defines);
                }
            }
            public static void RemoveScriptingDefineSymbols(string define)
            {
                GetScriptingDefineSymbols(out var defines);
                if (defines.Contains(define))
                {
                    ArrayUtility.Remove(ref defines, define);
                    SetScriptingDefineSymbols(defines);
                }
            }
            public static void GetScriptingDefineSymbols(out string[] defines)
            {
                var target = (NamedBuildTarget)typeof(NamedBuildTarget).GetMethod("FromActiveSettings", CommonBindingFlags | BindingFlags.Static).Invoke(null, new object[] { EditorUserBuildSettings.activeBuildTarget });
                PlayerSettings.GetScriptingDefineSymbols(target, out defines);
            }
            public static void SetScriptingDefineSymbols(string[] defines)
            {
                var target = (NamedBuildTarget)typeof(NamedBuildTarget).GetMethod("FromActiveSettings", CommonBindingFlags | BindingFlags.Static).Invoke(null, new object[] { EditorUserBuildSettings.activeBuildTarget });
                PlayerSettings.SetScriptingDefineSymbols(target, defines);
            }
            #endregion

            #region 路径相关
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

            public static bool IsValidFolder(string path)
            {
                return path.Contains(Application.dataPath);
            }

            public static string ConvertToAssetsPath(string path)
            {
                return path.Replace(Application.dataPath, "Assets");
            }

            public static string ConvertToWorldPath(string path)
            {
                if (path.StartsWith("Assets/")) path.Remove(0, "Assets/".Length);
                else if (path.StartsWith("Assets")) path.Remove(0, "Assets".Length);
                return Application.dataPath + "/" + path;
            }

            public static string GetAssetFolder(Object asset)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(path)) return null;
                path = path.Remove(path.IndexOf(GetFileName(path)));
                return path.EndsWith('/') ? path.Remove(path.LastIndexOf('/')) : path;
            }

            public static string GetAssetPathWhere<T>(Predicate<T> predicate, string folder = null) where T : Object
            {
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", string.IsNullOrEmpty(folder) ? null : new string[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    try
                    {
                        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                        if (predicate(asset)) return path;
                    }
                    catch
                    {
                        Debug.LogWarning($"找不到路径：{path}");
                    }
                }
                return null;
            }
            #endregion

            #region 序列化相关
            public static FieldInfo GetFieldInfo(SerializedProperty property)
            {
                if (TryGetValue(property, out _, out var field)) return field;
                return null;
            }
            public static Type GetFieldType(SerializedProperty property)
            {
                if (TryGetValue(property, out _, out var field)) return field.FieldType;
                return null;
            }

            /// <summary>
            /// 尝试获取拥有此成员的对象值。当<paramref name="property"/>位于<see cref="IList"/>中时，返回对应<see cref="IList"/>
            /// </summary>
            /// <returns>是否成功获取</returns>
            public static bool TryGetOwnerValue(SerializedProperty property, out object owner)
            {
                owner = null;
                if (property.serializedObject.targetObject)
                {
                    try
                    {
                        string[] paths = property.propertyPath.Replace(".Array.data[", "[").Split('.');
                        object temp = property.serializedObject.targetObject;
                        FieldInfo fieldInfo = null;
                        for (int i = 0; i < paths.Length; i++)
                        {
                            if (paths[i].EndsWith(']'))
                            {
                                if (int.TryParse(paths[i].Split('[', ']')[^2], out var index))
                                {
                                    fieldInfo = temp.GetType().GetField(paths[i][..^$"[{index}]".Length], CommonBindingFlags);
                                    if (i == paths.Length - 1)
                                    {
                                        if (fieldInfo != null)
                                        {
                                            owner = fieldInfo.GetValue(temp);
                                            return true;
                                        }
                                    }
                                    else temp = (fieldInfo.GetValue(temp) as IList)[index];
                                }
                            }
                            else
                            {
                                fieldInfo = temp.GetType().GetField(paths[i], CommonBindingFlags);
                                if (i == paths.Length - 1)
                                {
                                    if (fieldInfo != null)
                                    {
                                        owner = temp;
                                        return true;
                                    }
                                }
                                temp = fieldInfo.GetValue(temp);
                            }
                        }
                    }
                    catch// (Exception ex)
                    {
                        //Debug.LogException(ex);
                        owner = default;
                        return false;
                    }
                }
                return false;
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
            /// <param name="property"><see cref="SerializedProperty"/></param>
            /// <param name="fieldInfo">字段信息，找不到关联字段时是null，若<paramref name="property"/>处于<see cref="IList"/>中，此字段信息指向<see cref="IList"/></param>
            /// <returns>获取到的字段值</returns>
            public static bool TryGetValue(SerializedProperty property, out object value, out FieldInfo fieldInfo)
            {
                value = default;
                fieldInfo = null;
                if (property.serializedObject.targetObject)
                {
                    try
                    {
                        string[] paths = property.propertyPath.Replace(".Array.data[", "[").Split('.');
                        value = property.serializedObject.targetObject;
                        for (int i = 0; i < paths.Length; i++)
                        {
                            if (paths[i].EndsWith(']'))
                            {
                                if (int.TryParse(paths[i].Split('[', ']')[^2], out var index))
                                {
                                    fieldInfo = value.GetType().GetField(paths[i][..^$"[{index}]".Length], CommonBindingFlags);
                                    value = (fieldInfo.GetValue(value) as IList)[index];
                                }
                            }
                            else
                            {
                                fieldInfo = value.GetType().GetField(paths[i], CommonBindingFlags);
                                value = fieldInfo.GetValue(value);
                            }
                        }
                        return fieldInfo != null;
                    }
                    catch// (Exception ex)
                    {
                        //Debug.LogException(ex);
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
                        string[] paths = property.propertyPath.Replace(".Array.data[", "[").Split('.');
                        for (int i = 0; i < paths.Length; i++)
                        {
                            if (paths[i].EndsWith(']'))
                            {
                                if (int.TryParse(paths[i].Split('[', ']')[^2], out var index))
                                {
                                    fieldInfo = temp.GetType().GetField(paths[i][..^$"[{index}]".Length], CommonBindingFlags);
                                    temp = (fieldInfo.GetValue(temp) as IList)[index];
                                }
                            }
                            else
                            {
                                fieldInfo = temp.GetType().GetField(paths[i], CommonBindingFlags);
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
                    catch// (Exception ex)
                    {
                        //Debug.LogException(ex);
                        return false;
                    }
                }
                return false;
            }
            public static void FromJsonOverwrite(string json, object objectToOverwrite)
            {
                EditorJsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            }
            public static string ToJson(object value)
            {
                return EditorJsonUtility.ToJson(value);
            }
            #endregion

            #region 资源相关
            #region 加载
            /// <summary>
            /// 加载所有<typeparamref name="T"/>类型的资源
            /// </summary>
            /// <typeparam name="T">UnityEngine.Object类型</typeparam>
            /// <param name="folder">以Assets开头的指定加载文件夹路径</param>
            /// <returns>找到的资源</returns>
            public static List<T> LoadAssets<T>(string folder = null, string extension = null, bool ignorePackages = true) where T : Object
            {
                return LoadAssetsWhere<T>(null, folder, extension, ignorePackages);
            }
            public static List<T> LoadMainAssets<T>(string folder = null, string extension = null, bool ignorePackages = true) where T : Object
            {
                return LoadAssetsWhere<T>(AssetDatabase.IsMainAsset, folder, extension, ignorePackages);
            }
            public static List<T> LoadMainAssetsWhere<T>(Predicate<T> predicate, string folder = null, string extension = null, bool ignorePackages = true) where T : Object
            {
                return LoadAssetsWhere<T>(x => AssetDatabase.IsMainAsset(x) && (predicate?.Invoke(x) ?? true), folder, extension, ignorePackages);
            }
            public static List<T> LoadAssetsWhere<T>(Predicate<T> predicate, string folder = null, string extension = null, bool ignorePackages = true) where T : Object
            {
                List<string> folders = null;
                if (ignorePackages || !string.IsNullOrEmpty(folder))
                {
                    folders = new List<string>();
                    if (ignorePackages) folders.Add("Assets");
                    if (!string.IsNullOrEmpty(folder)) folders.Add(folder);
                }
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", folders?.ToArray());
                List<T> assets = new List<T>();
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) || (!string.IsNullOrEmpty(extension) && extension.StartsWith('.') && !path.EndsWith(extension))) continue;
                    try
                    {
                        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                        if (asset && (predicate == null || predicate(asset))) assets.Add(asset);
                    }
                    catch
                    {
                        Debug.LogWarning($"找不到路径：{path}");
                    }
                }
                return assets;
            }
            public static List<Object> LoadAssets(Type type, string folder = null, string extension = null, bool ignorePackages = true)
            {
                return LoadAssetsWhere(null, type, folder, extension, ignorePackages);
            }
            public static List<Object> LoadAssetsWhere(Predicate<Object> predicate, Type type, string folder = null, string extension = null, bool ignorePackages = true)
            {
                List<string> folders = null;
                if (ignorePackages || !string.IsNullOrEmpty(folder))
                {
                    folders = new List<string>();
                    if (ignorePackages) folders.Add("Assets");
                    if (!string.IsNullOrEmpty(folder)) folders.Add(folder);
                }
                string[] guids = AssetDatabase.FindAssets($"t:{type.Name}", folders?.ToArray());
                List<Object> assets = new List<Object>();
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) || (!string.IsNullOrEmpty(extension) && extension.StartsWith('.') && !path.EndsWith(extension))) continue;
                    try
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath(path, type);
                        if (asset && (predicate == null || predicate(asset))) assets.Add(asset);
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
            public static T LoadAsset<T>(string folder = null, string extension = null, bool ignorePackages = true) where T : Object
            {
                return LoadAssetWhere<T>(null, folder, extension, ignorePackages);
            }
            public static T LoadAssetWhere<T>(Predicate<T> predicate, string folder = null, string extension = null, bool ignorePackages = true) where T : Object
            {
                List<string> folders = null;
                if (ignorePackages || !string.IsNullOrEmpty(folder))
                {
                    folders = new List<string>();
                    if (ignorePackages) folders.Add("Assets");
                    if (!string.IsNullOrEmpty(folder)) folders.Add(folder);
                }
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", folders?.ToArray());
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) || (!string.IsNullOrEmpty(extension) && extension.StartsWith('.') && !path.EndsWith(extension))) continue;
                    try
                    {
                        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                        if (asset && (predicate == null || predicate(asset))) return asset;
                    }
                    catch
                    {
                        Debug.LogWarning($"找不到路径：{path}");
                    }
                }
                return null;
            }
            public static Object LoadAsset(Type type, string folder = null, string extension = null, bool ignorePackages = true)
            {
                return LoadAssetWhere(null, type, folder, extension, ignorePackages);
            }
            public static Object LoadAssetWhere(Predicate<Object> predicate, Type type, string folder = null, string extension = null, bool ignorePackages = true)
            {
                List<string> folders = null;
                if (ignorePackages || !string.IsNullOrEmpty(folder))
                {
                    folders = new List<string>();
                    if (ignorePackages) folders.Add("Assets");
                    if (!string.IsNullOrEmpty(folder)) folders.Add(folder);
                }
                string[] guids = AssetDatabase.FindAssets($"t:{type.Name}", folders?.ToArray());
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) || (!string.IsNullOrEmpty(extension) && extension.StartsWith('.') && !path.EndsWith(extension))) continue;
                    try
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath(path, type);
                        if (asset && (predicate == null || predicate(asset))) return asset;
                    }
                    catch
                    {
                        Debug.LogWarning($"找不到路径：{path}");
                    }
                }
                return null;
            }
            #endregion

            public static void SaveChange(Object asset)
            {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
            }

            public static bool IsLocalAssets(Object asset)
            {
                if (asset == null) return false;
                return AssetDatabase.Contains(asset);
            }

            public static T SaveFilePanel<T>(Func<T> creation, string assetName = null, string title = "选择保存位置", string extension = "asset", string folder = null, string root = null, bool ping = false, bool select = false) where T : Object
            {
                while (true)
                {
                    if (string.IsNullOrEmpty(assetName)) assetName = "new " + Regex.Replace(typeof(T).Name, "([a-z])([A-Z])", "$1 $2").ToLower();
                    string path = EditorUtility.SaveFilePanelInProject(title, assetName, extension, null, string.IsNullOrEmpty(folder) ? "Assets" : folder);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (!string.IsNullOrEmpty(root) && !path.Contains($"Assets/{root}"))
                            if (!EditorUtility.DisplayDialog("路径错误", $"请选择Assets/{root}范围内的路径", "继续", "取消"))
                                return null;
                        try
                        {
                            T obj = creation();
                            AssetDatabase.CreateAsset(obj, ConvertToAssetsPath(path));
                            if (select) Selection.activeObject = obj;
                            if (ping) EditorGUIUtility.PingObject(obj);
                            return obj;
                        }
                        catch
                        {
                            if (!EditorUtility.DisplayDialog("保存失败", "请检查路径或者资源的有效性。", "继续", "取消"))
                                return null;
                        }
                    }
                    else return null;
                }
            }
            public static Object SaveFilePanel(Func<Object> creation, string assetName = null, string title = "选择保存位置", string extension = "asset", string folder = null, string root = null, bool ping = false, bool select = false)
            {
                return SaveFilePanel<Object>(creation, assetName, title, extension, folder, root, ping, select);
            }
            public static void SaveFolderPanel(Action<string> callback, string path = null)
            {
                while (true)
                {
                    path = EditorUtility.SaveFolderPanel("选择保存路径", path ?? "Assets", null);
                    if (!string.IsNullOrEmpty(path))
                        if (!IsValidFolder(path))
                        {
                            if (!EditorUtility.DisplayDialog("路径错误", $"请选择Assets范围内的路径", "继续", "取消"))
                                break;
                        }
                        else
                        {
                            path = ConvertToAssetsPath(path);
                            callback?.Invoke(path);
                            break;
                        }
                    else break;
                }
            }
            #endregion

            #region 成员绘制
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
                MinMaxSlider(rect, new GUIContent(label), ref minValue, ref maxValue, minLimit, maxLimit);
            }
            public static void MinMaxSlider(Rect rect, GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
            {
                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height), label);
                EditorGUI.indentLevel = 0;
                minValue = EditorGUI.FloatField(new Rect(rect.x + EditorGUIUtility.labelWidth + 2, rect.y, 40, rect.height), minValue);
                maxValue = EditorGUI.FloatField(new Rect(rect.x + rect.width - 40, rect.y, 40, rect.height), maxValue);
                EditorGUI.MinMaxSlider(new Rect(rect.x + EditorGUIUtility.labelWidth + 45, rect.y, rect.width - EditorGUIUtility.labelWidth - 88, rect.height), ref minValue, ref maxValue, minLimit, maxLimit);
                if (minValue < minLimit) minValue = minLimit;
                if (maxValue > maxLimit) maxValue = maxLimit;
                EditorGUI.indentLevel = indentLevel;
            }

            #endregion

            public static void DrawGizmosCircle(Vector3 center, float radius, Vector3? normal = null, Color? color = null)
            {
#if UNITY_EDITOR
                float delta = radius * 0.001f;
                if (delta < 0.0001f) delta = 0.0001f;
                Color colorBef = Gizmos.color;
                if (color != null) Gizmos.color = color.Value;
                Vector3 firstPoint = Vector3.zero;
                Vector3 fromPoint = Vector3.zero;
                Vector3 yAxis;
                if (normal != null) yAxis = normal.Value.normalized;
                else yAxis = Vector3.forward.normalized;
                Vector3 xAxis = Vector3.ProjectOnPlane(Vector3.right, yAxis).normalized;
                Vector3 zAxis = Vector3.Cross(xAxis, yAxis).normalized;
                for (float perimeter = 0; perimeter < 2 * Mathf.PI; perimeter += delta)
                {
                    Vector3 toPoint = new Vector3(radius * Mathf.Cos(perimeter), 0, radius * Mathf.Sin(perimeter));
                    toPoint = center + toPoint.x * xAxis + toPoint.z * zAxis;
                    if (perimeter == 0) firstPoint = toPoint;
                    else Gizmos.DrawLine(fromPoint, toPoint);
                    fromPoint = toPoint;
                }
                Gizmos.DrawLine(firstPoint, fromPoint);
                Gizmos.color = colorBef;
#endif
            }
            public static void DrawGizmosSector(Vector3 origin, Vector3 direction, float radius, float angle, Vector3? normal = null)
            {
#if UNITY_EDITOR
                Vector3 axis;
                if (normal != null) axis = normal.Value.normalized;
                else axis = Vector3.forward.normalized;
                Vector3 end = (Quaternion.AngleAxis(angle / 2, axis) * direction).normalized;
                Gizmos.DrawLine(origin, origin + end * radius);
                Vector3 from = (Quaternion.AngleAxis(-angle / 2, axis) * direction).normalized;
                Gizmos.DrawLine(origin, origin + from * radius);
                Handles.DrawWireArc(origin, axis, from, angle, radius);
#endif
            }

            public static class Style
            {
                public static GUIStyle middleRight
                {
                    get
                    {
                        GUIStyle style = GUIStyle.none;
                        style.alignment = TextAnchor.MiddleRight;
                        style.normal.textColor = GUI.skin.label.normal.textColor;
                        return style;
                    }
                }
                public static GUIStyle middleCenter
                {
                    get
                    {
                        GUIStyle style = GUIStyle.none;
                        style.alignment = TextAnchor.MiddleCenter;
                        style.normal.textColor = GUI.skin.label.normal.textColor;
                        return style;
                    }
                }
                public static GUIStyle bold
                {
                    get
                    {
                        GUIStyle style = GUIStyle.none;
                        style.normal.textColor = GUI.skin.label.normal.textColor;
                        style.fontStyle = FontStyle.Bold;
                        return style;
                    }
                }

                public static GUIStyle Colorful(Color color)
                {
                    GUIStyle style = GUIStyle.none;
                    style.normal.textColor = color;
                    return style;
                }
            }

            public static class Script
            {
                public struct ScriptTemplate
                {
                    public string fileName;
                    public string folder;
                    public TextAsset templateFile;
                }

                public static void CreateNewScript(ScriptTemplate template)
                {
                    string path = $"{template.folder}";
                    if (path.EndsWith("/")) path = path[0..^1];

                    Object script = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    Selection.activeObject = script;
                    EditorGUIUtility.PingObject(script);

                    string templatePath = AssetDatabase.GetAssetPath(template.templateFile);
                    ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.fileName);
                }
                public static void CreateNewScript(string fileName, string folder, TextAsset templateFile)
                {
                    CreateNewScript(new ScriptTemplate() { fileName = fileName, folder = folder, templateFile = templateFile });
                }
            }
        }
#endif
    }
}
