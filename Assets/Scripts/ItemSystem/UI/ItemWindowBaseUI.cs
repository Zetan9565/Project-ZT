using UnityEngine;
using UnityEngine.UI;

public class ItemWindowBaseUI : MonoBehaviour
{
    public CanvasGroup itemWindow;

    [HideInInspector]
    public RectTransform windowsRect;

    [HideInInspector]
    public Canvas windowCanvas;

    public Image icon;

    public Text nameText;

    public Text typeText;

    public Text effectText;

    public Text priceTitle;
    public Text priceText;
    public Text weightText;

    public Text mulFunTitle;
    public Text mulFunText;

    public GemstoneAgent gemstone_1;
    public GemstoneAgent gemstone_2;

    public Text descriptionText;

    /// <summary>
    /// 耐久度
    /// </summary>
    public DurabilityAgent durability;

    private void Awake()
    {
        if (!itemWindow.GetComponent<GraphicRaycaster>()) itemWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = itemWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowsRect = itemWindow.GetComponent<RectTransform>();

        gemstone_1.Clear();
        gemstone_2.Clear();
    }
}