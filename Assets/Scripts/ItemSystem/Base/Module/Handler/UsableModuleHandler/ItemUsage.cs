namespace ZetanStudio.Item.Module
{
    public abstract class ItemUsage : ItemHandler
    {
        protected sealed override bool DoHandle(ItemData item)
        {
            if (item.GetModule<UsableModule>() is not UsableModule usable) return false;
            if (item.GetModuleData<CoolDownData>() is CoolDownData data)
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
                if (item.GetModule<CoolDownModule>() is CoolDownModule cool)
                    result = cool.Cooler.Handle(item) && Complete(item, usable.Cost);
                else result = Complete(item, usable.Cost);
                if (result) NotifyCenter.PostNotify(BackpackManager.BackpackUseItem, item);
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
            return BackpackManager.Instance.LoseItem(item, cost);
        }
    }
}