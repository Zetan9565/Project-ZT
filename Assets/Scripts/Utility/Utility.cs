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

namespace ZetanStudio
{
    using Extension;

    public static partial class Utility
    {
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

        public static Vector2 GetRealSize(RectTransform rectTransform)
        {
            var rect = GetScreenSpaceRect(rectTransform);
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return new Vector2(rect.width, rect.height);
        }

        public static void ResizeRenderTextTure(RenderTexture texture, int width, int height)
        {
            if (!texture) return;
            texture.Release();
            texture.width = width;
            texture.height = height;
            texture.Create();
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