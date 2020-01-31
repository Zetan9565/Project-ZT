using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlantUI : MonoBehaviour
{
    public CanvasGroup plantWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Dropdown pageSelector;

    public GameObject seedCellPrefab;
    public Transform seedCellsParent;

    public Button closeButton;

    public CanvasGroup descriptionWindow;

    public Text nameText;

    public ItemAgent icon;

    public Text description;

    public InputField searchInput;

    public Button searchButton;

    private void Awake()
    {
        if (!plantWindow.GetComponent<GraphicRaycaster>()) plantWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = plantWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        icon.Init();
        closeButton.onClick.AddListener(PlantManager.Instance.CloseWindow);
        pageSelector.onValueChanged.AddListener(PlantManager.Instance.SetPage);
    }
}
