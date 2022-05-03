using UnityEngine;

namespace ZetanStudio.Item.Module
{
    public abstract class CommonModule : ItemModule
    {
        [field: SerializeField]
        public string Name { get; private set; }

        public abstract object GetParameter();

        public static bool IsCommon(System.Type type)
        {
            return typeof(CommonModule).IsAssignableFrom(type);
        }

        public static bool Duplicate(CommonModule module, CommonModule other)
        {
            if (module == null || other == null) return false;
            if (module == other) return false;
            return other.Name == module.Name && other.GetType() == module.GetType();
        }
    }

    public abstract class CommonModule<T> : CommonModule
    {
        [field: SerializeField]
        public T Parameter { get; private set; }

        public override bool IsValid => !string.IsNullOrEmpty(Name);

        public sealed override object GetParameter()
        {
            return Parameter;
        }
    }
}