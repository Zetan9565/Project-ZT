using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public abstract class ScrollGridView<TItem, TData> : GridView<TItem, TData>, IScrollList where TItem : GridItem<TItem, TData>
{
    public ScrollRect ScrollRect { get; protected set; }

    public override void SetSelected(int index, bool selected = true)
    {
        base.SetSelected(index, selected);
        ScrollToIndex(index);
    }

    protected override void RefreshLayoutGroup()
    {
        ScrollRect = GetComponent<ScrollRect>();
        container = IScrollList.ScrollRectPresetHelper<ScrollGridView<TItem, TData>, TItem, TData>(this);
        base.RefreshLayoutGroup();
    }

    public void ScrollToIndex(int index)
    {
        IScrollList.ScrollToIndexHelper_Grid<ScrollGridView<TItem, TData>, TItem, TData>(this, index);
    }
}