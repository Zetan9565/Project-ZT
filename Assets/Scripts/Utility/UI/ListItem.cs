using System;
using UnityEngine;
using UnityEngine.Events;

public sealed class ListItem : ListItem<ListItem, object>
{
    public ListItemEvent onAwake = new ListItemEvent();
    public ListItemEvent onInit = new ListItemEvent();
    public ListItemEvent onRefresh = new ListItemEvent();
    public ListItemEvent onRefreshSelected = new ListItemEvent();
    public ListItemEvent onClear = new ListItemEvent();

    protected override void OnAwake()
    {
        onAwake?.Invoke(this);
    }

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

    public override void Clear()
    {
        base.Clear();
        onClear?.Invoke(this);
    }

    [Serializable]
    public class ListItemEvent : UnityEvent<ListItem> { }
}

[RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
public abstract class ListItem<TSelf, TData> : MonoBehaviour where TSelf : ListItem<TSelf, TData>
{
    public ListView<TSelf, TData> View { get; protected set; }

    public virtual TData Data { get; protected set; }

    /// <summary>
    /// 根据当前<see cref="Data"/>刷新
    /// </summary>
    public abstract void Refresh();

    public int Index { get; protected set; }

    protected bool isSelected;
    /// <summary>
    /// 是否选中，通过此属性设置选中状态不受<see cref="ListView{TItem, TData}.Selectable"/>的限制，但不会触发<see cref="ListView{TItem, TData}"/>的选中回调，只会触发<see cref="RefreshSelected"/>
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected != value)
            {
                isSelected = value;
                RefreshSelected();
            }
        }
    }

    /// <summary>
    /// 间接调用<see cref="ListView{TItem, TData}.SetSelected(int, bool)"/>选中此元素
    /// </summary>
    /// <param name="selected"></param>
    public void SetSelected(bool selected = true)
    {
        View.SetSelected(Index, selected);
    }

    /// <summary>
    /// 设置新的<see cref="Data"/>并刷新
    /// </summary>
    /// <param name="data">要替换的新<see cref="Data"/></param>
    public virtual void Refresh(TData data)
    {
        Data = data;
        Refresh();
        RefreshSelected();
    }

    /// <summary>
    /// 初始化，由<see cref="ListView{TItem, TData}"/>调用
    /// </summary>
    /// <param name="view"></param>
    /// <param name="index"></param>
    public void Init(ListView<TSelf, TData> view, int index)
    {
        View = view;
        SetIndex(index);
        OnInit();
    }

    protected void SetIndex(int index)
    {
        Index = index;
    }

    private void Awake()
    {
        OnAwake();
    }

    /// <summary>
    /// <see cref="Awake"/>时调用，默认为空
    /// </summary>
    protected virtual void OnAwake() { }
    /// <summary>
    /// <see cref="Init(ListView{TSelf, TData}, int)"/>时调用，默认为空
    /// </summary>
    protected virtual void OnInit() { }
    /// <summary>
    /// <see cref="IsSelected"/>改变时调用，默认为空
    /// </summary>
    protected virtual void RefreshSelected() { }
    /// <summary>
    /// 由<see cref="ListView{TItem, TData}"/>调用，默认将<see cref="Data"/>置空，并清除选择状态
    /// </summary>
    public virtual void Clear() { Data = default; isSelected = false; RefreshSelected(); }
}