using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    public class ItemData
    {
        public readonly string ID;

        public string ModelID => Model ? Model.ID : string.Empty;
        public string Name => Model ? Model.Name : string.Empty;
        public string Description => Model ? Model.Description : string.Empty;
        public ItemType Type => Model ? Model.Type : new ItemType();
        public ItemQuality Quality => Model ? Model.Quality : new ItemQuality();
        public bool Discardable => Model ? Model.Discardable : false;
        public float Weight => Model ? Model.Weight : 0f;
        public int StackLimit => Model ? Model.StackLimit : 1;
        public bool StackAble => Model ? Model.StackAble : false;
        public bool UnlimitStack => Model ? Model.InfiniteStack : false;

        public Item Model { get; }

        public bool IsLocked
        {
            get
            {
                if (GetModuleData<LockableData>() is LockableData data) return data.isLocked;
                else return false;
            }
            set
            {
                if (GetModuleData<LockableData>() is LockableData data) data.isLocked = value;
            }
        }

        public bool IsInstance => ID != ModelID;

        private readonly List<ItemModuleData> moduleDatas = new List<ItemModuleData>();
        private ReadOnlyCollection<ItemModuleData> readOnlyModuleDatas;
        public ReadOnlyCollection<ItemModuleData> ModuleDatas
        {
            get
            {
                if (readOnlyModuleDatas == null) readOnlyModuleDatas = moduleDatas.AsReadOnly();
                return readOnlyModuleDatas;
            }
        }

        private readonly KeyedByTypeCollection<ItemModuleData> keydModuleDatas = new KeyedByTypeCollection<ItemModuleData>();

        public T GetModule<T>() where T : ItemModule => Model.GetModule<T>();

        public T GetModuleData<T>() where T : ItemModuleData
        {
            if (keydModuleDatas.TryGetValue(typeof(T), out var data)) return data as T;
            else return null;
        }

        public ItemData(Item model, bool instance = true)
        {
            Model = model;
            ID = instance ? $"{model.ID}-{Guid.NewGuid():N}" : model.ID;
            if (instance)
                foreach (var module in model.Modules)
                {
                    if (module is not CommonModule && module.CreateData(this) is ItemModuleData moduleData)
                    {
                        moduleDatas.Add(moduleData);
                        keydModuleDatas.Add(moduleData);
                    }
                }
        }

        public static implicit operator bool(ItemData self)
        {
            return self != null;
        }
    }
}