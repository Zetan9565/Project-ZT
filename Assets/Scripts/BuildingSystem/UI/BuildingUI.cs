using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : WindowUI
{
    public GameObject buildingInfoCellPrefab;
    public Transform buildingInfoCellsParent;

    public GameObject buildingCellPrefab;
    public Transform buildingCellsParent;

    public GameObject buildingFlagPrefab;

    public ScrollRect cellsRect;

    public CanvasGroup descriptionWindow;
    public Text nameText;
    public Text desciptionText;

    public CanvasGroup listWindow;
    public Button closeListButton;

    public CanvasGroup infoWindow;
    [HideInInspector]
    public Text infoNameText;
    public Text infoDesText;
    public Button manageButton;
    public Text manageBtnName;
    public Button destroyButton;
    public Button closeInfoButton;

    public Button buildButton;
    public Button backButton;
    public GameObject joyStick;

    public GameObject morePanel;
    public Button constructButton;
    public Button materialButton;
    public Button warehouseButton;
    public Button workerButton;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(BuildingManager.Instance.CloseWindow);
        closeListButton.onClick.AddListener(BuildingManager.Instance.HideBuiltList);
        if (!infoWindow.gameObject.GetComponent<GraphicRaycaster>()) infoWindow.gameObject.AddComponent<GraphicRaycaster>();
        closeInfoButton.onClick.AddListener(BuildingManager.Instance.HideInfo);
        buildButton.onClick.AddListener(BuildingManager.Instance.DoBuild);
        backButton.onClick.AddListener(BuildingManager.Instance.GoBack);
        constructButton.onClick.AddListener(BuildingManager.Instance.StartConstruct);
        materialButton.onClick.AddListener(BuildingManager.Instance.SetMaterials);
        warehouseButton.onClick.AddListener(BuildingManager.Instance.SetWarehouse);
        workerButton.onClick.AddListener(BuildingManager.Instance.SendWorker);
    }
}
