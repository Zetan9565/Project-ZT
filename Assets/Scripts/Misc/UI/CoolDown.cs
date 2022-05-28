using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Extension;

[RequireComponent(typeof(Image))]
public class CoolDown : MonoBehaviour
{
    public bool active = true;
    protected virtual bool Active => active && GetTime != null && GetTotal != null;

    [SerializeField]
    private Image mask;

    [SerializeField]
    private Text text;

    private Coroutine coroutine;

    private void Awake()
    {
        if (!text) text = GetComponentInChildren<Text>();
        if (!text) text = transform.CreateChild("CDText").AddComponent<Text>();
        OnAwake();
    }

    protected virtual void OnAwake() { }

    public virtual Func<float> GetTime { get; set; }
    public virtual Func<float> GetTotal { get; set; }

    private IEnumerator OnUpdate()
    {
        while (Active)
        {
            float time = GetTime();
            mask.fillAmount = time / GetTotal();
            text.text = time > 0 ? MiscFuntion.SecondsToSortTime(time) : string.Empty;
            yield return null;
        }
        Stop();
    }

    public void Restart()
    {
        active = true;
        ZetanUtility.SetActive(this, true);
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine= StartCoroutine(OnUpdate());
    }
    public void Stop()
    {
        active = false;
        mask.fillAmount = 0;
        text.text = string.Empty;
        ZetanUtility.SetActive(this, false);
    }
}
