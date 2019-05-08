using UnityEngine;
using UnityEngine.UI;

public class WarehouseUI : MonoBehaviour
{
    public CanvasGroup warehouseWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Text money;
    public Text size;

    //public Button warehouseButton;
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
        //warehouseButton.onClick.AddListener(WarehouseManager.Instance.OpenWindow);
    }

    private void OnDestroy()
    {
        if (WarehouseManager.Instance) WarehouseManager.Instance.ResetUI();
    }
}
