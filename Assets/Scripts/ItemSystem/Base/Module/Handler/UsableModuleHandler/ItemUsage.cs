using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    public abstract class ItemUsage : ItemHandler
    {
        public abstract string Name { get; }

        protected sealed override bool DoHandle(ItemData item)
        {
            if (item.GetModule<UsableModule>() is not UsableModule usable) return false;
            if (BackpackManager.Instance.CanLose(item, usable.Cost) && Use(item))
                return BackpackManager.Instance.LoseItem(item, usable.Cost);
            return false;
        }

        protected abstract bool Use(ItemData item);
    }
}