using System;

namespace ZetanStudio
{
    public class ShowIfAttribute : EnhancedPropertyAttribute, ICheckValueAttribute
    {
        public string[] Paths { get; }
        public object[] Values { get; }
        public bool And { get; }
        public string Relational { get; }
        public Type DeclarationType { get; }

        public readonly bool readOnly;

        public ShowIfAttribute(Type declarationType, bool readOnly = false)
        {
            DeclarationType = declarationType;
            this.readOnly = readOnly;
        }
        /// <param name="path">路径</param>
        /// <param name="value">值</param>
        /// <param name="readOnly">只否只读</param>
        public ShowIfAttribute(string path, object value, bool readOnly = false)
        {
            Paths = new string[] { path };
            Values = new object[] { value };
            this.readOnly = readOnly;
        }
        /// <summary>
        /// 两个数组长度必须一致
        /// </summary>
        /// <param name="paths">路径列表</param>
        /// <param name="values">值列表</param>
        /// <param name="and">是否进行与操作</param>
        /// <param name="readOnly">只否只读</param>
        public ShowIfAttribute(string[] paths, object[] values, bool and = true, bool readOnly = false)
        {
            Paths = paths;
            Values = values;
            And = and;
            this.readOnly = readOnly;
        }
        /// <summary>
        /// 参数格式：(是否进行与操作，路径1，值1，路径2，值2，……，路径n，值n)
        /// </summary>
        /// <param name="and">是否进行与操作</param>
        /// <param name="readOnly">只否只读</param>
        /// <param name="pathValuePair">径值对</param>
        public ShowIfAttribute(bool and, bool readOnly, params object[] pathValuePair)
        {
            And = and;
            this.readOnly = readOnly;
            Paths = new string[pathValuePair.Length / 2];
            Values = new object[pathValuePair.Length / 2];
            for (int i = 0; i < pathValuePair.Length; i += 2)
            {
                Paths[i / 2] = pathValuePair[i] as string;
                Values[i / 2] = pathValuePair[i + 1];
            }
        }
        /// <summary>
        /// 带关系表达式
        /// </summary>
        /// <param name="paths">路径列表</param>
        /// <param name="values">值列表</param>
        /// <param name="relational">关系表达式，支持括号、与(&)、或(|)、非(!)</param>
        /// <param name="readOnly">只否只读而不是隐藏</param>
        public ShowIfAttribute(string[] paths, object[] values, string relational, bool readOnly = false)
        {
            Paths = paths;
            Values = values;
            Relational = relational;
            this.readOnly = readOnly;
        }
    }
}