using UnityEngine;
using UnityEngine.EventSystems;

public class HelpInfoAgent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string infoToShow;

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (!string.IsNullOrEmpty(infoToShow)) TipsManager.Instance.ShowText(transform.position, infoToShow, true);
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (!string.IsNullOrEmpty(infoToShow)) TipsManager.Instance.ShowText(transform.position, infoToShow);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (!string.IsNullOrEmpty(infoToShow)) TipsManager.Instance.Hide();
#endif
    }
}
