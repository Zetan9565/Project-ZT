using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ZetanStudio.Extension
{
    public static class IEnumerableExtension
    {
        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            if (source == null || source is ISet<T>) return -1;
            if (source is IList<T> gList) return gList.IndexOf(item);
            if (source is IList list) return list.IndexOf(item);
            int index = 0;
            foreach (var temp in source)
            {
                if (Equals(temp, item)) return index;
                index++;
            }
            return -1;
        }
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null || predicate == null || source is ISet<T>) return -1;
            if (source is List<T> list) return list.FindIndex(predicate);
            if (source is T[] array) return Array.FindIndex(array, predicate);
            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item)) return index;
                index++;
            }
            return -1;
        }
        public static bool IsSubsetOf<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source == null || other == null) return false;
            return source.Except(other).Any();
        }
        public static bool ExistsDuplicate<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Any(g => g.Count() > 1);
        }
        public static bool None<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return !source.Any(predicate);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action?.Invoke(item);
            }
        }
    }

    public static class TransformExtension
    {
        public static Transform CreateChild(this Transform source, params Type[] components)
        {
            return source.CreateChild(null, components);
        }
        public static Transform CreateChild(this Transform source, string name, params Type[] components)
        {
            if (string.IsNullOrEmpty(name)) name = $"Child ({source.transform.childCount})";
            GameObject child = new GameObject(name, components);
            child.transform.SetParent(source, false);
            return child.transform;
        }

        /// <summary>
        /// 查找子对象，没有则创建
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n">名字</param>
        /// <returns>子对象（注意：当子对象的<see cref="Transform"/>被替换成<see cref="RectTransform"/>时，会销毁此处返回的<see cref="Transform"/>）</returns>
        public static Transform FindOrCreate(this Transform source, string n)
        {
            var child = source.Find(n);
            return child != null ? child : source.CreateChild(n);
        }
    }

    public static class RectTransformExtension
    {
        /// <summary>
        /// 查找子对象，没有则创建
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n">名字</param>
        /// <returns>子对象</returns>
        public static RectTransform FindOrCreate(this RectTransform source, string n)
        {
            var child = source.Find(n);
            return child != null ? child.GetOrAddComponent<RectTransform>() : source.CreateChild(n).GetOrAddComponent<RectTransform>();
        }
    }

    public static class ComponentExtension
    {
        public static RectTransform GetRectTransform(this Component source)
        {
            return source.GetComponent<RectTransform>();
        }

        public static T AddComponent<T>(this Component source) where T : Component
        {
            return source.gameObject.AddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this Component source) where T : Component
        {
            var comp = source.GetComponent<T>();
            return comp != null ? comp : source.gameObject.AddComponent<T>();
        }

        public static Component GetComponentInFamily(this Component source, Type type)
        {
            Component component = source.GetComponentInParent(type);
            if (Equals(component, null)) component = source.GetComponentInChildren(type);
            return component;
        }
        public static T GetComponentInFamily<T>(this Component source) where T : Component
        {
            T component = source.GetComponentInParent<T>();
            if (Equals(component, null)) component = source.GetComponentInChildren<T>();
            return component;
        }

        public static T[] GetComponentsInChildrenInOrder<T>(this Component source) where T : Component
        {
            List<T> finds = new List<T>();
            for (int i = 0; i < source.transform.childCount; i++)
            {
                if (source.transform.GetChild(i).TryGetComponent<T>(out var c)) finds.Add(c);
            }
            return finds.ToArray();
        }

        public static string GetPath(this Component source)
        {
            StringBuilder sb = new StringBuilder();
            Transform parent = source.transform.parent;
            while (parent)
            {
                sb.Append(parent.gameObject.name);
                sb.Append("/");
                parent = parent.parent;
            }
            sb.Append(source.gameObject.name);
            return sb.ToString();
        }
    }

    public static class GameObjectExtension
    {
        public static RectTransform GetRectTransform(this GameObject source)
        {
            return source.GetComponent<RectTransform>();
        }
        public static T GetOrAddComponent<T>(this GameObject source) where T : Component
        {
            var comp = source.GetComponent<T>();
            return comp != null ? comp : source.AddComponent<T>();
        }
        public static Component GetComponentInFamily(this GameObject source, Type type)
        {
            Component component = source.GetComponentInParent(type);
            if (Equals(component, null)) component = source.GetComponentInChildren(type);
            return component;
        }
        public static T GetComponentInFamily<T>(this GameObject source) where T : Component
        {
            T component = source.GetComponentInParent<T>();
            if (Equals(component, null)) component = source.GetComponentInChildren<T>();
            return component;
        }

        public static GameObject CreateChild(this GameObject source, string name = null, params Type[] components)
        {
            if (string.IsNullOrEmpty(name)) name = $"Child ({source.transform.childCount})";
            GameObject child = new GameObject(name, components);
            child.transform.SetParent(source.transform, false);
            return child;
        }

        public static void SetActiveEx(this GameObject source, bool value)
        {
            if (!source) return;
            if (source.activeSelf != value) source.SetActive(value);
        }

        public static GameObject Instantiate(this GameObject source)
        {
            return Object.Instantiate(source);
        }
        public static GameObject Instantiate(this GameObject source, Transform parent)
        {
            return Object.Instantiate(source, parent);
        }
        public static GameObject Instantiate(this GameObject source, Transform parent, bool worldPositionStays)
        {
            return Object.Instantiate(source, parent, worldPositionStays);
        }
        public static GameObject Instantiate(this GameObject source, Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(source, position, rotation);
        }
        public static GameObject Instantiate(this GameObject source, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Object.Instantiate(source, position, rotation, parent);
        }

        public static string GetPath(this GameObject source)
        {
            StringBuilder sb = new StringBuilder();
            Transform parent = source.transform.parent;
            Stack<string> parents = new Stack<string>();
            while (parent)
            {
                parents.Push(parent.gameObject.name);
                parent = parent.parent;
            }
            while (parents.Count > 0)
            {
                sb.Append(parents.Pop());
                sb.Append("/");
            }
            sb.Append(source.name);
            return sb.ToString();
        }
    }

    public static class RenderTextureExtension
    {
        public static void Resize(this RenderTexture texture, int width, int height)
        {
            Utility.ResizeRenderTextTure(texture, width, height);
        }
    }

#if UNITY_EDITOR
    namespace Editor
    {
        public static class SerializedObjectExtension
        {
            public static SerializedProperty FindAutoProperty(this SerializedObject obj, string propertyPath)
            {
                return obj.FindProperty($"<{propertyPath}>k__BackingField");
            }
            public static SerializedProperty FindPropertyEx(this SerializedObject obj, string propertyPath)
            {
                return obj.FindProperty(propertyPath) ?? obj.FindAutoProperty(propertyPath);
            }
        }
        public static class SerializedPropertyExtension
        {
            public static SerializedProperty FindAutoProperty(this SerializedProperty prop, string propertyPath)
            {
                return prop.FindPropertyRelative($"<{propertyPath}>k__BackingField");
            }
            public static SerializedProperty FindPropertyEx(this SerializedProperty prop, string propertyPath)
            {
                return prop.FindPropertyRelative(propertyPath) ?? prop.FindAutoProperty(propertyPath);
            }
            public static int GetArrayIndex(this SerializedProperty prop)
            {
                if (int.TryParse(prop.propertyPath.Split('[', ']')[^2], out int index)) return index;
                return -1;
            }
            public static FieldInfo GetFieldInfo(this SerializedProperty prop)
            {
                return Utility.Editor.GetFieldInfo(prop);
            }
            public static Type GetFieldType(this SerializedProperty prop)
            {
                return Utility.Editor.GetFieldType(prop);
            }
            /// <summary>
            /// 获取SerializedProperty关联字段的值
            /// </summary>
            /// <param name="source"><see cref="SerializedProperty"/></param>
            /// <param name="fieldInfo">字段信息，找不到关联字段时是null</param>
            /// <returns>获取到的字段值</returns>
            public static bool TryGetValue(this SerializedProperty source, out object value)
            {
                return Utility.Editor.TryGetValue(source, out value, out _);
            }
            public static bool TryGetValue(this SerializedProperty source, out object value, out FieldInfo fieldInfo)
            {
                return Utility.Editor.TryGetValue(source, out value, out fieldInfo);
            }

            /// <summary>
            /// 设置SerializedProperty关联字段的值
            /// </summary>
            /// <returns>是否成功</returns>
            public static bool TrySetValue(this SerializedProperty source, object value)
            {
                return Utility.Editor.TrySetValue(source, value);
            }

            public static bool TryGetOwnerValue(this SerializedProperty source, out object value)
            {
                return Utility.Editor.TryGetOwnerValue(source, out value);
            }

            public static bool IsRawName(this SerializedProperty source, string name)
            {
                return source.name == name || source.name == $"<{name}>k__BackingField";
            }
            public static string GetRawName(this SerializedProperty source)
            {
                if (Regex.Match(source.name, @"<([\w]+)>k__BackingField") is Match match && match.Success)
                    return match.Groups[1].Value;
                return source.name;
            }

            //public static bool MoveArrayElements(this SerializedProperty source, int[] srcIndices, int dstIndex, out int[] newIndices)
            //{
            //    if (source is null)
            //    {
            //        throw new ArgumentNullException(nameof(source));
            //    }
            //    if (!source.isArray)
            //    {
            //        throw new ArgumentException(nameof(source) + "不是数组");
            //    }
            //    if (srcIndices is null)
            //    {
            //        throw new ArgumentNullException(nameof(srcIndices));
            //    }
            //    newIndices = new int[0];

            //    if (srcIndices.Length < 1) return false;

            //    bool hasMoved = false;
            //    Array.Sort(srcIndices);
            //    int upperCount = 0;
            //    for (int i = 0; i < srcIndices.Length; i++)
            //    {
            //        if (!hasMoved && (srcIndices[i] != dstIndex - 1 || i > 1 && srcIndices[i] != srcIndices[i - 1] + 1))
            //        {
            //            newIndices = new int[srcIndices.Length];
            //            hasMoved = true;
            //        }
            //        if (srcIndices[i] < dstIndex) upperCount++;
            //    }
            //    int tempDstIndex = dstIndex;
            //    for (int i = 0; i < srcIndices.Length; i++)
            //    {
            //        var index = srcIndices[i];
            //        if (index < dstIndex)
            //        {
            //            if (index == tempDstIndex - 1) continue;
            //            Utility.Log(source.MoveArrayElement(index, tempDstIndex - 1), index, tempDstIndex - 1);
            //            for (int j = i + 1; j < srcIndices.Length; j++)
            //            {
            //                if (srcIndices[j] < dstIndex) srcIndices[j]--;
            //            }
            //        }
            //        else if (index > dstIndex)
            //        {
            //            Utility.Log(source.MoveArrayElement(index, tempDstIndex), index, tempDstIndex);
            //            tempDstIndex++;
            //        }
            //        if (hasMoved) newIndices[i] = dstIndex - upperCount + i;
            //    }
            //    return hasMoved;
            //}
        }

        public static class PropertyDrawerExtension
        {
            public static PropertyDrawer GetCustomDrawer(this PropertyDrawer source)
            {
                var drawers = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>().ToArray();
                for (int i = 0; i < drawers.Length; i++)
                {
                    var type = drawers[i];
                    foreach (var attr in type.GetCustomAttributes<CustomPropertyDrawer>())
                    {
                        var child = (bool)typeof(CustomPropertyDrawer).GetField("m_UseForChildren", Utility.CommonBindingFlags).GetValue(attr);
                        var forType = typeof(CustomPropertyDrawer).GetField("m_Type", Utility.CommonBindingFlags).GetValue(attr) as Type;
                        if (forType.Equals(source.fieldInfo.FieldType)) return makeDrawer(type);
                        else if (child && forType.IsAssignableFrom(source.fieldInfo.FieldType))
                        {
                            for (int j = i + 1; j < drawers.Length; j++)
                            {
                                foreach (var attr2 in drawers[j].GetCustomAttributes<CustomPropertyDrawer>())
                                {
                                    var forType2 = typeof(CustomPropertyDrawer).GetField("m_Type", Utility.CommonBindingFlags).GetValue(attr2) as Type;
                                    if (forType2.Equals(source.fieldInfo.FieldType)) return makeDrawer(drawers[j]);
                                }
                            }
                            return makeDrawer(type);
                        }
                    }
                }
                return null;

                PropertyDrawer makeDrawer(Type type)
                {
                    var drawer = Activator.CreateInstance(type) as PropertyDrawer;
                    typeof(PropertyDrawer).GetField("m_FieldInfo", Utility.CommonBindingFlags).SetValue(drawer, source.fieldInfo);
                    return drawer;
                }
            }
            /// <summary>
            /// 尝试获取拥有此成员的对象值。当<paramref name="property"/>位于<see cref="IList"/>中时，返回对应<see cref="IList"/>
            /// </summary>
            /// <returns>是否成功获取</returns>
            public static bool TryGetOwnerValue(this PropertyDrawer source, SerializedProperty property, out object owner)
            {
                owner = default;
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
                                    fieldInfo = temp.GetType().GetField(paths[i][..^$"[{index}]".Length], Utility.CommonBindingFlags);
                                    if (fieldInfo == source.fieldInfo)
                                    {
                                        owner = fieldInfo.GetValue(temp);
                                        return true;
                                    }
                                    temp = (fieldInfo.GetValue(temp) as IList)[index];
                                }
                            }
                            else
                            {
                                fieldInfo = temp.GetType().GetField(paths[i], Utility.CommonBindingFlags);
                                if (fieldInfo == source.fieldInfo)
                                {
                                    owner = temp;
                                    return true;
                                }
                                temp = fieldInfo.GetValue(temp);
                            }
                        }
                    }
                    catch
                    {
                        owner = default;
                        return false;
                    }
                }
                return false;
            }
        }

        public static class VisualElementExtension
        {
            public static void RegisterTooltipCallback(this VisualElement element, Func<string> tooltip)
            {
                element.tooltip = "";
                element.RegisterCallback<TooltipEvent>(e =>
                {
                    if (e.currentTarget == element)
                    {
                        e.rect = element.worldBound;
                        e.tooltip = tooltip?.Invoke();
                        e.StopImmediatePropagation();
                    }
                });
            }
        }
    }
#endif
}