namespace ZetanStudio.ItemSystem.Module
{
    public abstract class ItemCooler : ItemHandler
    {
        private static ItemCooler instance;
        protected override ItemHandler Instance
        {
            get
            {
                if (!instance) instance = Instantiate(this);
                return instance;
            }
        }

        protected sealed override bool DoHandle(ItemData item)
        {
            return item.GetModule<CoolDownModule>() && StartCoolDown(item);
        }

        private bool StartCoolDown(ItemData item)
        {
            return (Instance as ItemCooler).DoStartCoolDown(item);
        }
        protected abstract bool DoStartCoolDown(ItemData item);

        public float GetTime(ItemData item)
        {
            return (Instance as ItemCooler).DoGetTime(item);
        }
        protected abstract float DoGetTime(ItemData item);

        public void SetTime(ItemData item, float time)
        {
            (Instance as ItemCooler).DoSetTime(item, time);
        }
        protected abstract void DoSetTime(ItemData item, float time);

        public bool HasCooled(ItemData item)
        {
            return (Instance as ItemCooler).DoHasCooled(item);
        }
        protected abstract bool DoHasCooled(ItemData item);
    }
}