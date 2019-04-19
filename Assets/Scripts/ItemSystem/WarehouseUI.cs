using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WarehouseUI : MonoBehaviour
{
    public CanvasGroup warehouseWindow;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Text money;
    public Text size;

    public Button openButton;
    public Button closeButton;
    public Button sortButton;

    public ScrollRect gridRect;

    private void Awake()
    {
        openButton.onClick.AddListener(WarehouseManager.Instance.OpenWarehouseWindow);
        closeButton.onClick.AddListener(WarehouseManager.Instance.CloseWarehouseWindow);
        sortButton.onClick.AddListener(WarehouseManager.Instance.Sort);
    }
}
