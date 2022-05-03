using System.Collections;
using UnityEngine;

public class WarehouseData : StructureData, IWarehouseKeeper
{
    public string name;

    public Inventory Inventory { get; }

    public override string Name
    {
        get
        {
            if (entity) return entity.Name;
            else return "无实体仓库";
        }
    }

    public string WarehouseName => Name;

    public WarehouseData(StructureInformation info, Vector3 position, int space) : base(info, position)
    {
        Inventory = new Inventory(space, ignoreLock: true);
    }
}