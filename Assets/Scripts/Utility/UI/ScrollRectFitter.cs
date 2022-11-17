using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Extension;

namespace ZetanStudio.UI
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform), typeof(ScrollRect))]
    public class ScrollRectFitter : MonoBehaviour
    {
        public RectOffset padding;
        public Vector2 minSize;
        public Vector2 maxSize;
        private ScrollRect scrollRect;
        private LayoutElement layout;
        private RectTransform rectTransform;

        private void Awake()
        {
            if (GetComponentInParent<LayoutGroup>()) layout = this.GetOrAddComponent<LayoutElement>();
            scrollRect = GetComponent<ScrollRect>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (!isActiveAndEnabled) return;
            if (scrollRect.content && padding != null)
            {
                if (GetComponentInParent<LayoutGroup>())
                {
                    if (!layout) layout = this.GetOrAddComponent<LayoutElement>();
                    if (!layout.enabled) layout.enabled = true;
                    float preferredWidth = scrollRect.content.rect.width + padding.horizontal;
                    if (preferredWidth < maxSize.x) layout.minWidth = preferredWidth < minSize.x ? minSize.x > 0 ? minSize.x : -1 : preferredWidth;
                    else layout.minWidth = maxSize.x > 0 ? maxSize.x : -1;
                    float preferredHeight = scrollRect.content.rect.height + padding.vertical;
                    if (preferredHeight < maxSize.y) layout.minHeight = preferredHeight < minSize.y ? minSize.y > 0 ? minSize.y : -1 : preferredHeight;
                    else layout.minHeight = maxSize.y > 0 ? maxSize.y : -1;
                }
                else
                {
                    if (layout && layout.enabled) layout.enabled = false;
                    float preferredWidth = scrollRect.content.rect.width + padding.horizontal;
                    if (preferredWidth < maxSize.x)
                    {
                        if (preferredWidth < minSize.x)
                        {
                            if (minSize.x > 0) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minSize.x);
                        }
                        else rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
                    }
                    else if (maxSize.x > 0) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize.x);
                    float preferredHeight = scrollRect.content.rect.height + padding.vertical;
                    if (preferredHeight < maxSize.y)
                    {
                        if (preferredHeight < minSize.y)
                        {
                            if (minSize.y > 0) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minSize.y);
                        }
                        else rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
                    }
                    else if (maxSize.y > 0) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize.y);
                }
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
}