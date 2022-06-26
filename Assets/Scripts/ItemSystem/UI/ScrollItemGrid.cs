using System;
using ZetanStudio.ItemSystem;

public class ScrollItemGrid : ScrollGridView<ItemSlot, ItemSlotData>, ISlotContainer, IFiltableItemContainer
{
    private Predicate<ItemSlot> darkCondition;
    private Predicate<ItemSlot> markCondition;
    private Predicate<Item> filter;

    protected override void InitItem(ItemSlot item, int index)
    {
        base.InitItem(item, index);
        if (item is ItemSlotEx s) s.SetScrollRect(ScrollRect);
    }

    public void DoFilter(Predicate<Item> filter)
    {
        this.filter = filter;
        AddItemFilter(DoFilter, true);
    }
    private bool DoFilter(ItemSlot i)
    {
        return i.IsEmpty || (filter?.Invoke(i.Data?.Model) ?? true);
    }
    public void DarkIf(Predicate<ItemSlot> condition)
    {
        darkCondition = condition;
        ForEach(x => x.SetDarkCondition(condition, true));
    }
    public void MarkIf(Predicate<ItemSlot> condition)
    {
        markCondition = condition;
        ForEach(x => x.SetMarkCondition(condition, true));
    }

    protected override void OnModifyItem(ItemSlot item)
    {
        item.SetDarkCondition(darkCondition);
        item.SetMarkCondition(markCondition);
    }
}