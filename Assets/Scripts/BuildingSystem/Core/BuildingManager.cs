﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[DisallowMultipleComponent]
public class BuildingManager : MonoBehaviour, IWindow
{
    private static BuildingManager instance;
    public static BuildingManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<BuildingManager>();
            return instance;
        }
    }

    public bool IsUIOpen { get; private set; }
    public bool IsPausing { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            if (!UI) return null;
            return UI.windowCanvas;
        }
    }

    [SerializeField]
    private BuildingUI UI;

    [HideInInspector]
    public BuildingInfomation currentInfo;

    [HideInInspector]
    public BuildingPreview preview;

    [Range(1, 2)]
    public int gridSize = 1;

    public List<BuildingInfomation> BuildingsLearned { get; private set; } = new List<BuildingInfomation>();

    private List<BuildingInfoAgent> buildingInfoAgents = new List<BuildingInfoAgent>();

    private List<BuildingAgent> buildingAgents = new List<BuildingAgent>();

    private Dictionary<BuildingInfomation, List<Building>> buildings = new Dictionary<BuildingInfomation, List<Building>>();

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

    public void Init()
    {
        foreach (BuildingInfoAgent ba in buildingInfoAgents)
        {
            if (ba) ba.Clear(true);
        }
        buildingInfoAgents.RemoveAll(x => !x || !x.gameObject.activeSelf || !x.gameObject);
        foreach (BuildingInfomation bi in BuildingsLearned)
        {
            BuildingInfoAgent ba = ObjectPool.Instance.Get(UI.buildingInfoCellPrefab, UI.buildingInfoCellsParent).GetComponent<BuildingInfoAgent>();
            ba.Init(bi, UI.cellsRect);
            buildingInfoAgents.Add(ba);
        }
    }

    public bool Learn(BuildingInfomation buildingInfo)
    {
        if (!buildingInfo) return false;
        if (BuildingsLearned.Contains(buildingInfo))
        {
            MessageManager.Instance.NewMessage("这种设施已经学会建造");
            return false;
        }
        BuildingsLearned.Add(buildingInfo);
        MessageManager.Instance.NewMessage(string.Format("学会了 [{0}] 的建造方法!", buildingInfo.Name));
        return true;
    }

    public void CreatPreview(BuildingInfomation info)
    {
        if (info == null) return;
        HideDescription();
        HideBuiltList();
        currentInfo = info;
        preview = Instantiate(currentInfo.Preview);
        WindowsManager.Instance.PauseAll(true);
        IsPreviewing = true;
#if UNITY_ANDROID
        MyTools.SetActive(CancelArea, true);
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
        preview.transform.position = MyTools.PositionToGrid(GetMovePosition(), gridSize);
        if (preview.ColliderCount > 0)
        {
            if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.red;
        }
        else
        {
            if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.white;
        }
        if (MyTools.IsMouseInsideScreen)
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
        MyTools.SetActive(CancelArea, false);
        UIManager.Instance.EnableJoyStick(true);
#endif
    }

    public void Build()
    {
        if (!currentInfo) return;
        if (BuildAble)
        {
            if (currentInfo.CheckMaterialsEnough(BackpackManager.Instance.MBackpack))
            {
                Building building = Instantiate(currentInfo.Prefab);
                if (building.StarBuild(currentInfo, preview.Position))
                {
                    foreach (MatertialInfo m in currentInfo.Materials)
                    {
                        if (!BackpackManager.Instance.TryLoseItem_Boolean(m.Item, m.Amount))
                        {
                            FinishPreview();
                            return;
                        }
                    }
                    foreach (MatertialInfo m in currentInfo.Materials)
                    {
                        BackpackManager.Instance.LoseItem(m.Item, m.Amount);
                    }
                    if (!buildings.ContainsKey(currentInfo))
                        buildings.Add(currentInfo, new List<Building>());
                    buildings[currentInfo].Add(building);
                    if ((building.GetComponentsInChildren<Collider>().Length > 1 || building.GetComponentsInChildren<Collider2D>().Length > 1)
                        && AStarManager.Instance && Camera.main)
                    {
                        //刷新九个屏幕范围内的寻路网格
                        AStarManager.Instance.UpdateAStars(Camera.main.ScreenToWorldPoint(-new Vector2(Screen.width, Screen.height)),
                            Camera.main.ScreenToWorldPoint(2 * new Vector2(Screen.width, Screen.height)));
                    }
                }
            }
            else MessageManager.Instance.NewMessage("耗材不足");
            FinishPreview();
        }
        else
        {
            MessageManager.Instance.NewMessage("空间不足");
#if UNITY_ANDROID
            FinishPreview();
#endif
        }
    }

    public void LoadData(BuildingSystemData buildingSystemData)
    {
        BuildingsLearned.Clear();
        BuildingInfomation[] buildingInfos = Resources.LoadAll<BuildingInfomation>("");
        foreach (string learned in buildingSystemData.learneds)
        {
            BuildingInfomation find = Array.Find(buildingInfos, x => x.IDStarter == learned);
            if (find) BuildingsLearned.Add(find);
        }
        foreach (BuildingData buildingData in buildingSystemData.buildingDatas)
        {
            BuildingInfomation find = Array.Find(buildingInfos, x => x.IDStarter == buildingData.IDStarter);
            if (find)
            {
                Building building = Instantiate(find.Prefab);
                building.LoadBuild(buildingData.IDStarter, buildingData.IDTail, find.Name, buildingData.leftBuildTime,
                    new Vector3(buildingData.posX, buildingData.posY, buildingData.posZ));
            }
        }
        if (AStarManager.Instance) AStarManager.Instance.UpdateAStars();
    }

    Vector2 GetMovePosition()
    {
        if (MyTools.IsMouseInsideScreen)
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else return preview.transform.position;
    }

    public void DestroyToDestroyBuilding()
    {
        if (!ToDestroy) return;
        StartCoroutine(WaitToDestroy(ToDestroy));
        ToDestroy.TryDestroy();
    }

    public Building ToDestroy { get; private set; }
    bool confirmDestroy;

    public void RequestDestroy(Building building)
    {
        ToDestroy = building;
    }

    public void ConfirmDestroy()
    {
        confirmDestroy = true;
    }

    IEnumerator WaitToDestroy(Building building)
    {
        yield return new WaitUntil(() => { return confirmDestroy; });
        if (building && building.gameObject)
        {
            BuildingAgent ba = buildingAgents.Find(x => x.MBuilding == building);
            if (ba)
            {
                ba.Hide();
                ba.Clear();
            }
            if (buildingAgents.Count < 1 && currentInfo == building.MBuildingInfo && UI.listWindow.alpha > 0) HideBuiltList();
            Destroy(building.gameObject);
        }
        if ((building.GetComponentsInChildren<Collider>().Length > 1 || building.GetComponentsInChildren<Collider2D>().Length > 1)
            && AStarManager.Instance && Camera.main)
        {
            //刷新九个屏幕范围内的寻路网格
            AStarManager.Instance.UpdateAStars(Camera.main.ScreenToWorldPoint(-new Vector2(Screen.width, Screen.height)),
                Camera.main.ScreenToWorldPoint(2 * new Vector2(Screen.width, Screen.height)));
        }
        confirmDestroy = false;
        CannotDestroy();
    }

    #region UI相关
    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        if (DialogueManager.Instance.IsTalking) return;
        Init();
        UI.buildingWindow.alpha = 1;
        UI.buildingWindow.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
    }
    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.buildingWindow.alpha = 0;
        UI.buildingWindow.blocksRaycasts = false;
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
        FinishPreview();
        HideDescription();
        HideBuiltList();
        MyTools.SetActive(UI.destroyButton.gameObject, false);
    }
    public void OpenCloseWindow()
    {
        if (IsUIOpen) CloseWindow();
        else OpenWindow();
    }
    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.buildingWindow.alpha = 1;
            UI.buildingWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.buildingWindow.alpha = 0;
            UI.buildingWindow.blocksRaycasts = false;
            MyTools.SetActive(UI.destroyButton.gameObject, false);
        }
        IsPausing = pause;
    }

    public void ShowDescription(BuildingInfomation buildingInfo)
    {
        if (buildingInfo == null) return;
        currentInfo = buildingInfo;
        List<string> materialsInfo = buildingInfo.GetMaterialsInfo(BackpackManager.Instance.MBackpack).ToList();
        string materials = string.Empty;
        int lineCount = materialsInfo.Count;
        for (int i = 0; i < materialsInfo.Count; i++)
        {
            string endLine = i == lineCount - 1 ? string.Empty : "\n";
            materials += materialsInfo[i] + endLine;
        }
        UI.nameText.text = buildingInfo.Name;
        UI.desciptionText.text = string.Format("<b>描述</b>\n{0}\n<b>耗时: </b>{1}\n<b>耗材{2}</b>\n{3}",
            buildingInfo.Description,
            buildingInfo.BuildTime > 0 ? buildingInfo.BuildTime.ToString("F2") + 's' : "立即",
            buildingInfo.CheckMaterialsEnough(BackpackManager.Instance.MBackpack) ? "<color=green>(可建造)</color>" : "<color=red>(耗材不足)</color>",
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

    public void ShowBuiltList(BuildingInfomation buildingInfo)
    {
        if (!this.buildings.ContainsKey(buildingInfo)) return;
        List<Building> buildings = this.buildings[buildingInfo];
        if (buildings.Count > buildingAgents.Count)
        {
            for (int i = 0; i < buildings.Count - buildingAgents.Count; i++)
            {
                BuildingAgent ba = ObjectPool.Instance.Get(UI.buildingCellPrefab, UI.buildingCellsParent).GetComponent<BuildingAgent>();
                ba.Hide();
                buildingAgents.Add(ba);
            }
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

    public void UpdateUI()
    {
        if (!IsUIOpen) return;
        Init();
        if (UI.descriptionWindow.alpha > 0) ShowDescription(currentInfo);
    }

    public void CanDestroy(Building building)
    {
        if (!IsUIOpen || ToDestroy) return;
        ToDestroy = building;
        if (!IsPausing) MyTools.SetActive(UI.destroyButton.gameObject, true);
    }
    public void CannotDestroy()
    {
        ToDestroy = null;
        MyTools.SetActive(UI.destroyButton.gameObject, false);
        if (ConfirmHandler.Instance.IsUIOpen) ConfirmHandler.Instance.CloseWindow();
    }

    public void SetUI(BuildingUI UI)
    {
        this.UI = UI;
    }
    public void ResetUI()
    {
        buildingInfoAgents.Clear();
        buildingAgents.Clear();
        IsUIOpen = false;
        IsPausing = false;
        WindowsManager.Instance.Remove(this);
    }
    #endregion
}