using System;

public class ScrollItemGrid : ScrollGridView<ItemSlotBase, ItemSlotData>, ISlotContainer
{
    protected override void InitItem(ItemSlotBase item, int index)
    {
        base.InitItem(item, index);
        if (item is ItemSlot s) s.SetScrollRect(ScrollRect);
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