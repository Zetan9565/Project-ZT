using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways, RequireComponent(typeof(ScrollRect), typeof(LayoutElement))]
public class ScrollRectFitter : MonoBehaviour
{
    public RectOffset padding;
    public Vector2 minSize;
    public Vector2 maxSize;
    private ScrollRect scrollRect;
    private LayoutElement layout;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        layout = GetComponent<LayoutElement>();
    }

    private void LateUpdate()
    {
        if (scrollRect.content && padding != null)
        {
            float offsetWidth = scrollRect.content.rect.width + padding.horizontal;
            if (offsetWidth < maxSize.x) layout.minWidth = offsetWidth < minSize.x ? (minSize.x > 0 ? minSize.x : -1) : offsetWidth;
            else layout.minWidth = maxSize.x > 0 ? maxSize.x : -1;
            float offsetHeigt = scrollRect.content.rect.height + padding.vertical;
            if (offsetHeigt < maxSize.y) layout.minHeight = offsetHeigt < minSize.y ? (minSize.y > 0 ? minSize.y : -1) : offsetHeigt;
            else layout.minHeight = maxSize.y > 0 ? maxSize.y : -1;
        }
    }
    private void OnEnable()
    {
        if (layout) layout.hideFlags = HideFlags.NotEditable;
    }
    private void OnDisable()
    {
        if (layout) layout.hideFlags = HideFlags.None;
    }
}
