using System;
using UnityEngine;

namespace ZetanStudio
{
    [Serializable]
    public sealed class TypeReference : ISerializationCallbackReceiver
    {
        public string typeName;

        public Type value;

        public TypeReference() { }

        public TypeReference(string typeName)
        {
            this.typeName = typeName;
        }

        public TypeReference(Type type)
        {
            value = type;
            OnAfterDeserialize();
        }

        public void OnAfterDeserialize()
        {
            if (value != null)
            {
                typeName = $"{(string.IsNullOrEmpty(value.Namespace) ? string.Empty : $"{value.Namespace}.")}{value.Name}";
            }
        }

        public void OnBeforeSerialize()
        {
            value = Utility.GetTypeWithoutAssembly(typeName);
        }

        public static implicit operator Type(TypeReference self)
        {
            return self.value;
        }
    }
}