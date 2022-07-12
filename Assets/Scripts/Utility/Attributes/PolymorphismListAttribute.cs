using System;
using UnityEngine;

namespace ZetanStudio
{
    public class PolymorphismListAttribute : PropertyAttribute
    {
        public readonly string getNameMethod;
        public readonly string getGroupMethod;
        public readonly Type[] excludedTypes;

        public PolymorphismListAttribute(params Type[] excludedTypes)
        {
            this.excludedTypes = excludedTypes;
        }
        public PolymorphismListAttribute(string staticGetNameMethod, params Type[] excludedTypes) : this(excludedTypes)
        {
            getNameMethod = staticGetNameMethod;
        }
        public PolymorphismListAttribute(string staticGetGroupMethod, string staticGetNameMethod, params Type[] excludedTypes) : this(staticGetNameMethod, excludedTypes)
        {
            getGroupMethod = staticGetGroupMethod;
        }
    }
}
