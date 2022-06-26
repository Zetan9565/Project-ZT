using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZetanStudio.ItemSystem
{
    [CreateAssetMenu]
    public sealed class ItemDatabase : SingletonScriptableObject<ItemDatabase>
    {
        [SerializeField]
        private List<Item> items = new List<Item>();

        public static Dictionary<string, Item> ToDictionary()
        {
            if (Instance) return Instance.items.ToDictionary(x => x.ID);
            else return null;
        }

        public static List<Item> GetItems()
        {
            return new List<Item>(Instance.items);
        }
        public static List<Item> GetItemsWhere(System.Predicate<Item> predicate)
        {
            var results = new List<Item>();
            foreach (var item in Instance.items)
            {
                if (predicate(item))
                    results.Add(item);
            }
            return results;
        }

#if UNITY_EDITOR
        public static class Editor
        {
            public static List<Item> GetItems()
            {
                return GetOrCreate().items;
            }
            public static List<Item> GetItems(ItemTemplate template)
            {
                List<Item> results = new List<Item>();
                foreach (var item in GetOrCreate().items)
                {
                    if (Item.Editor.MatchTemplate(item, template))
                        results.Add(item);
                }
                return results;
            }
            public static List<Item> GetItemsWhere(System.Predicate<Item> predicate)
            {
                List<Item> results = new List<Item>();
                foreach (var item in GetOrCreate().items)
                {
                    if (predicate(item))
                        results.Add(item);
                }
                return results;
            }

            public static Item CloneItem(Item item)
            {
                var instance = GetOrCreate();
                Item cloned = Instantiate(item);
                instance.items.Add(cloned);
                AssetDatabase.AddObjectToAsset(cloned, instance);
                return cloned;
            }
            public static Item MakeItem(ItemTemplate template)
            {
                var instance = GetOrCreate();
                Item item = CreateInstance<Item>();
                Item.Editor.ApplyTemplate(item, template);
                Item.Editor.SetAutoID(item, instance.items, template ? template.IDPrefix : null);
                item.name = item.ID;
                ZetanUtility.Editor.SaveChange(item);
                instance.items.Add(item);
                AssetDatabase.AddObjectToAsset(item, instance);
                ZetanUtility.Editor.SaveChange(instance);
                return item;
            }
            public static Item MakeItem(ItemFilterAttribute itemFilter)
            {
                var instance = GetOrCreate();
                Item item = CreateInstance<Item>();
                Item.Editor.ApplyFilter(item, itemFilter);
                if (string.IsNullOrEmpty(item.ID)) Item.Editor.SetAutoID(item, instance.items);
                item.name = item.ID;
                ZetanUtility.Editor.SaveChange(item);
                instance.items.Add(item);
                AssetDatabase.AddObjectToAsset(item, instance);
                ZetanUtility.Editor.SaveChange(instance);
                return item;
            }
            public static bool DeleteItem(Item item)
            {
                if (!item || !instance) return false;
                if (!Instance.items.Remove(item)) return false;
                AssetDatabase.RemoveObjectFromAsset(item);
                ZetanUtility.Editor.SaveChange(instance);
                DestroyImmediate(item);
                return true;
            }
        }
#endif
    }
}