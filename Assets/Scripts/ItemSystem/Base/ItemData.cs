using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio.ItemSystem
{
    public class ItemData
    {
        public readonly string ID;

        public string ModelID => Model ? Model.ID : string.Empty;
        public string Name => ItemFactory.GetName(Model);
        public string ColorName => ItemFactory.GetColorName(Model);
        public Sprite Icon => Model ? Model.Icon : null;
        public string Description => ItemFactory.GetDescription(Model);
        public ItemType Type => Model ? Model.Type : new ItemType();
        public ItemQuality Quality => Model ? Model.Quality : new ItemQuality();
        public bool Discardable => Model ? Model.Discardable : false;
        public float Weight => Model ? Model.Weight : 0f;
        public int StackLimit => Model ? Model.StackLimit : 1;
        public bool StackAble => Model ? Model.StackAble : false;
        public bool InfiniteStack => Model ? Model.InfiniteStack : false;
        public ReadOnlyCollection<ItemModule> Modules => Model ? Model.Modules : new ReadOnlyCollection<ItemModule>(new List<ItemModule>());

        public Item Model { get; }

        public bool IsLocked
        {
            get
            {
                if (TryGetModuleData<LockableData>(out var data)) return data.isLocked;
                else return false;
            }
            set
            {
                if (TryGetModuleData<LockableData>(out var data)) data.isLocked = value;
            }
        }

        public bool IsInstance => ID != ModelID;

        private readonly List<ItemModuleData> moduleData = new List<ItemModuleData>();
        private ReadOnlyCollection<ItemModuleData> readOnlyModuleData;
        public ReadOnlyCollection<ItemModuleData> ModuleData
        {
            get
            {
                if (readOnlyModuleData == null) readOnlyModuleData = moduleData.AsReadOnly();
                return readOnlyModuleData;
            }
        }

        private readonly Dictionary<Type, ItemModuleData> keydModuleData = new Dictionary<Type, ItemModuleData>();

        public T GetModule<T>() where T : ItemModule => Model.GetModule<T>();
        public bool TryGetModule<T>(out T module) where T : ItemModule
        {
            return module = GetModule<T>();
        }
        public CommonModule<T> GetCommonModule<T>(string name) => Model.GetCommonModule<T>(name);
        public bool TryGetCommonModule<T>(string name, out CommonModule<T> module)
        {
            return module = GetCommonModule<T>(name);
        }
        public T GetModuleData<T>() where T : ItemModuleData
        {
            if (keydModuleData.TryGetValue(typeof(T), out var data)) return data as T;
            else return null;
        }
        public bool TryGetModuleData<T>(out T module) where T : ItemModuleData
        {
            return module = GetModuleData<T>();
        }
        public ItemData(Item model) : this(model, true) { }
        private ItemData(Item model, bool instance = true)
        {
            Model = model;
            ID = instance ? $"{model.ID}-{Guid.NewGuid():N}" : model.ID;
            if (instance)
                foreach (var module in model.Modules)
                {
                    if (module is not CommonModule && module.CreateData(this) is ItemModuleData moduleData)
                    {
                        this.moduleData.Add(moduleData);
                        keydModuleData.Add(moduleData.GetType(), moduleData);
                    }
                }
        }
        public ItemData(GenericData data)
        {
            if (data.TryReadString("modelID", out var modelID)) Model = ItemFactory.GetModel(modelID);
            else throw new KeyNotFoundException("modelID");
            if (data.TryReadString("ID", out var ID)) this.ID = ID;
            else throw new KeyNotFoundException("ID");

            foreach (var module in Model.Modules)
            {
                if (module is not CommonModule && module.CreateData(this) is ItemModuleData moduleData)
                {
                    this.moduleData.Add(moduleData);
                    keydModuleData.Add(moduleData.GetType(), moduleData);
                    if (data.TryReadData("modules", out var modules))
                    {
                        if (modules.TryReadData(module.GetType().FullName, out var msd))
                            moduleData.LoadSaveData(msd);
                    }
                }
            }
        }
        public static implicit operator bool(ItemData self)
        {
            return self != null;
        }

        public GenericData GetSaveData()
        {
            var data = new GenericData();
            data["ID"] = ID;
            data["modelID"] = ModelID;
            var modules = new GenericData();
            data["modules"] = modules;
            foreach (var module in moduleData)
            {
                var md = module.GetSaveData();
                if (md != null) modules[module.GetType().FullName] = md;
            }
            return data;
        }

        public static ItemData Empty(Item model) => new ItemData(model, false);
    }
}