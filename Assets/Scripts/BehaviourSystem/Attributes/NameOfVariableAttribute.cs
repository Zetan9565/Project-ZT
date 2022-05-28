using System;

namespace ZetanStudio.BehaviourTree
{
    public class NameOfVariableAttribute : EnhancedPropertyAttribute
    {
        public readonly Type type;
        public readonly bool global;

        public NameOfVariableAttribute(Type type, bool global = false)
        {
            this.type = type;
            this.global = global;
        }
    }
}