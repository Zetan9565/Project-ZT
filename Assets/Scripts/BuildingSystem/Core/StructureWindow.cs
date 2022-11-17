using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.InventorySystem;
using ZetanStudio.PlayerSystem;
using ZetanStudio.StructureSystem;
using ZetanStudio.StructureSystem.UI;
using ZetanStudio.UI;
using InputManager = ZetanStudio.InputManager;

public class StructureWindow : Window, IHideable
{
    public StructureInfoList infoList;
    public StructureList structureList;

    public CanvasGroup descriptionWindow;
    public Text nameText;
    public Text desciptionText;

    public CanvasGroup listWindow;
    public Button closeListButton;

    public Button buildButton;
    public Button backButton;
    public GameObject joyStick;

    private StructureInformation currentInfo;
    private StructurePreview2D preview;

    [Range(1, 2)]
    public int gridSize = 1;

    public bool IsPreviewing { get; private set; }

    private bool isDraging;
    private Vector2 input;
    private float moveTime = 0.1f;

    public bool IsLocating { get; private set; }
    public StructureData LocatingStructure { get; private set; }
    public bool IsHidden { get; private set; }

    #region 预览相关
    public void CreatPreview(StructureInformation info)
    {
        if (!PlayerManager.Instance.CheckIsNormalWithAlert())
            return;
        if (info == null) return;
        HideDescription();
        HideBuiltList();
        currentInfo = info;
        preview = Instantiate(currentInfo.Preview, StructureManager.StructureRoot);
        WindowsManager.HideAll(true);
        IsPreviewing = true;
        isDraging = true;
        PlayerManager.Instance.SetPlayerState(CharacterStates.Busy, CharacterBusyStates.UI);
        ShowAndMovePreview();
    }
    public void DoPlace()
    {
        if (!preview) return;

        isDraging = false;
        Utility.SetActive(buildButton, true);
        Utility.SetActive(backButton, true);
#if UNITY_ANDROID
        Utility.SetActive(joyStick, true);
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
            StructureManager.TryBuild(currentInfo, preview);
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
        Utility.SetActive(buildButton, false);
        Utility.SetActive(backButton, false);
        Utility.SetActive(joyStick, false);
        WindowsManager.HideAll(false);
        if (PlayerManager.Instance.Player.GetState(out var main, out var sub) && main == CharacterStates.Busy && sub == CharacterBusyStates.UI)
            PlayerManager.Instance.SetPlayerState(CharacterStates.Normal, CharacterNormalStates.Idle);
        CameraMovement2D.Instance.Stop();
    }

    public void ShowAndMovePreview()
    {
        if (!preview || !isDraging) return;
        preview.transform.position = Utility.PositionToGrid(GetMovePosition(), gridSize, preview.CenterOffset);
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
        if (Utility.IsMouseInsideScreen()) return Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
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
            preview.transform.position = Utility.PositionToGrid((Vector2)preview.transform.position + input, gridSize, preview.CenterOffset);
            CameraMovement2D.Instance.MoveTo(preview.transform.position);
        }
    }
    #endregion

    #region UI相关
    protected override bool OnOpen(params object[] args)
    {
        Refresh();
        Utility.SetActive(buildButton, false);
        Utility.SetActive(backButton, false);
        Utility.SetActive(joyStick, false);
        return true;
    }
    protected override bool OnClose(params object[] args)
    {
        FinishPreview();
        HideDescription();
        HideBuiltList();
        Utility.SetActive(backButton, false);
        WindowsManager.CloseWindow<FloatTipsPanel>();
        return true;
    }

    public void ShowDescription(StructureInformation structureInfo)
    {
        if (structureInfo == null) return;
        currentInfo = structureInfo;
        List<string> materialsInfo = BackpackManager.Instance.GetMaterialsInfoString(structureInfo.Materials);
        string materials = string.Empty;
        int lineCount = materialsInfo.Count;
        for (int i = 0; i < materialsInfo.Count; i++)
        {
            string endLine = i == lineCount - 1 ? string.Empty : "\n";
            materials += materialsInfo[i] + endLine;
        }
        nameText.text = structureInfo.Name;
        desciptionText.text = string.Format("<b>描述</b>\n{0}\n<b>耗时: </b>{1}\n<b>建造材料：{2}</b>\n{3}",
            structureInfo.Description,
            structureInfo.BuildTime > 0 ? structureInfo.BuildTime.ToString("F2") + 's' : "立即",
            BackpackManager.Instance.IsMaterialsEnough(structureInfo.Materials) ? "<color=green>(可建造)</color>" : "<color=red>(耗材不足)</color>",
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

    public void ShowBuiltList(StructureInformation info)
    {
        var structures = StructureManager.GetStructures(info);
        if (structures == null || structures.Count < 1)
        {
            HideBuiltList();
            return;
        }
        structureList.Refresh(structures);
        listWindow.alpha = 1;
        listWindow.blocksRaycasts = true;
    }
    public void HideBuiltList()
    {
        structureList.Refresh(null);
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
        infoList.Refresh(StructureManager.StructuresLearned);
        structureList.Refresh(null);
    }

    public void LocateStructure(StructureData structure)
    {
        if (!structure || !structure.entity && !structure.preview) return;
        Utility.SetActive(backButton, true);
        if (structure.entity) CameraMovement2D.Instance.MoveTo(structure.entity.transform.position);
        else if (structure.preview) CameraMovement2D.Instance.MoveTo(structure.preview.transform.position);
        WindowsManager.HideAll(true);
        IsLocating = true;
        LocatingStructure = structure;
    }
    private void FinishLocation()
    {
        Utility.SetActive(backButton, false);
        CameraMovement2D.Instance.Stop();
        WindowsManager.HideAll(false);
        IsLocating = false;
        LocatingStructure = null;
    }

    public void GoBack()
    {
        if (IsLocating)
            FinishLocation();
        else if (IsPreviewing)
            FinishPreview(true);
    }

    public void OnDestroyStructure(params object[] msg)
    {
        if (!IsOpen || msg.Length < 2) return;
        if (msg[1] is not List<StructureData> structures) return;
        if (structures.Count < 1) HideBuiltList();
        else if (listWindow.alpha > 0) structureList.Refresh(structures);
    }
    #endregion

    protected override void OnAwake()
    {
        closeListButton.onClick.AddListener(HideBuiltList);
        buildButton.onClick.AddListener(DoBuild);
        backButton.onClick.AddListener(GoBack);
        infoList.SetItemModifier(b => b.SetWindow(this));
        structureList.SetItemModifier(b => b.SetWindow(this));
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(StructureManager.StructureDestroy, OnDestroyStructure, this);
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