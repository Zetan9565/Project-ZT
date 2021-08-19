using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropData
{
    public CropInformation Info { get; }

    public Crop entity;

    public string entityID;

    public FieldData parent;

    public int currentStageIndex;

    public CropStage currentStage;
    [HideInInspector]
    public CropStage nextStage;

    public bool HarvestAble => currentStage && currentStage.HarvestAble;
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
        growthTime = 0;
        growthDays = 1;
        stageTime = 0;
        stageDays = 0;
        growthRate = 1;
        harvestTimes = 0;
        dry = false;
        pest = false;
        parent = field;

        currentStageIndex = 0;
        currentStage = info.Stages[0];
        HandlingNextStage();

        GameManager.CropDatas.TryGetValue(Info, out var crops);
        if (crops != null)
        {
            entityID = Info.ID + "N" + crops.Count.ToString().PadLeft(4, '0');
            crops.Add(this);
        }
        else
        {
            entityID = Info.ID + "N0000";
            GameManager.CropDatas.Add(Info, new List<CropData>() { this });
        }
    }

    public void Grow(float realTime)
    {
        float deltaTime = realTime * growthRate;

        growthTime += deltaTime;
        growthDays = Mathf.CeilToInt(growthTime / TimeManager.Instance.ScaleDayToReal);

        //if (realTime > 1) Debug.Log("pass: " + (float)realTime + " " + currentStage);

        if (!currentStage || currentStage.LastingDays < 0)
            return;

        stageTime += deltaTime;
        stageDays = Mathf.CeilToInt(stageTime / TimeManager.Instance.ScaleDayToReal);
        while (stageDays > currentStage.LastingDays && ToNextStage()) ;
    }

    private bool ToNextStage()
    {
        if (currentStage.LastingDays < 1) return false;

        stageTime -= TimeManager.Instance.ScaleDayToReal * currentStage.LastingDays;
        stageTime = stageTime < 0 ? 0 : stageTime;
        stageDays = Mathf.CeilToInt(stageTime / TimeManager.Instance.ScaleDayToReal);

        currentStage = nextStage;
        OnStageChange?.Invoke(currentStage);
        if (!currentStage)
        {
            parent.RemoveCrop(this);
            return false;
        }
        currentStageIndex = Info.Stages.IndexOf(currentStage);

        harvestTimes++;
        HandlingNextStage();

        return true;
    }

    private void HandlingNextStage()
    {
        int nextIndex;
        if (currentStage.RepeatAble && (harvestTimes < currentStage.RepeatTimes || currentStage.RepeatTimes < 0))
            nextIndex = currentStage.IndexToReturn;
        else
        {
            nextIndex = currentStageIndex + 1;
            harvestTimes = 0;
        }
        if (nextIndex > 0 && nextIndex < Info.Stages.Count)
            nextStage = Info.Stages[nextIndex];
        else nextStage = null;
    }

    public void OnHarvest()
    {
        ToNextStage();
    }

    void CheckRate(int humidity)
    {

    }

    public static implicit operator bool(CropData self)
    {
        return self != null;
    }
}
