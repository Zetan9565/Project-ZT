using System.Collections;
using UnityEngine;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item.Module
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
            return item.GetModule<CoolDownModule>() is not null && StartCoolDown(item);
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
    }
}