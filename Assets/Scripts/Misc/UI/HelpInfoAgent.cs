using UnityEngine;
using UnityEngine.EventSystems;
using ZetanStudio;

public class HelpInfoAgent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool autoHide;
    [HideIf("autoHide", false)]
    public float hideDelay = 3;

    [TextArea]
    public string infoToShow;

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (!string.IsNullOrEmpty(infoToShow))
        {
            if (!autoHide) FloatTipsPanel.ShowText(transform.position, Tr(infoToShow), closeBtn: true);
            else FloatTipsPanel.ShowText(transform.position, Tr(infoToShow), hideDelay);
        }
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (!autoHide) FloatTipsPanel.ShowText(transform.position, Tr(infoToShow));
        else FloatTipsPanel.ShowText(transform.position, Tr(infoToShow), hideDelay);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (!string.IsNullOrEmpty(infoToShow)) WindowsManager.CloseWindow<FloatTipsPanel>();
#endif
    }

    private string Tr(string text)
    {
        return LM.Tr(GetType().Name, text);
    }
}
