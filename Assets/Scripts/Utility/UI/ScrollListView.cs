using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Extension;

[RequireComponent(typeof(ScrollRect))]
public abstract class ScrollListView<TItem, TData> : ListView<TItem, TData>, IScrollList where TItem : ListItem<TItem, TData>
{
    public ScrollRect ScrollRect { get; protected set; }

    public override void SetSelected(int index, bool selected = true)
    {
        base.SetSelected(index, selected);
        if (selected) ScrollToIndex(index);
    }

    protected override void RefreshLayoutGroup()
    {
        ScrollRect = GetComponent<ScrollRect>();
        container = IScrollList.ScrollRectPresetHelper<ScrollListView<TItem, TData>, TItem, TData>(this);
        base.RefreshLayoutGroup();
    }

    public void ScrollToIndex(int index)
    {
        IScrollList.ScrollToIndexHelper_List<ScrollListView<TItem, TData>, TItem, TData>(this, index);
    }
}
public interface IScrollList
{
    ScrollRect ScrollRect { get; }
    /// <summary>
    /// 滚动至指定元素
    /// </summary>
    /// <param name="index">第几个元素(从1开始)</param>
    void ScrollToIndex(int index);

    public static RectTransform ScrollRectPresetHelper<TList, TItem, TData>(TList scollList) where TList : ListView<TItem, TData>, IScrollList where TItem : ListItem<TItem, TData>
    {
        scollList.ScrollRect.horizontal = scollList.Direction == LayoutDirection.Horizontal;
        scollList.ScrollRect.vertical = scollList.Direction == LayoutDirection.Vertical;
        if (!scollList.ScrollRect.viewport) scollList.ScrollRect.viewport = scollList.GetRectTransform().FindOrCreate("Viewport");
        scollList.ScrollRect.viewport.GetOrAddComponent<Image>();
        scollList.ScrollRect.viewport.GetOrAddComponent<Mask>().showMaskGraphic = false;
        PresetRect(scollList.ScrollRect.viewport, false);
        if (!scollList.ScrollRect.content) scollList.ScrollRect.content = scollList.ScrollRect.viewport.FindOrCreate("Container");
        PresetRect(scollList.ScrollRect.content, true);
        var fitter = scollList.ScrollRect.content.GetOrAddComponent<ContentSizeFitter>();
        fitter.horizontalFit = scollList.Direction == LayoutDirection.Horizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = scollList.Direction == LayoutDirection.Vertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        return scollList.ScrollRect.content;

        void PresetRect(RectTransform rectTransform, bool isContainer)
        {
            if (!isContainer)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
            }
            switch (scollList.Direction)
            {
                case LayoutDirection.Horizontal:
                    if (isContainer)
                    {
                        rectTransform.anchorMin = Vector2.zero;
                        rectTransform.anchorMax = Vector2.up;
                    }
                    rectTransform.pivot = Vector2.right;
                    break;
                case LayoutDirection.Vertical:
                    if (isContainer)
                    {
                        rectTransform.anchorMin = Vector2.up;
                        rectTransform.anchorMax = Vector2.one;
                    }
                    rectTransform.pivot = Vector2.up;
                    break;
                default:
                    break;
            }
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    public static void ScrollToIndexHelper_List<TList, TItem, TData>(TList scollList, int index) where TList : ListView<TItem, TData>, IScrollList where TItem : ListItem<TItem, TData>
    {
        switch (scollList.Direction)
        {
            case LayoutDirection.Horizontal:
                float width = 0;
                for (int i = 0; i < index - 1 && width < scollList.ScrollRect.content.rect.width; i++)
                {
                    if (scollList.Items[i].gameObject.activeSelf)
                        width += scollList.Items[i].GetRectTransform().rect.width + scollList.Spacing.x;
                }
                float offset = scollList.ScrollRect.content.rect.width - scollList.ScrollRect.viewport.rect.width;
                width = width <= offset ? width : offset > 0 ? offset : 0;
                scollList.ScrollRect.content.anchoredPosition = new Vector2(width, scollList.ScrollRect.content.anchoredPosition.y);
                scollList.ScrollRect.StopMovement();
                break;
            case LayoutDirection.Vertical:
                float height = 0;
                for (int i = 0; i < index - 1 && height < scollList.ScrollRect.content.rect.height; i++)
                {
                    if (scollList.Items[i].gameObject.activeSelf)
                        height += scollList.Items[i].GetRectTransform().rect.height + scollList.Spacing.y;
                }
                offset = scollList.ScrollRect.content.rect.height - scollList.ScrollRect.viewport.rect.height;
                height = height <= offset ? height : offset > 0 ? offset : 0;
                scollList.ScrollRect.content.anchoredPosition = new Vector2(scollList.ScrollRect.content.anchoredPosition.x, height);
                scollList.ScrollRect.StopMovement();
                //ZetanUtility.Log(height, scollList.Items[index - 1].GetRectTransform().anchoredPosition.y);
                break;
        }
    }
    public static void ScrollToIndexHelper_Grid<TList, TItem, TData>(TList scollList, int index) where TList : GridView<TItem, TData>, IScrollList where TItem : GridItem<TItem, TData>
    {
        switch (scollList.Direction)
        {
            case LayoutDirection.Horizontal:
                float width = 0;
                int actCount = scollList.Items.Count(x => x.gameObject.activeSelf);
                for (int i = 0; i < actCount && i < index - 1 && width < scollList.ScrollRect.content.rect.width; i += scollList.ConstraintCount)
                {
                    width += scollList.ApplyCellSize.x + scollList.Spacing.x;
                }
                float offset = scollList.ScrollRect.content.rect.width - scollList.ScrollRect.viewport.rect.width;
                width = width <= offset ? width : offset > 0 ? offset : 0;
                scollList.ScrollRect.content.anchoredPosition = new Vector2(width, scollList.ScrollRect.content.anchoredPosition.y);
                scollList.ScrollRect.StopMovement();
                break;
            case LayoutDirection.Vertical:
                float height = 0;
                actCount = scollList.Items.Count(x => x.gameObject.activeSelf);
                for (int i = 0; i < actCount && i < index - 1 && height < scollList.ScrollRect.content.rect.height; i += scollList.ConstraintCount)
                {
                    height += scollList.ApplyCellSize.y + scollList.Spacing.y;
                }
                offset = scollList.ScrollRect.content.rect.height - scollList.ScrollRect.viewport.rect.height;
                height = height <= offset ? height : offset > 0 ? offset : 0;
                scollList.ScrollRect.content.anchoredPosition = new Vector2(scollList.ScrollRect.content.anchoredPosition.x, height);
                scollList.ScrollRect.StopMovement();
                break;
        }
    }
    public static void ScrollToIndexHelper_Grid<TList, TItem, TData>(TList scollList, int row, int col) where TList : GridView<TItem, TData>, IScrollList where TItem : GridItem<TItem, TData>
    {
        switch (scollList.Direction)
        {
            case LayoutDirection.Horizontal:
                ScrollToIndexHelper_Grid<TList, TItem, TData>(scollList, col * scollList.ConstraintCount + row);
                break;
            case LayoutDirection.Vertical:
                ScrollToIndexHelper_Grid<TList, TItem, TData>(scollList, row * scollList.ConstraintCount + col);
                break;
        }
    }
}