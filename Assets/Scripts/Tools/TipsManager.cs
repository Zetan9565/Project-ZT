using System.Collections;
using UnityEngine;

public class TipsManager : SingletonMonoBehaviour<TipsManager>
{
    [SerializeField]
    private TipsUI UI;

    public void ShowText(Vector2 position, string text, float lastingTime, bool displayClose = false)
    {
        UI.textTips.position = position;
        ZetanUtility.SetActive(UI.textTips.gameObject, true);
        UI.textTipsText.text = text;
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        hideNameCoroutine = StartCoroutine(HideTextDelay(lastingTime));
        ZetanUtility.SetActive(UI.textTipsCloseBtn.gameObject, displayClose);
    }
    public void ShowText(Vector2 position, string text, bool displayClose = false)
    {
        UI.textTips.position = position;
        ZetanUtility.SetActive(UI.textTips.gameObject, true);
        UI.textTipsText.text = text;
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        ZetanUtility.SetActive(UI.textTipsCloseBtn.gameObject, displayClose);
    }

    public void HideText()
    {
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        ZetanUtility.SetActive(UI.textTips.gameObject, false);
        UI.textTipsText.text = string.Empty;
    }

    public void HideAll()
    {
        HideText();
    }

    private Coroutine hideNameCoroutine;
    private IEnumerator HideTextDelay(float time)
    {
        if (time < 1) yield break;
        else yield return new WaitForSeconds(time);
        HideText();
    }

    private readonly WaitForSeconds waitForSeconds = new WaitForSeconds(0.01f);
    IEnumerator FixTextWidth()
    {
        while (true)
        {
            if (UI && UI.gameObject && UI.textTips.gameObject.activeSelf)
            {
                if (UI.textTipsText.rectTransform.rect.width < 380 && UI.textTipsFitter.horizontalFit != UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize)
                    UI.textTipsFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                else if (UI.textTipsText.rectTransform.rect.width > 380 && UI.textTipsFitter.horizontalFit != UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                    UI.textTipsFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                if (UI.textTips.rect.width > 400) UI.textTips.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
                Rect rect = ZetanUtility.GetScreenSpaceRect(UI.textTips);
                float halfRectWidth = rect.width / 2;
                if (UI.textTips.position.x - halfRectWidth < 0) UI.textTips.position += Vector3.right * (halfRectWidth - UI.textTips.position.x);
                if (UI.textTips.position.x + halfRectWidth > Screen.width) UI.textTips.position -= Vector3.right * (UI.textTips.position.x + halfRectWidth - Screen.width);
                if (UI.textTips.position.y + rect.height > Screen.height) UI.textTips.position -= Vector3.up * (UI.textTips.position.y + rect.height - Screen.height);
                yield return waitForSeconds;
            }
            else yield return new WaitUntil(() => { return UI && UI.gameObject && UI.textTips.gameObject.activeSelf; });
        }
    }

    private void Start()
    {
        StartCoroutine(FixTextWidth());
    }
}
