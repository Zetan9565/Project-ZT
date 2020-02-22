using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : WindowUI
{
    public GameObject buildingInfoCellPrefab;
    public Transform buildingInfoCellsParent;

    public GameObject buildingCellPrefab;
    public Transform buildingCellsParent;

    public ScrollRect cellsRect;

    public Button destroyButton;

    public GameObject cancelArea;

    public CanvasGroup descriptionWindow;
    public Text nameText;
    public Text desciptionText;

    public CanvasGroup listWindow;
    public Button closeList;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(BuildingManager.Instance.CloseWindow);
        destroyButton.onClick.AddListener(BuildingManager.Instance.DestroyBuildingToDestroy);
        closeList.onClick.AddListener(BuildingManager.Instance.HideBuiltList);
    }
}
