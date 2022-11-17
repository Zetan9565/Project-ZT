using ZetanStudio.InventorySystem;

namespace ZetanStudio.ItemSystem.Module
{
    public abstract class ItemUsage : ItemHandler
    {
        protected sealed override bool DoHandle(ItemData item)
        {
            if (!item.TryGetModule<UsableModule>(out var usable)) return false;
            UsableData data = item.GetModuleData<UsableData>();
            string msg = data.CanUseWithMsg(item) ?? null;
            if (!string.IsNullOrEmpty(msg))
            {
                MessageManager.Instance.New(msg);
                return false;
            }
            if (Prepare(item, usable.Cost) && Use(item))
            {
                bool result = data.CanUse(item) && Complete(item, usable.Cost);
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