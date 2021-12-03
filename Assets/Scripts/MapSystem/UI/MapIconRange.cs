﻿using UnityEngine;
using UnityEngine.UI;

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
}
