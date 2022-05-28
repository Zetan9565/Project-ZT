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

        [field: SerializeField, Label("默认ID前缀")]
        public string IDPrefix { get; private set; } = "ITEM";

        [field: SerializeField, Label("默认道具类型"), Enum(typeof(ItemType))]
        public int Type { get; private set; } = 0;

        [field: SerializeField, Label("默认描述"), TextArea]
        public string Description { get; private set; }

        [field: SerializeField, Label("堆叠上限"), Min(0)]
        public int StackLimit { get; private set; } = 1;

        [field: SerializeField, Label("可丢弃")]
        public bool Discardable { get; private set; } = true;

        [SerializeReference]
        private List<ItemModule> modules = new List<ItemModule>();
        public ReadOnlyCollection<ItemModule> Modules => modules.AsReadOnly();

#if UNITY_EDITOR
        public static class Editor
        {
            public static ItemModule AddModule(ItemTemplate template, Type type, int index = -1, KeyedByTypeCollection<ItemModule> keyedModules = null)
            {
                if (template == null || type == null) return null;
                if (keyedModules == null) keyedModules = new KeyedByTypeCollection<ItemModule>(template.modules.Where(x => x is not CommonModule));
                if (!CommonModule.IsCommon(type) && keyedModules.Contains(type)) return null;
                var attr = type.GetCustomAttribute<ItemModule.RequireAttribute>();
                if (attr != null)
                {
                    foreach (var m in attr.modules)
                    {
                        if (m != type) AddModule(template, m, keyedModules: keyedModules);
                    }
                }
                ItemModule module = Activator.CreateInstance(type) as ItemModule;
                if (index < 0) template.modules.Add(module);
                else template.modules.Insert(index, module);
                if (!CommonModule.IsCommon(type)) keyedModules.Add(module);
                ZetanUtility.Editor.SaveChange(template);
                return module;
            }
            public static bool RemoveModule(ItemTemplate template, ItemModule module)
            {
                if (!template || module == null) return false;
                template.modules.Remove(module);
                ZetanUtility.Editor.SaveChange(template);
                return true;
            }
        }
#endif
    }
}