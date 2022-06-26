using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.ItemSystem
{
    public static class ItemFactory
    {
        public readonly static Dictionary<string, Item> models = new Dictionary<string, Item>();
        public static ReadOnlyDictionary<string, Item> Models => new ReadOnlyDictionary<string, Item>(models);

        public readonly static Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
        public static ReadOnlyDictionary<string, ItemData> Items => new ReadOnlyDictionary<string, ItemData>(items);

        private readonly static Dictionary<string, ItemData> stackableItems = new Dictionary<string, ItemData>();

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            models.Clear();
            foreach (var item in Item.GetItems())
            {
                models[item.ID] = item;
            }
        }

        public static Item GetModel(string ID)
        {
            if (models.TryGetValue(ID, out var model)) return model;
            return null;
        }

        public static ItemData MakeItem(Item model)
        {
            ItemData item;
            if (model.StackAble)
            {
                if (!stackableItems.TryGetValue(model.ID, out item))
                {
                    item = new ItemData(model);
                    stackableItems[model.ID] = item;
                    items[item.ID] = item;
                }
            }
            else
            {
                item = new ItemData(model);
                items[item.ID] = item;
            }
            return item;
        }
        public static ItemData MakeItem(string ID)
        {
            if (models.TryGetValue(ID, out var model))
                return MakeItem(model);
            else return null;
        }
        public static ItemData GetItem(string ID)
        {
            items.TryGetValue(ID, out var item);
            return item;
        }

        [SaveMethod(999)]
        public static void SaveData(SaveData saveData)
        {
            var items = new SaveDataItem();
            foreach (var item in ItemFactory.items)
            {
                items.subData[item.Key] = item.Value.GetSaveData();
            }
            saveData.data["items"] = items;
        }
        [LoadMethod(-999)]
        public static void LoadData(SaveData saveData)
        {
            ItemFactory.items.Clear();
            if (saveData.data.TryGetValue("items", out var items))
                foreach (var sdi in items.subData.Values)
                {
                    if (sdi.stringData.TryGetValue("ID", out var ID))
                        if (!ItemFactory.items.ContainsKey(ID))
                        {
                            var item = new ItemData(sdi);
                            ItemFactory.items[ID] = item;
                            if (item.Model.StackAble) stackableItems[item.ModelID] = item;
                        }
                }
            ZetanUtility.Log(ZetanUtility.SerializeObject(ItemFactory.items, false));
        }
    }
}