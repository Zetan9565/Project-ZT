namespace ZetanStudio.ItemSystem.Module
{
    public abstract class ItemUsage : ItemHandler
    {
        protected sealed override bool DoHandle(ItemData item)
        {
            if (!item.TryGetModule<UsableModule>(out var usable)) return false;
            if (item.TryGetModuleData<CoolDownData>(out var data))
            {
                if (!data.Available)
                {
                    MessageManager.Instance.New(data.Module.Message);
                    return false;
                }
            }
            if (Prepare(item, usable.Cost) && Use(item))
            {
                bool result;
                if (item.TryGetModule<CoolDownModule>(out var cool)) result = cool.Cooler.Handle(item) && Complete(item, usable.Cost);
                else result = Complete(item, usable.Cost);
                if (result) Nodify(item);
                return result;
            }
            return false;
        }

        protected abstract bool Use(ItemData item);

        protected virtual bool Prepare(ItemData item, int cost)
        {
            return BackpackManager.Instance.CanLose(item, cost);
        }
        protected virtual bool Complete(ItemData item, int cost)
        {
            return BackpackManager.Instance.Lose(item, cost);
        }
        protected virtual void Nodify(ItemData item)
        {
            NotifyCenter.PostNotify(BackpackManager.BackpackUseItem, item);
        }
        public static bool UseItem(ItemData item)
        {
            return item && item.TryGetModule<UsableModule>(out var usable) && usable.Usage.Handle(item);
        }
    }
}