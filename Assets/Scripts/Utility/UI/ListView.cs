using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.UI
{
    using Collections;
    using Extension;
    using ZetanStudio;

    public sealed class ListView : ListView<ListItem, object> { }

    public abstract class ListView<TItem, TData> : ListViewBase, IForEach<TItem>, IForEachBreakable<TItem> where TItem : ListItem<TItem, TData>
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
            get => clickable;
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
            get => clickable && selectable;
            set
            {
                selectable = value;
                if (selectable && !clickable) Clickable = true;
                if (!selectable)
                    foreach (var item in selectedItems.Select(x => x))
                    {
                        RemoveFromSelection(item);
                    }
            }
        }
        public int SelectionLimit
        {
            get => selectionLimit;
            set
            {
                if (selectionLimit != value)
                {
                    selectionLimit = value;
                    if (value < 0) selectionLimit = 0;
                    else if (value == 1)
                    {
                        foreach (var index in selectedIndices)
                        {
                            items[index].IsSelected = false;
                        }
                        selectedIndices.Clear();
                        selectedItems.Clear();
                        selectedDatas.Clear();
                    }
                }
            }
        }
        public bool MultiSelection
        {
            get => Selectable && selectionLimit != 1;
            set
            {
                SelectionLimit = value ? 0 : 1;
                if (value && !Selectable) Selectable = true;
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
        public ReadOnlyCollection<TItem> Items => items.AsReadOnly();
        protected readonly HashSet<TItem> selectedItems = new HashSet<TItem>();
        public ReadOnlySet<TItem> SelectedItems => new ReadOnlySet<TItem>(selectedItems);

        protected IList<TData> datas;
        public ReadOnlyCollection<TData> Datas => new ReadOnlyCollection<TData>(datas);
        protected readonly HashSet<TData> selectedDatas = new HashSet<TData>();
        public ReadOnlySet<TData> SelectedDatas => new ReadOnlySet<TData>(selectedDatas);
        public int Count => items.Count;

        protected SimplePool<TItem> cache;
        protected HashSet<Predicate<TItem>> itemFilters = new HashSet<Predicate<TItem>>();
        protected Action<TItem> itemModifier;
        protected Action<TItem> selectCallback;
        protected Action<IEnumerable<TItem>> multiSelectCallback;
        protected Action<TItem> clickCallback;
        private RectTransform rectTransform;
        public sealed override RectTransform RectTransform => rectTransform;

        #region 刷新相关
        /// <summary>
        /// 设置<typeparamref name="TItem"/>的额外修改方法，每次<see cref="Refresh()"/>都会调用
        /// </summary>
        /// <param name="itemModifier">修改方法</param>
        /// <param name="setImmediate">是否立即生效</param>
        public void SetItemModifier(Action<TItem> itemModifier, bool setImmediate = false)
        {
            this.itemModifier = itemModifier;
            if (setImmediate) DoModifier();
        }
        public void AddItemFilter(Predicate<TItem> itemFilter, bool setImmediate = false)
        {
            if (itemFilter == null) return;
            itemFilters.Add(itemFilter);
            if (setImmediate) DoFilter();
        }
        public void RemoveItemFilter(Predicate<TItem> itemFilter, bool setImmediate = false)
        {
            itemFilters.Remove(itemFilter);
            if (setImmediate) DoFilter();
        }
        public void DoModifier()
        {
            ForEach(itemModifier);
        }
        public void DoFilter()
        {
            ForEach(x => Utility.SetActive(x, !itemFilters.Any(f => !f.Invoke(x))));
            Rebuild();
        }
        /// <summary>
        /// 设置新的<see cref="Datas"/>并刷新
        /// </summary>
        public void Refresh(IEnumerable<TData> datas)
        {
            if (datas == null) this.datas = new List<TData>();
            else this.datas = new List<TData>(datas);
            Refresh();
        }
        /// <summary>
        /// 设置新的<see cref="Datas"/>并刷新
        /// </summary>
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
            ClearSelectionWithoutNofity();
            for (int i = 0; i < datas.Count; i++)
            {
                TItem item = items[i];
                item.Clear();
                ModifyItem(item, i);
                item.Refresh(datas[i]);
                Utility.SetActive(item, !itemFilters.Any(f => !f.Invoke(item)));
            }
            Rebuild();
        }

        public void Rebuild()
        {
            if (!layoutGroup) return;
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
        /// 为指定数据位置的元素设置新<see cref="ListItem{TSelf, TData}.Data"/>并刷新，这个方法不会修改<see cref="Datas"/>，所以在调用<see cref="Refresh()"/>后会还原
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
            if (!container || Utility.IsPrefab(container.gameObject))
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
            var clicker = item.GetComponent<Clickable>();
            if (Clickable)
            {
                if (!clicker) clicker = item.gameObject.AddComponent<Clickable>();
                clicker.isEnabled = true;
                clicker.onClick.RemoveAllListeners();
                clicker.onClick.AddListener(() => { OnItemClick(item); });
            }
            else
            {
                if (clicker) clicker.isEnabled = false;
            }
        }
        #endregion
        #endregion

        #region 其它
        public bool Contains(TItem item)
        {
            return items.Contains(item);
        }
        public int FindIndex(Predicate<TItem> predicate)
        {
            if (predicate == null) return -1;
            return items.FindIndex(predicate);
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
            OnModifyItem(item);
        }
        protected virtual void OnModifyItem(TItem item) { }
        protected void CreateItem()
        {
            items.Add(cache.Get(container));
        }
        protected void RemoveItem(TItem item)
        {
            items.Remove(item);
            item.Clear();
            cache.Put(item);
        }
        /// <summary>
        /// 从现状列表中移除所有满足条件的元素，这个方法不会修改<see cref="Datas"/>，所以在调用<see cref="Refresh()"/>后会还原
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
                    item.Clear();
                    cache.Put(item);
                }
            }
        }

        public void ForEach(Action<TItem> action)
        {
            if (action == null) return;
            items.ForEach(action);
        }
        public void ForEach(Predicate<TItem> action)
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

        public virtual void PreplacePrefab(TItem prefab, bool rebuild = false)
        {
            this.prefab = prefab;
            CreateCache();
            if (rebuild) Refresh();
        }
        private void CreateCache()
        {
            var poolRoot = container.FindOrCreate("Caches");
            poolRoot.SetAsFirstSibling();
            Utility.SetActive(poolRoot, false);
            if (cache != null) cache.Clear();
            cache = new SimplePool<TItem>(prefab, poolRoot, cacheCapacity);
        }
        #endregion

        #region 选中相关
        public void SetClickCallback(Action<TItem> callback)
        {
            clickCallback = callback;
        }
        /// <summary>
        /// 设置单选回调
        /// </summary>
        public void SetSelectCallback(Action<TItem> callback)
        {
            selectCallback = callback;
        }
        /// <summary>
        /// 设置多选回调
        /// </summary>
        public void SetSelectCallback(Action<IEnumerable<TItem>> callback)
        {
            multiSelectCallback = callback;
        }
        /// <summary>
        /// 设置指定位置元素的选中状态。仅在<see cref="ListView{TItem, TData}.Selectable"/> = true且<see cref="ListView{TItem, TData}.MultiSelection"/> = false时生效<br/>
        /// </summary>
        /// <param name="index">元素下标</param>
        /// <param name="selected">是否选中</param>
        public override void SetSelected(int index, bool selected = true)
        {
            if (MultiSelection || index < 0 || index > items.Count - 1) return;
            if (selected) AddToSelection(items[index]);
            else RemoveFromSelection(items[index]);
        }
        /// <summary>
        /// 选中多个指定位置元素。仅在<see cref="ListView{TItem, TData}.Selectable"/> = true时生效<br/>
        /// </summary>
        /// <param name="selection">元素下标</param>
        public override void SetSelection(params int[] selection)
        {
            if (!Selectable) return;
            var results = new List<TItem>();
            var indices = new HashSet<int>(selection);
            for (int i = 0; i < items.Count; i++)
            {
                if (indices.Contains(i))
                {
                    results.Add(items[i]);
                    AddToSelection(items[i]);
                }
                else RemoveFromSelection(items[i]);
            }
            multiSelectCallback?.Invoke(results);
        }
        public void ClearSelection()
        {
            ClearSelectionWithoutNofity();
            if (!MultiSelection && selectedItems.Count < 1) selectCallback?.Invoke(null);
            multiSelectCallback?.Invoke(selectedItems);
        }
        public void ClearSelectionWithoutNofity()
        {
            foreach (var item in selectedItems)
            {
                item.IsSelected = false;
            }
            selectedIndices.Clear();
            selectedItems.Clear();
            selectedDatas.Clear();
        }
        /// <summary>
        /// 选中符合给定条件的元素，并取消选中不符合条件的；在单选状态下，只会选中第一个符合条件的。仅在<see cref="ListView{TItem, TData}.Selectable"/> = true时生效
        /// </summary>
        /// <param name="predicate">条件</param>
        public virtual void SelectIf(Predicate<TData> predicate)
        {
            if (!Selectable) return;
            if (predicate == null) predicate = (i) => false;
            bool find = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (MultiSelection) SetSelected(i + 1, predicate(items[i].Data));
                else if (!find && predicate(items[i].Data))
                {
                    find = true;
                    SetSelected(i + 1, true);
                }
                else SetSelected(i + 1, false);
            }
        }
        protected void OnItemClick(TItem item)
        {
            if (Clickable)
            {
                clickCallback?.Invoke(item);
                if (Selectable)
                {
                    if (item.IsSelected) RemoveFromSelection(item);
                    else AddToSelection(item);
                }
            }
        }
        protected void AddToSelection(TItem item)
        {
            if (MultiSelection && selectionLimit > 0 && selectedItems.Count >= SelectionLimit) return;
            else if (!MultiSelection)
            {
                var temp = new List<TItem>(selectedItems.Select(x => x));
                foreach (var si in temp)
                {
                    if (si != item)
                    {
                        si.IsSelected = false;
                        selectedIndices.Remove(si.Index);
                        selectedItems.Remove(si);
                        selectedDatas.Remove(si.Data);
                    }
                }
            }
            item.IsSelected = true;
            selectedIndices.Add(item.Index);
            selectedItems.Add(item);
            selectedDatas.Add(item.Data);
            if (!MultiSelection) selectCallback?.Invoke(item);
            multiSelectCallback?.Invoke(selectedItems);
        }
        protected void RemoveFromSelection(TItem item)
        {
            item.IsSelected = false;
            selectedIndices.Remove(item.Index);
            selectedItems.Remove(item);
            selectedDatas.Remove(item.Data);
            if (!MultiSelection && selectedItems.Count < 1) selectCallback?.Invoke(null);
            multiSelectCallback?.Invoke(selectedItems);
        }
        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            RefreshLayoutGroup();
            CreateCache();
            OnAwake();
        }
        /// <summary>
        /// <see cref="Awake"/>时调用，默认为空
        /// </summary>
        protected virtual void OnAwake() { }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            try
            {
                Vector3[] corners = new Vector3[4];
                GetComponent<RectTransform>().GetWorldCorners(corners);
                corners[0] = new Vector3(corners[0].x + padding.left, corners[0].y + padding.bottom);
                corners[1] = new Vector3(corners[1].x + padding.left, corners[1].y - padding.top);
                corners[2] = new Vector3(corners[2].x - padding.right, corners[2].y - padding.top);
                corners[3] = new Vector3(corners[3].x - padding.right, corners[3].y + padding.bottom);
                Gizmos.DrawLine(corners[0], corners[1]);
                Gizmos.DrawLine(corners[1], corners[2]);
                Gizmos.DrawLine(corners[2], corners[3]);
                Gizmos.DrawLine(corners[3], corners[0]);
            }
            catch { }
        }
#endif
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
        [SerializeField, Min(0)]
        protected int selectionLimit = 1;

        protected LayoutGroup layoutGroup;

        [SerializeField, Min(10)]
        protected int cacheCapacity = 200;
        [SerializeField]
        protected RectTransform container;
        public RectTransform Container => container;

        public int SelectedIndex
        {
            get => !clickable || !selectable || selectedIndices.Count < 1 ? -1 : selectedIndices.FirstOrDefault();
            set => SetSelected(value);
        }

        protected readonly HashSet<int> selectedIndices = new HashSet<int>();
        public ReadOnlySet<int> SelectedIndices => new ReadOnlySet<int>(selectedIndices);

        public abstract RectTransform RectTransform { get; }

        public abstract void SetSelected(int index, bool selected = true);
        /// <summary>
        /// 选中多个指定位置元素。仅在<see cref="ListView{TItem, TData}.Selectable"/> = true时生效<br/>
        /// </summary>
        /// <param name="selection">元素下标</param>
        public abstract void SetSelection(params int[] selection);
    }

    public enum LayoutDirection
    {
        Horizontal,
        Vertical,
    }
}