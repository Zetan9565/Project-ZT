using System;
using ZetanStudio.Item;

public class CraftList : ScrollListView<CraftAgent, Item>, IFiltableItemContainer
{
    public void DoFilter(Predicate<Item> filter)
    {
        AddItemFilter(i => filter?.Invoke(i.Data) ?? true);
    }
}