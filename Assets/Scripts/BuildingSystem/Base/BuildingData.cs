using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData
{
    public string IDPrefix;

    public string IDTail;

    public string entityID;
    public Building entity;
    public BuildingPreview preview;

    public string scene;
    public Vector3 position;

    public string name
    {
        get
        {
            return Info ? Info.name + (Vector2)position : "未设置";
        }
    }

    public bool IsBuilt { get; private set; }

    public BuildingInformation Info { get; private set; }

    public float leftBuildTime;

    public BuildingData(BuildingInformation info, Vector3 position)
    {
        Info = info;
        this.position = position;
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        leftBuildTime = Info.BuildTime;
    }

    public bool StartBuild()
    {
        if (!Info) return false;
        IDPrefix = Info.IDPrefix;
        leftBuildTime = Info.BuildTime;
        GetIDTail();
        if (string.IsNullOrEmpty(IDTail))
        {
            MessageManager.Instance.New($"[{name}]已经达到最大建设数量");
            return false;
        }
        entityID = IDPrefix + IDTail;
        IsBuilt = false;
        return true;
    }

    public void LoadBuild(BuildingInformation info, BuildingSaveData buildingData)
    {
        if (Info != info) return;
        IDPrefix = buildingData.IDPrefix;
        IDTail = buildingData.IDTail;
        leftBuildTime = buildingData.leftBuildTime;
        entityID = IDPrefix + IDTail;
        if (leftBuildTime > 0)
        {
            IsBuilt = false;
        }
        else
        {
            IsBuilt = true;
            OnBuilt();
        }
    }

    private void BuildComplete()
    {
        IsBuilt = true;
        OnBuilt();
    }

    public void TimePass(float realTime)
    {
        if (IsBuilt) return;
        leftBuildTime -= realTime;
        if (leftBuildTime <= 0)
            BuildComplete();
    }

    private void OnBuilt()
    {
        if (preview) preview.OnBuilt();
    }

    private void GetIDTail()
    {
        Building[] buildings = UnityEngine.Object.FindObjectsOfType<Building>();
        IDTail = string.Empty;
        for (int i = 1; i < 1000; i++)
        {
            IDTail = i.ToString().PadLeft(3, '0');
            string newID = IDPrefix + IDTail;
            if (!Array.Exists(buildings, x => x.EntityID == newID && x.Data != this))
                break;
        }
    }

    public static implicit operator bool(BuildingData self)
    {
        return self != null;
    }
}