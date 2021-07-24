using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropData
{
    public CropInformation Info { get; }

    public Crop entity;

    public string entityID;

    public FieldData parent;

    public int currentStageIndex;

    public CropStage currentStage;
    public CropStage nextStage;

    public bool harvestAble;
    public int harvestTimes;

    public float growthTime;
    public int growthDays;

    public float stageTime;
    public int stageDays;

    public float growthRate;

    public bool dry;
    public bool pest;

    public event CropStageListner OnStageChange;

    public CropData(CropInformation info, FieldData field)
    {
        Info = info;
        parent = field;
        GameManager.CropDatas.TryGetValue(Info, out var crops);
        if (crops != null)
        {
            entityID = Info.ID + "I" + crops.Count.ToString().PadLeft(4);
            crops.Add(this);
        }
        else
        {
            entityID = Info.ID + "I0000";
            GameManager.CropDatas.Add(Info, new List<CropData>() { this });
        }
    }

    public void Grow(float realTime)
    {
        float deltaTime = realTime * growthRate;

        growthTime += deltaTime;
        growthDays = Mathf.CeilToInt(growthTime / TimeManager.OneDay);

        if (currentStage.LastingDays < 0)
            return;
        stageTime += deltaTime;
        stageDays = Mathf.CeilToInt(stageTime / TimeManager.OneDay);
        while (stageDays >= currentStage.LastingDays)
        {
            ToNextStage();
        }
    }

    private void ToNextStage()
    {
        if (currentStage.LastingDays < 1) return;

        stageTime -= TimeManager.OneDay * currentStage.LastingDays;
        stageTime = stageTime < 0 ? 0 : stageTime;
        stageDays = Mathf.CeilToInt(stageTime / TimeManager.OneDay);
        currentStage = nextStage;
        currentStageIndex = Info.Stages.IndexOf(currentStage);
        harvestTimes++;
        if (currentStage.RepeatAble && (harvestTimes < currentStage.RepeatTimes || currentStage.RepeatTimes < 0))
            nextStage = Info.Stages[currentStage.IndexToReturn];
        else
        {
            nextStage = Info.Stages[currentStageIndex + 1];
            harvestTimes = 0;
        }
        if (currentStageIndex == Info.Stages.Count - 1)
        {
            parent.RemoveCrop(this);
        }
        OnStageChange?.Invoke(currentStage);
    }

    public void OnHarvest()
    {
        harvestTimes++;
        ToNextStage();
    }

    public List<ItemInfo> OnHarvestSuccess()
    {
        harvestAble = false;
        List<ItemInfo> lootItems = DropItemInfo.Drop(currentStage.GatherInfo.ProductItems);
        OnHarvest();
        return lootItems;
    }


    public static implicit operator bool(CropData self)
    {
        return self != null;
    }
}
