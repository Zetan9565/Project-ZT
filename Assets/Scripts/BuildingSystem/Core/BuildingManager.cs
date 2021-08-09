using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, AddComponentMenu("ZetanStudio/管理器/建筑管理器")]
public class BuildingManager : WindowHandler<BuildingUI, BuildingManager>, IOpenCloseAbleWindow
{
    private BuildingInformation currentInfo;
    private BuildingPreview preview;

    private Transform buildingRoot;
    private Transform BuildingRoot
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

    [Range(1, 2)]
    public int gridSize = 1;

    private readonly List<BuildingInformation> buildingsLearned = new List<BuildingInformation>();
    private readonly List<BuildingInfoAgent> buildingInfoAgents = new List<BuildingInfoAgent>();
    private readonly List<BuildingAgent> buildingAgents = new List<BuildingAgent>();
    private readonly Dictionary<BuildingInformation, List<BuildingData>> buildings = new Dictionary<BuildingInformation, List<BuildingData>>();

    public bool IsPreviewing { get; private set; }

    private bool isDraging;
    private float moveTime = 0.1f;

    public bool IsManaging { get; private set; }
    public Building CurrentBuilding { get; private set; }
    private bool isInfoPausing;

    public bool IsLocating { get; private set; }
    public Building LocatingBuilding { get; private set; }

    #region 数据相关
    public bool Learn(BuildingInformation buildingInfo)
    {
        if (!buildingInfo) return false;
        if (HadLearned(buildingInfo))
        {
            ConfirmManager.Instance.New("这种设施已经学会建造。");
            return false;
        }
        buildingsLearned.Add(buildingInfo);
        ConfirmManager.Instance.New(string.Format("学会了 [{0}] 的建造方法!", buildingInfo.name));
        return true;
    }
    public bool HadLearned(BuildingInformation buildingInfo)
    {
        return buildingsLearned.Contains(buildingInfo);
    }

    public void SaveData(SaveData data)
    {
        data.buildingSystemData.learneds = buildingsLearned.Select(x => x.IDPrefix).ToArray();
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
        buildingsLearned.Clear();
        BuildingInformation[] buildingInfos = Resources.LoadAll<BuildingInformation>("");
        foreach (string learned in buildingSystemData.learneds)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.IDPrefix == learned);
            if (find) buildingsLearned.Add(find);
        }
        foreach (BuildingSaveData saveData in buildingSystemData.buildingDatas)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.IDPrefix == saveData.IDPrefix);
            if (find)
            {
                BuildingData data = new BuildingData(find, new Vector3(saveData.posX, saveData.posY, saveData.posZ));
                data.LoadBuild(find, saveData);
                if (data.leftBuildTime <= 0)
                {
                    Build(data);
                }
                else if (saveData.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    BuildingPreview building = Instantiate(find.Preview);
                    building.StartBuild(data);
                }
                if (!buildings.ContainsKey(find))
                    buildings.Add(find, new List<BuildingData>());
                buildings[find].Add(data);
            }
        }
        AStarManager.Instance.UpdateGraphs();
    }
    #endregion

    #region 预览相关
    public void CreatPreview(BuildingInformation info)
    {
        if (info == null) return;
        HideDescription();
        HideBuiltList();
        currentInfo = info;
        preview = Instantiate(currentInfo.Preview, BuildingRoot);
        WindowsManager.Instance.PauseAll(true);
        IsPreviewing = true;
        isDraging = true;
        ShowAndMovePreview();
        PlayerManager.Instance.PlayerController.controlAble = false;
    }
    public void DoPlace()
    {
        if (!preview) return;

        isDraging = false;
        ZetanUtility.SetActive(UI.buildButton, true);
        ZetanUtility.SetActive(UI.backButton, true);
#if UNITY_ANDROID
        ZetanUtility.SetActive(UI.joyStick, true);
#endif
        CameraMovement2D.Instance.MoveTo(preview.transform.position);

        if (!preview.BuildAble)
            MessageManager.Instance.New("存在障碍物");
    }
    public void DoBuild()
    {
        if (!currentInfo || !preview) return;
        if (preview.BuildAble)
        {
            if (BackpackManager.Instance.IsMaterialsEnough(currentInfo.Materials))
            {
                List<ItemSelectionData> materials = BackpackManager.Instance.GetMaterialsFromBackpack(currentInfo.Materials);
                if (materials != null && BackpackManager.Instance.TryLoseItems_Boolean(materials))
                {
                    BuildingData data = new BuildingData(currentInfo, preview.transform.position);
                    if (data.StartBuild())
                    {
                        preview.StartBuild(data);
                        foreach (ItemSelectionData m in materials)
                        {
                            BackpackManager.Instance.LoseItem(m.source, m.amount);
                        }
                        if (!buildings.ContainsKey(currentInfo))
                            buildings.Add(currentInfo, new List<BuildingData>());
                        buildings[currentInfo].Add(data);
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
            FinishPreview();
        }
        else
        {
            MessageManager.Instance.New("存在障碍物");
        }
    }
    private void FinishPreview(bool destroyPreview = false)
    {
        if (!IsPreviewing) return;
        if (destroyPreview) Destroy(preview.gameObject);
        preview = null;
        currentInfo = null;
        WindowsManager.Instance.PauseAll(false);
        IsPreviewing = false;
        UIManager.Instance.EnableJoyStick(true);
        ZetanUtility.SetActive(UI.buildButton, false);
        ZetanUtility.SetActive(UI.backButton, false);
        ZetanUtility.SetActive(UI.joyStick, false);
        CameraMovement2D.Instance.Stop();
        PlayerManager.Instance.PlayerController.controlAble = true;
    }

    public void ShowAndMovePreview()
    {
        if (!preview || !isDraging) return;
        preview.transform.position = ZetanUtility.PositionToGrid(GetMovePosition(), gridSize, preview.CenterOffset);
        if (ZetanUtility.IsMouseInsideScreen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoPlace();
            }
            if (Input.GetMouseButtonDown(1))
            {
                FinishPreview();
            }
        }
    }
    private Vector2 GetMovePosition()
    {
        if (ZetanUtility.IsMouseInsideScreen) return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else return preview.transform.position;
    }

    public void MovePreview()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        var move = new Vector2(horizontal, vertical).normalized;
        if (move.sqrMagnitude > 0.25)
            moveTime += Time.deltaTime;
        if (moveTime >= 0.1f)
        {
            moveTime = 0;
            preview.transform.position = ZetanUtility.PositionToGrid((Vector2)preview.transform.position + move, gridSize, preview.CenterOffset);
            CameraMovement2D.Instance.MoveTo(preview.transform.position);
        }
    }

    public void MakeFlag(BuildingPreview preview)
    {
        var bf = ObjectPool.Get(UI.buildingFlagPrefab, UIManager.Instance.BuildingFlagParent).GetComponent<BuildingFlag>();
        bf.Init(preview);
    }
    public void Build(BuildingData buildingData)
    {
        if (buildingData.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) return;
        if (!buildingGroups.TryGetValue(buildingData.Info, out var parent))
        {
            parent = new GameObject(buildingData.Info.name).transform;
            parent.SetParent(BuildingRoot);
            buildingGroups.Add(buildingData.Info, parent);
        }
        Building building = Instantiate(buildingData.Info.Prefab, parent);
        building.Build(buildingData);
    }
    #endregion

    #region UI相关
    public override void OpenWindow()
    {
        if (DialogueManager.Instance && DialogueManager.Instance.IsTalking) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        Init();
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        FinishPreview();
        HideDescription();
        HideBuiltList();
        ZetanUtility.SetActive(UI.backButton, false);
        TipsManager.Instance.Hide();
    }
    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) CloseWindow();
        else OpenWindow();
    }

    public void ShowDescription(BuildingInformation buildingInfo)
    {
        if (buildingInfo == null) return;
        currentInfo = buildingInfo;
        List<string> materialsInfo = BackpackManager.Instance.GetMaterialsInfoString(buildingInfo.Materials).ToList();
        string materials = string.Empty;
        int lineCount = materialsInfo.Count;
        for (int i = 0; i < materialsInfo.Count; i++)
        {
            string endLine = i == lineCount - 1 ? string.Empty : "\n";
            materials += materialsInfo[i] + endLine;
        }
        UI.nameText.text = buildingInfo.name;
        UI.desciptionText.text = string.Format("<b>描述</b>\n{0}\n<b>耗时: </b>{1}\n<b>建造材料：{2}</b>\n{3}",
            buildingInfo.Description,
            buildingInfo.BuildTime > 0 ? buildingInfo.BuildTime.ToString("F2") + 's' : "立即",
            BackpackManager.Instance.IsMaterialsEnough(buildingInfo.Materials) ? "<color=green>(可建造)</color>" : "<color=red>(耗材不足)</color>",
            materials);
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = false;
    }
    public void HideDescription()
    {
        UI.descriptionWindow.alpha = 0;
        UI.descriptionWindow.blocksRaycasts = false;
        UI.nameText.text = string.Empty;
        UI.desciptionText.text = string.Empty;
    }

    public void ShowBuiltList(BuildingInformation buildingInfo)
    {
        this.buildings.TryGetValue(buildingInfo, out var buildings);
        if (buildings == null || buildings.Count < 1)
        {
            HideBuiltList();
            return;
        }
        while (buildings.Count > buildingAgents.Count)
        {
            BuildingAgent ba = ObjectPool.Get(UI.buildingCellPrefab, UI.buildingCellsParent).GetComponent<BuildingAgent>();
            ba.Hide();
            buildingAgents.Add(ba);
        }
        while (buildings.Count < buildingAgents.Count)
        {
            buildingAgents[buildingAgents.Count - 1].Clear(true);
            buildingAgents.RemoveAt(buildingAgents.Count - 1);
        }
        for (int i = 0; i < buildings.Count; i++)
        {
            buildingAgents[i].Init(buildings[i]);
            buildingAgents[i].Show();
        }
        UI.listWindow.alpha = 1;
        UI.listWindow.blocksRaycasts = true;
    }
    public void HideBuiltList()
    {
        foreach (BuildingAgent ba in buildingAgents)
        {
            ba.Clear();
            ba.Hide();
        }
        UI.listWindow.alpha = 0;
        UI.listWindow.blocksRaycasts = false;
    }

    public void ShowInfo()
    {
        if (!CurrentBuilding || !CurrentBuilding.Info || isInfoPausing) return;
        UI.infoWindow.alpha = 1;
        UI.infoWindow.blocksRaycasts = true;
        UI.infoNameText.text = CurrentBuilding.name;
        UI.infoDesText.text = CurrentBuilding.Info.Description;
        ZetanUtility.SetActive(UI.manageButton, CurrentBuilding.Info.Manageable);
        UI.manageButton.onClick.AddListener(CurrentBuilding.OnManage);
        UI.manageBtnName.text = CurrentBuilding.Info.ManageBtnName;
        UI.destroyButton.onClick.AddListener(delegate { DestroyBuilding(CurrentBuilding.Data); });
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(BuildingManager), true);
    }
    public void HideInfo()
    {
        UI.infoWindow.alpha = 0;
        UI.infoWindow.blocksRaycasts = false;
        UI.infoNameText.text = string.Empty;
        UI.infoDesText.text = string.Empty;
        UI.manageButton.onClick.RemoveAllListeners();
        UI.destroyButton.onClick.RemoveAllListeners();
        if (CurrentBuilding) CurrentBuilding.OnCancelManage();
        CurrentBuilding = null;
        IsManaging = false;
        isInfoPausing = false;
        if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.CloseWindow();
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(BuildingManager), false);
    }

    public void PauseDisplayInfo(bool pause)
    {
        if (!isInfoPausing && pause && UI.infoWindow.alpha > 0)
        {
            isInfoPausing = true;
            UI.infoWindow.alpha = 0;
            UI.infoWindow.blocksRaycasts = false;
        }
        else if (isInfoPausing && !pause && UI.infoWindow.alpha < 1)
        {
            isInfoPausing = false;
            UI.infoWindow.alpha = 1;
            UI.infoWindow.blocksRaycasts = true;
        }
    }

    public void UpdateUI()
    {
        if (!IsUIOpen) return;
        Init();
        if (UI.descriptionWindow.alpha > 0) ShowDescription(currentInfo);
    }

    public bool Manage(Building building)
    {
        if (GatherManager.Instance.IsGathering)
        {
            MessageManager.Instance.New("请先等待采集完成");
            return false;
        }
        if (CurrentBuilding == building || !building) return false;
        CurrentBuilding = building;
        IsManaging = true;
        ShowInfo();
        return true;
    }
    public void CancelManage()
    {
        if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.CloseWindow();
        HideInfo();
    }

    public override void SetUI(BuildingUI UI)
    {
        buildingInfoAgents.RemoveAll(x => !x || !x.gameObject);
        buildingAgents.RemoveAll(x => !x || !x.gameObject);
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
        Init();
    }
    #endregion

    #region 其它
    private void Update()
    {
        if (IsPreviewing)
        {
            if (isDraging)
                ShowAndMovePreview();
            else MovePreview();
        }
        foreach (var list in buildings.Values)
        {
            list.ForEach(x => x.TimePass(Time.deltaTime));
        }
    }

    public void Init()
    {
        foreach (BuildingInfoAgent ba in buildingInfoAgents)
        {
            if (ba) ba.Clear(true);
        }
        buildingInfoAgents.RemoveAll(x => !x || !x.gameObject.activeSelf || !x.gameObject);
        foreach (BuildingInformation bi in buildingsLearned)
        {
            BuildingInfoAgent ba = ObjectPool.Get(UI.buildingInfoCellPrefab, UI.buildingInfoCellsParent).GetComponent<BuildingInfoAgent>();
            ba.Init(bi, UI.cellsRect);
            buildingInfoAgents.Add(ba);
        }
        ZetanUtility.SetActive(UI.buildButton, false);
        ZetanUtility.SetActive(UI.backButton, false);
        ZetanUtility.SetActive(UI.joyStick, false);
    }

    public void LocateBuilding(Building building)
    {
        ZetanUtility.SetActive(UI.backButton, true);
        CameraMovement2D.Instance.MoveTo(building.transform.position);
        WindowsManager.Instance.PauseAll(true);
        IsLocating = true;
        LocatingBuilding = building;
    }
    private void FinishLocation()
    {
        ZetanUtility.SetActive(UI.backButton, false);
        CameraMovement2D.Instance.Stop();
        WindowsManager.Instance.PauseAll(false);
        IsLocating = false;
        LocatingBuilding = null;
    }

    public void GoBack()
    {
        if (IsLocating)
            FinishLocation();
        else if (IsPreviewing)
            FinishPreview(true);
    }

    public void DestroyBuilding(BuildingData building)
    {
        if (!building || !building.entity || !building.entity.gameObject) return;
        ConfirmManager.Instance.New(string.Format("确定拆毁{0}{1}吗？", name, (Vector2)transform.position),
        delegate
        {
            if (building && building.entity && building.entity.gameObject)
            {
                BuildingAgent ba = buildingAgents.Find(x => x.MBuilding == building);
                if (ba)
                {
                    ba.Hide();
                    ba.Clear();
                    buildingAgents.Remove(ba);
                }
                if (this.buildings.TryGetValue(building.Info, out var buildings))
                {
                    buildings.Remove(building);
                    if (buildings.Count < 1) this.buildings.Remove(building.Info);
                }
                if (buildingAgents.Count < 1 && currentInfo == building.Info && UI.listWindow.alpha > 0) HideBuiltList();
                if (building.entity && building.entity.gameObject)
                {
                    if (AStarManager.Instance)
                    {
                        var colliders = building.entity.GetComponentsInChildren<Collider>();
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
                            var collider2Ds = building.entity.GetComponentsInChildren<Collider2D>();
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
                    building.entity.Destroy();
                }
            }
            CancelManage();
        });
    }
    #endregion
}