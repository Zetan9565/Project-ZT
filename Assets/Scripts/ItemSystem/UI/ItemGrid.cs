using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
public class ItemGrid : GridView<ItemSlotBase, ItemSlotData>, ISlotContainer
{
    public void DarkIf(Predicate<ItemSlotBase> condition)
    {
        ForEach(x => x.SetDarkCondition(condition, true));
    }
    public void MarkIf(Predicate<ItemSlotBase> condition)
    {
        ForEach(x => x.SetMarkCondition(condition, true));
    }
}