using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

public class ZetanUtility
{
    public static bool IsMouseInsideScreen => Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width && Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height;

    /// <summary>
    /// 概率计算
    /// </summary>
    /// <param name="probability">百分比数值</param>
    /// <returns>概率命中</returns>
    public static bool Probability(float probability)
    {
        if (probability < 0) return false;
        return UnityEngine.Random.Range(100, 10001) * 0.01f <= probability;
    }

    public static string ColorText(string text, Color color)
    {
        return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
    }

    public static bool IsPrefab(GameObject gameObject)
    {
        return gameObject.scene.name == null;
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

    public static Rect GetScreenSpaceRect(RectTransform rectTransform)
    {
        Vector2 size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
        float x = rectTransform.position.x + rectTransform.anchoredPosition.x;
        float y = Screen.height - (rectTransform.position.y - rectTransform.anchoredPosition.y);
        return new Rect(x, y, size.x, size.y);
    }

    public static void DrawGizmosCircle(Vector3 center, float radius, Vector3 rotateAxis)
    {
        float delta = radius * 0.001f;
        if (delta < 0.0001f) delta = 0.0001f;
        Vector3 firstPoint = Vector3.zero;
        Vector3 fromPoint = Vector3.zero;
        Vector3 yAxis = rotateAxis.normalized;
        Vector3 xAxis = Vector3.ProjectOnPlane(Vector3.right, yAxis).normalized;
        Vector3 zAxis = Vector3.Cross(xAxis, yAxis).normalized;
        for (float perimeter = 0; perimeter < 2 * Mathf.PI; perimeter += delta)
        {
            Vector3 toPoint = new Vector3(radius * Mathf.Cos(perimeter), 0, radius * Mathf.Sin(perimeter));
            toPoint = center + toPoint.x * xAxis + toPoint.y * yAxis + toPoint.z * zAxis;
            if (perimeter == 0) firstPoint = toPoint;
            else Gizmos.DrawLine(fromPoint, toPoint);
            fromPoint = toPoint;
        }
        Gizmos.DrawLine(firstPoint, fromPoint);
    }
    public static void DrawGizmosCircle(Vector3 center, float radius, Vector3 rotateAxis, Color color)
    {
        float delta = radius * 0.001f;
        if (delta < 0.0001f) delta = 0.0001f;
        Color colorBef = Gizmos.color;
        Gizmos.color = color;
        Vector3 firstPoint = Vector3.zero;
        Vector3 fromPoint = Vector3.zero;
        Vector3 yAxis = rotateAxis.normalized;
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
    }

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

    public static void KeepInsideScreen(RectTransform rectTransform, bool left = true, bool right = true, bool top = true, bool bottom = true)
    {
        Rect fixedRect = GetScreenSpaceRect(rectTransform);
        float leftWidth = rectTransform.pivot.x * fixedRect.width;
        float rightWidth = fixedRect.width - leftWidth;
        float bottomHeight = rectTransform.pivot.y * fixedRect.height;
        float topHeight = fixedRect.height - bottomHeight;
        if (left && rectTransform.position.x - leftWidth < 0) rectTransform.position += Vector3.right * (leftWidth - rectTransform.position.x);
        if (right && rectTransform.position.x + rightWidth > Screen.width) rectTransform.position -= Vector3.right * (rectTransform.position.x + rightWidth - Screen.width);
        if (bottom && rectTransform.position.y - bottomHeight < 0) rectTransform.position += Vector3.up * (bottomHeight - rectTransform.position.y);
        if (top && rectTransform.position.y + topHeight > Screen.height) rectTransform.position -= Vector3.up * (rectTransform.position.y + topHeight - Screen.height);//保证顶部显示
    }

    public static bool IsPointInsideScreen(Vector3 worldPosition)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPosition);
        return viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
    }

    public static void FadeTo<T>(float alpha, float duration, IFadeAble<T> fader) where T : MonoBehaviour
    {
        if (fader.FadeCoroutine != null) fader.MonoBehaviour.StopCoroutine(fader.FadeCoroutine);
        fader.FadeCoroutine = fader.MonoBehaviour.StartCoroutine(fader.Fade(alpha, duration));
    }
    public static void ScaleTo<T>(Vector3 scale, float duration, IScaleAble<T> scaler) where T : MonoBehaviour
    {
        if (scaler.ScaleCoroutine != null) scaler.MonoBehaviour.StopCoroutine(scaler.ScaleCoroutine);
        scaler.ScaleCoroutine = scaler.MonoBehaviour.StartCoroutine(scaler.Scale(scale, duration));
    }

    public static string SerializeObject(object target)
    {
        Type type = target.GetType();
        if (!type.IsValueType && target == null) return string.Empty;
        StringBuilder sb = new StringBuilder("[");
        sb.Append(type.Name);
        sb.Append("] = ");
        if (type.IsClass)
        {
            sb.Append("{\n");
            foreach (var fie in type.GetFields())
            {
                sb.Append(SerializeObject(fie.GetValue(target)));
                sb.Append("\n");
            }
            foreach (var pro in type.GetProperties())
            {
                sb.Append(SerializeObject(pro.GetValue(target)));
                sb.Append("\n");
            }
            sb.Append("}");
            return sb.ToString();
        }
        else
        {
            sb.Append(target);
            return sb.ToString();
        }
    }

    #region Vector相关
    public static Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    public static Vector3 MousePositionAsWorld => Camera.main.ScreenToWorldPoint(Input.mousePosition);

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

    public static Vector3 SizeBetween(Vector3 point1, Vector3 point2)
    {
        return new Vector3(Mathf.Abs(point1.x - point2.x), Mathf.Abs(point1.y - point2.y), Mathf.Abs(point1.z - point2.z));
    }
    #endregion

    #region 文件安全相关
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
        RijndaelManaged rDel = new RijndaelManaged
        {
            Key = keyBytes,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        //加密过程
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, cTransform, CryptoStreamMode.Write);
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
        RijndaelManaged rDel = new RijndaelManaged
        {
            Key = keyBytes,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        //解密过程
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(encryptedStream, cTransform, CryptoStreamMode.Read);
        byte[] buffer = new byte[1024];
        int bytesRead;
        do
        {
            bytesRead = cs.Read(buffer, 0, 1024);
            ms.Write(buffer, 0, bytesRead);
        } while (bytesRead > 0);

        //必须这样做，直接返回ms会报错
        MemoryStream results = new MemoryStream(ms.GetBuffer());
        return results;
    }

    public static string GetMD5(string fileName)
    {
        try
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
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
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
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
public enum UpdateMode
{
    Update,
    LateUpdate,
    FixedUpdate
}

[Serializable]
public class ScopeInt
{
    [SerializeField]
    private int min;
    public int Min
    {
        get { return min; }
        set
        {
            if (max < value) min = max;
            else min = value;
            if (min > current) current = min;
        }
    }

    [SerializeField]
    private int max;
    public int Max
    {
        get { return max; }
        set
        {
            if (value < 0) max = 0;
            else if (value < min) max = min + 1;
            else max = value;
            if (max < current) current = max;
        }
    }

    private int current;
    public int Current
    {
        get
        {
            return current;
        }

        set
        {
            if (value > Max) current = Max;
            else if (value < Min) current = Min;
            else current = value;
        }
    }

    public ScopeInt()
    {
        Min = 0;
        Max = 1;
    }

    public ScopeInt(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public ScopeInt(int max)
    {
        Min = 0;
        Max = max;
    }

    public bool IsMax
    {
        get
        {
            return current == Max;
        }
    }

    public bool IsMin
    {
        get
        {
            return current == Min;
        }
    }

    public void ToMin()
    {
        Current = Min;
    }

    public void ToMax()
    {
        Current = Max;
    }

    /// <summary>
    /// 余下的部分
    /// </summary>
    public int Rest { get { return Max - Current; } }
    /// <summary>
    /// 四分之一
    /// </summary>
    public int Quarter { get { return (int)(Max * 0.25f); } }

    public int Half { get { return (int)(Max * 0.5f); } }

    /// <summary>
    /// 四分之三
    /// </summary>
    public int Three_Fourths { get { return (int)(Max * 0.75f); } }

    /// <summary>
    /// 三分之一
    /// </summary>
    public int One_Third { get { return Max / 3; } }

    #region 运算符重载
    #region 加减乘除
    public static int operator +(ScopeInt left, int right)
    {
        return left.Current + right;
    }
    public static int operator -(ScopeInt left, int right)
    {
        return left.Current - right;
    }
    public static float operator +(ScopeInt left, float right)
    {
        return (int)(left.Current + right);
    }
    public static float operator -(ScopeInt left, float right)
    {
        return (int)(left.Current - right);
    }
    public static int operator +(int left, ScopeInt right)
    {
        return left + right.Current;
    }
    public static int operator -(int left, ScopeInt right)
    {
        return left - right.Current;
    }
    public static float operator +(float left, ScopeInt right)
    {
        return left + right.Current;
    }
    public static float operator -(float left, ScopeInt right)
    {
        return left - right.Current;
    }


    public static ScopeInt operator *(ScopeInt left, int right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = left.Current * right };
    }
    public static ScopeInt operator /(ScopeInt left, int right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = left.Current * right };
    }
    public static ScopeInt operator *(ScopeInt left, float right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = (int)(left.Current * right) };
    }
    public static ScopeInt operator /(ScopeInt left, float right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = (int)(left.Current / right) };
    }
    public static int operator *(int left, ScopeInt right)
    {
        return left * right.Current;
    }
    public static int operator /(int left, ScopeInt right)
    {
        return left / right.Current;
    }
    public static float operator *(float left, ScopeInt right)
    {
        return left * right.Current;
    }
    public static float operator /(float left, ScopeInt right)
    {
        return left / right.Current;
    }

    public static ScopeInt operator ++(ScopeInt original)
    {
        original.Current++;
        return original;
    }
    public static ScopeInt operator --(ScopeInt original)
    {
        original.Current--;
        return original;
    }
    #endregion

    #region 大于、小于
    public static bool operator >(ScopeInt left, int right)
    {
        return left.Current > right;
    }
    public static bool operator <(ScopeInt left, int right)
    {
        return left.Current < right;
    }
    public static bool operator >(ScopeInt left, float right)
    {
        return left.Current > right;
    }
    public static bool operator <(ScopeInt left, float right)
    {
        return left.Current < right;
    }
    public static bool operator >(int left, ScopeInt right)
    {
        return left > right.Current;
    }
    public static bool operator <(int left, ScopeInt right)
    {
        return left < right.Current;
    }
    public static bool operator >(float left, ScopeInt right)
    {
        return left > right.Current;
    }
    public static bool operator <(float left, ScopeInt right)
    {
        return left < right.Current;
    }
    #endregion

    #region 大于等于、小于等于
    public static bool operator >=(ScopeInt left, int right)
    {
        return left.Current >= right;
    }
    public static bool operator <=(ScopeInt left, int right)
    {
        return left.Current <= right;
    }
    public static bool operator >=(ScopeInt left, float right)
    {
        return left.Current >= right;
    }
    public static bool operator <=(ScopeInt left, float right)
    {
        return left.Current <= right;
    }
    public static bool operator >=(int left, ScopeInt right)
    {
        return left >= right.Current;
    }
    public static bool operator <=(int left, ScopeInt right)
    {
        return left <= right.Current;
    }
    public static bool operator >=(float left, ScopeInt right)
    {
        return left >= right.Current;
    }
    public static bool operator <=(float left, ScopeInt right)
    {
        return left <= right.Current;
    }
    #endregion

    #region 等于、不等于
    public static bool operator ==(ScopeInt left, int right)
    {
        return left.Current == right;
    }
    public static bool operator !=(ScopeInt left, int right)
    {
        return left.Current != right;
    }
    public static bool operator ==(ScopeInt left, float right)
    {
        return left.Current == right;
    }
    public static bool operator !=(ScopeInt left, float right)
    {
        return left.Current != right;
    }
    public static bool operator ==(int left, ScopeInt right)
    {
        return left == right.Current;
    }
    public static bool operator !=(int left, ScopeInt right)
    {
        return left != right.Current;
    }
    public static bool operator ==(float left, ScopeInt right)
    {
        return left == right.Current;
    }
    public static bool operator !=(float left, ScopeInt right)
    {
        return left != right.Current;
    }
    #endregion

    public static explicit operator float(ScopeInt original)
    {
        return original.Current;
    }
    public static explicit operator int(ScopeInt original)
    {
        return original.Current;
    }
    public static implicit operator ScopeFloat(ScopeInt original)
    {
        return new ScopeFloat(original.Min, original.Max) { Current = original.Current };
    }
    #endregion

    public override string ToString()
    {
        return Current + "/" + Max;
    }

    public string ToString(string format)
    {
        if (format == "//") return Min + "/" + Current + "/" + Max;
        else if (format == "[/]") return "[" + Current + "/" + Max + "]";
        else if (format == "[//]") return "[" + Min + "/" + Current + "/" + Max + "]";
        else if (format == "(/)") return "(" + Current + "/" + Max + ")";
        else if (format == "(//)") return "(" + Min + "/" + Current + "/" + Max + ")";
        else return ToString();
    }

    public string ToString(string start, string split, string end, bool showMin = false)
    {
        if (showMin)
        {
            return start + Min + split + Current + split + Max + end;
        }
        return start + Current + split + Max + end;
    }

    public override bool Equals(object obj)
    {
        return obj is ScopeInt @int &&
               min == @int.min &&
               max == @int.max &&
               current == @int.current;
    }

    public override int GetHashCode()
    {
        var hashCode = 1173473123;
        hashCode = hashCode * -1521134295 + min.GetHashCode();
        hashCode = hashCode * -1521134295 + Min.GetHashCode();
        hashCode = hashCode * -1521134295 + max.GetHashCode();
        hashCode = hashCode * -1521134295 + Max.GetHashCode();
        hashCode = hashCode * -1521134295 + current.GetHashCode();
        hashCode = hashCode * -1521134295 + Current.GetHashCode();
        hashCode = hashCode * -1521134295 + IsMax.GetHashCode();
        hashCode = hashCode * -1521134295 + IsMin.GetHashCode();
        hashCode = hashCode * -1521134295 + Rest.GetHashCode();
        hashCode = hashCode * -1521134295 + Quarter.GetHashCode();
        hashCode = hashCode * -1521134295 + Half.GetHashCode();
        hashCode = hashCode * -1521134295 + Three_Fourths.GetHashCode();
        hashCode = hashCode * -1521134295 + One_Third.GetHashCode();
        return hashCode;
    }
}

[Serializable]
public class ScopeFloat
{
    [SerializeField]
    private float min;
    public float Min
    {
        get { return min; }
        set
        {
            if (max < value) min = max;
            else min = value;
        }
    }

    [SerializeField]
    private float max;
    public float Max
    {
        get { return max; }
        set
        {
            if (value < 0) max = 0;
            else if (value < min) max = min + 1;
            else max = value;
        }
    }

    private float current;
    public float Current
    {
        get
        {
            return current;
        }

        set
        {
            if (value > Max) current = Max;
            else if (value < Min) current = Min;
            else current = value;
        }
    }

    public ScopeFloat()
    {
        Min = 0;
        Max = 1;
    }

    public ScopeFloat(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public ScopeFloat(float max)
    {
        Min = 0;
        Max = max;
    }

    public bool IsMax
    {
        get
        {
            return current == Max;
        }
    }

    public bool IsMin
    {
        get
        {
            return current == Min;
        }
    }

    /// <summary>
    /// 余下的部分
    /// </summary>
    public float Rest { get { return Max - Current; } }

    public void ToMin()
    {
        Current = Min;
    }

    public void ToMax()
    {
        Current = Max;
    }

    /// <summary>
    /// 四分之一
    /// </summary>
    public float Quarter { get { return Max * 0.25f; } }

    public float Half { get { return Max * 0.5f; } }

    /// <summary>
    /// 四分之三
    /// </summary>
    public float Three_Fourths { get { return Max * 0.75f; } }

    /// <summary>
    /// 三分之一
    /// </summary>
    public float One_Third { get { return Max / 3; } }

    #region 运算符重载
    #region 加减乘除
    public static int operator +(ScopeFloat left, int right)
    {
        return (int)left.Current + right;
    }
    public static int operator -(ScopeFloat left, int right)
    {
        return (int)left.Current - right;
    }
    public static float operator +(ScopeFloat left, float right)
    {
        return left.Current + right;
    }
    public static float operator -(ScopeFloat left, float right)
    {
        return left.Current - right;
    }
    public static int operator +(int left, ScopeFloat right)
    {
        return (int)(left + right.Current);
    }
    public static int operator -(int left, ScopeFloat right)
    {
        return (int)(left - right.Current);
    }
    public static float operator +(float left, ScopeFloat right)
    {
        return left + right.Current;
    }
    public static float operator -(float left, ScopeFloat right)
    {
        return left - right.Current;
    }

    public static ScopeFloat operator *(ScopeFloat left, int right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current * right };
    }
    public static ScopeFloat operator /(ScopeFloat left, int right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current / right };
    }
    public static ScopeFloat operator *(ScopeFloat left, float right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current * right };
    }
    public static ScopeFloat operator /(ScopeFloat left, float right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current / right };
    }
    public static int operator *(int left, ScopeFloat right)
    {
        return (int)(left * right.Current);
    }
    public static int operator /(int left, ScopeFloat right)
    {
        return (int)(left / right.Current);
    }
    public static float operator *(float left, ScopeFloat right)
    {
        return left * right.Current;
    }
    public static float operator /(float left, ScopeFloat right)
    {
        return left / right.Current;
    }

    public static ScopeFloat operator ++(ScopeFloat original)
    {
        original.Current++;
        return original;
    }
    public static ScopeFloat operator --(ScopeFloat original)
    {
        original.Current--;
        return original;
    }
    #endregion

    #region 大于、小于
    public static bool operator >(ScopeFloat left, int right)
    {
        return left.Current > right;
    }
    public static bool operator <(ScopeFloat left, int right)
    {
        return left.Current < right;
    }
    public static bool operator >(ScopeFloat left, float right)
    {
        return left.Current > right;
    }
    public static bool operator <(ScopeFloat left, float right)
    {
        return left.Current < right;
    }
    public static bool operator >(int left, ScopeFloat right)
    {
        return left > right.Current;
    }
    public static bool operator <(int left, ScopeFloat right)
    {
        return left < right.Current;
    }
    public static bool operator >(float left, ScopeFloat right)
    {
        return left > right.Current;
    }
    public static bool operator <(float left, ScopeFloat right)
    {
        return left < right.Current;
    }
    #endregion

    #region 大于等于、小于等于
    public static bool operator >=(ScopeFloat left, int right)
    {
        return left.Current >= right;
    }
    public static bool operator <=(ScopeFloat left, int right)
    {
        return left.Current <= right;
    }
    public static bool operator >=(ScopeFloat left, float right)
    {
        return left.Current >= right;
    }
    public static bool operator <=(ScopeFloat left, float right)
    {
        return left.Current <= right;
    }
    public static bool operator >=(int left, ScopeFloat right)
    {
        return left >= right.Current;
    }
    public static bool operator <=(int left, ScopeFloat right)
    {
        return left <= right.Current;
    }
    public static bool operator >=(float left, ScopeFloat right)
    {
        return left >= right.Current;
    }
    public static bool operator <=(float left, ScopeFloat right)
    {
        return left <= right.Current;
    }
    #endregion

    #region 等于、不等于
    public static bool operator ==(ScopeFloat left, int right)
    {
        return left.Current == right;
    }
    public static bool operator !=(ScopeFloat left, int right)
    {
        return left.Current != right;
    }
    public static bool operator ==(ScopeFloat left, float right)
    {
        return left.Current == right;
    }
    public static bool operator !=(ScopeFloat left, float right)
    {
        return left.Current != right;
    }
    public static bool operator ==(int left, ScopeFloat right)
    {
        return left == right.Current;
    }
    public static bool operator !=(int left, ScopeFloat right)
    {
        return left != right.Current;
    }
    public static bool operator ==(float left, ScopeFloat right)
    {
        return left == right.Current;
    }
    public static bool operator !=(float left, ScopeFloat right)
    {
        return left != right.Current;
    }
    #endregion
    public static explicit operator float(ScopeFloat original)
    {
        return original.Current;
    }
    public static explicit operator int(ScopeFloat original)
    {
        return (int)original.Current;
    }
    public static explicit operator ScopeInt(ScopeFloat original)
    {
        return new ScopeInt((int)original.Min, (int)original.Max) { Current = (int)original.Current };
    }
    #endregion

    public override string ToString()
    {
        return Current.ToString() + "/" + Max.ToString();
    }

    public string ToString(string format)
    {
        string amount = Regex.Replace(format, @"[^F^0-9]+", "");
        if (format.Contains("//"))
        {
            return Min.ToString(amount) + "/" + Current.ToString(amount) + "/" + Max.ToString(amount);
        }
        else if (format == "[/]")
        {
            return "[" + Current.ToString(amount) + "/" + Max.ToString(amount) + "]";
        }
        else if (format == "[//]")
        {
            return "[" + Min.ToString(amount) + "/" + Current.ToString(amount) + "/" + Max.ToString(amount) + "]";
        }
        else if (format == "(/)")
        {
            return "(" + Current.ToString(amount) + "/" + Max.ToString(amount) + ")";
        }
        else if (format == "(//)")
        {
            return "(" + Min.ToString(amount) + "/" + Current.ToString(amount) + "/" + Max.ToString(amount) + ")";
        }
        else if (!string.IsNullOrEmpty(amount)) return Current.ToString(amount) + "/" + Max.ToString(amount);
        else return ToString();
    }

    /// <summary>
    /// 转成字符串
    /// </summary>
    /// <param name="start">字符串开头</param>
    /// <param name="split">数字分隔符</param>
    /// <param name="end">字符串结尾</param>
    /// <param name="decimalDigit">小数保留个数</param>
    /// <param name="showMin">是否显示最小值</param>
    /// <returns>目标字符串</returns>
    public string ToString(string start, string split, string end, int decimalDigit, bool showMin = false)
    {
        if (showMin)
        {
            return start + Min.ToString("F" + decimalDigit) + split + Current.ToString("F" + decimalDigit) + split + Max.ToString("F" + decimalDigit) + end;
        }
        return start + Current.ToString("F" + decimalDigit) + split + Max.ToString("F" + decimalDigit) + end;
    }

    public override bool Equals(object obj)
    {
        return obj is ScopeFloat @float &&
               min == @float.min &&
               max == @float.max &&
               current == @float.current;
    }

    public override int GetHashCode()
    {
        var hashCode = 1173473123;
        hashCode = hashCode * -1521134295 + min.GetHashCode();
        hashCode = hashCode * -1521134295 + Min.GetHashCode();
        hashCode = hashCode * -1521134295 + max.GetHashCode();
        hashCode = hashCode * -1521134295 + Max.GetHashCode();
        hashCode = hashCode * -1521134295 + current.GetHashCode();
        hashCode = hashCode * -1521134295 + Current.GetHashCode();
        hashCode = hashCode * -1521134295 + IsMax.GetHashCode();
        hashCode = hashCode * -1521134295 + IsMin.GetHashCode();
        hashCode = hashCode * -1521134295 + Rest.GetHashCode();
        hashCode = hashCode * -1521134295 + Quarter.GetHashCode();
        hashCode = hashCode * -1521134295 + Half.GetHashCode();
        hashCode = hashCode * -1521134295 + Three_Fourths.GetHashCode();
        hashCode = hashCode * -1521134295 + One_Third.GetHashCode();
        return hashCode;
    }
}

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

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }
}