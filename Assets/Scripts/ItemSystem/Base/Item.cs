using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using ZetanStudio.ItemSystem.Module;
#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
#endif

namespace ZetanStudio.ItemSystem
{
    /// <summary>
    /// 设计思路：<br/>
    ///1. <b>共用</b> 且 <b>固定不变</b> 的内容可作为成员变量，如<see cref="ID"/>、<see cref="Name"/>这种固定不变的；<br/>
    ///2. 而 <b>不需要共用</b> 或 <b>会发生改变 </b>的内容可作为额外模块，例如，不是所有道具都可以锁定，且锁定状态会发生改变，就<br/>
    ///做成<see cref="LockableModule"/>，用相应的<see cref="LockableData"/>去记录状态。
    /// </summary>
    [KeywordsGroup("道具")]
    public sealed class Item : ScriptableObject, IKeyword
    {
        private const bool useDatabase = true;
        public static bool UseDatabase => useDatabase;

        public const string assetsFolder = "Assets/Resources/Configuration/Item";

        [field: SerializeField]
        public string ID { get; private set; }

        string IKeyword.IDPrefix => "ITEM";

        [field: SerializeField, Label("名称")]
        public string Name { get; private set; } = "未命名道具";

        [field: SerializeField, Label("图标"), SpriteSelector]
        public Sprite Icon { get; private set; }

        [field: SerializeField, Label("描述"), TextArea]
        public string Description { get; private set; }

        [SerializeField, Label("类型"), Enum(typeof(ItemType))]
        private int type = 0;
        public ItemType Type => ItemTypeEnum.Instance[type];

        [SerializeField, Label("品质"), ItemQuality]
        private int quality = 0;
        public ItemQuality Quality => ItemQualityEnum.Instance[quality];

        [field: SerializeField, Label("重量"), Min(0)]
        public float Weight { get; private set; }

        [field: SerializeField, Label("堆叠上限"), Min(0)]
        public int StackLimit { get; private set; } = 1;
        public bool StackAble => InfiniteStack || StackLimit > 1;
        public bool InfiniteStack => StackLimit < 1;

        [field: SerializeField, Label("可丢弃")]
        public bool Discardable { get; private set; } = true;

        [SerializeReference]
        private List<ItemModule> modules = new List<ItemModule>();
        public ReadOnlyCollection<ItemModule> Modules => modules.AsReadOnly();

        string IKeyword.Group => Type.Name;

        Color IKeyword.Color => Quality.Color;

        public T GetModule<T>() where T : ItemModule
        {
            return modules.Find(x => x is T) as T;
        }
        public bool TryGetModule<T>(out T module) where T : ItemModule
        {
            return module = GetModule<T>();
        }
        public CommonModule<T> GetCommonModule<T>(string name)
        {
            return modules.Find(x => x is CommonModule<T> c && c.Name == name) as CommonModule<T>;
        }
        public bool TryGetCommonModule<T>(string name, out CommonModule<T> module)
        {
            return module = GetCommonModule<T>(name);
        }

        public class Comparer : IComparer<Item>
        {
            public static Comparer Default => new Comparer();

            public int Compare(Item x, Item y)
            {
                if (x.Type.Priority < y.Type.Priority) return -1;
                else if (x.Type.Priority > y.Type.Priority) return 1;
                else if (x.Quality.Priority < y.Quality.Priority) return -1;
                else if (x.Quality.Priority > y.Quality.Priority) return 1;
                else
                {
                    int result = string.Compare(x.Name, y.Name);
                    if (result != 0) return result;
                    return string.Compare(x.ID, y.ID);
                }
            }
        }

        [RuntimeGetKeywordsMethod]
        public static List<Item> GetItems()
        {
            if (UseDatabase) return ItemDatabase.GetItems();
            else return new List<Item>(Resources.LoadAll<Item>(assetsFolder.Remove(0, "Assets/Resources".Length)));
        }
        public static List<Item> GetItemsWhere(Predicate<Item> predicate)
        {
            if (UseDatabase) return ItemDatabase.GetItemsWhere(predicate);
            else return new List<Item>(Resources.LoadAll<Item>(assetsFolder.Remove(0, "Assets/Resources".Length)).Where(x => predicate(x)));
        }

#if UNITY_EDITOR
        #region Editor相关
        public static class Editor
        {
            [GetKeywordsMethod]
#pragma warning disable IDE0051 // 删除未使用的私有成员
            private static List<Item> GetItemKeywords() => GetItems();
#pragma warning restore IDE0051 // 删除未使用的私有成员
            public static List<Item> GetItems(ItemTemplate template = null)
            {
                if (template)
                    if (UseDatabase) return ItemDatabase.Editor.GetItems(template);
                    else return Utility.Editor.LoadMainAssetsWhere<Item>(x => MatchTemplate(x, template), assetsFolder);
                if (UseDatabase) return ItemDatabase.Editor.GetItems();
                else return Utility.Editor.LoadMainAssets<Item>(assetsFolder);
            }
            public static List<Item> GetItemsWhere(Predicate<Item> predicate)
            {
                if (UseDatabase) return ItemDatabase.Editor.GetItemsWhere(predicate);
                else return Utility.Editor.LoadMainAssetsWhere<Item>(predicate, assetsFolder);
            }
            public static void SetAutoID(Item item, IEnumerable<Item> items, string IDPrefix = null)
            {
                int i = 0;
                string prefix = !string.IsNullOrEmpty(IDPrefix) ? IDPrefix : "ITEM";
                string id = prefix + i.ToString().PadLeft(4, '0');
                while (ExistID(id))
                {
                    id = prefix + i.ToString().PadLeft(4, '0');
                    i++;
                }
                item.ID = id;
                Utility.Editor.SaveChange(item);

                bool ExistID(string id)
                {
                    return items.Any(x => x.ID == id && x != item);
                }
            }
            public static string GetAutoID(Item item, IEnumerable<Item> items, string IDPrefix = null)
            {
                int i = 0;
                string prefix = !string.IsNullOrEmpty(IDPrefix) ? IDPrefix : "ITEM";
                string id = prefix + i.ToString().PadLeft(4, '0');
                while (ExistID(id))
                {
                    id = prefix + i.ToString().PadLeft(4, '0');
                    i++;
                }
                return id;

                bool ExistID(string id)
                {
                    return items.Any(x => x.ID == id && x != item);
                }
            }
            public static void ApplyTemplate(Item item, ItemTemplate template)
            {
                if (template)
                {
                    item.type = template.Type;
                    item.StackLimit = template.StackLimit;
                    item.Discardable = template.Discardable;
                    var keyedModules = new Dictionary<Type, ItemModule>();
                    foreach (var module in item.modules)
                    {
                        if (module is not CommonModule) keyedModules.Add(module.GetType(), module);
                    }
                    foreach (var module in template.Modules)
                    {
                        if (module is not CommonModule && keyedModules.ContainsKey(module.GetType())) continue;
                        var copy = module.Copy();
                        item.modules.Add(copy);
                    }
                    Utility.Editor.SaveChange(item);
                }
            }
            public static void ApplyFilter(Item item, ItemFilterAttribute itemFilter)
            {
                if (itemFilter != null)
                {
                    foreach (var filter in itemFilter.filters)
                    {
                        if (filter is string f)
                        {
                            f = f.ToLower();
                            bool revers = f.StartsWith("not ");
                            f = f.Replace("not ", "");
                            string value = f.Split(':')[^1];
                            if (f.StartsWith("n:") || f.StartsWith("name:"))
                            {
                                if (!revers) item.Name = value;
                            }
                            else if (f.StartsWith("i:") || f.StartsWith("id:"))
                            {
                                if (!revers) item.ID = value;
                            }
                            else if (f.StartsWith("t:") || f.StartsWith("type:"))
                            {
                                if (int.TryParse(value, out var index))
                                {
                                    if (!revers) item.type = index;
                                }
                                else if (!revers) item.quality = ItemTypeEnum.NameToIndex(value);
                            }
                            else if (f.StartsWith("q:") || f.StartsWith("quality:"))
                            {
                                if (int.TryParse(value, out var index))
                                {
                                    if (!revers) item.quality = index;
                                }
                                else if (!revers) item.quality = ItemQualityEnum.NameToIndex(value);
                            }
                            else if (f.StartsWith("d:") || f.StartsWith("des:") || f.StartsWith("desc:") || f.StartsWith("description:"))
                            {
                                if (!revers) item.Description = value;
                            }
                            else if (f.StartsWith("m:") || f.StartsWith("module:"))
                                if (!revers) AddModule(item, Utility.GetTypeWithoutAssembly(value));
                        }
                        else if (filter is Type type)
                            AddModule(item, type);
                    }
                    Utility.Editor.SaveChange(item);
                }
            }
            public static bool MatchTemplate(Item item, ItemTemplate template)
            {
                if (!item) return false;
                if (!template) return true;
                if (item.type != template.Type) return false;

                var keyedModules = new Dictionary<Type, ItemModule>();
                Dictionary<Type, HashSet<string>> commons = new Dictionary<Type, HashSet<string>>();
                foreach (var module in item.modules)
                {
                    if (module is not CommonModule common)
                        keyedModules.Add(module.GetType(), module);
                    else if (commons.TryGetValue(common.GetType(), out var find))
                        find.Add(common.Name);
                    else commons.Add(common.GetType(), new HashSet<string>() { common.Name });
                }
                foreach (var module in template.Modules)
                {
                    //若道具的模块没有模版上应有的模块，则不匹配
                    if (module is not CommonModule common)
                    {
                        if (!keyedModules.ContainsKey(module.GetType()))
                            return false;
                    }
                    else if (!commons.TryGetValue(common.GetType(), out var find) || !find.Contains(common.Name))
                        return false;
                }
                return true;
            }

            public static ItemModule AddModule(Item item, Type type, int index = -1)
            {
                if (item == null || type == null) return null;
                if (!CommonModule.IsCommon(type) && item.modules.Exists(x => ItemModule.Duplicate(x, type, out _))) return null;
                var attr = type.GetCustomAttribute<ItemModule.RequireAttribute>(true);
                if (attr != null)
                {
                    foreach (var m in attr.modules)
                    {
                        if (m != type) AddModule(item, m);
                    }
                }
                ItemModule module = Activator.CreateInstance(type) as ItemModule;
                if (index < 0) item.modules.Add(module);
                else item.modules.Insert(index, module);
                Utility.Editor.SaveChange(item);
                return module;
            }
            public static bool RemoveModule(Item item, ItemModule module)
            {
                if (!item || module == null) return false;
                item.modules.Remove(module);
                Utility.Editor.SaveChange(item);
                return true;
            }
            public static bool ClearInvalidModule(Item item)
            {
                int count = item.modules.RemoveAll(x => !x);
                if (count > 0 && UseDatabase) Utility.Editor.SaveChange(ItemDatabase.Instance);
                return count > 0;
            }
        }
        #endregion
#endif
    }
}