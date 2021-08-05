using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent, RequireComponent(typeof(Button))]
public class ButtonWithText : MonoBehaviour
{
    private Text text;

    private Action callback;

    private Action<object> callback_param;
    private Func<object> callback_getParam;

    private void Awake()
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
        }
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Init(string text, Action callback)
    {
        this.text.text = text;
        this.callback = callback;
    }

    public void Init(string text, Action<object> callback, Func<object> param)
    {
        this.text.text = text;
        callback_param = callback;
        callback_getParam = param;
    }

    public void Init(ButtonWithTextData data)
    {
        text.text = data.text;
        callback = data.callback;
        callback_param = data.callback_param;
        callback_getParam = data.callcack_getParam;
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