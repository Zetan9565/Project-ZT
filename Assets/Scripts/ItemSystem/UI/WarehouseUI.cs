using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class WarehouseUI : MonoBehaviour
{
    public CanvasGroup warehouseWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Dropdown pageSelector;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Text money;
    public Text size;

    public Button closeButton;
    public Button sortButton;

    public ScrollRect gridRect;

    private void Awake()
    {
        if (!warehouseWindow.gameObject.GetComponent<GraphicRaycaster>()) warehouseWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = warehouseWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        closeButton.onClick.AddListener(WarehouseManager.Instance.CloseWindow);
        sortButton.onClick.AddListener(WarehouseManager.Instance.Sort);
        pageSelector.onValueChanged.AddListener(WarehouseManager.Instance.SetPage);
    }
}
