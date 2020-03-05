using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TipsManager : SingletonMonoBehaviour<TipsManager>
{
    [SerializeField]
    private TipsUI UI;

    private readonly List<TipsButton> buttons = new List<TipsButton>();

    public void ShowText(Vector2 position, string text, float lastingTime, bool displayClose = false)
    {
        UI.tipBackground.position = position;
        ZetanUtility.SetActive(UI.tipBackground.gameObject, true);
        ZetanUtility.SetActive(UI.tipsContent.gameObject, true);
        UI.tipsContent.text = text;
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        hideNameCoroutine = StartCoroutine(HideTextDelay(lastingTime));
        if (displayClose) MakeButton("关闭", Hide);
    }
    public void ShowText(Vector2 position, string text, bool displayClose = false)
    {
        UI.tipBackground.position = position;
        ZetanUtility.SetActive(UI.tipBackground.gameObject, true);
        ZetanUtility.SetActive(UI.tipsContent.gameObject, true);
        UI.tipsContent.text = text;
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        if (displayClose) MakeButton("关闭", Hide);
    }

    public void Hide()
    {
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        ZetanUtility.SetActive(UI.tipBackground.gameObject, false);
        ZetanUtility.SetActive(UI.tipsContent.gameObject, false);
        UI.tipsContent.text = string.Empty;
        foreach (var tb in buttons)
            tb.Hide();
        UI.buttonParent.constraintCount = 1;
    }

    private Coroutine hideNameCoroutine;
    private IEnumerator HideTextDelay(float time)
    {
        if (time < 1) yield break;
        else yield return new WaitForSeconds(time);
        Hide();
    }

    private readonly WaitForSeconds waitForSeconds = new WaitForSeconds(0.01f);
    IEnumerator FixTextWidth()
    {
        while (true)
        {
            if (UI && UI.gameObject && UI.tipBackground.gameObject.activeSelf)
            {
                if (UI.tipsContent.rectTransform.rect.width < 380 && UI.tipsFitter.horizontalFit != UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize)
                    UI.tipsFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                else if (UI.tipsContent.rectTransform.rect.width > 380 && UI.tipsFitter.horizontalFit != UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                    UI.tipsFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                if (UI.tipBackground.rect.width > 400) UI.tipBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
                ZetanUtility.KeepInsideScreen(UI.tipBackground);
                yield return waitForSeconds;
            }
            else yield return new WaitUntil(() => { return UI && UI.gameObject && UI.tipBackground.gameObject.activeSelf; });
        }
    }

    public void MakeButton(string name, UnityAction clickAction, int index = -1)
    {
        TipsButton tb = buttons.Find(x => x.IsHiding && x.name != name);
        if (!tb)
        {
            tb = ObjectPool.Instance.Get(UI.buttonPrefab.gameObject, UI.buttonParent.transform).GetComponent<TipsButton>();
            buttons.Add(tb);
        }
        tb.Show(name, clickAction);
        int bc = buttons.FindAll(x => !x.IsHiding).Count;
        if (bc < 3) UI.buttonParent.constraintCount = bc;
        else UI.buttonParent.constraintCount = 3;
        if (index >= 0 && index < tb.transform.parent.childCount - 1) tb.transform.SetSiblingIndex(0);
    }

    private void Start()
    {
        StartCoroutine(FixTextWidth());
    }
}
