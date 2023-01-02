using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace ZetanStudio.ItemSystem.Module
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

#if UNITY_EDITOR
            [UnityEditor.InitializeOnLoadMethod]
            private static void CheckLoop()
            {
                foreach (var type in UnityEditor.TypeCache.GetTypesDerivedFrom<ItemModule>())
                {
                    if (type.GetCustomAttribute<RequireAttribute>() is RequireAttribute attr)
                        foreach (var mType in attr.modules)
                        {
                            if (mType.GetCustomAttribute<RequireAttribute>() is RequireAttribute mAttr && mAttr.modules.Contains(type))
                                Utility.LogWarning($"模块 {type.Name} 和 {mType.Name} 存在相互依赖，是否是有意为之?");
                        }
                }
            }
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
            private static void CheckItems()
            {
                foreach (var item in Item.Editor.GetItems())
                {
                    List<Type> requires = new List<Type>();
                    foreach (var module in item.Modules)
                    {
                        if (module && module.GetType().GetCustomAttribute<RequireAttribute>() is RequireAttribute require)
                            requires.AddRange(require.modules);
                    }
                    foreach (var r in requires)
                    {
                        if (Item.Editor.AddModule(item, r))
                            Utility.LogWarning($"补充了道具 {item.Name} 缺失的模块: {GetName(r)}");
                    }
                }
            }
            [UnityEditor.InitializeOnLoadMethod]
            private static void CheckTemplates()
            {
                foreach (var temp in Utility.Editor.LoadAssets<ItemTemplate>())
                {
                    List<Type> requires = new List<Type>();
                    foreach (var module in temp.Modules)
                    {
                        if (module && module.GetType().GetCustomAttribute<RequireAttribute>() is RequireAttribute require)
                            requires.AddRange(require.modules);
                    }
                    foreach (var r in requires)
                    {
                        if (ItemTemplate.Editor.AddModule(temp, r))
                            Utility.LogWarning($"补充了模板 {temp.Name} 缺失的模块: {GetName(r)}");
                    }
                }
            }
#endif
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

        public virtual ItemModuleData CreateData(ItemData item)
        {
            //ZetanUtility.Log(GetType().Assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && typeof(ItemModuleData).IsAssignableFrom(t) && t.BaseType.IsGenericType && t.BaseType.GetGenericArguments()[0] == GetType()));
            return null;
        }

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

        public static bool Duplicate(ItemModule module, Type type, out Type dupliacte)
        {
            dupliacte = null;
            if (!module) return true;
            if (module is CommonModule) return false;
            type = baseType(type);
            var mType = baseType(module.GetType());
            var result = mType.IsAssignableFrom(type);
            if (!result) result = type.IsAssignableFrom(mType);
            if (result)
            {
                dupliacte = module.GetType();
                return result;
            }
            return false;

            static Type baseType(Type type)
            {
                while (type.BaseType != null && type.BaseType != typeof(ItemModule))
                {
                    type = type.BaseType;
                }
                return type;
            }
        }

        public static implicit operator bool(ItemModule obj)
        {
            return obj != null;
        }
    }

    public abstract class ItemModuleData
    {
        public ItemData Item { get; protected set; }

        public abstract ItemModule GetModule();

        public static implicit operator bool(ItemModuleData obj)
        {
            return obj != null;
        }

        public abstract GenericData GenerateSaveData();
        public abstract void LoadSaveData(GenericData data);
    }

    public abstract class ItemModuleData<T> : ItemModuleData where T : ItemModule
    {
        public T Module { get; protected set; }

        public sealed override ItemModule GetModule() => Module;

        protected ItemModuleData(ItemData item, T module)
        {
            Item = item;
            Module = module;
        }
    }
}