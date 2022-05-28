using System;
using ZetanStudio.Item;

public class ScrollItemGrid : ScrollGridView<ItemSlotBase, ItemSlotData>, ISlotContainer, IFiltableItemContainer
{
    protected override void InitItem(ItemSlotBase item, int index)
    {
        base.InitItem(item, index);
        if (item is ItemSlot s) s.SetScrollRect(ScrollRect);
    }

    public void DoFilter(Predicate<Item> filter)
    {
        AddItemFilter(i => i.IsEmpty || (filter?.Invoke(i.Data?.Model) ?? true), true);
    }

    public void DarkIf(Predicate<ItemSlotBase> condition)
    {
        ForEach(x => x.SetDarkCondition(condition, true));
    }
    public void MarkIf(Predicate<ItemSlotBase> condition)
    {
        ForEach(x => x.SetMarkCondition(condition, true));
    }
}