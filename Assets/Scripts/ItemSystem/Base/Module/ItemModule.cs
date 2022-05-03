using System;
using System.Reflection;

namespace ZetanStudio.Item.Module
{
    [Serializable]
    public abstract class ItemModule
    {
        public sealed class NameAttribute : Attribute
        {
            public readonly string name;

            public NameAttribute(string name)
            {
                this.name = name;
            }
        }

        public sealed class RequireAttribute : Attribute
        {
            public readonly Type[] modules;

            public RequireAttribute(Type module, params Type[] modules)
            {
                this.modules = new Type[] { module };
                Array.Resize(ref this.modules, modules.Length + 1);
                for (int i = 0; i < modules.Length; i++)
                {
                    this.modules[i + 1] = modules[i];
                }
            }
        }

        [Obsolete("暂不支持不共存模块设置")]
        public sealed class NotCoexistAttribute : Attribute
        {
            public readonly Type[] modules;

            public NotCoexistAttribute(Type module, params Type[] modules)
            {
                this.modules = new Type[] { module };
                Array.Resize(ref this.modules, modules.Length + 1);
                for (int i = 0; i < modules.Length; i++)
                {
                    this.modules[i + 1] = modules[i];
                }
            }
        }

        public abstract bool IsValid { get; }

        public virtual ItemModuleData CreateData(ItemData item) => null;

        public static string GetName(Type type)
        {
            string name = System.Text.RegularExpressions.Regex.Replace(type.Name.Replace("Module", string.Empty), "([a-z])([A-Z])", "$1 $2");
            if (type.GetCustomAttribute<NameAttribute>() is NameAttribute attribute) name = $"{attribute.name} ({name})";
            return name;
        }
        public string GetName()
        {
            return GetName(GetType());
        }

        public ItemModule Copy() => MemberwiseClone() as ItemModule;

        public static bool Duplicate(ItemModule module, Type type)
        {
            return module is not CommonModule && (module.GetType().IsAssignableFrom(type) || type.IsAssignableFrom(module.GetType()));
        }
    }
}