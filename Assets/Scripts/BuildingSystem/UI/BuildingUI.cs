using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    public CanvasGroup buildingWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public GameObject buildingInfoCellPrefab;
    public Transform buildingInfoCellsParent;

    public GameObject buildingCellPrefab;
    public Transform buildingCellsParent;

    public ScrollRect cellsRect;

    public Button closeButton;

    public Button destroyButton;

    public GameObject cancelArea;

    public CanvasGroup descriptionWindow;
    public Text nameText;
    public Text desciptionText;

    public CanvasGroup listWindow;
    public Button closeList;

    private void Awake()
    {
        if (!buildingWindow.GetComponent<GraphicRaycaster>()) buildingWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = buildingWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        closeButton.onClick.AddListener(BuildingManager.Instance.CloseWindow);
        destroyButton.onClick.AddListener(BuildingManager.Instance.DestroyToDestroyBuilding);
        closeList.onClick.AddListener(BuildingManager.Instance.HideBuiltList);
    }
}
