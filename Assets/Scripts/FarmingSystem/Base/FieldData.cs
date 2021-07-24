using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldData
{
    public FieldInformation Info { get; }

    public Field entity;

    public int fertility;

    public int humidity;

    public int spaceOccup;

    public List<CropData> Crops { get; } = new List<CropData>();

    public FieldData(FieldInformation info)
    {
        Info = info;
        humidity = info.Humidity;
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
        Crops.Remove(crop);
    }

    public void TimePass(float realTime)
    {
        foreach (CropData crop in Crops)
        {
            crop.Grow(realTime);
        }
    }

    public void OnDestroy()
    {
        Crops.Clear();
    }

    public static implicit operator bool(FieldData self)
    {
        return self != null;
    }
}
