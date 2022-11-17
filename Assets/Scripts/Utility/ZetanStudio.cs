using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
#endif

namespace ZetanStudio
{
    using Extension;

    public static class Utility
    {
        #region 通用
        #region 杂项
        public static Scene GetActiveScene() => SceneManager.GetActiveScene();

        /// <summary>
        /// 概率计算
        /// </summary>
        /// <param name="probability">概率(0~1)</param>
        /// <returns>是否命中</returns>
        public static bool Probability(float probability) => probability > 0f && Random.Range(0f, 1f) <= probability;
        public static T LoadResource<T>() where T : Object
        {
            try
            {
                return Resources.LoadAll<T>("")[0];
            }
            catch
            {
                return null;
            }
        }
        public static Object LoadResource(Type type)
        {
            try
            {
                return Resources.LoadAll("", type)[0];
            }
            catch
            {
                return null;
            }
        }
        public static int CompareStringNumbericSuffix(string x, string y)
        {
            var m1 = Regex.Match(x, @"(\w+) *(\d+)");
            var m2 = Regex.Match(y, @"(\w+) *(\d+)");
            if (m1.Success && m2.Success)
            {
                var pre1 = m1.Groups[1].Value;
                var pre2 = m2.Groups[1].Value;
                if (pre1 != pre2) return string.Compare(x, y);
                var num1 = int.Parse(m1.Groups[2].Value);
                var num2 = int.Parse(m2.Groups[2].Value);
                if (num1 < num2) return -1;
                else if (num1 > num2) return 1;
                else return 0;
            }
            else return string.Compare(x, y);
        }

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

        public static void Stopwatch(Action action)
        {
            if (action == null) throw new ArgumentNullException();
            System.Diagnostics.Stopwatch sw = new();
            sw.Start(); action?.Invoke(); sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + " ms");
        }
        public static T Stopwatch<T>(Func<T> action)
        {
            if (action == null) throw new ArgumentNullException();
            System.Diagnostics.Stopwatch sw = new();
            sw.Start(); var @return = action.Invoke(); sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + " ms");
            return @return;
        }

        public static IList<T> RandomOrder<T>(IList<T> list)
        {
            var result = Activator.CreateInstance(list.GetType()) as IList<T>;
            var indices = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                indices.Add(i);
            }
            while (result.Count < list.Count)
            {
                var index = Random.Range(0, indices.Count);
                if (index < 0 || index >= indices.Count) break;
                result.Add(list[indices[index]]);
                indices.RemoveAt(index);
            }
            return result;
        }
        public static T[] RandomOrder<T>(T[] array)
        {
            var result = new T[array.Length];
            var indices = new List<int>();
            for (int i = 0; i < array.Length; i++)
            {
                indices.Add(i);
            }
            int take = 0;
            while (take < array.Length)
            {
                var index = Random.Range(0, indices.Count);
                if (index < 0 || index >= indices.Count) break;
                result[take] = array[indices[index]];
                indices.RemoveAt(index);
                take++;
            }
            return result;
        }
        #endregion

        #region 文本
        public static string ColorText(string text, Color color)
        {
            if (!color.Equals(Color.clear)) return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
            else return text;
        }
        public static string ColorText(object content, Color color)
        {
            if (!color.Equals(Color.clear)) return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), content.ToString());
            else return content.ToString();
        }
        public static string BoldText(string text) => $"<b>{text}</b>";
        public static string ItalicText(string text) => $"<i>{text}</i>";
        public static string RemoveTags(string text)
        {
            return Regex.Replace(text,
                @"<color=?>|<color=[a-z]+>|<color=""[a-z]+"">|<color='[a-z]+'>|<color=#[a-f\d]{6}>|<color=#[a-f\d]{8}>|<\/color>|<size>|<size=\d{0,3}>|<\/size>|<b>|<\/b>|<i>|<\/i>",
                "", RegexOptions.IgnoreCase);
        }
        #endregion

        #region 游戏对象
        public static bool IsPrefab(GameObject gameObject)
        {
            if (!gameObject) return false;
            return gameObject.scene.name == null;
        }

        public static bool IsDontDestroyOnLoad(GameObject gameObject)
        {
            if (!gameObject) return false;
            return gameObject.scene.name == "DontDestroyOnLoad";
        }
        public static bool IsDontDestroyOnLoad(Component component)
        {
            if (!component) return false;
            return component.gameObject.scene.name == "DontDestroyOnLoad";
        }

        public static Transform CreateChild(Transform parent, string name = null)
        {
            return parent.CreateChild(name);
        }
        public static GameObject CreateChild(GameObject parent, string name = null)
        {
            return parent.CreateChild(name);
        }

        public static void SetActive(GameObject gameObject, bool value)
        {
            if (!gameObject) return;
            if (gameObject.activeSelf != value) gameObject.SetActive(value);
        }
        public static void SetActive(Component component, bool value)
        {
            if (!component) return;
            SetActive(component.gameObject, value);
        }
        #endregion

        #region 屏幕相关
        public static bool IsMouseInsideScreen()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
            return mousePosition.x >= 0 && mousePosition.x <= Screen.width && mousePosition.y >= 0 && mousePosition.y <= Screen.height;
#else
                return Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width && Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height;
#endif
        }
        public static bool IsPointInsideScreen(Vector3 worldPosition)
        {
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPosition);
            return viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
        }

        public static Rect GetScreenSpaceRect(RectTransform rectTransform)
        {
            Vector2 size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
            float x = rectTransform.position.x + rectTransform.anchoredPosition.x;
            float y = Screen.height - (rectTransform.position.y - rectTransform.anchoredPosition.y);
            return new Rect(x, y, size.x, size.y);
        }

        public static void KeepInsideScreen(RectTransform rectTransform, bool left = true, bool right = true, bool top = true, bool bottom = true)
        {
            Rect repairedRect = GetScreenSpaceRect(rectTransform);
            float leftWidth = rectTransform.pivot.x * repairedRect.width;
            float rightWidth = repairedRect.width - leftWidth;
            float bottomHeight = rectTransform.pivot.y * repairedRect.height;
            float topHeight = repairedRect.height - bottomHeight;
            if (left && rectTransform.position.x - leftWidth < 0) rectTransform.position += Vector3.right * (leftWidth - rectTransform.position.x);
            if (right && rectTransform.position.x + rightWidth > Screen.width) rectTransform.position -= Vector3.right * (rectTransform.position.x + rightWidth - Screen.width);
            if (bottom && rectTransform.position.y - bottomHeight < 0) rectTransform.position += Vector3.up * (bottomHeight - rectTransform.position.y);
            if (top && rectTransform.position.y + topHeight > Screen.height) rectTransform.position -= Vector3.up * (rectTransform.position.y + topHeight - Screen.height);//保证顶部显示
        }
        #endregion

        #region 反射相关
        public static T Clone<T>(T value)
        {
            if (typeof(T).GetCustomAttribute<SerializableAttribute>(true) is null)
                throw new ArgumentException($"类型{typeof(T)}不受支持，因为它不含特性[Serializable]");
            using var ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, value);
            ms.Position = 0;
            return (T)bf.Deserialize(ms);
        }
        private static string SerializeObject(object target, bool includeProperty, int depth, int indentation)
        {
            if (target == null) return $"Null";
            Type type = target.GetType();
            if (type.IsEnum) return $"{type}.{target}";
            if (IsNumericType(type) || type == typeof(bool)) return target.ToString();
            StringBuilder sb = new StringBuilder();
            if (type == typeof(string))
            {
                sb.Append("\"");
                sb.Append(target);
                sb.Append("\"");
            }
            else if (depth < 0) return target.ToString();
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (!typeof(IDictionary).IsAssignableFrom(type))
                {
                    bool canIndex = type.IsArray || typeof(IList).IsAssignableFrom(type);
                    var tEnum = (target as IEnumerable).GetEnumerator();
                    sb.Append("{\n");
                    int index = 0;
                    while (tEnum.MoveNext())
                    {
                        for (int i = 0; i < indentation + 1; i++)
                        {
                            sb.Append("    ");
                        }
                        sb.Append("[");
                        sb.Append(canIndex ? index.ToString() : "?");
                        sb.Append("] = ");
                        sb.Append(SerializeObject(tEnum.Current, includeProperty, depth - 1, indentation + 1));
                        sb.Append(",\n");
                        index++;
                    }
                    for (int i = 0; i < indentation; i++)
                    {
                        sb.Append("    ");
                    }
                    sb.Append("}");
                }
                else
                {
                    var dict = target as IDictionary;
                    sb.Append("{\n");
                    foreach (var key in dict.Keys)
                    {
                        for (int i = 0; i < indentation + 1; i++)
                        {
                            sb.Append("    ");
                        }
                        sb.Append('[');
                        sb.Append(SerializeObject(key, includeProperty, depth - 1, indentation + 1));
                        sb.Append("] = ");
                        sb.Append(SerializeObject(dict[key], includeProperty, depth - 1, indentation + 1));
                        sb.Append(",\n");
                    }
                    for (int i = 0; i < indentation; i++)
                    {
                        sb.Append("    ");
                    }
                    sb.Append("}");
                }
            }
            else
            {
                sb.Append("{\n");
                foreach (var field in type.GetFields(CommonBindingFlags))
                {
                    try
                    {
                        for (int i = 0; i < indentation + 1; i++)
                        {
                            sb.Append("    ");
                        }
                        sb.Append("[");
                        sb.Append(field.Name);
                        sb.Append("] = ");
                        sb.Append(SerializeObject(field.GetValue(target), includeProperty, depth - 1, indentation + 1));
                        sb.Append(",\n");
                    }
                    catch
                    {
                        sb.Append("?<access denied>");
                        sb.Append(",\n");
                        continue;
                    }
                }
                if (includeProperty)
                    foreach (var property in type.GetProperties(CommonBindingFlags))
                    {
                        try
                        {
                            for (int i = 0; i < indentation + 1; i++)
                            {
                                sb.Append("    ");
                            }
                            sb.Append("<");
                            sb.Append(property.Name);
                            sb.Append("> = ");
                            sb.Append(SerializeObject(property.GetValue(target), includeProperty, depth - 1, indentation + 1));
                            sb.Append(",\n");
                        }
                        catch
                        {
                            sb.Append("?<access denied>");
                            sb.Append(",\n");
                            continue;
                        }
                    }
                for (int i = 0; i < indentation; i++)
                {
                    sb.Append("    ");
                }
                sb.Append("}");
            }
            return sb.ToString();

            static bool IsNumericType(Type type)
            {
                if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Vector2Int) || type == typeof(Vector3Int))
                    return true;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        return true;
                    case TypeCode.Object:
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                            return IsNumericType(Nullable.GetUnderlyingType(type));
                        return false;
                    default:
                        return false;
                }
            }
        }
        public static string SerializeObject(object target, bool includeProperty = false, int depth = 3)
        {
            return SerializeObject(target, includeProperty, depth, 0);
        }
        public static string SerializeObject(IEnumerable<string> target)
        {
            return SerializeObject(target, false, 1, 0);
        }
        public static string SerializeObject(IEnumerable<int> target)
        {
            return SerializeObject(target, false, 1, 0);
        }
        public static string SerializeObject(IEnumerable<float> target)
        {
            return SerializeObject(target, false, 1, 0);
        }
        public static string SerializeObject(IEnumerable<bool> target)
        {
            return SerializeObject(target, false, 1, 0);
        }

        public static string GetMemberName<T>(Expression<Func<T>> memberAccessor)
        {
            try
            {
                return ((MemberExpression)memberAccessor.Body).Member.Name;
            }
            catch
            {
                return null;
            }
        }
        public static MemberInfo GetMemberInfo<T>(Expression<Func<T>> memberAccessor)
        {
            try
            {
                return ((MemberExpression)memberAccessor.Body).Member;
            }
            catch
            {
                return null;
            }
        }

        public static bool TryGetValue(string path, object target, out object value, out MemberInfo memberInfo)
        {
            value = default;
            memberInfo = null;
            string[] fields = path.Split('.');
            object mv = target;
            if (mv == null) return false;
            var mType = mv.GetType();
            for (int i = 0; i < fields.Length; i++)
            {
                memberInfo = mType?.GetField(fields[i], CommonBindingFlags);
                if (memberInfo is FieldInfo field)
                {
                    mv = field.GetValue(mv);
                    mType = mv?.GetType();
                }
                else
                {
                    memberInfo = mType?.GetProperty(fields[i], CommonBindingFlags);
                    if (memberInfo is PropertyInfo property)
                    {
                        mv = property.GetValue(mv);
                        mType = mv?.GetType();
                    }
                    else return false;
                }
            }
            if (memberInfo != null)
            {
                value = mv;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 通过类名获取类<see cref="Type"/>
        /// </summary>
        /// <param name="name">类名，含命名空间</param>
        public static Type GetTypeWithoutAssembly(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Assembly.GetCallingAssembly().GetType(name) is Type type) return type;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("Assembly-CSharp") || x.FullName.StartsWith("ZetanStudio")))
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.Name == name)
                        return t;
                }
            }
            return null;
        }

        public static Type GetTypeByFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;
            fullName = fullName.Split(',')[0];
            return AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType(fullName)).FirstOrDefault(x => x != null);
        }

        public static Type[] GetTypesDerivedFrom(Type type)
        {
            List<Type> results = new List<Type>();
            foreach (var t in Assembly.GetCallingAssembly().GetTypes())
            {
                if (t != type && type.IsAssignableFrom(t))
                    results.Add(t);
            }
            return results.ToArray();
        }
        public static Type[] GetTypesDerivedFrom<T>()
        {
            return GetTypesDerivedFrom(typeof(T));
        }
        public static Type[] GetTypesWithAttribute(Type attribute)
        {
            List<Type> results = new List<Type>();
            if (!typeof(Attribute).IsAssignableFrom(attribute)) return results.ToArray();
            foreach (var t in Assembly.GetCallingAssembly().GetTypes())
            {
                if (t.GetCustomAttribute(attribute) != null)
                    results.Add(t);
            }
            return results.ToArray();
        }
        public static Type[] GetTypesWithAttribute<T>() where T : Attribute
        {
            return GetTypesWithAttribute(typeof(T));
        }
        public static MethodInfo[] GetMethodsWithAttribute(Type attribute)
        {
            List<MethodInfo> results = new List<MethodInfo>();
            if (!typeof(Attribute).IsAssignableFrom(attribute)) return results.ToArray();
            foreach (var t in Assembly.GetCallingAssembly().GetTypes())
            {
                foreach (var method in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (method.GetCustomAttribute(attribute) != null)
                        results.Add(method);
                }
            }
            return results.ToArray();
        }
        public static MethodInfo[] GetMethodsWithAttribute<T>() where T : Attribute
        {
            return GetMethodsWithAttribute(typeof(T));
        }

        public static string GetInspectorName(Enum enumValue)
        {
            if (enumValue != null)
            {
                MemberInfo[] mi = enumValue.GetType().GetMember(enumValue.ToString());
                if (mi != null && mi.Length > 0)
                {
                    if (Attribute.GetCustomAttribute(mi[0], typeof(InspectorNameAttribute)) is InspectorNameAttribute attr)
                    {
                        return attr.displayName;
                    }
                }
            }
            return enumValue.ToString();
        }

        public static string[] GetInspectorNames(Type type)
        {
            List<string> names = new List<string>();
            foreach (Enum value in Enum.GetValues(type))
            {
                names.Add(GetInspectorName(value));
            }
            return names.ToArray();
        }

        public static IList CreateListInstance(Type elementType)
        {
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
        }

        public const BindingFlags CommonBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        #endregion

        #region Debug.Log相关
        public static T Log<T>(T message)
        {
            Debug.Log(message);
            return message;
        }
        public static void Log(params object[] messages)
        {
            if (messages != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < messages.Length; i++)
                {
                    sb.Append(messages[i] is null ? "Null" : messages[i]);
                    if (i != messages.Length - 1) sb.Append(", ");
                }
                Debug.Log(sb);
            }
            else Debug.Log("Null");
        }
        public static T LogWarning<T>(T message)
        {
            Debug.LogWarning(message);
            return message;
        }
        public static void LogWarning(params object[] messages)
        {
            if (messages != null)
            {
                StringBuilder sb = new StringBuilder(messages[0]?.ToString());
                for (int i = 1; i < messages.Length; i++)
                {
                    sb.Append(messages[i]);
                    if (i != messages.Length - 1) sb.Append(", ");
                }
                Debug.LogWarning(sb);
            }
            else Debug.LogWarning(messages);
        }
        public static T LogError<T>(T message)
        {
            Debug.LogError(message);
            return message;
        }
        public static void LogError(params object[] messages)
        {
            if (messages != null)
            {
                StringBuilder sb = new StringBuilder(messages[0]?.ToString());
                for (int i = 1; i < messages.Length; i++)
                {
                    sb.Append(messages[i]);
                    if (i != messages.Length - 1) sb.Append(", ");
                }
                Debug.LogError(sb);
            }
            else Debug.LogError(messages);
        }
        #endregion

        #region Vector相关
        public static Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        public static Vector3 MousePositionToWorld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Pointer.current.position.ReadValue());
#else
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
#endif
            }
        }

        public static Vector3 PositionToGrid(Vector3 originalPos, float gridSize = 1.0f, float offset = 1.0f)
        {
            Vector3 newPos = originalPos;
            newPos -= Vector3.one * offset;
            newPos /= gridSize;
            newPos = new Vector3(Mathf.Round(newPos.x), Mathf.Round(newPos.y), Mathf.Round(newPos.z));
            newPos *= gridSize;
            newPos += Vector3.one * offset;
            return newPos;
        }
        public static Vector2 PositionToGrid(Vector2 originalPos, float gridSize = 1.0f, float offset = 1.0f)
        {
            Vector2 newPos = originalPos;
            newPos -= Vector2.one * offset;
            newPos /= gridSize;
            newPos = new Vector2(Mathf.Round(newPos.x), Mathf.Round(newPos.y));
            newPos *= gridSize;
            newPos += Vector2.one * offset;
            return newPos;
        }

        public static float Slope(Vector3 from, Vector3 to)
        {
            float height = Mathf.Abs(from.y - to.y);//高程差
            float length = Vector2.Distance(new Vector2(from.x, from.z), new Vector2(to.x, to.z));//水平差
            return Mathf.Atan(height / length) * Mathf.Rad2Deg;
        }

        public static bool Vector3LessThan(Vector3 v1, Vector3 v2)
        {
            return v1.x < v2.x && v1.y <= v2.y && v1.z <= v2.z || v1.x <= v2.x && v1.y < v2.y && v1.z <= v2.z || v1.x <= v2.x && v1.y <= v2.y && v1.z < v2.z;
        }

        public static bool Vector3LargeThan(Vector3 v1, Vector3 v2)
        {
            return v1.x > v2.x && v1.y >= v2.y && v1.z >= v2.z || v1.x >= v2.x && v1.y > v2.y && v1.z >= v2.z || v1.x >= v2.x && v1.y >= v2.y && v1.z > v2.z;
        }

        public static Vector3 CenterBetween(Vector3 point1, Vector3 point2)
        {
            float x = point1.x - (point1.x - point2.x) * 0.5f;
            float y = point1.y - (point1.y - point2.y) * 0.5f;
            float z = point1.z - (point1.z - point2.z) * 0.5f;
            return new Vector3(x, y, z);
        }
        public static Vector2 CenterBetween(Vector2 point1, Vector2 point2)
        {
            float x = point1.x - (point1.x - point2.x) * 0.5f;
            float y = point1.y - (point1.y - point2.y) * 0.5f;
            return new Vector2(x, y);
        }

        public static Vector3 SizeBetween(Vector3 point1, Vector3 point2)
        {
            return new Vector3(Mathf.Abs(point1.x - point2.x), Mathf.Abs(point1.y - point2.y), Mathf.Abs(point1.z - point2.z));
        }

        public static Vector2 GetVectorFromAngle(float angle)
        {
            float angleRad = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }
        #endregion

        #region 文件和安全
        public static FileStream OpenFile(string path, FileMode fileMode, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            try
            {
                return new FileStream(path, fileMode, fileAccess);
            }
            catch
            {
                return null;
            }
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// 加密字符串，多用于JSON
        /// </summary>
        /// <param name="unencryptText">待加密明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string unencryptText, string key)
        {
            if (key.Length != 32 && key.Length != 16) return unencryptText;
            //密钥
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            //待加密明文数组
            byte[] unencryptBytes = Encoding.UTF8.GetBytes(unencryptText);

            //Rijndael加密算法
            RijndaelManaged rDel = new RijndaelManaged
            {
                Key = keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform cTransform = rDel.CreateEncryptor();

            //返回加密后的密文
            byte[] resultBytes = cTransform.TransformFinalBlock(unencryptBytes, 0, unencryptBytes.Length);
            return Convert.ToBase64String(resultBytes, 0, resultBytes.Length);
        }
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="encrytedText">待解密密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string Decrypt(string encrytedText, string key)
        {
            if (key.Length != 32 && key.Length != 16) return encrytedText;
            //解密密钥
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            //待解密密文数组
            byte[] encryptBytes = Convert.FromBase64String(encrytedText);

            //Rijndael解密算法
            RijndaelManaged rDel = new RijndaelManaged
            {
                Key = keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform cTransform = rDel.CreateDecryptor();

            //返回解密后的明文
            byte[] resultBytes = cTransform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length);
            return Encoding.UTF8.GetString(resultBytes);
        }

        public static MemoryStream Encrypt(Stream unencryptStream, string key)
        {
            if (key.Length != 32 && key.Length != 16) return null;
            if (unencryptStream == null) return null;
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            //加密过程
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, new RijndaelManaged
            {
                Key = keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            }.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] buffer = new byte[1024];
            unencryptStream.Position = 0;
            int bytesRead;
            do
            {
                bytesRead = unencryptStream.Read(buffer, 0, 1024);
                cs.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            cs.FlushFinalBlock();

            byte[] resultBytes = ms.ToArray();
            unencryptStream.SetLength(0);
            unencryptStream.Write(resultBytes, 0, resultBytes.Length);
            return ms;
        }
        public static MemoryStream Decrypt(Stream encryptedStream, string key)
        {
            if (key.Length != 32 && key.Length != 16) return null;
            if (encryptedStream == null) return null;
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            //解密过程
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(encryptedStream, new RijndaelManaged
            {
                Key = keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            }.CreateDecryptor(), CryptoStreamMode.Read);
            byte[] buffer = new byte[1024];
            int bytesRead;
            do
            {
                bytesRead = cs.Read(buffer, 0, 1024);
                ms.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            MemoryStream result = new MemoryStream(ms.GetBuffer());
            return result;
        }

        public static string GetMD5(string fileName)
        {
            try
            {
                using FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        public static bool CompareMD5(string fileName, string md5hashToCompare)
        {
            try
            {
                using FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString() == md5hashToCompare;
            }
            catch
            {
                return false;
            }
        }

        public static string GetMD5(FileStream file)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(file);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        public static bool CompareMD5(FileStream file, string md5hashToCompare)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(file);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString() == md5hashToCompare;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #endregion

        #region Editor
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
        #endregion
    }

    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> equals;
        private readonly Func<T, int> hashCode;

        public EqualityComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            this.equals = equals ?? throw new ArgumentNullException(nameof(equals));
            this.hashCode = hashCode ?? throw new ArgumentNullException(nameof(hashCode));
        }

        public bool Equals(T x, T y)
        {
            return equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return hashCode(obj);
        }
    }
    public class Comparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> comparison;

        public Comparer(Func<T, T, int> comparison)
        {
            this.comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }
    public class NumbericSuffixStringComparer : IComparer<string>
    {
        public int Compare(string x, string y) => Utility.CompareStringNumbericSuffix(x, y);
    }

    public enum UpdateMode
    {
        Update,
        LateUpdate,
        FixedUpdate
    }
}
namespace ZetanStudio.Collections
{
    public class Heap<T> where T : class, IHeapItem<T>
    {
        private readonly T[] items;
        private readonly int maxSize;
        private readonly HeapType heapType;

        public int Count { get; private set; }

        public Heap(int size, HeapType heapType = HeapType.MinHeap)
        {
            items = new T[size];
            maxSize = size;
            this.heapType = heapType;
        }

        public void Add(T item)
        {
            if (Count >= maxSize) return;
            item.HeapIndex = Count;
            items[Count] = item;
            Count++;
            SortUpForm(item);
        }

        public T RemoveRoot()
        {
            if (Count < 1) return default;
            T root = items[0];
            root.HeapIndex = -1;
            Count--;
            if (Count > 0)
            {
                items[0] = items[Count];
                items[0].HeapIndex = 0;
                SortDownFrom(items[0]);
            }
            return root;
        }

        public bool Contains(T item)
        {
            if (item == default || item.HeapIndex < 0 || item.HeapIndex > Count - 1) return false;
            return Equals(items[item.HeapIndex], item);//用items.Contains()就等着哭吧
        }

        public void Clear()
        {
            Count = 0;
        }

        public bool Exists(Predicate<T> predicate)
        {
            return Array.Exists(items, predicate);
        }

        public T[] ToArray()
        {
            return items;
        }

        public List<T> ToList()
        {
            return items.ToList();
        }

        private void SortUpForm(T item)
        {
            int parentIndex = (int)((item.HeapIndex - 1) * 0.5f);
            while (true)
            {
                T parent = items[parentIndex];
                if (Equals(parent, item)) return;
                if (heapType == HeapType.MinHeap ? item.CompareTo(parent) < 0 : item.CompareTo(parent) > 0)
                {
                    if (!Swap(item, parent))
                        return;//交换不成功则退出，防止死循环
                }
                else return;
                parentIndex = (int)((item.HeapIndex - 1) * 0.5f);
            }
        }

        private void SortDownFrom(T item)
        {
            while (true)
            {
                int leftChildIndex = item.HeapIndex * 2 + 1;
                int rightChildIndex = item.HeapIndex * 2 + 2;
                if (leftChildIndex < Count)
                {
                    int swapIndex = leftChildIndex;
                    if (rightChildIndex < Count && (heapType == HeapType.MinHeap ?
                        items[rightChildIndex].CompareTo(items[leftChildIndex]) < 0 : items[rightChildIndex].CompareTo(items[leftChildIndex]) > 0))
                        swapIndex = rightChildIndex;
                    if (heapType == HeapType.MinHeap ? items[swapIndex].CompareTo(item) < 0 : items[swapIndex].CompareTo(item) > 0)
                    {
                        if (!Swap(item, items[swapIndex]))
                            return;//交换不成功则退出，防止死循环
                    }
                    else return;
                }
                else return;
            }
        }

        public void Update()
        {
            if (Count < 1) return;
            SortDownFrom(items[0]);
            SortUpForm(items[Count - 1]);
        }

        private bool Swap(T item1, T item2)
        {
            if (!Contains(item1) || !Contains(item2)) return false;
            items[item1.HeapIndex] = item2;
            items[item2.HeapIndex] = item1;
            int item1Index = item1.HeapIndex;
            item1.HeapIndex = item2.HeapIndex;
            item2.HeapIndex = item1Index;
            return true;
        }

        public static implicit operator bool(Heap<T> self)
        {
            return self != null;
        }

        public enum HeapType
        {
            MinHeap,
            MaxHeap
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }

    public class ReadOnlySet<T> : ISet<T>, IReadOnlyCollection<T>
    {
        private readonly ISet<T> set;

        public int Count => set.Count;

        public bool IsReadOnly => true;

        public ReadOnlySet(ISet<T> set)
        {
            this.set = set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)set).GetEnumerator();
        }

        bool ISet<T>.Add(T item)
        {
            throw new InvalidOperationException("只读");
        }

        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return set.Equals(other);
        }

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException("只读");
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException("只读");
        }

        public bool Contains(T item)
        {
            return set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException("只读");
        }
    }
}
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

            public static bool IsName(this SerializedProperty source, string name)
            {
                return source.name == name || source.name == $"<{name}>k__BackingField";
            }
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
namespace ZetanStudio.Serialization
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class Json
    {
        public static string ToJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });
        }
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });
        }
    }

    public class PloyListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> values = new List<T>();
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    values.Add((T)item.Value.ToObject(Type.GetType(item.Name)));
                }
            }
            catch { }
            return values;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<T>)value;
            var obj = new JObject();
            foreach (var item in list)
            {
                obj.Add(item.GetType().AssemblyQualifiedName, JToken.FromObject(item));
            }
            serializer.Serialize(writer, obj);
        }
    }
    public class PloyArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> values = new List<T>();
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    values.Add((T)item.Value.ToObject(Type.GetType(item.Name)));
                }
            }
            catch { }
            return values.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (T[])value;
            var obj = new JObject();
            foreach (var item in list)
            {
                obj.Add(item.GetType().AssemblyQualifiedName, JToken.FromObject(item));
            }
            serializer.Serialize(writer, obj);
        }
    }
    public class PloyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    return item.Value.ToObject(Type.GetType(item.Name));
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JObject() { { value.GetType().AssemblyQualifiedName, JToken.FromObject(value) } });
        }
    }
}