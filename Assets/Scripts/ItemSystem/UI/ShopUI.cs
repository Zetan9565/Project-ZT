using UnityEngine;
using UnityEngine.UI;

public class ShopUI : WindowUI
{
    public Text shopName;

    public GameObject merchandiseCellPrefab;
    public Transform merchandiseCellsParent;

    public Toggle commodityTab;
    public Toggle acquisitionTab;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(ShopManager.Instance.CloseWindow);
        commodityTab.onValueChanged.AddListener(delegate { if (ShopManager.Instance) ShopManager.Instance.SetPage(0); });
        acquisitionTab.onValueChanged.AddListener(delegate { if (ShopManager.Instance) ShopManager.Instance.SetPage(1); });
        commodityTab.isOn = true;
    }
}
