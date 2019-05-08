using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public CanvasGroup shopWindow;

    [HideInInspector]
    public Canvas windowCancas;

    public Text shopName;

    public GameObject merchandiseCellPrefab;
    public Transform merchandiseCellsParent;

    public Button closeButton;

    public Toggle commodityTab;
    public Toggle acquisitionTab;

    private void Awake()
    {
        if (!shopWindow.GetComponent<GraphicRaycaster>()) shopWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCancas = shopWindow.GetComponent<Canvas>();
        windowCancas.overrideSorting = true;
        closeButton.onClick.AddListener(ShopManager.Instance.CloseWindow);
        commodityTab.onValueChanged.AddListener(delegate { ShopManager.Instance.SetPage(0); });
        acquisitionTab.onValueChanged.AddListener(delegate { ShopManager.Instance.SetPage(1); });
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance) ShopManager.Instance.ResetUI();
    }
}
