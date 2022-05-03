using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZetanStudio.Item;
using ZetanStudio.Item.Module;

public class ItemData
{
    public readonly string ID;

    public string ModelID => Model_old ? Model_old.ID : string.Empty;
    public string Name => Model_old ? Model_old.Name : string.Empty;

    public ItemBase Model_old { get; }
    public ItemNew Model { get; }

    public bool isLocked;

    public bool IsInstance => ID != ModelID;

    public readonly List<ItemModuleData> moduleDatas = new List<ItemModuleData>();
    private ReadOnlyCollection<ItemModuleData> readOnlyModuleDatas;
    public ReadOnlyCollection<ItemModuleData> ModuleDatas
    {
        get
        {
            if (readOnlyModuleDatas == null) readOnlyModuleDatas = moduleDatas.AsReadOnly();
            return readOnlyModuleDatas;
        }
    }

    private readonly Dictionary<Type, ItemModuleData> keydModuleDatas = new Dictionary<Type, ItemModuleData>();

    public ItemModule GetModule<T>() where T : ItemModule => Model.GetModule<T>();

    public ItemModuleData<T> GetModuleData<T>() where T : ItemModule
    {
        if (keydModuleDatas.TryGetValue(typeof(T), out var data)) return data as ItemModuleData<T>;
        else return null;
    }

    public ItemData(ItemBase model, bool instance = true)
    {
        if (instance) ID = $"{model.ID}-{Guid.NewGuid():N}";
        else ID = model.ID;
        Model_old = model;
    }
    public ItemData(ItemNew model, bool instance = true)
    {
        if (instance) ID = $"{model.ID}-{Guid.NewGuid():N}";
        else ID = model.ID;
        Model = model;
        if (IsInstance)
            foreach (var module in model.Modules)
            {
                if (module.CreateData(this) is ItemModuleData moduleData)
                {
                    moduleDatas.Add(moduleData);
                    keydModuleDatas[model.GetType()] = moduleData;
                }
            }
    }

    public static implicit operator bool(ItemData self)
    {
        return self != null;
    }
}
public class EquipmentData : ItemData
{
    public List<GemItem> gems = new List<GemItem>();

    public EquipmentData(ItemBase model, bool instance = true) : base(model, instance)
    {
    }
}