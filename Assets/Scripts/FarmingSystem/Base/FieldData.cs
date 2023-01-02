using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.FarmingSystem;

public class FieldData
{
    public FieldInformation Info { get; }

    public Field entity;

    public int fertility;

    public int humidity;

    public int spaceOccup;

    public List<CropData> Crops { get; } = new List<CropData>();

    private readonly List<CropData> dieCrops = new List<CropData>();

    public FieldData(FieldInformation info)
    {
        Info = info;
        humidity = info.Humidity;
        spaceOccup = info.Capacity;
    }

    public CropData PlantCrop(CropInformation crop)
    {
        if (!crop) return null;

        if (spaceOccup < crop.Size) return null;

        CropData cropData = new CropData(crop, this);
        Crops.Add(cropData);
        return cropData;
    }

    public void RemoveCrop(CropData crop)
    {
        if (!crop) return;
        dieCrops.Add(crop);
    }

    public void TimePass(float realTime)
    {
        foreach (CropData crop in Crops)
        {
            crop.Grow(realTime);
        }
        foreach (CropData crop in dieCrops)
        {
            if (crop.entity) crop.entity.Recycle();
            if (GameManager.Crops.TryGetValue(crop.Info, out var find))
                find.Remove(crop.entity);

            Crops.Remove(crop);
            if (GameManager.CropDatas.TryGetValue(crop.Info, out var find2))
                find2.Remove(crop);
        }
        dieCrops.Clear();
    }

    public void OnDestroy()
    {
        Crops.Clear();
        dieCrops.Clear();
    }

    public static implicit operator bool(FieldData obj)
    {
        return obj != null;
    }
}
