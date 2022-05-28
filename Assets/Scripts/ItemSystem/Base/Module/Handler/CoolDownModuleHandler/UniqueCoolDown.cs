using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [CreateAssetMenu(fileName = "unique cool down", menuName = "Zetan Studio/道具/冷却/独立冷却")]
    public sealed class UniqueCoolDown : ItemCooler
    {
        public override string Name => "独立冷却";

        private Dictionary<ItemData, Timer> timers = new Dictionary<ItemData, Timer>();

        protected override float DoGetTime(ItemData item)
        {
            timers.TryGetValue(item, out var timer);
            return timer?.Time ?? item.GetModule<CoolDownModule>().Time;
        }

        protected override bool DoStartCoolDown(ItemData item)
        {
            if (item.GetModule<CoolDownModule>() is not CoolDownModule cool) return false;
            var timer = TimerManager.Instance.Create(default(System.Action), cool.Time);
            timers[item] = timer;
            return timer != null;
        }
    }
}