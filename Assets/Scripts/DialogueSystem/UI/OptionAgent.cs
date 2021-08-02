using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OptionAgent : MonoBehaviour
{
    private Text titleText;

    private Action onClick;

    private void Awake()
    {
        titleText = GetComponentInChildren<Text>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Init(string text, Action callBack)
    {
        titleText.text = text;
        onClick = callBack;
    }

    public void OnClick()
    {
        onClick?.Invoke();
    }

    public void Recycle()
    {
        titleText.text = string.Empty;
        onClick = null;
        ObjectPool.Put(gameObject);
    }
}
