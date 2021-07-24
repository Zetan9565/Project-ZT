using System.Collections.Generic;
using UnityEngine;

public class Field : Building
{
    public FieldData Data { get; private set; }

    public List<Crop> Crops { get; } = new List<Crop>();

    public new Collider2D collider;

    public override bool Interactive
    {
        get
        {
            return base.Interactive && Data && FieldManager.Instance.CurrentField != this;
        }
    }

    public override void OnCancelManage()
    {
        if (FieldManager.Instance.CurrentField == this)
            FieldManager.Instance.CancelManage();
    }

    public override void AskDestroy()
    {
        ConfirmManager.Instance.New("耕地内的作物不会保留，确定退耕吗？",
            delegate { BuildingManager.Instance.DestroyBuilding(this); });
    }

    public void Init(FieldInformation field)
    {

    }

    public void PlantCrop(CropInformation crop, Vector3 position)
    {
        if (!crop) return;

        CropData cropData = Data.PlantCrop(crop);

        if (cropData)
        {
            Crop entity = ObjectPool.Get(crop.Prefab, transform);
            entity.Plant(cropData, this, position);
        }
    }

    public void RemoveCrop(Crop crop)
    {
        if (!crop) return;

        Data.RemoveCrop(crop.Data);
    }

    public override void Destroy()
    {
        base.Destroy();

    }

    protected override void OnBuilt()
    {
        if (MBuildingInfo.Addendas.Count > 0)
        {
            if (MBuildingInfo.Addendas[0] is FieldInformation info)
                Data = new FieldData(info);
        }
    }

    public override void OnManage()
    {
        base.OnManage();
        FieldManager.Instance.Manage(this);
    }
}
