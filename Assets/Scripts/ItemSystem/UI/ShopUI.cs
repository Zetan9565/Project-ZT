using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public CanvasGroup shopWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Text shopName;

    public GameObject merchandiseCellPrefab;
    public Transform merchandiseCellsParent;

    public Button closeButton;

    public Toggle commodityTab;
    public Toggle acquisitionTab;

    private void Awake()
    {
        if (!shopWindow.GetComponent<GraphicRaycaster>()) shopWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = shopWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        closeButton.onClick.AddListener(ShopManager.Instance.CloseWindow);
        commodityTab.onValueChanged.AddListener(delegate { if (ShopManager.Instance) ShopManager.Instance.SetPage(0); });
        acquisitionTab.onValueChanged.AddListener(delegate { if (ShopManager.Instance) ShopManager.Instance.SetPage(1); });
        commodityTab.isOn = true;
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance) ShopManager.Instance.ResetUI();
    }
}
