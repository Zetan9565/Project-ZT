using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWindow : Window, IHideable
{
    public BuildingInfoList infoList;
    public BuildingList buildingList;

    public CanvasGroup descriptionWindow;
    public Text nameText;
    public Text desciptionText;

    public CanvasGroup listWindow;
    public Button closeListButton;

    public Button buildButton;
    public Button backButton;
    public GameObject joyStick;

    private BuildingInformation currentInfo;
    private BuildingPreview2D preview;

    [Range(1, 2)]
    public int gridSize = 1;

    public bool IsPreviewing { get; private set; }

    private bool isDraging;
    private Vector2 input;
    private float moveTime = 0.1f;

    public bool IsLocating { get; private set; }
    public BuildingData LocatingBuilding { get; private set; }
    public bool IsHidden { get; private set; }

    #region 预览相关
    public void CreatPreview(BuildingInformation info)
    {
        if (!PlayerManager.Instance.CheckIsNormalWithAlert())
            return;
        if (info == null) return;
        HideDescription();
        HideBuiltList();
        currentInfo = info;
        preview = Instantiate(currentInfo.Preview, BuildingManager.Instance.BuildingRoot);
        NewWindowsManager.HideAll(true);
        IsPreviewing = true;
        isDraging = true;
        PlayerManager.Instance.SetPlayerState(CharacterStates.Busy, CharacterBusyStates.UI);
        ShowAndMovePreview();
    }
    public void DoPlace()
    {
        if (!preview) return;

        isDraging = false;
        ZetanUtility.SetActive(buildButton, true);
        ZetanUtility.SetActive(backButton, true);
#if UNITY_ANDROID
        ZetanUtility.SetActive(joyStick, true);
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
            BuildingManager.Instance.TryBuild(currentInfo, preview);
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
        IsPreviewing = false;
        UIManager.Instance.EnableJoyStick(true);
        ZetanUtility.SetActive(buildButton, false);
        ZetanUtility.SetActive(backButton, false);
        ZetanUtility.SetActive(joyStick, false);
        NewWindowsManager.HideAll(false);
        if (PlayerManager.Instance.Player.GetState(out var main, out var sub) && main == CharacterStates.Busy && sub == CharacterBusyStates.UI)
            PlayerManager.Instance.SetPlayerState(CharacterStates.Normal, CharacterNormalStates.Idle);
        CameraMovement2D.Instance.Stop();
    }

    public void ShowAndMovePreview()
    {
        if (!preview || !isDraging) return;
        preview.transform.position = ZetanUtility.PositionToGrid(GetMovePosition(), gridSize, preview.CenterOffset);
#if UNITY_STANDALONE
        if (ZetanUtility.IsMouseInsideScreen)
        {
            if (InputManager.GetMouseButtonDown(0))
            {
                DoPlace();
            }
            if (InputManager.GetMouseButtonDown(1))
            {
                FinishPreview();
            }
        }
#endif
    }
    private Vector2 GetMovePosition()
    {
        if (ZetanUtility.IsMouseInsideScreen) return Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
        else return preview.transform.position;
    }

    public void MovePreview()
    {
        if (isDraging) return;
        var horizontal = InputManager.GetAsix("Horizontal");
        var vertical = InputManager.GetAsix("Vertical");
        var input = new Vector2(horizontal, vertical).normalized;
        if (input.sqrMagnitude > 0.25)
            moveTime += Time.deltaTime;
        if (moveTime >= 0.1f)
        {
            moveTime = 0;
            preview.transform.position = ZetanUtility.PositionToGrid((Vector2)preview.transform.position + input, gridSize, preview.CenterOffset);
            CameraMovement2D.Instance.MoveTo(preview.transform.position);
        }
    }
    #endregion

    #region UI相关
    protected override bool OnOpen(params object[] args)
    {
        if (NewWindowsManager.IsWindowOpen<DialogueWindow>()) return false;
        Refresh();
        ZetanUtility.SetActive(buildButton, false);
        ZetanUtility.SetActive(backButton, false);
        ZetanUtility.SetActive(joyStick, false);
        return true;
    }
    protected override bool OnClose(params object[] args)
    {
        FinishPreview();
        HideDescription();
        HideBuiltList();
        ZetanUtility.SetActive(backButton, false);
        NewWindowsManager.CloseWindow<FloatTipsPanel>();
        return true;
    }

    public void ShowDescription(BuildingInformation buildingInfo)
    {
        if (buildingInfo == null) return;
        currentInfo = buildingInfo;
        List<string> materialsInfo = BackpackManager.Instance.GetMaterialsInfoString(buildingInfo.Materials);
        string materials = string.Empty;
        int lineCount = materialsInfo.Count;
        for (int i = 0; i < materialsInfo.Count; i++)
        {
            string endLine = i == lineCount - 1 ? string.Empty : "\n";
            materials += materialsInfo[i] + endLine;
        }
        nameText.text = buildingInfo.Name;
        desciptionText.text = string.Format("<b>描述</b>\n{0}\n<b>耗时: </b>{1}\n<b>建造材料：{2}</b>\n{3}",
            buildingInfo.Description,
            buildingInfo.BuildTime > 0 ? buildingInfo.BuildTime.ToString("F2") + 's' : "立即",
            BackpackManager.Instance.IsMaterialsEnough(buildingInfo.Materials) ? "<color=green>(可建造)</color>" : "<color=red>(耗材不足)</color>",
            materials);
        descriptionWindow.alpha = 1;
        descriptionWindow.blocksRaycasts = false;
    }
    public void HideDescription()
    {
        descriptionWindow.alpha = 0;
        descriptionWindow.blocksRaycasts = false;
        nameText.text = string.Empty;
        desciptionText.text = string.Empty;
    }

    public void ShowBuiltList(BuildingInformation info)
    {
        var buildings = BuildingManager.Instance.GetBuildings(info);
        if (buildings == null || buildings.Count < 1)
        {
            HideBuiltList();
            return;
        }
        buildingList.Refresh(buildings);
        listWindow.alpha = 1;
        listWindow.blocksRaycasts = true;
    }
    public void HideBuiltList()
    {
        buildingList.Refresh(null);
        listWindow.alpha = 0;
        listWindow.blocksRaycasts = false;
    }

    public void UpdateUI(params object[] msg)
    {
        if (!IsOpen) return;
        Refresh();
        if (descriptionWindow.alpha > 0) ShowDescription(currentInfo);
    }
    #endregion

    #region 其它
    private void Update()
    {
        if (IsPreviewing)
        {
            if (isDraging) ShowAndMovePreview();
            else MovePreview();
        }
    }

    public void Refresh()
    {
        infoList.Refresh(BuildingManager.Instance.BuildingsLearned);
        buildingList.Refresh(null);
    }

    public void LocateBuilding(BuildingData building)
    {
        if (!building || !building.entity && !building.preview) return;
        ZetanUtility.SetActive(backButton, true);
        if (building.entity) CameraMovement2D.Instance.MoveTo(building.entity.transform.position);
        else if (building.preview) CameraMovement2D.Instance.MoveTo(building.preview.transform.position);
        NewWindowsManager.HideAll(true);
        IsLocating = true;
        LocatingBuilding = building;
    }
    private void FinishLocation()
    {
        ZetanUtility.SetActive(backButton, false);
        CameraMovement2D.Instance.Stop();
        NewWindowsManager.HideAll(false);
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

    public void OnDestroyBuilding(params object[] msg)
    {
        if (!IsOpen || msg.Length < 2) return;
        if (msg[1] is not List<BuildingData> buildings) return;
        if (buildings.Count < 1) HideBuiltList();
        else if (listWindow.alpha > 0) buildingList.Refresh(buildings);
    }
    #endregion

    protected override void OnAwake()
    {
        closeListButton.onClick.AddListener(HideBuiltList);
        buildButton.onClick.AddListener(DoBuild);
        backButton.onClick.AddListener(GoBack);
        infoList.SetItemModifier(b => b.SetWindow(this));
        buildingList.SetItemModifier(b => b.SetWindow(this));
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(BuildingManager.BuildingDestroy, OnDestroyBuilding, this);
        NotifyCenter.AddListener(BackpackManager.BackpackItemAmountChanged, UpdateUI, this);
    }

    public void Hide(bool hide, params object[] args)
    {
        if (!IsOpen) return;
        if (IsHidden != hide)
        {
            if (hide)
            {
                content.alpha = 0;
                content.blocksRaycasts = false;
            }
            else
            {
                content.alpha = 1;
                content.blocksRaycasts = true;
            }
            IsHidden = hide;
        }
    }
}