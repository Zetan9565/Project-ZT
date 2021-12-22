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
    public class HideIf_BTAttribute : Attribute
    {
        public readonly string path;
        public readonly object value;
        public readonly bool readOnly;

        public HideIf_BTAttribute(string path, object value, bool readOnly = false)
        {
            this.path = path;
            this.value = value;
            this.readOnly = readOnly;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class Tag_BTAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class NameOfVariableAttribute : Attribute
    {
        public readonly Type type;

        public NameOfVariableAttribute(Type type)
        {
            this.type = type;
        }
    }
}