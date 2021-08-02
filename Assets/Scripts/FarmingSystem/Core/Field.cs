using System.Collections.Generic;
using UnityEngine;

public class Field : Building
{
    public FieldData FData { get; private set; }

    public List<Crop> Crops { get; } = new List<Crop>();

    public Renderer Range { get; private set; }

    private void Awake()
    {
        Range = GetComponent<Renderer>();
    }

    public override bool IsInteractive
    {
        get
        {
            return base.IsInteractive && FData && FieldManager.Instance.CurrentField != this;
        }
    }

    public override void OnCancelManage()
    {
        base.OnCancelManage();
        if (FieldManager.Instance.CurrentField == this)
            FieldManager.Instance.CancelManage();
    }

    public void Init(FieldInformation field)
    {
        FData = new FieldData(field)
        {
            entity = this
        };
        FieldManager.Instance.Reclaim(FData);
    }

    public void PlantCrop(CropInformation crop, Vector3 position)
    {
        if (!crop) return;

        if (FData.spaceOccup < crop.Size)
        {
            MessageManager.Instance.New("空间不足");
            return;
        }

        CropData cropData = FData.PlantCrop(crop);

        if (cropData)
        {
            Crop entity = ObjectPool.Get(crop.Prefab, transform);
            entity.Init(cropData, this, position);
            FieldManager.Instance.Plant(entity);
        }
    }

    public void Remove(Crop crop)
    {
        if (!crop) return;
        FData.RemoveCrop(crop.Data);
    }

    public override void Destroy()
    {
        FData.OnDestroy();
        base.Destroy();
    }

    protected override void OnBuilt()
    {
        if (Info.Addendas.Count > 0)
        {
            if (Info.Addendas[0] is FieldInformation info)
                Init(info);
        }
    }

    public override void OnManage()
    {
        base.OnManage();
        FieldManager.Instance.Manage(this);
    }
}
