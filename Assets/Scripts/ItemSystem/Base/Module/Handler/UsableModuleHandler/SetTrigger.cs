﻿using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [CreateAssetMenu(fileName = "set trigger", menuName = "Zetan Studio/道具/用途/设置触发器")]
    public sealed class SetTrigger : ItemUsage
    {
        public override string Name => "设置触发器";

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<TriggerModule>() is not TriggerModule module) return false;
            TriggerManager.Instance.SetTrigger(module.Name, module.State);
            return true;
        }
    }
}
