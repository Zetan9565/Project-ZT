using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZetanExtends;

public sealed class ListView : ListView<ListItem, object> { }

public abstract class ListView<TItem, TData> : ListViewBase where TItem : ListItem<TItem, TData>
{
    public LayoutDirection Direction
    {
        get => direction;
        set
        {
            if (direction != value)
            {
                direction = value;
                RefreshLayoutGroup();
            }
        }
    }
    public TextAnchor ChildAlignment
    {
        get => childAlignment;
        set
        {
            if (childAlignment != value)
                layoutGroup.childAlignment = childAlignment = value;
        }
    }
    public Vector2 Spacing
    {
        get => spacing;
        set
        {
            if (spacing != value)
            {
                spacing = value;
                RefreshSpacing();
            }
        }

    }
    public RectOffset Padding
    {
        get => padding;
        set => layoutGroup.padding = padding = value;
    }
    public bool OverrideCellSize
    {
        get => overrideCellSize;
        set
        {
            if (overrideCellSize != value)
            {
                overrideCellSize = value;
                if (value) RefreshOverrideCellSize();
            }
        }
    }
    public bool FillWidth
    {
        get => fillWidth && overrideCellSize;
        set
        {
            if (fillWidth != value)
            {
                fillWidth = value;
                if (value) RefreshOverrideCellSize();
            }
        }
    }
    public bool FillHeight
    {
        get => fillHeight && overrideCellSize;
        set
        {
            if (fillHeight != value)
            {
                fillHeight = value;
                if (value) RefreshOverrideCellSize();
            }
        }
    }
    public Vector2 ApplyCellSize
    {
        get => applyCellSize;
        set
        {
            if (applyCellSize != value)
            {
                applyCellSize = value;
                if (overrideCellSize) ForEach(RefreshCellSize);
            }
        }
    }
    public bool Clickable
    {
        get => clickable || selectable;
        set
        {
            if (clickable != value)
            {
                clickable = value;
                ForEach(RefreshClickable);
            }
        }
    }
    public bool Selectable
    {
        get => selectable;
        set
        {
            if (selectable != value)
            {
                selectable = value;
                ForEach(RefreshClickable);
            }
        }
    }
    public bool MultiSelection
    {
        get => multiSelection;
        set
        {
            if (multiSelection != value)
            {
                multiSelection = value;
                if (!value && selectedIndexes.Count > 1)
                {
                    foreach (var index in selectedIndexes)
                    {
                        items[index].IsSelected = false;
                    }
                    selectedIndexes.Clear();
                }
            }
        }
    }

    [SerializeField]
    protected TItem prefab;
    public TItem Prefab => prefab;
    public virtual TItem this[int index]
    {
        get
        {
            if (index >= 0 && index < items.Count) return items[index];
            else return null;
        }
    }
    protected List<TItem> items = new List<TItem>();
    private ReadOnlyCollection<TItem> readOnlyItems;
    public ReadOnlyCollection<TItem> Items
    {
        get
        {
            if (readOnlyItems == null) readOnlyItems = items.AsReadOnly();
            return readOnlyItems;
        }
    }
    protected IList<TData> datas;
    public ReadOnlyCollection<TData> Datas => new ReadOnlyCollection<TData>(datas);
    public int Count => items.Count;

    protected SimplePool<TItem> cache;
    protected Action<TItem> itemModifier;
    protected Action<TItem> selectCallback;
    protected Action<TItem> clickCallback;

    #region 刷新相关
    /// <summary>
    /// 设置<typeparamref name="TItem"/>的额外修改方法
    /// </summary>
    /// <param name="itemModifier">修改方法</param>
    /// <param name="setImmediate">是否立即生效</param>
    public virtual void SetItemModifier(Action<TItem> itemModifier, bool setImmediate = false)
    {
        this.itemModifier = itemModifier;
        if (setImmediate) ForEach(this.itemModifier);
    }
    /// <summary>
    /// 设置新的<see cref="Datas"/>并刷新
    /// </summary>
    /// <param name="datas"></param>
    public void Refresh(IEnumerable<TData> datas)
    {
        if (datas == null) this.datas = new List<TData>();
        else this.datas = new List<TData>(datas);
        Refresh();
    }
    /// <summary>
    /// 设置新的<see cref="Datas"/>并刷新
    /// </summary>
    /// <param name="datas"></param>
    public void Refresh(IList<TData> datas)
    {
        if (datas == null) this.datas = new List<TData>();
        else this.datas = datas;
        Refresh();
    }
    /// <summary>
    /// 根据已有<see cref="Datas"/>刷新
    /// </summary>
    public virtual void Refresh()
    {
        if (datas == null) datas = new List<TData>();
        while (items.Count < datas.Count)
        {
            CreateItem();
        }
        while (items.Count > datas.Count)
        {
            RemoveItem(items[^1]);
        }
        for (int i = 0; i < datas.Count; i++)
        {
            ModifyItem(items[i], i);
            items[i].Refresh(datas[i]);
        }
        layoutGroup.enabled = false;
        layoutGroup.enabled = true;
    }

    /// <summary>
    /// 指定数据位置的元素根据已有的<see cref="ListItem{TSelf, TData}.Data"/>刷新
    /// </summary>
    /// <param name="index">数据下标，从0开始</param>
    public void RefreshItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        items[index].Refresh();
    }
    /// <summary>
    /// 为指定数据位置的元素设置新<see cref="ListItem{TSelf, TData}.Data"/>并刷新，这个方法不会修改<see cref="Datas"/>，所以在调用<see cref="Refresh"/>后会还原
    /// </summary>
    /// <param name="index">数据下标，从0开始</param>
    /// <param name="data">新数据</param>
    public void RefreshItem(int index, TData data)
    {
        if (index < 0 || index >= items.Count) return;
        items[index].Refresh(data);
    }
    /// <summary>
    /// 对所有满足条件的元素根据已有的<see cref="ListItem{TSelf, TData}.Data"/>刷新
    /// </summary>
    /// <param name="predicate">条件</param>
    public void RefreshItemIf(Predicate<TItem> predicate)
    {
        if (predicate == null) return;
        for (int i = 0; i < items.Count; i++)
        {
            if (predicate(items[i]))
                items[i].Refresh(datas[i]);
        }
    }

    #region 内部刷新
    protected virtual void RefreshLayoutGroup()
    {
        if (!container || ZetanUtility.IsPrefab(container.gameObject))
        {
            container = this.GetRectTransform();
            switch (direction)
            {
                case LayoutDirection.Horizontal:
                    container.pivot = Vector2.right;
                    break;
                case LayoutDirection.Vertical:
                    container.pivot = Vector2.up;
                    break;
            }
        }
        HorizontalOrVerticalLayoutGroup layoutGroup = null;
        switch (direction)
        {
            case LayoutDirection.Horizontal:
                if (container.GetComponent<LayoutGroup>() is LayoutGroup layout1 && layout1 is not HorizontalLayoutGroup) DestroyImmediate(layout1);
                layoutGroup = container.GetOrAddComponent<HorizontalLayoutGroup>();
                break;
            case LayoutDirection.Vertical:
                if (container.GetComponent<LayoutGroup>() is LayoutGroup layout2 && layout2 is not VerticalLayoutGroup) DestroyImmediate(layout2);
                layoutGroup = container.GetOrAddComponent<VerticalLayoutGroup>();
                break;
        }
        layoutGroup.padding = padding;
        layoutGroup.childAlignment = childAlignment;
        layoutGroup.childForceExpandHeight = direction == LayoutDirection.Horizontal;
        layoutGroup.childForceExpandWidth = direction == LayoutDirection.Vertical;
        this.layoutGroup = layoutGroup;
        RefreshSpacing();
        RefreshOverrideCellSize();
    }
    protected virtual void RefreshOverrideCellSize()
    {
        var layoutGroup = this.layoutGroup as HorizontalOrVerticalLayoutGroup;
        layoutGroup.childControlWidth = FillWidth;
        layoutGroup.childControlHeight = FillHeight;
        applyCellSize = cellSize;
        ForEach(RefreshCellSize);
    }
    protected virtual void RefreshCellSize(TItem item)
    {
        if (overrideCellSize)
        {
            var rectTransfrom = item.GetRectTransform();
            if (!fillWidth)
                rectTransfrom.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, applyCellSize.x);
            if (!fillHeight)
                rectTransfrom.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, applyCellSize.y);
        }
    }
    protected virtual void RefreshSpacing()
    {
        if (this.layoutGroup is HorizontalOrVerticalLayoutGroup layoutGroup)
            layoutGroup.spacing = direction == LayoutDirection.Horizontal ? spacing.x : spacing.y;
    }
    protected void RefreshClickable(TItem item)
    {
        var graphic = item.GetComponent<Graphic>();
        var clicker = item.GetComponent<Clickable>();
        if (Clickable)
        {
            if (!graphic) graphic = item.gameObject.AddComponent<EmptyGraphic>();
            graphic.raycastTarget = true;
            if (!clicker) clicker = item.gameObject.AddComponent<Clickable>();
            clicker.isEnabled = true;
            clicker.onClick.RemoveAllListeners();
            clicker.onClick.AddListener(() => { OnItemClick(item); });
        }
        else
        {
            if (graphic) graphic.raycastTarget = false;
            if (clicker) clicker.isEnabled = false;
        }
    }
    #endregion
    #endregion

    #region 其它
    public bool ContainsItem(TItem item)
    {
        return items.Contains(item);
    }
    public int FindIndex(Predicate<TItem> predicate)
    {
        if (predicate == null) return -1;
        return items.FindIndex(predicate);
    }
    public int FindPosition(Predicate<TItem> predicate)
    {
        return FindIndex(predicate) + 1;
    }
    protected virtual void InitItem(TItem item, int index)
    {
        item.Init(this, index);
    }
    protected void ModifyItem(TItem item, int index)
    {
        InitItem(item, index);
        RefreshCellSize(item);
        RefreshClickable(item);
        itemModifier?.Invoke(item);
    }
    protected void CreateItem()
    {
        items.Add(cache.Get(container));
    }
    protected void RemoveItem(TItem item)
    {
        items.Remove(item);
        item.OnClear();
        cache.Put(item);
    }
    /// <summary>
    /// 从现状列表中移除所有满足条件的元素，这个方法不会修改<see cref="Datas"/>，所以在调用<see cref="Refresh"/>后会还原
    /// </summary>
    /// <param name="predicate">条件</param>
    public void RemoveItemIf(Predicate<TItem> predicate)
    {
        if (predicate == null) return;
        TItem item;
        for (int i = 0; i < items.Count; i++)
        {
            item = items[i];
            if (predicate(item))
            {
                items.RemoveAt(i);
                item.OnClear();
                cache.Put(item);
            }
        }
    }

    public void ForEach(Action<TItem> action)
    {
        if (action == null) return;
        items.ForEach(action);
    }
    /// <summary>
    /// 带中断的遍历
    /// </summary>
    /// <param name="action">返回值表示是否中断的访问器</param>
    public void ForEachWithBreak(Predicate<TItem> action)
    {
        foreach (var item in items)
        {
            if (action(item))
                break;
        }
    }

    public virtual void Clear()
    {
        Refresh(null);
    }
    #endregion

    #region 选中相关
    public void SetClickCallback(Action<TItem> callback)
    {
        clickCallback = callback;
    }
    public void SetSelectCallback(Action<TItem> callback)
    {
        selectCallback = callback;
    }
    /// <summary>
    /// 选中从1数起的指定位置元素。仅在<see cref="ListView{TItem, TData}.Selectable"/> = true时生效
    /// </summary>
    /// <param name="index">元素位置，从1开始</param>
    /// <param name="selected">是否选中</param>
    public virtual void SetSelected(int index, bool selected = true)
    {
        if (!selectable || index < 1 || index > items.Count) return;
        items[index - 1].IsSelected = selected;
        OnItemSelectedChanged(items[index - 1]);
        selectCallback?.Invoke(items[index - 1]);
    }
    public void DeselectAll()
    {
        ForEach(item =>
        {
            item.IsSelected = false;
            OnItemSelectedChanged(item);
            selectCallback?.Invoke(item);
        });
    }
    /// <summary>
    /// 选中符合给定条件的元素，并取消选中不符合条件的；在单选状态下，只会选中第一个符合条件的。仅在<see cref="ListView{TItem, TData}.Selectable"/> = true时生效
    /// </summary>
    /// <param name="predicate">条件</param>
    public virtual void SelectIf(Predicate<TData> predicate)
    {
        if (!selectable) return;
        if (predicate == null) predicate = (i) => false;
        bool find = false;
        ForEach(item =>
        {
            if (multiSelection) item.IsSelected = predicate(item.Data);
            else if (!find && predicate(item.Data))
            {
                find = true;
                item.IsSelected = true;
            }
            else item.IsSelected = false;
            OnItemSelectedChanged(item);
            selectCallback?.Invoke(item);
        });
    }
    protected void OnItemClick(TItem item)
    {
        if (Clickable)
        {
            clickCallback?.Invoke(item);
            if (selectable)
            {
                item.IsSelected = !item.IsSelected;
                OnItemSelectedChanged(item);
                selectCallback?.Invoke(item);
            }
        }
    }
    protected void OnItemSelectedChanged(TItem item)
    {
        if (item.IsSelected)
            if (!multiSelection)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i != item.Index) items[i].IsSelected = false;
                }
                selectedIndexes.Clear();
                selectedIndexes.Add(item.Index);
            }
            else
            {
                if (!selectedIndexes.Contains(item.Index)) selectedIndexes.Add(item.Index);
                var map = selectedIndexes.ToHashSet();
                for (int i = 0; i < items.Count; i++)
                {
                    if (!map.Contains(i)) items[i].IsSelected = false;
                }
            }
        else selectedIndexes.Remove(item.Index);
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        RefreshLayoutGroup();
        var poolRoot = container.FindOrCreate("Caches");
        poolRoot.SetAsFirstSibling();
        ZetanUtility.SetActive(poolRoot, false);
        cache = new SimplePool<TItem>(prefab, cacheCapacity, poolRoot);
        OnAwake();
    }
    /// <summary>
    /// <see cref="Awake"/>时调用，默认为空
    /// </summary>
    protected virtual void OnAwake() { }
    #endregion
}

[DefaultExecutionOrder(-2), RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
public abstract class ListViewBase : MonoBehaviour
{
    [SerializeField]
    protected LayoutDirection direction = LayoutDirection.Vertical;
    [SerializeField]
    protected TextAnchor childAlignment;
    [SerializeField]
    protected RectOffset padding;
    [SerializeField]
    protected Vector2 spacing = new Vector2(2, 2);
    [SerializeField]
    protected bool overrideCellSize;
    [SerializeField]
    protected bool fillWidth;
    [SerializeField]
    protected bool fillHeight;
    [SerializeField]
    protected Vector2 cellSize = new Vector2(100, 100);
    [SerializeField]
    protected Vector2 applyCellSize;


    [SerializeField]
    protected bool clickable;
    [SerializeField]
    protected bool selectable;
    [SerializeField]
    protected bool multiSelection;

    protected LayoutGroup layoutGroup;

    [SerializeField, Min(10)]
    protected int cacheCapacity = 200;
    [SerializeField]
    protected RectTransform container;
    public RectTransform Container => container;

    protected readonly List<int> selectedIndexes = new List<int>();
    public ReadOnlyCollection<int> SelectedIndexes => selectedIndexes.AsReadOnly();
}

public enum LayoutDirection
{
    Horizontal,
    Vertical,
}