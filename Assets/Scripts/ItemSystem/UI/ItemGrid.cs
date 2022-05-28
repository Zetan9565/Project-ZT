using System;
using UnityEngine;
using ZetanStudio.Item;

[RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
public class ItemGrid : GridView<ItemSlotBase, ItemSlotData>, ISlotContainer, IFiltableItemContainer
{
    public void DarkIf(Predicate<ItemSlotBase> condition)
    {
        ForEach(x => x.SetDarkCondition(condition, true));
    }

    public void DoFilter(Predicate<Item> filter)
    {
        AddItemFilter(i => i.IsEmpty || (filter?.Invoke(i.Data?.Model) ?? true), true);
    }

    public void MarkIf(Predicate<ItemSlotBase> condition)
    {
        ForEach(x => x.SetMarkCondition(condition, true));
    }
}
public interface IFiltableItemContainer
{
    public void DoFilter(Predicate<Item> filter);
}