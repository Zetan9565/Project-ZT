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
public class ScopeMumber
{
    private int min;
    public int Min
    {
        get { return min; }
        set
        {
            if (value < 0) min = 0;
            else min = value;
        }
    }

    private int max;
    public int Max
    {
        get{ return max; }
        set
        {
            if (value < 0) max = 0;
            else if (value < min) max = min;
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

    public ScopeMumber()
    {
        Min = 0;
        Max = 1;
    }

    public ScopeMumber(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public ScopeMumber(int max)
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
}