using System;

namespace ZetanStudio.CraftSystem.UI
{
    using ItemSystem;
    using ItemSystem.UI;
    using ZetanStudio.UI;

    public class CraftList : ScrollListView<CraftAgent, Item>, IFiltableItemContainer
    {
        private Predicate<Item> filter;

        public void DoFilter(Predicate<Item> filter)
        {
            this.filter = filter;
            AddItemFilter(DoFilter, true);
        }
        private bool DoFilter(CraftAgent i)
        {
            return filter?.Invoke(i.Data) ?? true;
        }
    }
}