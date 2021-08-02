using UnityEngine;
using UnityEngine.EventSystems;

public class HelpInfoAgent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool autoHide;
    public float hideDelay = 3;

    [TextArea]
    public string infoToShow;

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (!string.IsNullOrEmpty(infoToShow))
        {
            if (!autoHide) TipsManager.Instance.ShowText(transform.position, infoToShow, true);
            else TipsManager.Instance.ShowText(transform.position, infoToShow, hideDelay);
        }
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (!autoHide) TipsManager.Instance.ShowText(transform.position, infoToShow);
        else TipsManager.Instance.ShowText(transform.position, infoToShow, hideDelay);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (!string.IsNullOrEmpty(infoToShow)) TipsManager.Instance.Hide();
#endif
    }
}
