using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/建筑管理器")]
public class BuildingManager : WindowHandler<BuildingUI, BuildingManager>, IOpenCloseAbleWindow
{
    private BuildingInformation currentInfo;

    private BuildingPreview preview;

    [Range(1, 2)]
    public int gridSize = 1;

    private readonly List<BuildingInformation> buildingsLearned = new List<BuildingInformation>();
    private readonly List<BuildingInfoAgent> buildingInfoAgents = new List<BuildingInfoAgent>();
    private readonly List<BuildingAgent> buildingAgents = new List<BuildingAgent>();
    private readonly Dictionary<BuildingInformation, List<Building>> buildings = new Dictionary<BuildingInformation, List<Building>>();

    public bool IsPreviewing { get; private set; }
    public bool BuildAble
    {
        get
        {
            if (preview && preview.ColliderCount < 1) return true;
            return false;
        }
    }

    private bool isDraging;
    private float moveTime = 0.1f;

    public bool IsManaging { get; private set; }
    public Building CurrentBuilding { get; private set; }

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
        foreach (Building b in FindObjectsOfType<Building>())
        {
            data.buildingSystemData.buildingDatas.Add(new BuildingSaveData(b));
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
        foreach (BuildingSaveData buildingData in buildingSystemData.buildingDatas)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.IDPrefix == buildingData.IDPrefix);
            if (find)
            {
                Building building = Instantiate(find.Prefab);
                building.LoadBuild(buildingData.IDPrefix, buildingData.IDTail, find.name, buildingData.leftBuildTime,
                    new Vector3(buildingData.posX, buildingData.posY, buildingData.posZ));
                var bf = ObjectPool.Get(UI.buildingFlagPrefab, UIManager.Instance.BuildingFlagParent).GetComponent<BuildingFlag>();
                bf.Init(building);
            }
        }
        AStarManager.Instance.UpdateGraphs();
    }
    #endregion

    #region 预览相关
    void Update()
    {
        if (IsPreviewing)
        {
            if (isDraging)
                ShowAndMovePreview();
            else MovePreview();
        }
    }

    public void CreatPreview(BuildingInformation info)
    {
        if (info == null) return;
        HideDescription();
        HideBuiltList();
        currentInfo = info;
        preview = Instantiate(currentInfo.Preview);
        WindowsManager.Instance.PauseAll(true);
        IsPreviewing = true;
        isDraging = true;
        ShowAndMovePreview();
        PlayerManager.Instance.PlayerController.controlAble = false;
    }
    public void Place()
    {
        isDraging = false;
        ZetanUtility.SetActive(UI.buildButton, true);
        ZetanUtility.SetActive(UI.backButton, true);
#if UNITY_ANDROID
        ZetanUtility.SetActive(UI.joyStick, true);
#endif
        CameraMovement2D.Instance.MoveTo(preview.transform.position);

        if (!BuildAble)
            MessageManager.Instance.New("存在障碍物");
    }
    public void Build()
    {
        if (!currentInfo) return;
        if (BuildAble)
        {
            if (BackpackManager.Instance.IsMaterialsEnough(currentInfo.Materials))
            {
                Building building = Instantiate(currentInfo.Prefab);
                if (building.StarBuild(currentInfo, preview.Position))
                {
                    foreach (MaterialInfo m in currentInfo.Materials)
                    {
                        if (!BackpackManager.Instance.TryLoseItem_Boolean(m.ItemInfo))
                        {
                            FinishPreview();
                            return;
                        }
                    }
                    foreach (MaterialInfo m in currentInfo.Materials)
                    {
                        BackpackManager.Instance.LoseItem(m.ItemInfo);
                    }
                    if (!buildings.ContainsKey(currentInfo))
                        buildings.Add(currentInfo, new List<Building>());
                    buildings[currentInfo].Add(building);
                    var bf = ObjectPool.Get(UI.buildingFlagPrefab, UIManager.Instance.BuildingFlagParent).GetComponent<BuildingFlag>();
                    bf.Init(building);
                    if (AStarManager.Instance)
                    {
                        var collider2Ds = building.GetComponentsInChildren<Collider2D>();
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
                            var colliders = building.GetComponentsInChildren<Collider>();
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
            else MessageManager.Instance.New("耗材不足");
            FinishPreview();
        }
        else
        {
            MessageManager.Instance.New("存在障碍物");
        }
    }
    private void FinishPreview()
    {
        if (preview) Destroy(preview.gameObject);
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
        CheckCollider();
        if (ZetanUtility.IsMouseInsideScreen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Place();
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
        if (move.sqrMagnitude > 0)
            moveTime += Time.deltaTime;
        if (moveTime >= 0.1f)
        {
            moveTime = 0;
            preview.transform.position = ZetanUtility.PositionToGrid((Vector2)preview.transform.position + move, gridSize, preview.CenterOffset);
            CameraMovement2D.Instance.MoveTo(preview.transform.position);
        }
        CheckCollider();
    }
    private void CheckCollider()
    {
        if (preview.ColliderCount > 0)
        {
            if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.red;
        }
        else
        {
            if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.white;
        }
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
        List<string> materialsInfo = BackpackManager.Instance.GetMaterialsInfo(buildingInfo.Materials).ToList();
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
        if (!CurrentBuilding || !CurrentBuilding.MBuildingInfo || isInfoPausing) return;
        UI.infoWindow.alpha = 1;
        UI.infoWindow.blocksRaycasts = true;
        UI.infoNameText.text = CurrentBuilding.name;
        UI.infoDesText.text = CurrentBuilding.MBuildingInfo.Description;
        ZetanUtility.SetActive(UI.mulFuncButton, CurrentBuilding.MBuildingInfo.Manageable);
        UI.mulFuncButton.onClick.AddListener(CurrentBuilding.OnManage);
        UI.destroyButton.onClick.AddListener(CurrentBuilding.AskDestroy);
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(BuildingManager), true);
    }
    public void HideInfo()
    {
        UI.infoWindow.alpha = 0;
        UI.infoWindow.blocksRaycasts = false;
        UI.infoNameText.text = string.Empty;
        UI.infoDesText.text = string.Empty;
        UI.mulFuncButton.onClick.RemoveAllListeners();
        UI.destroyButton.onClick.RemoveAllListeners();
        if (CurrentBuilding) CurrentBuilding.OnCancelManage();
        CurrentBuilding = null;
        IsManaging = false;
        isInfoPausing = false;
        if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.CloseWindow();
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(BuildingManager), false);
    }

    private bool isInfoPausing;
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
            FinishPreview();
    }

    public void DestroyBuilding(Building building)
    {
        if (building && building.gameObject)
        {
            BuildingAgent ba = buildingAgents.Find(x => x.MBuilding == building);
            if (ba)
            {
                ba.Hide();
                ba.Clear();
                buildingAgents.Remove(ba);
                this.buildings.TryGetValue(building.MBuildingInfo, out var buildings);
                if (buildings != null)
                {
                    buildings.Remove(building);
                    if (buildings.Count < 1) this.buildings.Remove(building.MBuildingInfo);
                }
            }
            if (buildingAgents.Count < 1 && currentInfo == building.MBuildingInfo && UI.listWindow.alpha > 0) HideBuiltList();
            if (AStarManager.Instance)
            {
                var colliders = building.GetComponentsInChildren<Collider>();
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
                    var collider2Ds = building.GetComponentsInChildren<Collider2D>();
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
            building.Destroy();
        }
        CancelManage();
    }
    #endregion
}