using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FloatTipsPanel : Window
{
    public RectTransform tipBackground;
    public ContentSizeFitter tipsFitter;
    public Text tipsContent;

    public TipsButton buttonPrefab;
    public GridLayoutGroup buttonParent;

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    private readonly List<TipsButton> buttons = new List<TipsButton>();

    public static FloatTipsPanel ShowText(Vector2 position, string text, float duration = 0, bool closeBtn = false)
    {
        return NewWindowsManager.OpenWindow<FloatTipsPanel>(position, text, duration, closeBtn);
    }

    public void Hide()
    {
        Close();
    }

    protected override bool OnOpen(params object[] args)
    {
        if (args.Length < 4) return false;
        var par = (position: (Vector2)args[0], text: args[1] as string, duration: (float)args[2], closeBtn: (bool)args[3]);
        tipBackground.position = par.position;
        ZetanUtility.SetActive(tipBackground.gameObject, true);
        ZetanUtility.SetActive(tipsContent.gameObject, true);
        tipsContent.text = par.text;
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        if (par.duration > 0) hideNameCoroutine = StartCoroutine(HideTextDelay(par.duration));
        if (par.closeBtn) MakeButton("关闭", Hide);
        return true;
    }

    protected override bool OnClose(params object[] args)
    {
        if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
        ZetanUtility.SetActive(tipsContent.gameObject, false);
        tipsContent.text = string.Empty;
        foreach (var tb in buttons)
            tb.Hide();
        buttonParent.constraintCount = 1;
        return true;
    }

    private Coroutine hideNameCoroutine;
    private IEnumerator HideTextDelay(float time)
    {
        if (time < 1) yield break;
        else yield return new WaitForSeconds(time);
        Hide();
    }

    private readonly WaitForEndOfFrame wait = new WaitForEndOfFrame();
    IEnumerator FixTextWidth()
    {
        while (true)
        {
            if (IsOpen)
            {
                if (tipsContent.rectTransform.rect.width < 380 && tipsFitter.horizontalFit != ContentSizeFitter.FitMode.PreferredSize)
                    tipsFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                else if (tipsContent.rectTransform.rect.width > 380 && tipsFitter.horizontalFit != ContentSizeFitter.FitMode.Unconstrained)
                    tipsFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                if (tipBackground.rect.width > 400) tipBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
                ZetanUtility.KeepInsideScreen(tipBackground);
                yield return wait;
            }
            else yield return new WaitUntil(() => IsOpen);
        }
    }

    public void MakeButton(string name, UnityAction clickAction, int index = -1)
    {
        TipsButton tb = buttons.Find(x => x.IsHiding && x.name != name);
        if (!tb)
        {
            tb = ObjectPool.Get(buttonPrefab.gameObject, buttonParent.transform).GetComponent<TipsButton>();
            buttons.Add(tb);
        }
        tb.Show(name, clickAction);
        int bc = buttons.FindAll(x => !x.IsHiding).Count;
        if (bc < 3) buttonParent.constraintCount = bc;
        else buttonParent.constraintCount = 3;
        if (index >= 0 && index < tb.transform.parent.childCount - 1) tb.transform.SetSiblingIndex(0);
    }

    protected override void OnStart()
    {
        base.OnStart();
        StartCoroutine(FixTextWidth());
    }
}