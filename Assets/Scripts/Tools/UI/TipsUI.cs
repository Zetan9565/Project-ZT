using UnityEngine;
using UnityEngine.UI;

public class TipsUI : MonoBehaviour
{
    public RectTransform tipBackground;
    public ContentSizeFitter tipsFitter;
    public Text tipsContent;

    private Canvas tipsCanvas;

    public TipsButton buttonPrefab;
    public GridLayoutGroup buttonParent;

    private void Awake()
    {
        if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        tipsCanvas = GetComponent<Canvas>();
        tipsCanvas.sortingLayerName = "UI";
        tipsCanvas.overrideSorting = true;
        tipsCanvas.sortingOrder = 998;
    }
}