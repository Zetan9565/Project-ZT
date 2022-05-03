using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent, AddComponentMenu("Zetan Studio/管理器/建筑管理器")]
public class StructureManager : SingletonMonoBehaviour<StructureManager>
{
    private Transform structureRoot;
    public Transform StructureRoot
    {
        get
        {
            if (!structureRoot)
            {
                GameObject root = new GameObject("Structures");
                structureRoot = root.transform;
            }
            return structureRoot;
        }
    }
    private readonly Dictionary<StructureInformation, Transform> structureGroups = new Dictionary<StructureInformation, Transform>();

    public List<StructureInformation> StructuresLearned { get; } = new List<StructureInformation>();
    private readonly Dictionary<string, List<StructureData>> structures = new Dictionary<string, List<StructureData>>();

    #region 数据相关
    public bool Learn(StructureInformation structureInfo)
    {
        if (!structureInfo) return false;
        if (HadLearned(structureInfo))
        {
            ConfirmWindow.StartConfirm("这种设施已经学会建造。");
            return false;
        }
        StructuresLearned.Add(structureInfo);
        ConfirmWindow.StartConfirm(string.Format("学会了 [{0}] 的建造方法!", structureInfo.Name));
        return true;
    }
    public bool HadLearned(StructureInformation structureInfo)
    {
        return StructuresLearned.Contains(structureInfo);
    }

    public void SaveData(SaveData data)
    {
        data.structureSystemData.learneds = StructuresLearned.Select(x => x.ID).ToArray();
        foreach (var dict in structures)
        {
            foreach (var build in dict.Value)
            {
                data.structureSystemData.structureDatas.Add(new StructureSaveData(build));
            }
        }
    }
    public void LoadData(StructureSystemSaveData structureSystemData)
    {
        StructuresLearned.Clear();
        StructureInformation[] structureInfos = Resources.LoadAll<StructureInformation>("Configuration");
        foreach (string learned in structureSystemData.learneds)
        {
            StructureInformation find = Array.Find(structureInfos, x => x.ID == learned);
            if (find) StructuresLearned.Add(find);
        }
        foreach (StructureSaveData saveData in structureSystemData.structureDatas)
        {
            StructureInformation find = Array.Find(structureInfos, x => x.ID == saveData.modelID);
            if (find)
            {
                StructureData data = new StructureData(find, new Vector3(saveData.posX, saveData.posY, saveData.posZ));
                data.LoadBuild(find, saveData);
                if (data.leftBuildTime <= 0)
                {
                    DoneBuild(data);
                }
                else if (saveData.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    StructurePreview2D preview = Instantiate(find.Preview);
                    preview.StartBuild(data);
                    MakeFlag(preview);
                }
                AddStructure(data);
            }
        }
        AStarManager.Instance.UpdateGraphs();
    }
    #endregion

    #region 其它
    private void Update()
    {
        foreach (var list in structures.Values)
        {
            list.ForEach(x => x.TimePass(Time.deltaTime));
        }
    }

    public void DestroyStructure(StructureData structure)
    {
        if (!structure) return;
        ConfirmWindow.StartConfirm(string.Format("确定拆毁{0}{1}吗？", structure.Name, (Vector2)transform.position),
        delegate
        {
            if (structure)
            {
                if (this.structures.TryGetValue(structure.Info.ID, out var structures))
                {
                    structures.Remove(structure);
                    if (structures.Count < 1) this.structures.Remove(structure.Info.ID);
                }
                if (structure.entity && structure.entity.gameObject)
                {
                    UpdateAStar(structure.entity);
                    structure.entity.Destroy();
                }
                else if (structure.preview && structure.preview.gameObject)
                {
                    UpdateAStar(structure.preview);
                    structure.preview.Destroy();
                }
                NotifyCenter.PostNotify(StructureDestroy, structure, structures);
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

    public List<StructureData> GetStructures(StructureInformation info)
    {
        this.structures.TryGetValue(info.ID, out var structures);
        return structures;
    }
    public void TryBuild(StructureInformation currentInfo, StructurePreview2D preview)
    {
        if (BackpackManager.Instance.IsMaterialsEnough(currentInfo.Materials))
        {
            List<ItemWithAmount> materials = BackpackManager.Instance.GetMaterialsFromInventory(currentInfo.Materials);
            if (materials != null && BackpackManager.Instance.CanLose(materials))
            {
                StructureData data = new StructureData(currentInfo, preview.transform.position);
                if (data.StartBuild())
                {
                    //data.StartConstruct();
                    preview.StartBuild(data);
                    BackpackManager.Instance.LoseItem(materials);
                    AddStructure(data);
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
    public void DoneBuild(StructureData data)
    {
        if (!data) return;
        if (data.preview) data.preview.OnBuilt();
        data.preview = null;
        if (data.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) return;
        if (!structureGroups.TryGetValue(data.Info, out var parent))
        {
            parent = new GameObject(data.Info.Name).transform;
            parent.SetParent(StructureRoot);
            structureGroups.Add(data.Info, parent);
        }
        Structure2D structure = Instantiate(data.Info.Prefab, parent);
        structure.Init(data);
        MessageManager.Instance.New($"[{data.Name}] 建造完成了");
        NotifyCenter.PostNotify(StructureBuilt, data);
    }
    public void AddStructure(StructureData data)
    {
        if (this.structures.TryGetValue(data.Info.ID, out var structures))
            structures.Add(data);
        else
            this.structures.Add(data.Info.ID, new List<StructureData>() { data });
    }
    public void MakeFlag(StructurePreview2D preview)
    {
        var bf = ObjectPool.Get(MiscSettings.Instance.StructureFlagPrefab, UIManager.Instance.StructureFlagParent).GetComponent<StructureFlag>();
        bf.Init(preview);
    }
    #endregion

    #region 消息
    public const string StructureBuilt = "StructureBuilt";
    public const string StructureDestroy = "StructureDestroy";
    #endregion
}