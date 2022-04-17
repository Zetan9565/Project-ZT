using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent, RequireComponent(typeof(Button))]
public class ButtonWithText : ListItem<ButtonWithText, ButtonWithTextData>
{
    private Text text;

    private Action callback;

    private Action<object> callback_param;
    private Func<object> callback_getParam;

    protected override void OnAwake()
    {
        text = GetComponentInChildren<Text>();
        if (!text)
        {
            text = new GameObject("Text", typeof(Text)).GetComponent<Text>();
            text.font = Resources.Load<Font>("Font/Default");
            text.transform.SetParent(transform);
            RectTransform rectTransform = text.transform as RectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Init(ButtonWithTextData data)
    {
        text.text = data.text;
        callback = data.callback;
        callback_param = data.callback_param;
        callback_getParam = data.callcack_getParam;
    }
    public void Init(string text, Action callback)
    {
        this.text.text = text;
        this.callback = callback;
        callback_param = null;
        callback_getParam = null;
    }

    public void OnClick()
    {
        callback?.Invoke();
        callback_param?.Invoke(callback_getParam?.Invoke());
    }

    public void Recycle()
    {
        text.text = string.Empty;
        callback = null;
        callback_param = null;
        callback_getParam = null;
        ObjectPool.Put(gameObject);
    }

    public override void Refresh()
    {
        text.text = Data.text;
        callback = Data.callback;
        callback_param = Data.callback_param;
        callback_getParam = Data.callcack_getParam;
    }
}

public class ButtonWithTextData
{
    public string text;
    public Action callback;
    public Action<object> callback_param;
    public Func<object> callcack_getParam;

    public ButtonWithTextData(string text, Action callback)
    {
        this.text = text;
        this.callback = callback;
    }

    public ButtonWithTextData(string text, Action<object> callback, Func<object> param)
    {
        this.text = text;
        callback_param = callback;
        callcack_getParam = param;
    }
}