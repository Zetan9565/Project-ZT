using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureManageWindow : InteractionWindow<Interactive2D>, IHideable
{
    public Text infoNameText;
    public Text infoDesText;
    public Button manageButton;
    public Text manageText;
    public Button destroyButton;

    public GameObject morePanel;
    public Button constructButton;
    public Button materialButton;
    public Button warehouseButton;
    public Button workerButton;

    public bool IsManaging { get; private set; }
    public Structure2D CurrentStructure { get; private set; }

    public StructurePreview2D CurrentPreview { get; private set; }

    public override Interactive2D Target => CurrentStructure ? CurrentStructure : CurrentPreview;

    private StructureData currentData;
    private WarehouseSelectionWindow warehouse;

    public bool IsHidden { get; private set; }

    protected override void OnAwake()
    {
        if (!content.gameObject.GetComponent<GraphicRaycaster>()) content.gameObject.AddComponent<GraphicRaycaster>();
        constructButton.onClick.AddListener(StartConstruct);
        materialButton.onClick.AddListener(SetMaterials);
        warehouseButton.onClick.AddListener(SetWarehouse);
        workerButton.onClick.AddListener(SendWorker);
    }

    protected override bool OnOpen(params object[] args)
    {
        if (args == null || args.Length < 1 || IsHidden || !PlayerManager.Instance.CheckIsNormalWithAlert())
            return false;
        if (args[0] is Structure2D structure)
        {
            if (CurrentStructure == structure || !structure.Info) return false;
            CurrentStructure = structure;
            currentData = structure.Data;
            CurrentPreview = null;
            IsManaging = true;
            infoNameText.text = CurrentStructure.Name;
            infoDesText.text = CurrentStructure.Info.Description;
            ZetanUtility.SetActive(manageButton, CurrentStructure.Info.Manageable);
            manageButton.onClick.AddListener(ManageCurrent);
            manageText.text = CurrentStructure.Info.ManageBtnName;
            destroyButton.onClick.AddListener(delegate { StructureManager.Instance.DestroyStructure(CurrentStructure.Data); });
            return base.OnOpen(args);
        }
        else if (args[0] is StructurePreview2D preview)
        {
            if (CurrentPreview == preview || !preview.Data) return false;
            CurrentPreview = preview;
            currentData = preview.Data;
            CurrentStructure = null;
            IsManaging = true;
            infoNameText.text = CurrentPreview.Data.Info.Name;
            infoDesText.text = CurrentPreview.Data.Info.Description;
            ZetanUtility.SetActive(manageButton, !CurrentPreview.Data.Info.AutoBuild);
            manageButton.onClick.AddListener(SwitchMorePanel);
            manageText.text = "更多";
            destroyButton.onClick.AddListener(delegate { StructureManager.Instance.DestroyStructure(CurrentPreview.Data); });
            return base.OnOpen(args);
        }
        else return false;
    }

    protected override bool OnClose(params object[] args)
    {
        base.OnClose(args);
        ZetanUtility.SetActive(morePanel, false);
        infoNameText.text = string.Empty;
        infoDesText.text = string.Empty;
        manageButton.onClick.RemoveAllListeners();
        destroyButton.onClick.RemoveAllListeners();
        CurrentStructure = null;
        CurrentPreview = null;
        IsManaging = false;
        IsHidden = false;
        if (warehouse) warehouse.Close();
        WindowsManager.CloseWindow<ConfirmWindow>();
        return true;
    }

    private void ManageCurrent()
    {
        if (!PlayerManager.Instance.CheckIsNormalWithAlert())
            return;
        if (CurrentStructure.Info.Manageable && CurrentStructure.DoManage())
            WindowsManager.HideWindow(this, true);
    }

    protected override void OnInterrupt()
    {
        WindowsManager.CloseWindow<ConfirmWindow>();
        WindowsManager.HideWindow(this, false);
    }

    private void SwitchMorePanel()
    {
        ZetanUtility.SetActive(morePanel, !morePanel.activeSelf);
    }
    private void StartConstruct()
    {
        if (!CurrentPreview) return;
        if (CurrentPreview.StartConstruct())
            ProgressBar.Instance.New(CurrentPreview.Data.currentStage.BuildTime, delegate
            {
                return CurrentPreview.Data.currentStage.BuildTime - CurrentPreview.Data.leftBuildTime;
            },
            delegate
            {
                return CurrentPreview == null || CurrentPreview.Data == null || !CurrentPreview.Data.IsBuilding;
            },
            delegate
            {
                if (CurrentPreview)
                    CurrentPreview.PauseConstruct();
            }, "施工中");
        ZetanUtility.SetActive(morePanel, false);
    }
    private void SetMaterials()
    {
        ZetanUtility.SetActive(morePanel, false);
        InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, OnPutMaterials, "预留材料", selectCondition: (slot) => { return slot && slot.Item && slot.Item.Model_old.MaterialType != MaterialType.None; });
    }
    private void OnPutMaterials(IEnumerable<ItemWithAmount> materials)
    {
        List<ItemInfoBase> materialsConvert = new List<ItemInfoBase>();
        foreach (var isd in materials)
        {
            materialsConvert.Add(new ItemInfoBase(isd.source.Model_old, isd.amount));
        }
        CurrentPreview.PutMaterials(materialsConvert);
    }
    private void SendWorker()
    {
        ZetanUtility.SetActive(morePanel, false);
    }
    private void SetWarehouse()
    {
        ZetanUtility.SetActive(morePanel, false);
        //warehouse = NewWindowsManager.OpenWindow<WarehouseSelectionWindow>();
        //if (warehouse) warehouse.onClose += () => warehouse = null;
    }

    private void OnBuidlingDestroy(params object[] msg)
    {
        if (msg.Length > 0 && currentData && currentData == msg[0])
            Interrupt();
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(StructureManager.StructureDestroy, OnBuidlingDestroy, this);
    }

    public void Hide(bool hide, params object[] args)
    {
        if (args == null || args.Length > 0 && (CurrentPreview && CurrentPreview.Equals(args[0]) || CurrentStructure && CurrentStructure.Equals(args[0])))
        {
            if (IsHidden != hide)
            {
                IsHidden = hide;
                content.alpha = hide ? 0 : 1;
                content.blocksRaycasts = !hide;
            }
            if (warehouse && warehouse.IsOpen)
                warehouse.Close();
            warehouse = null;
        }
    }
}