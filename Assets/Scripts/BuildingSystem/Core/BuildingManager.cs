using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/建筑管理器")]
public class BuildingManager : WindowHandler<BuildingUI, BuildingManager>, IOpenCloseAbleWindow
{
    [HideInInspector]
    public BuildingInformation currentInfo;

    [HideInInspector]
    public BuildingPreview preview;

    [Range(1, 2)]
    public int gridSize = 1;

    private readonly List<BuildingInformation> buildingsLearned = new List<BuildingInformation>();

    private readonly List<BuildingInfoAgent> buildingInfoAgents = new List<BuildingInfoAgent>();

    private readonly List<BuildingAgent> buildingAgents = new List<BuildingAgent>();

    private readonly Dictionary<BuildingInformation, List<Building>> buildings = new Dictionary<BuildingInformation, List<Building>>();

    public GameObject CancelArea { get { return UI.cancelArea; } }

    public bool IsPreviewing { get; private set; }

    public bool BuildAble
    {
        get
        {
            if (preview && preview.ColliderCount < 1) return true;
            return false;
        }
    }

    public bool ManageAble { get; private set; }

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
        ZetanUtility.SetActive(UI.locationGoBackBtn, false);
    }

    public bool IsLocating { get; private set; }
    public Building LocatingBuilding { get; private set; }
    public void LocateBuilding(Building building)
    {
        ZetanUtility.SetActive(UI.locationGoBackBtn, true);
        CameraMovement2D.Instance.MoveTo(building.transform.position);
        WindowsManager.Instance.PauseAll(true);
        IsLocating = true;
        LocatingBuilding = building;
    }
    public void LocationGoBack()
    {
        ZetanUtility.SetActive(UI.locationGoBackBtn, false);
        CameraMovement2D.Instance.Stop();
        WindowsManager.Instance.PauseAll(false);
        IsLocating = false;
        LocatingBuilding = null;
    }

    public void SaveData(SaveData data)
    {
        data.buildingSystemData.learneds = buildingsLearned.Select(x => x.IDStarter).ToArray();
        foreach (Building b in FindObjectsOfType<Building>())
        {
            data.buildingSystemData.buildingDatas.Add(new BuildingData(b));
        }
    }

    public bool Learn(BuildingInformation buildingInfo)
    {
        if (!buildingInfo) return false;
        if (HadLearned(buildingInfo))
        {
            ConfirmManager.Instance.New("这种设施已经学会建造。");
            return false;
        }
        buildingsLearned.Add(buildingInfo);
        ConfirmManager.Instance.New(string.Format("学会了 [{0}] 的建造方法!", buildingInfo.Name));
        return true;
    }
    public bool HadLearned(BuildingInformation buildingInfo)
    {
        return buildingsLearned.Contains(buildingInfo);
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
#if UNITY_ANDROID
        ZetanUtility.SetActive(CancelArea, true);
        UIManager.Instance.EnableJoyStick(false);
#endif
        ShowAndMovePreview();
    }

#if UNITY_STANDALONE
    void Update()
    {
        if (IsPreviewing)
        {
            ShowAndMovePreview();
        }
    }
#endif

    public void ShowAndMovePreview()
    {
        if (!preview) return;
        preview.transform.position = ZetanUtility.PositionToGrid(GetMovePosition(), gridSize, preview.CenterOffset);
        if (preview.ColliderCount > 0)
        {
            if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.red;
        }
        else
        {
            if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.white;
        }
        if (ZetanUtility.IsMouseInsideScreen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Build();
            }
            if (Input.GetMouseButtonDown(1))
            {
                FinishPreview();
            }
        }
    }

    public void FinishPreview()
    {
        if (preview) Destroy(preview.gameObject);
        preview = null;
        currentInfo = null;
        WindowsManager.Instance.PauseAll(false);
        IsPreviewing = false;
#if UNITY_ANDROID
        ZetanUtility.SetActive(CancelArea, false);
        UIManager.Instance.EnableJoyStick(true);
#endif
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
            MessageManager.Instance.New("空间不足");
#if UNITY_ANDROID
            FinishPreview();
#endif
        }
    }

    public void LoadData(BuildingSystemData buildingSystemData)
    {
        buildingsLearned.Clear();
        BuildingInformation[] buildingInfos = Resources.LoadAll<BuildingInformation>("");
        foreach (string learned in buildingSystemData.learneds)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.IDStarter == learned);
            if (find) buildingsLearned.Add(find);
        }
        foreach (BuildingData buildingData in buildingSystemData.buildingDatas)
        {
            BuildingInformation find = Array.Find(buildingInfos, x => x.IDStarter == buildingData.IDStarter);
            if (find)
            {
                Building building = Instantiate(find.Prefab);
                building.LoadBuild(buildingData.IDStarter, buildingData.IDTail, find.Name, buildingData.leftBuildTime,
                    new Vector3(buildingData.posX, buildingData.posY, buildingData.posZ));
                var bf = ObjectPool.Get(UI.buildingFlagPrefab, UIManager.Instance.BuildingFlagParent).GetComponent<BuildingFlag>();
                bf.Init(building);
            }
        }
        AStarManager.Instance.UpdateGraphs();
    }

    private Vector2 GetMovePosition()
    {
        if (ZetanUtility.IsMouseInsideScreen) return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else return preview.transform.position;
    }

    public Building CurrentBuilding { get; private set; }

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
        CannotManage();
    }

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
        ZetanUtility.SetActive(UI.locationGoBackBtn, false);
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
        UI.nameText.text = buildingInfo.Name;
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
        if (!CurrentBuilding || !CurrentBuilding.MBuildingInfo) return;
        UI.infoWindow.alpha = 1;
        UI.infoWindow.blocksRaycasts = true;
        UI.infoNameText.text = CurrentBuilding.name;
        UI.infoDesText.text = CurrentBuilding.MBuildingInfo.Description;
        UI.mulFuncButton.onClick = CurrentBuilding.onButtonClick;
        UI.destroyButton.onClick.AddListener(CurrentBuilding.AskDestroy);
        UIManager.Instance.EnableInteract(false);
    }
    public void HideInfo()
    {
        UI.infoWindow.alpha = 0;
        UI.infoWindow.blocksRaycasts = false;
        UI.infoNameText.text = string.Empty;
        UI.infoDesText.text = string.Empty;
        UI.mulFuncButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        UI.destroyButton.onClick.RemoveAllListeners();
        CurrentBuilding = null;
        ManageAble = false;
        if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.CloseWindow();
    }

    public void UpdateUI()
    {
        if (!IsUIOpen) return;
        Init();
        if (UI.descriptionWindow.alpha > 0) ShowDescription(currentInfo);
    }

    public void CanManage(Building building)
    {
        if (CurrentBuilding == building || !building) return;
        CurrentBuilding = building;
        ManageAble = true;
        UIManager.Instance.EnableInteract(true, CurrentBuilding.name);
        //ShowInfo();
    }
    public void CannotManage()
    {
        CurrentBuilding = null;
        ManageAble = false;
        if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.CloseWindow();
        HideInfo();
        UIManager.Instance.EnableInteract(false);
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
}