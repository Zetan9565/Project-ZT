using System;
using UnityEngine;
using UnityEngine.Events;

public class GridItem : GridItem<GridItem, object>
{
    public GridItemEvent onInit = new GridItemEvent();
    public GridItemEvent onRefresh = new GridItemEvent();
    public GridItemEvent onRefreshSelected = new GridItemEvent();
    public GridItemEvent onClear = new GridItemEvent();

    protected override void OnInit()
    {
        base.OnInit();
        onInit?.Invoke(this);
    }

    public override void Refresh()
    {
        onRefresh?.Invoke(this);
    }

    protected override void RefreshSelected()
    {
        onRefreshSelected?.Invoke(this);
    }

    public override void OnClear()
    {
        base.OnClear();
        onClear?.Invoke(this);
    }

    [Serializable]
    public class GridItemEvent : UnityEvent<GridItem> { }
}

[RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
public abstract class GridItem<TSelf, TData> : ListItem<TSelf, TData> where TSelf : GridItem<TSelf, TData>
{
    public Vector2Int GridIndex { get; protected set; }

    public void Init(GridView<TSelf, TData> view, int rowIndex, int colIndex)
    {
        Init(view, rowIndex * view.ConstraintCount + colIndex);
        SetIndex(rowIndex, colIndex);
    }

    public void SetIndex(int rowIndex, int colIndex)
    {
        GridIndex.Set(rowIndex, colIndex);
    }
}