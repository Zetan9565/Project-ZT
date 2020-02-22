using UnityEngine;
using UnityEngine.UI;

public class TipsUI : MonoBehaviour
{
    public RectTransform textTips;
    public ContentSizeFitter textTipsFitter;
    public Text textTipsText;
    public Button textTipsCloseBtn;

    private Canvas tipsCanvas;

    private void Awake()
    {
        if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        tipsCanvas = GetComponent<Canvas>();
        tipsCanvas.sortingLayerName = "UI";
        tipsCanvas.overrideSorting = true;
        tipsCanvas.sortingOrder = 998;
        textTipsCloseBtn.onClick.AddListener(TipsManager.Instance.HideText);
    }
}