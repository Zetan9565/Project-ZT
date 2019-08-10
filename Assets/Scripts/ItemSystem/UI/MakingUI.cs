using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MakingUI : MonoBehaviour
{
    public CanvasGroup makingWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Dropdown pageSelector;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Button closeButton;

    public CanvasGroup descriptionWindow;

    public Text nameText;

    public ItemAgent icon;

    public Button makeButton;

    public Text description;

    private void Awake()
    {
        if (!makingWindow.GetComponent<GraphicRaycaster>()) makingWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = makingWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        icon.Init(ItemAgentType.Making);
        closeButton.onClick.AddListener(MakingManager.Instance.CloseWindow);
        makeButton.onClick.AddListener(MakingManager.Instance.MakeCurrent);
        pageSelector.onValueChanged.AddListener(MakingManager.Instance.SetPage);
    }
}
