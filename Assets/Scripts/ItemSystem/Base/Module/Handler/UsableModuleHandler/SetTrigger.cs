using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "set trigger", menuName = "Zetan Studio/道具/用途/设置触发器")]
    public sealed class SetTrigger : ItemUsage
    {
        public SetTrigger()
        {
            _name = "设置触发器";
        }

        protected override bool Use(ItemData item)
        {
            if (!item.TryGetModule<TriggerModule>(out var module)) return false;
            TriggerManager.SetTrigger(module.Name, module.State);
            return true;
        }
    }
}
