using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.StructureSystem
{
    public static class StructureManager
    {
        private static Transform structureRoot;
        public static Transform StructureRoot
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

        private static readonly Dictionary<StructureInformation, Transform> structureGroups = new Dictionary<StructureInformation, Transform>();

        public static List<StructureInformation> StructuresLearned { get; } = new List<StructureInformation>();
        private static readonly Dictionary<string, List<StructureData>> structures = new Dictionary<string, List<StructureData>>();

        #region 数据相关
        public static bool Learn(StructureInformation structureInfo)
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
        public static bool HadLearned(StructureInformation structureInfo)
        {
            return StructuresLearned.Contains(structureInfo);
        }

        [SaveMethod]
        public static void SaveData(SaveData saveData)
        {
            var structData = new SaveDataItem();
            saveData["structure"] = structData;
            structData.WriteAll(StructuresLearned.Select(x => x.ID));
            foreach (var dict in structures)
            {
                foreach (var str in dict.Value)
                {
                    structData.Write(str.GetSaveData());
                }
            }
        }
        [LoadMethod]
        public static void LoadData(SaveData saveData)
        {
            if (!saveData.TryReadData("structure", out var structData)) return;
            foreach (var kvp in structureGroups)
            {
                if (kvp.Value) UnityEngine.Object.Destroy(kvp.Value.gameObject);
            }
            structureGroups.Clear();
            Dictionary<string, StructureInformation> structDict = new Dictionary<string, StructureInformation>();
            foreach (var str in Resources.LoadAll<StructureInformation>("StructureInformation"))
            {
                structDict[str.ID] = str;
            }
            StructuresLearned.Clear();
            foreach (var id in structData.ReadStringList())
            {
                if (structDict.TryGetValue(id, out var find))
                    StructuresLearned.Add(find);
            }
            foreach (var kvp in structures)
            {
                foreach (var str in kvp.Value)
                {
                    if (str.entity) UnityEngine.Object.Destroy(str.entity.gameObject);
                }
            }
            structures.Clear();
            foreach (var str in structData.ReadDataList())
            {
                if (str.TryReadString("modelID", out var ID) && structDict.TryGetValue(ID, out var info))
                {
                    StructureData data = new StructureData(info, str);
                    if (data.leftBuildTime <= 0)
                    {
                        DoneBuild(data);
                    }
                    else if (str.TryReadString("scene", out var scene) && scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                    {
                        StructurePreview2D preview = UnityEngine.Object.Instantiate(info.Preview);
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
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            EmptyMonoBehaviour.Singleton.UpdateCallback -= Update;
            EmptyMonoBehaviour.Singleton.UpdateCallback += Update;
        }

        private static void Update()
        {
            foreach (var list in structures.Values)
            {
                list.ForEach(x => x.TimePass(Time.deltaTime));
            }
        }

        public static void DestroyStructure(StructureData structure)
        {
            if (!structure) return;
            ConfirmWindow.StartConfirm(string.Format("确定拆毁{0}吗？", structure.Name),
            delegate
            {
                if (structure)
                {
                    if (StructureManager.structures.TryGetValue(structure.Info.ID, out var structures))
                    {
                        structures.Remove(structure);
                        if (structures.Count < 1) StructureManager.structures.Remove(structure.Info.ID);
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

        public static List<StructureData> GetStructures(StructureInformation info)
        {
            StructureManager.structures.TryGetValue(info.ID, out var structures);
            return structures;
        }
        public static void TryBuild(StructureInformation currentInfo, StructurePreview2D preview)
        {
            if (BackpackManager.Instance.IsMaterialsEnough(currentInfo.Materials))
            {
                List<CountedItem> materials = BackpackManager.Instance.GetMaterials(currentInfo.Materials);
                if (materials != null && BackpackManager.Instance.CanLose(materials))
                {
                    StructureData data = new StructureData(currentInfo, preview.transform.position);
                    if (data.StartBuild())
                    {
                        //data.StartConstruct();
                        preview.StartBuild(data);
                        BackpackManager.Instance.Lose(materials);
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
        public static void DoneBuild(StructureData data)
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
            Structure2D structure = UnityEngine.Object.Instantiate(data.Info.Prefab, parent);
            structure.Init(data);
            MessageManager.Instance.New($"[{data.Name}] 建造完成了");
            NotifyCenter.PostNotify(StructureBuilt, data);
        }
        public static void AddStructure(StructureData data)
        {
            if (StructureManager.structures.TryGetValue(data.Info.ID, out var structures))
                structures.Add(data);
            else
                StructureManager.structures.Add(data.Info.ID, new List<StructureData>() { data });
        }
        public static void MakeFlag(StructurePreview2D preview)
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
}