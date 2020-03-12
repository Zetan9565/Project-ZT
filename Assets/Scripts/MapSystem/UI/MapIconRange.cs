using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MapIconRange : MonoBehaviour
{
    public new Transform transform { get; private set; }

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
        transform = base.transform;
    }
}
