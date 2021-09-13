using System;

namespace ZetanStudio.BehaviourTree
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeDescriptionAttribute : Attribute
    {
        public readonly string description;

        public NodeDescriptionAttribute(string description)
        {
            this.description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HideIfAttribute : Attribute
    {
        public readonly string path;
        public readonly object value;
        public readonly bool readOnly;

        public HideIfAttribute(string path, object value, bool readOnly = false)
        {
            this.path = path;
            this.value = value;
            this.readOnly = readOnly;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class Tag : Attribute { }
}