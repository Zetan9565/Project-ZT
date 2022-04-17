using System.Collections.Generic;
public class ItemData
{
    public readonly string ID;

    public string ModelID => Model ? Model.ID : string.Empty;
    public string Name => Model ? Model.Name : string.Empty;

    public ItemBase Model { get; }

    public bool isLocked;

    public bool IsInstance => ID != ModelID;

    public ItemData(ItemBase model, bool instance = true)
    {
        if (instance) ID = $"{model.ID}-{System.Guid.NewGuid():N}";
        else ID = model.ID;
        Model = model;
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