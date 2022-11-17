using System;
using UnityEngine;
using ZetanStudio.UI;

namespace ZetanStudio.ItemSystem.UI
{
    [RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
    public class ItemGrid : GridView<ItemSlot, ItemSlotData>, ISlotContainer, IFiltableItemContainer
    {
        private Predicate<ItemSlot> darkCondition;
        private Predicate<ItemSlot> markCondition;
        private Predicate<Item> filter;

        public void DarkIf(Predicate<ItemSlot> condition)
        {
            darkCondition = condition;
            ForEach(x => x.SetDarkCondition(condition, true));
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
    public interface IFiltableItemContainer
    {
        public void DoFilter(Predicate<Item> filter);
    }
}