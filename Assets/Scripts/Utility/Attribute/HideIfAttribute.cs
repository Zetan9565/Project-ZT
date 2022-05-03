using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideIfAttribute : PropertyAttribute
{
    public readonly string[] paths;
    public readonly object[] values;
    public readonly bool and;
    public readonly bool readOnly;
    public readonly string relational;
    public readonly Type type;

    public HideIfAttribute(Type type, bool readOnly = false)
    {
        this.type = type;
        this.readOnly = readOnly;
    }
    /// <param name="path">路径</param>
    /// <param name="value">值</param>
    /// <param name="and">是否进行与操作</param>
    /// <param name="readOnly">只否只读而不是隐藏</param>
    public HideIfAttribute(string path, object value, bool and = true, bool readOnly = false)
    {
        paths = new string[] { path };
        values = new object[] { value };
        this.and = and;
        this.readOnly = readOnly;
    }
    /// <summary>
    /// 两个数组长度必须一致
    /// </summary>
    /// <param name="paths">路径列表</param>
    /// <param name="values">值列表</param>
    /// <param name="and">是否进行与操作</param>
    /// <param name="readOnly">只否只读而不是隐藏</param>
    public HideIfAttribute(string[] paths, object[] values, bool and = true, bool readOnly = false)
    {
        this.paths = paths;
        this.values = values;
        this.and = and;
        this.readOnly = readOnly;
    }
    /// <summary>
    /// 参数格式：(是否进行与操作，路径1，值1，路径2，值2，……，路径n，值n)
    /// </summary>
    /// <param name="and">是否进行与操作</param>
    /// <param name="readOnly">只否只读而不是隐藏</param>
    /// <param name="pathValuePair">径值对</param>
    public HideIfAttribute(bool and, bool readOnly, params object[] pathValuePair)
    {
        this.and = and;
        this.readOnly = readOnly;
        paths = new string[pathValuePair.Length / 2];
        values = new object[pathValuePair.Length / 2];
        for (int i = 0; i < pathValuePair.Length; i += 2)
        {
            paths[i / 2] = pathValuePair[i] as string;
            values[i / 2] = pathValuePair[i + 1];
        }
    }
    /// <summary>
    /// 带关系表达式
    /// </summary>
    /// <param name="paths">路径列表</param>
    /// <param name="values">值列表</param>
    /// <param name="relational">关系表达式，支持括号、与(&)、或(|)、非(!)</param>
    /// <param name="readOnly">只否只读而不是隐藏</param>
    public HideIfAttribute(string[] paths, object[] values, string relational, bool readOnly = false)
    {
        this.paths = paths;
        this.values = values;
        this.relational = relational;
        this.readOnly = readOnly;
    }
}