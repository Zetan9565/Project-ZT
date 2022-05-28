using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Extension;

public abstract class GridView<TItem, TData> : ListView<TItem, TData>, IGridView where TItem : GridItem<TItem, TData>
{
    [SerializeField, HideWhenPlaying]
    protected bool flexible;
    public bool Flexible
    {
        get => flexible;
        set
        {
            if (flexible != value)
            {
                flexible = value;
                var layoutGroup = this.layoutGroup as GridLayoutGroup;
                if (Flexible)
                    layoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                else switch (direction)
                    {
                        case LayoutDirection.Horizontal:
                            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                            break;
                        case LayoutDirection.Vertical:
                            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                            break;
                    }
            }
        }
    }

    [SerializeField, HideWhenPlaying, HideIf("flexible", true), Min(1)]
    protected int constraintCount = 5;
    public int ConstraintCount
    {
        get => constraintCount;
        set
        {
            if (!flexible)
            {
                if (constraintCount != value)
                    (layoutGroup as GridLayoutGroup).constraintCount = constraintCount = value;
            }
            else Debug.LogWarning("尝试修改自适应网格布局的约束计数，已阻止");
        }
    }
    public virtual TItem this[int row, int col]
    {
        get
        {
            int index;
            if (direction == LayoutDirection.Vertical)
                index = row * constraintCount + col;
            else index = col * constraintCount + row;
            return this[index];
        }
    }

    #region 内部刷新
    protected override void RefreshLayoutGroup()
    {
        if (!container) container = GetComponent<RectTransform>();
        if (container.GetComponent<LayoutGroup>() is LayoutGroup layout && layout is not GridLayoutGroup) DestroyImmediate(layout);
        GridLayoutGroup layoutGroup = container.GetOrAddComponent<GridLayoutGroup>();
        layoutGroup.padding = padding;
        layoutGroup.childAlignment = childAlignment;
        layoutGroup.cellSize = applyCellSize = cellSize;
        layoutGroup.spacing = spacing;
        if (Flexible) layoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
        else switch (direction)
            {
                case LayoutDirection.Horizontal:
                    layoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    break;
                case LayoutDirection.Vertical:
                    layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    break;
            }
        if (Flexible)
        {
            if (direction == LayoutDirection.Vertical)
                constraintCount = Mathf.FloorToInt((layoutGroup.GetRectTransform().rect.size.x - padding.horizontal + spacing.x) / (cellSize.x + spacing.x));
            else constraintCount = Mathf.FloorToInt(layoutGroup.GetRectTransform().rect.size.y - padding.vertical + spacing.y / (cellSize.y + spacing.y));
        }
        layoutGroup.constraintCount = constraintCount;
        this.layoutGroup = layoutGroup;
    }
    protected override void RefreshOverrideCellSize()
    {
        applyCellSize = cellSize;
        (layoutGroup as GridLayoutGroup).cellSize = applyCellSize;
    }
    protected override void RefreshCellSize(TItem item) { }
    protected override void RefreshSpacing()
    {
        if (this.layoutGroup is GridLayoutGroup layoutGroup)
            layoutGroup.spacing = spacing;
    }
    #endregion

    protected override void InitItem(TItem item, int index)
    {
        int childIndex = index;
        item.Init(this, childIndex / ConstraintCount, childIndex & ConstraintCount);
    }

    public void Refresh(int row, int col, TData data)
    {
        if (row * constraintCount + col < items.Count)
            this[row, col].Refresh(data);
    }

    private void OnValidate()
    {
        overrideCellSize = true;
    }
}
public interface IGridView
{
    public bool Flexible { get; }
    public int ConstraintCount { get; }
}