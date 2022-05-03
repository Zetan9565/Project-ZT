using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using ZetanStudio.Item.Module;
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
#endif

namespace ZetanStudio.Item
{
    [CreateAssetMenu]
    public sealed class ItemTemplate : ScriptableObject
    {
        [field: SerializeField]
        public string Name { get; private set; } = "未命名模版";

        [field: SerializeField, DisplayName("默认ID前缀")]
        public string IDPrefix { get; private set; } = "ITEM";

        [field: SerializeField, DisplayName("默认道具类型"), Enum(typeof(ItemType))]
        public int DefaultType { get; private set; } = 0;

        [SerializeReference]
        private List<ItemModule> modules = new List<ItemModule>();
        public ReadOnlyCollection<ItemModule> Modules => modules.AsReadOnly();

#if UNITY_EDITOR
        public static class Editor
        {
            public static ItemModule AddModule(ItemTemplate template, Type type, KeyedByTypeCollection<ItemModule> keyedModules = null)
            {
                if (template == null || type == null) return null;
                if (keyedModules == null) keyedModules = new KeyedByTypeCollection<ItemModule>(template.modules.Where(x => x is not CommonModule));
                if (!CommonModule.IsCommon(type) && keyedModules.Contains(type)) return null;
                var attr = type.GetCustomAttribute<ItemModule.RequireAttribute>();
                if (attr != null)
                {
                    foreach (var m in attr.modules)
                    {
                        if (m != type) AddModule(template, m, keyedModules);
                    }
                }
                ItemModule module = Activator.CreateInstance(type) as ItemModule;
                template.modules.Add(module);
                if (!CommonModule.IsCommon(type)) keyedModules.Add(module);
                return module;
            }
            public static bool RemoveModule(ItemTemplate template, ItemModule module)
            {
                if (!template || module == null) return false;
                template.modules.Remove(module);
                return true;
            }
        }
#endif
    }
}