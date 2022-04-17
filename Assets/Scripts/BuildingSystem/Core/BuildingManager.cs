using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent, AddComponentMenu("Zetan Studio/管理器/建筑管理器")]
public class BuildingManager : SingletonMonoBehaviour<BuildingManager>
{
    private Transform buildingRoot;
    public Transform BuildingRoot
    {
        get
        {
            if (!buildingRoot)
            {
                GameObject root = new GameObject("Buildings");
                buildingRoot = root.transform;
            }
            return buildingRoot;
        }
    }
    private readonly Dictionary<BuildingInformation, Transform> buildingGroups = new Dictionary<BuildingInformation, Transform>();

    public List<BuildingInformation> BuildingsLearned { get; } = new List<BuildingInformation>();
    private readonly Dictionary<string, List<BuildingData>> buildings = new Dictionary<string, List<BuildingData>>();

    #region 数据相关
    public bool Learn(BuildingInformation buildingInfo)
    {
        if (!buildingInfo) return false;
        if (HadLearned(buildingInfo))
        {
            ConfirmWindow.StartConfirm("这种设施已经学会建造。");
            return false;
        }
        BuildingsLearned.Add(buildingInfo);
        ConfirmWindow.StartConfirm(string.Format("学会了 [{0}] 的建造方法!", buildingInfo.Name));
        return true;
    }
    public bool HadLearned(BuildingInformation buildingInfo)
    {
        return BuildingsLearned.Contains(buildingInfo);
    }

    public void SaveData(SaveData data)
    {
        data.buildingSystemData.learneds = BuildingsLearned.Select(x => x.ID).ToArray();
        foreach (var dict in buildings)
        {
            foreach (var build in dict.Value)
            {
                data.buildingSystemData.buildingDatas.Add(new BuildingSaveData(build));
            }
        }
    }
    public void LoadData(BuildingSystemSaveData buildingSystemData)
    {
        BuildingsLearned.Clear();
        BuildingInformation[] buildingInfos = Resources.LoadAll<BuildingInformation>("Configuration");
        foreach (string learned in buildingSystemData.learneds)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.ID == learned);
            if (find) BuildingsLearned.Add(find);
        }
        foreach (BuildingSaveData saveData in buildingSystemData.buildingDatas)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.ID == saveData.modelID);
            if (find)
            {
                BuildingData data = new BuildingData(find, new Vector3(saveData.posX, saveData.posY, saveData.posZ));
                data.LoadBuild(find, saveData);
                if (data.leftBuildTime <= 0)
                {
                    DoneBuild(data);
                }
                else if (saveData.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    BuildingPreview2D preview = Instantiate(find.Preview);
                    preview.StartBuild(data);
                    MakeFlag(preview);
                }
                AddBuilding(data);
            }
        }
        AStarManager.Instance.UpdateGraphs();
    }
    #endregion

    #region 其它
    private void Update()
    {
        foreach (var list in buildings.Values)
        {
            list.ForEach(x => x.TimePass(Time.deltaTime));
        }
    }

    public void DestroyBuilding(BuildingData building)
    {
        if (!building) return;
        ConfirmWindow.StartConfirm(string.Format("确定拆毁{0}{1}吗？", building.Name, (Vector2)transform.position),
        delegate
        {
            if (building)
            {
                if (this.buildings.TryGetValue(building.Info.ID, out var buildings))
                {
                    buildings.Remove(building);
                    if (buildings.Count < 1) this.buildings.Remove(building.Info.ID);
                }
                if (building.entity && building.entity.gameObject)
                {
                    UpdateAStar(building.entity);
                    building.entity.Destroy();
                }
                else if (building.preview && building.preview.gameObject)
                {
                    UpdateAStar(building.preview);
                    building.preview.Destroy();
                }
                NotifyCenter.PostNotify(BuildingDestroy, building, buildings);
            }
        });

        static void UpdateAStar(Component comp)
        {
            if (AStarManager.Instance)
            {
                var colliders = comp.GetComponentsInChildren<Collider>();
                if (colliders.Length > 0)
                {
                    Vector3 min = colliders[0].bounds.min;
                    Vector3 max = colliders[0].bounds.max;
                    for (int i = 1; i < colliders.Length; i++)
                    {
                        if (ZetanUtility.Vector3LessThan(colliders[i].bounds.min, min))
                            min = colliders[i].bounds.min;
                        if (ZetanUtility.Vector3LargeThan(colliders[i].bounds.max, max))
                            max = colliders[i].bounds.max;
                    }
                    AStarManager.Instance.UpdateGraphs(min, max);
                }
                else
                {
                    var collider2Ds = comp.GetComponentsInChildren<Collider2D>();
                    if (collider2Ds.Length > 0)
                    {
                        Vector3 min = collider2Ds[0].bounds.min;
                        Vector3 max = collider2Ds[0].bounds.max;
                        for (int i = 1; i < collider2Ds.Length; i++)
                        {
                            if (ZetanUtility.Vector3LessThan(collider2Ds[i].bounds.min, min))
                                min = collider2Ds[i].bounds.min;
                            if (ZetanUtility.Vector3LargeThan(collider2Ds[i].bounds.max, max))
                                max = collider2Ds[i].bounds.max;
                        }
                        AStarManager.Instance.UpdateGraphs(min, max);
                    }
                }
            }
        }
    }

    public List<BuildingData> GetBuildings(BuildingInformation info)
    {
        this.buildings.TryGetValue(info.ID, out var buildings);
        return buildings;
    }
    public void TryBuild(BuildingInformation currentInfo, BuildingPreview2D preview)
    {
        if (BackpackManager.Instance.IsMaterialsEnough(currentInfo.Materials))
        {
            List<ItemWithAmount> materials = BackpackManager.Instance.GetMaterialsFromInventory(currentInfo.Materials);
            if (materials != null && BackpackManager.Instance.CanLose(materials))
            {
                BuildingData data = new BuildingData(currentInfo, preview.transform.position);
                if (data.StartBuild())
                {
                    //data.StartConstruct();
                    preview.StartBuild(data);
                    BackpackManager.Instance.LoseItem(materials);
                    AddBuilding(data);
                    MakeFlag(preview);
                    if (AStarManager.Instance)
                    {
                        var collider2Ds = preview.GetComponentsInChildren<Collider2D>();
                        if (collider2Ds.Length > 0)
                        {
                            Vector3 min = collider2Ds[0].bounds.min;
                            Vector3 max = collider2Ds[0].bounds.max;
                            for (int i = 1; i < collider2Ds.Length; i++)
                            {
                                if (ZetanUtility.Vector3LessThan(collider2Ds[i].bounds.min, min))
                                    min = collider2Ds[i].bounds.min;
                                if (ZetanUtility.Vector3LargeThan(collider2Ds[i].bounds.max, max))
                                    max = collider2Ds[i].bounds.max;
                            }
                            AStarManager.Instance.UpdateGraphs(min, max);
                        }
                        else
                        {
                            var colliders = preview.GetComponentsInChildren<Collider>();
                            if (colliders.Length > 0)
                            {
                                Vector3 min = colliders[0].bounds.min;
                                Vector3 max = colliders[0].bounds.max;
                                for (int i = 1; i < colliders.Length; i++)
                                {
                                    if (ZetanUtility.Vector3LessThan(colliders[i].bounds.min, min)) min = colliders[i].bounds.min;
                                    if (ZetanUtility.Vector3LargeThan(colliders[i].bounds.max, max)) max = colliders[i].bounds.max;
                                }
                                AStarManager.Instance.UpdateGraphs(min, max);
                            }
                        }
                    }
                }
            }
            else MessageManager.Instance.New("某些材料无法使用");
        }
        else MessageManager.Instance.New("耗材不足");

    }
    public void DoneBuild(BuildingData data)
    {
        if (!data) return;
        if (data.preview) data.preview.OnBuilt();
        data.preview = null;
        if (data.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) return;
        if (!buildingGroups.TryGetValue(data.Info, out var parent))
        {
            parent = new GameObject(data.Info.Name).transform;
            parent.SetParent(BuildingRoot);
            buildingGroups.Add(data.Info, parent);
        }
        Building2D building = Instantiate(data.Info.Prefab, parent);
        building.Init(data);
        MessageManager.Instance.New($"[{data.Name}] 建造完成了");
        NotifyCenter.PostNotify(BuildingBuilt, data);
    }
    public void AddBuilding(BuildingData data)
    {
        if (this.buildings.TryGetValue(data.Info.ID, out var buildings))
            buildings.Add(data);
        else
            this.buildings.Add(data.Info.ID, new List<BuildingData>() { data });
    }
    public void MakeFlag(BuildingPreview2D preview)
    {
        var bf = ObjectPool.Get(MiscSettings.Instance.BuildingFlagPrefab, UIManager.Instance.BuildingFlagParent).GetComponent<BuildingFlag>();
        bf.Init(preview);
    }
    #endregion

    #region 消息
    public const string BuildingBuilt = "BuildingBuilt";
    public const string BuildingDestroy = "BuildingDestroy";
    #endregion
}