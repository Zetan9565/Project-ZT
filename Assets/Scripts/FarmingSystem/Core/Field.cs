using System.Collections.Generic;
using UnityEngine;

public class Field : Structure2D
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
            return base.IsInteractive && FData && !WindowsManager.IsWindowOpen<FieldWindow>();
        }
    }

    protected override void OnNotInteractable()
    {
        if (WindowsManager.IsWindowOpen<FieldWindow>(out var window) && window.CurrentField == this)
            window.CancelManage();
        base.OnNotInteractable();
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
            NotifyCenter.PostNotify(FieldManager.FieldCropPlanted, this, cropData);
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

    protected override void OnInit()
    {
        base.OnInit();
        if (Info.Addendas.Count > 0)
        {
            if (Info.Addendas[0] is FieldInformation info)
                Init(info);
        }
    }

    public override bool DoManage()
    {
        return WindowsManager.OpenWindowBy<FieldWindow>(this);
    }
}
