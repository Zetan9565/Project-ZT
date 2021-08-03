using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent, RequireComponent(typeof(Button))]
public class ButtonWithText : MonoBehaviour
{
    private Text text;

    private Action callback;

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

    public void OnClick()
    {
        callback?.Invoke();
    }

    public void Recycle()
    {
        text.text = string.Empty;
        ObjectPool.Put(gameObject);
    }
}