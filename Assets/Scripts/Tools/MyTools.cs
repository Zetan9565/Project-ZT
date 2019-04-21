using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class MyTools
{
    /// <summary>
    /// 概率计算
    /// </summary>
    /// <param name="probability">百分比数值</param>
    /// <returns>概率命中</returns>
    public static bool Probability(float probability)
    {
        if (probability < 0) return false;
        return UnityEngine.Random.Range(100, 10001) / 100.0f <= probability;
    }

    public static void SetActive(GameObject gameObject, bool value)
    {
        if (!gameObject) return;
        if (gameObject.activeSelf != value) gameObject.SetActive(value);
    }

    public static string GetChineseNumber(double value)
    {
        string originalNum = value.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
        string chineseNum = Regex.Replace(originalNum, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
        return Regex.Replace(chineseNum, ".", m => "负 空零一二三四五六七八九空空空空空空空分角十百千万亿兆京垓秭穰"[m.Value[0] - '-'].ToString());
    }

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
        int bytesRead = 0;
        byte[] buffer = new byte[1024];
        unencryptStream.Position = 0;
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
        int bytesRead = 0;
        byte[] buffer = new byte[1024];
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

    public int Rest { get { return Max - Current; } }
    /// <summary>
    /// 四分之一
    /// </summary>
    public int Quarter { get { return Max / 4; } }

    public int Half { get { return Max / 2; } }

    /// <summary>
    /// 四分之三
    /// </summary>
    public int Three_Fourth { get { return (int)(Max * 0.75f); } }

    /// <summary>
    /// 三分之一
    /// </summary>
    public int One_Third { get { return Max / 3; } }

    #region 运算符重载
    #region 加减乘除
    public static ScopeInt operator +(ScopeInt left, int right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = left.Current + right };
    }
    public static ScopeInt operator -(ScopeInt left, int right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = left.Current - right };
    }
    public static ScopeInt operator +(ScopeInt left, float right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = (int)(left.Current + right) };
    }
    public static ScopeInt operator -(ScopeInt left, float right)
    {
        return new ScopeInt(left.Min, left.Max) { Current = (int)(left.Current - right) };
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
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
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

    public string ToString(string star, string split, string end, bool showMin = false)
    {
        if (showMin)
        {
            return star + Min + split + Current + split + Max + end;
        }
        return star + Min + split + Current + split + Max + end;
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
    /// 余下部分
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
    public float Three_Fourth { get { return Max * 0.75f; } }

    /// <summary>
    /// 三分之一
    /// </summary>
    public float One_Third { get { return Max / 3; } }

    #region 运算符重载
    #region 加减乘除
    public static ScopeFloat operator +(ScopeFloat left, int right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current + right };
    }
    public static ScopeFloat operator -(ScopeFloat left, int right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current - right };
    }
    public static ScopeFloat operator +(ScopeFloat left, float right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current + right };
    }
    public static ScopeFloat operator -(ScopeFloat left, float right)
    {
        return new ScopeFloat(left.Min, left.Max) { Current = left.Current - right };
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
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
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
    /// <param name="star">字符串开头</param>
    /// <param name="split">数字分隔符</param>
    /// <param name="end">字符串结尾</param>
    /// <param name="decimalDigit">小数保留个数</param>
    /// <param name="showMin">是否显示最小值</param>
    /// <returns>目标字符串</returns>
    public string ToString(string star, string split, string end, int decimalDigit, bool showMin = false)
    {
        if (showMin)
        {
            return star + Min.ToString("F" + decimalDigit) + split + Current.ToString("F" + decimalDigit) + split + Max.ToString("F" + decimalDigit) + end;
        }
        return star + Min.ToString("F" + decimalDigit) + split + Current.ToString("F" + decimalDigit) + split + Max.ToString("F" + decimalDigit) + end;
    }
}