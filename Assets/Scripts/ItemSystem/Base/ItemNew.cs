using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    /// <summary>
    /// 设计思路：<br/>
    ///1. <b>共用</b> 且 <b>固定不变</b> 的内容可作为成员变量，如<see cref="ID"/>、<see cref="Name"/>这种固定不变的；<br/>
    ///2. 而 <b>不需要共用</b> 或 <b>会发生改变 </b>的内容可作为额外模块，例如，不是所有道具都可以锁定，且锁定状态会发生改变，就<br/>
    ///做成<see cref="LockableModule"/>: <see cref="ItemModule"/>，用相应的<see cref="LockableData"/>: <see cref="ItemModuleData"/>去记录状态。
    /// </summary>
    public sealed class ItemNew : ScriptableObject
    {
        [field: SerializeField]
        public string ID { get; private set; }

        [field: SerializeField, DisplayName("名称")]
        public string Name { get; private set; } = "未命名道具";

        [field: SerializeField, DisplayName("图标"), SpriteSelector]
        public Sprite Icon { get; private set; }

        [field: SerializeField, DisplayName("描述"), TextArea]
        public string Description { get; private set; }

        [SerializeField, DisplayName("类型"), Enum(typeof(ItemType))]
        private int type = 0;
        public ItemType Type => ItemTypeEnum.Instance[type];

        [SerializeField, DisplayName("品质"), ItemQuality]
        private int quality = 0;
        public ItemQuality Quality => ItemQualityEnum.Instance[quality];

        [field: SerializeField, DisplayName("重量"), Min(0)]
        public float Weight { get; private set; }

        [field: SerializeField, DisplayName("堆叠上限"), Min(0)]
        public int StackLimit { get; private set; } = 1;
        public bool StackAble => UnlimitStack || StackLimit > 1;
        public bool UnlimitStack => StackLimit < 1;

        [field: SerializeField, DisplayName("可丢弃")]
        public bool Discardable { get; private set; } = true;

        [SerializeReference]
        private List<ItemModule> modules = new List<ItemModule>();
        public ReadOnlyCollection<ItemModule> Modules => modules.AsReadOnly();

        public T GetModule<T>() where T : ItemModule
        {
            return modules.Find(x => x is T) as T;
        }
        public CommonModule<T> GetCommonModule<T>(string name)
        {
            return modules.Find(x => x is CommonModule<T> c && c.Name == name) as CommonModule<T>;
        }

        public class ItemComparer : IComparer<ItemNew>
        {
            public static ItemComparer Default => new ItemComparer();

            public int Compare(ItemNew x, ItemNew y)
            {
                if (x.Type.Priority < y.Type.Priority)
                    return -1;
                else if (x.Type.Priority > y.Type.Priority)
                    return 1;
                else if (x.Quality.Priority < y.Quality.Priority)
                    return -1;
                else if (x.Quality.Priority > y.Quality.Priority)
                    return 1;
                else
                {
                    int result = string.Compare(x.Name, y.Name);
                    if (result != 0) return result;
                    return string.Compare(x.ID, y.ID);
                }
            }
        }

#if UNITY_EDITOR
        #region Editor相关
        public static class Editor
        {
            public static void SetAutoID(ItemNew item, IEnumerable<ItemNew> items, string IDPrefix = null)
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

                bool ExistID(string id)
                {
                    return items.Any(x => x.ID == id && x != item);
                }
            }
            public static string GetAutoID(ItemNew item, IEnumerable<ItemNew> items, string IDPrefix = null)
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
            public static void ApplyTemplate(ItemNew item, ItemTemplate template)
            {
                if (template)
                {
                    item.type = template.DefaultType;
                    var keyedModules = new KeyedByTypeCollection<ItemModule>(item.modules.Where(x => x is not CommonModule));
                    foreach (var module in template.Modules)
                    {
                        if (module is not CommonModule && keyedModules.Contains(module.GetType())) continue;
                        var copy = module.Copy();
                        item.modules.Add(copy);
                    }
                }
            }
            public static bool MatchTemplate(ItemNew item, ItemTemplate template)
            {
                if (!item) return false;
                if (!template) return true;
                if (item.type != template.DefaultType) return false;

                var keyedModules = new KeyedByTypeCollection<ItemModule>();
                Dictionary<Type, HashSet<string>> commons = new Dictionary<Type, HashSet<string>>();
                foreach (var module in item.modules)
                {
                    if (module is not CommonModule common)
                        keyedModules.Add(module);
                    else if (commons.TryGetValue(common.GetType(), out var find))
                        find.Add(common.Name);
                    else commons.Add(common.GetType(), new HashSet<string>() { common.Name });
                }
                foreach (var module in template.Modules)
                {
                    //若道具的模块没有模版上应有的模块，则不匹配
                    if (module is not CommonModule common)
                    {
                        if (!keyedModules.Contains(module.GetType()))
                            return false;
                    }
                    else if (!commons.TryGetValue(common.GetType(), out var find) || !find.Contains(common.Name))
                        return false;
                }
                return true;
            }

            public static ItemModule AddModule(ItemNew item, Type type)
            {
                if (item == null || type == null) return null;
                if (!CommonModule.IsCommon(type) && item.modules.Exists(x => ItemModule.Duplicate(x, type))) return null;
                var attr = type.GetCustomAttribute<ItemModule.RequireAttribute>();
                if (attr != null)
                {
                    foreach (var m in attr.modules)
                    {
                        if (m != type) AddModule(item, m);
                    }
                }
                ItemModule module = Activator.CreateInstance(type) as ItemModule;
                item.modules.Add(module);
                return module;
            }
            public static bool RemoveModule(ItemNew item, ItemModule module)
            {
                if (!item || module == null) return false;
                item.modules.Remove(module);
                return true;
            }

            #region 旧版导入
            private static ItemModule GetOrAddModule(ItemNew item, Type type)
            {
                if (item == null || type == null) return null;
                if (item.modules.Find(x => x.GetType() == type) is ItemModule module) return module;
                else return AddModule(item, type);
            }
            public static void CopyFromOld(ItemNew item, ItemBase old)
            {
                if (!item || !old) return;
                item.ID = old.ID;
                item.Name = old.Name;
                item.Icon = old.Icon;
                item.Description = old.Description;
                item.Discardable = old.DiscardAble;
                item.StackLimit = old.StackNum;
                item.Weight = old.Weight;
                item.type = old.ItemType switch
                {
                    global::ItemType.Medicine or global::ItemType.Cuisine => ItemTypeEnum.NameToIndex("消耗品"),
                    global::ItemType.Weapon or global::ItemType.Armor or global::ItemType.Jewelry or global::ItemType.Tool => ItemTypeEnum.NameToIndex("装备"),
                    global::ItemType.Box or global::ItemType.Quest or global::ItemType.Bag => ItemTypeEnum.NameToIndex("特殊"),
                    global::ItemType.Material => ItemTypeEnum.NameToIndex("材料"),
                    global::ItemType.Valuables => ItemTypeEnum.NameToIndex("贸易品"),
                    global::ItemType.Gemstone => ItemTypeEnum.NameToIndex("镶嵌"),
                    global::ItemType.Book => ItemTypeEnum.NameToIndex("文件"),
                    global::ItemType.Seed => ItemTypeEnum.NameToIndex("放置"),
                    global::ItemType.Currency => ItemTypeEnum.NameToIndex("货币"),
                    _ => ItemTypeEnum.NameToIndex("普通"),
                };
                item.quality = (int)old.Quality;
                if (old.Usable)
                {
                    var module = GetOrAddModule(item, typeof(UsableModule));
                    if (old.Inexhaustible) typeof(UsableModule).GetProperty("Cost", ZetanUtility.CommonBindingFlags).SetValue(module, 0);
                }
                if (old.SellAble)
                {
                    var module = GetOrAddModule(item, typeof(SellableModule));
                    typeof(SellableModule).GetProperty("Price", ZetanUtility.CommonBindingFlags).SetValue(module, old.SellPrice);
                }
                if (old.LockAble) GetOrAddModule(item, typeof(LockableModule));
                if (old.Makable)
                {
                    var module = GetOrAddModule(item, typeof(MakableModule));
                    int makingMethord = old.MakingMethod switch
                    {
                        MakingMethod.Smelt => Making.MakingMethodEnum.NameToIndex("冶炼"),
                        MakingMethod.Forging => Making.MakingMethodEnum.NameToIndex("锻造"),
                        MakingMethod.Weaving => Making.MakingMethodEnum.NameToIndex("纺织"),
                        MakingMethod.Tailor => Making.MakingMethodEnum.NameToIndex("缝纫"),
                        MakingMethod.Cooking => Making.MakingMethodEnum.NameToIndex("烹饪"),
                        MakingMethod.Alchemy or MakingMethod.Pharmaceutical => Making.MakingMethodEnum.NameToIndex("制药"),
                        MakingMethod.Season => Making.MakingMethodEnum.NameToIndex("晾晒"),
                        MakingMethod.Triturate => Making.MakingMethodEnum.NameToIndex("研磨"),
                        _ => Making.MakingMethodEnum.NameToIndex("手工"),
                    };
                    typeof(MakableModule).GetField("makingMethod", ZetanUtility.CommonBindingFlags).SetValue(module, makingMethord);
                    typeof(MakableModule).GetField("canMakeByTry", ZetanUtility.CommonBindingFlags).SetValue(module, old.DIYAble);
                    typeof(MakableModule).GetProperty("Formulation", ZetanUtility.CommonBindingFlags).SetValue(module, old.Formulation);
                    typeof(MakableModule).GetProperty("Yields", ZetanUtility.CommonBindingFlags).SetValue(module, new MakingYield[] { new MakingYield(old.MinYield, 0.5f), new MakingYield(old.MaxYield, 0.5f) });
                }
                if (old is CurrencyItem currency)
                {
                    var module = GetOrAddModule(item, typeof(CurrencyModule));
                    typeof(CurrencyModule).GetField("type", ZetanUtility.CommonBindingFlags).SetValue(module, (int)currency.CurrencyType);
                    typeof(CurrencyModule).GetProperty("ValueEach", ZetanUtility.CommonBindingFlags).SetValue(module, currency.ValueEach);
                }
                else if (old is QuestItem quest)
                {
                    var module = GetOrAddModule(item, typeof(TriggerModule));
                    typeof(TriggerModule).GetProperty("Name", ZetanUtility.CommonBindingFlags).SetValue(module, quest.TriggerName);
                    typeof(TriggerModule).GetProperty("State", ZetanUtility.CommonBindingFlags).SetValue(module, quest.StateToSet);
                    module = GetOrAddModule(item, typeof(UsableModule));
                    typeof(UsableModule).GetProperty("Usage", ZetanUtility.CommonBindingFlags).SetValue(module, ZetanUtility.Editor.LoadAsset<SetTrigger>());
                }
                else if (old is SeedItem seed)
                {
                    var module = GetOrAddModule(item, typeof(SeedModule));
                    typeof(SeedModule).GetProperty("Crop", ZetanUtility.CommonBindingFlags).SetValue(module, seed.Crop);
                }
                else if (old is BoxItem box)
                {
                    var module = GetOrAddModule(item, typeof(ProductModule));
                    typeof(ProductModule).GetField("product", ZetanUtility.CommonBindingFlags).SetValue(module, box.ItemsInBox.ToArray());
                }
                else if (old is EquipmentItem equipment)
                {
                    var module = GetOrAddModule(item, typeof(EquipableModule));
                    int equipType = 0;
                    if (equipment is WeaponItem weapon) equipType = EquipmentTypeEnum.NameToIndex(weapon.IsPrimary ? "主武器" : "副武器");
                    else if (equipment is ArmorItem armor)
                    {
                        equipType = armor.ArmorType switch
                        {
                            ArmorType.Helmet => EquipmentTypeEnum.NameToIndex("头盔"),
                            ArmorType.Boots => EquipmentTypeEnum.NameToIndex("鞋子"),
                            ArmorType.Gloves => EquipmentTypeEnum.NameToIndex("手套"),
                            _ => EquipmentTypeEnum.NameToIndex("盔甲"),
                        };
                    }
                    typeof(EquipableModule).GetField("type", ZetanUtility.CommonBindingFlags).SetValue(module, equipType);
                    module = GetOrAddModule(item, typeof(GemSlotModule));
                    typeof(GemSlotModule).GetProperty("SlotCount", ZetanUtility.CommonBindingFlags).SetValue(module, equipment.GemSlotAmout);
                    module = GetOrAddModule(item, typeof(DurabilityModule));
                    typeof(DurabilityModule).GetProperty("Durability", ZetanUtility.CommonBindingFlags).SetValue(module, old.MaxDurability);
                }
                else if (old is BookItem book)
                {
                    if (book.BookType == BookType.Building)
                    {
                        var module = GetOrAddModule(item, typeof(StructureBlueprintModule));
                        typeof(StructureBlueprintModule).GetProperty("Structure", ZetanUtility.CommonBindingFlags).SetValue(module, book.BuildingToLearn);
                        module = GetOrAddModule(item, typeof(UsableModule));
                        typeof(UsableModule).GetProperty("Usage", ZetanUtility.CommonBindingFlags).SetValue(module, ZetanUtility.Editor.LoadAsset<LearnToBuildStructure>());
                    }
                    else if (book.BookType == BookType.Making)
                    {
                        AddModule(item, typeof(MakingBlueprintModule));
                        var module = GetOrAddModule(item, typeof(UsableModule));
                        typeof(UsableModule).GetProperty("Usage", ZetanUtility.CommonBindingFlags).SetValue(module, ZetanUtility.Editor.LoadAsset<LearnToMakeItem>());
                    }
                }
                else if (old is BagItem bag)
                {
                    var module = GetOrAddModule(item, typeof(SpaceExpandModule));
                    typeof(SpaceExpandModule).GetProperty("Space", ZetanUtility.CommonBindingFlags).SetValue(module, bag.ExpandSize);
                    module = GetOrAddModule(item, typeof(UsableModule));
                    typeof(UsableModule).GetProperty("Usage", ZetanUtility.CommonBindingFlags).SetValue(module, ZetanUtility.Editor.LoadAsset<ExpandBackpackSpace>());
                }
                if (old.MaterialType != global::MaterialType.None)
                {
                    var module = GetOrAddModule(item, typeof(MaterialModule));
                    int materialType = old.MaterialType switch
                    {
                        global::MaterialType.Ore => MaterialTypeEnum.NameToIndex("矿石"),
                        global::MaterialType.Metal => MaterialTypeEnum.NameToIndex("矿石"),
                        global::MaterialType.Plant => MaterialTypeEnum.NameToIndex("纤维"),
                        global::MaterialType.Cloth => MaterialTypeEnum.NameToIndex("布料"),
                        global::MaterialType.Meat => MaterialTypeEnum.NameToIndex("兽肉"),
                        global::MaterialType.Fur => MaterialTypeEnum.NameToIndex("皮革"),
                        global::MaterialType.Fruit => MaterialTypeEnum.NameToIndex("果实"),
                        global::MaterialType.Liquid => MaterialTypeEnum.NameToIndex("液体"),
                        global::MaterialType.Condiment => MaterialTypeEnum.NameToIndex("调料"),
                        _ => 0,
                    };
                    typeof(MaterialModule).GetField("type", ZetanUtility.CommonBindingFlags).SetValue(module, materialType);
                }
            }
            #endregion
        }
        #endregion
#endif
    }
}