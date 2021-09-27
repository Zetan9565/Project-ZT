using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public WarehouseData materialsKeeper;

    public List<ItemInfoBase> materialsStored = new List<ItemInfoBase>();

    public string name
    {
        get
        {
            return Info ? Info.Name + (Vector2)position : "未设置";
        }
    }

    public bool IsBuilt { get; private set; }
    public bool IsBuilding { get; private set; }

    public BuildingInformation Info { get; private set; }

    public float leftBuildTime;
    public BuildingStage currentStage;
    private int currentStageIndex;

    public BuildingData(BuildingInformation info, Vector3 position)
    {
        Info = info;
        this.position = position;
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    public bool StartBuild()
    {
        if (!Info) return false;
        IDPrefix = Info.IDPrefix;
        GetIDTail();
        if (string.IsNullOrEmpty(IDTail))
        {
            MessageManager.Instance.New($"[{name}]已经达到最大建设数量");
            return false;
        }
        entityID = IDPrefix + IDTail;
        IsBuilt = false;
        currentStageIndex = 0;
        leftBuildTime = 0;
        HandlingStage();
        if (Info.AutoBuild)
            StartConstruct();
        return true;
    }

    public void ToNextStage()
    {
        currentStageIndex++;
        if (currentStageIndex >= Info.Stages.Count)
        {
            if (preview) preview.OnDoneConstruct();
            BuildComplete();
        }
        else
        {
            HandlingStage();
            if (Info.AutoBuild && IsMaterialsEnough(currentStage.Materials)) return;//自动建造且材料充足，则不停止施工
            if (preview) preview.OnDoneConstruct();
            PauseConstruct();
        }
    }

    private bool IsMaterialsEnough(IEnumerable<MaterialInfo> targetMaterials)
    {
        if (targetMaterials == null || materialsStored == null || materialsStored.Count() < 1) return false;
        foreach (var material in targetMaterials)
        {
            if (material.MakingType == MakingType.SingleItem)
            {
                if (material.Item.StackAble)
                {
                    ItemInfoBase find = materialsStored.Find(x => x.ItemID == material.ItemID);
                    if (!find) return false;//所提供的材料中没有这种材料
                    if (find.Amount < material.Amount) return false;//若材料数量不足，则无法制作
                }
                else if (materialsStored.FindAll(x => x.ItemID == material.ItemID).Count < material.Amount)
                {
                    return false;
                }
            }
            else
            {
                var finds = materialsStored.Where(x => x.item.MaterialType == material.MaterialType);//找到种类相同的道具
                if (finds.Count() > 0)
                {
                    if (finds.Select(x => x.Amount).Sum() < material.Amount) return false;//若材料总数不足，则无法制作
                }
                else return false;//材料不足
            }
        }
        return true;
    }

    private void CostMaterials(IEnumerable<MaterialInfo> targetMaterials)
    {
        foreach (var material in targetMaterials)
        {
            if (material.MakingType == MakingType.SingleItem)
            {
                if (material.Item.StackAble)
                {
                    ItemInfoBase find = materialsStored.Find(x => x.ItemID == material.ItemID);
                    if (find) find.Amount -= material.Amount;
                }
                else
                {
                    int amount = material.Amount;
                    var finds = materialsStored.FindAll(x => x.ItemID == material.ItemID);
                    if (finds.Count >= amount)
                        while (amount > 0)
                        {
                            materialsStored.Remove(finds[0]);
                            finds.RemoveAt(0);
                            amount--;
                        }
                }
            }
            else
            {
                var finds = materialsStored.Where(x => x.item.MaterialType == material.MaterialType);//找到种类相同的道具
                if (finds.Count() > 0)
                {

                }
            }
        }
    }

    private void HandlingStage()
    {
        if (currentStageIndex < 0) return;
        currentStage = Info.Stages[currentStageIndex];
        if (currentStageIndex > 0)
        {
            leftBuildTime = currentStage.BuildTime - leftBuildTime;
        }
        else leftBuildTime = currentStage.BuildTime;
    }

    public void LoadBuild(BuildingInformation info, BuildingSaveData buildingData)
    {
        if (Info != info) return;
        IDPrefix = buildingData.IDPrefix;
        IDTail = buildingData.IDTail;
        leftBuildTime = buildingData.leftBuildTime;
        currentStageIndex = buildingData.stageIndex;
        entityID = IDPrefix + IDTail;
        if (leftBuildTime > 0 && currentStageIndex >= 0 && currentStageIndex < info.Stages.Count)
        {
            IsBuilt = false;
            if (Info.AutoBuild)
                StartConstruct();
        }
        else
        {
            BuildComplete();
        }
    }

    private void BuildComplete()
    {
        IsBuilt = true;
        IsBuilding = false;
        OnBuilt();
    }

    public void StartConstruct()
    {
        IsBuilding = true;
    }
    public void PauseConstruct()
    {
        IsBuilding = false;
    }

    public void TimePass(float realTime)
    {
        if (IsBuilt || !IsBuilding) return;
        leftBuildTime -= realTime;
        if (leftBuildTime <= 0)
        {
            ToNextStage();
        }
    }

    private void OnBuilt()
    {
        if (preview) preview.OnBuilt();
        preview = null;
    }

    public void PutMaterials(IEnumerable<ItemInfoBase> materials)
    {
        if (IsBuilt) return;
        foreach (var material in materials)
        {
            if (material.item.StackAble)
            {
                ItemInfoBase find = materialsStored.Find(x => x.ItemID == material.ItemID);
                if (find) find.Amount += material.Amount;
                else materialsStored.Add(new ItemInfoBase(material.item, material.Amount));
            }
            else
            {
                materialsStored.Add(new ItemInfoBase(material.item));
            }
        }
        if (Info.AutoBuild && !IsBuilding)
        {
            if (IsMaterialsEnough(currentStage.Materials))
                StartConstruct();
        }
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

    public List<string> GetMaterialsInfoString(IEnumerable<MaterialInfo> materials)
    {
        List<string> info = new List<string>();
        using (var materialEnum = materials.GetEnumerator())
            while (materialEnum.MoveNext())
                if (materialEnum.Current.MakingType == MakingType.SingleItem)
                {
                    int amount = 0;
                    if (materialEnum.Current.Item.StackAble)
                    {
                        ItemInfoBase find = materialsStored.Find(x => x.ItemID == materialEnum.Current.ItemID);
                        if (find) amount = find.Amount;
                    }
                    else
                    {
                        amount = materialsStored.FindAll(x => x.ItemID == materialEnum.Current.ItemID).Count();
                    }
                    info.Add($"{materialEnum.Current.ItemName}\t[{amount}/{materialEnum.Current.Amount}]");
                }
                else
                {
                    var finds = materialsStored.FindAll(x => x.item.MaterialType == materialEnum.Current.MaterialType);
                    int amount = 0;
                    foreach (var item in finds)
                        amount += item.Amount;
                    info.Add($"{materialEnum.Current.ItemName}\t[{amount}/{materialEnum.Current.Amount}]");
                }
        return info;
    }

    public static implicit operator bool(BuildingData self)
    {
        return self != null;
    }
}