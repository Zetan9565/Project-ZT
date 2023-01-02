using System;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;

[RequireComponent(typeof(Image))]
public class MapIconRange : MonoBehaviour
{
    private Image range;

    public Color Color
    {
        get => range.color;
        set => range.color = value;
    }

    public RectTransform rectTransform;

    private void Awake()
    {
        range = GetComponent<Image>();
        range.raycastTarget = false;
        rectTransform = range.rectTransform;
    }

    public void Init(float radius, Color? color = null)
    {
        Vector2 size = new Vector2(radius * 2, radius * 2);
        if (rectTransform.sizeDelta != size) rectTransform.sizeDelta = size;
        Color = color ?? Color;
    }
    public void Show(float radius)
    {
        Vector2 size = new Vector2(radius * 2, radius * 2);
        if (rectTransform.sizeDelta != size) rectTransform.sizeDelta = size;
        Utility.SetActive(this, true);
    }
    public void Hide()
    {
        Utility.SetActive(this, false);
    }

    public void Recycle()
    {
        ObjectPool.Put(gameObject);
    }
}
