using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "unique cool down", menuName = "Zetan Studio/道具/冷却/独立冷却")]
    public sealed class UniqueCoolDown : ItemCooler
    {
        public UniqueCoolDown()
        {
            _name = "独立冷却";
        }

        private readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

        protected override float DoGetTime(ItemData item)
        {
            if (timers.TryGetValue(ID(item), out var timer)) return timer.Time;
            return item.GetModule<CoolDownModule>().Time;
        }
        protected override void DoSetTime(ItemData item, float time)
        {
            if (!item.TryGetModule<CoolDownModule>(out var cool)) return;
            if (timers.TryGetValue(ID(item), out var timer)) timer.Stop();
            timer = Timer.Create(default(System.Action), cool.Time, true);
            timer.Restart(time);
            timers[ID(item)] = timer;
        }

        private static string ID(ItemData item)
        {
            return item.StackAble ? item.ModelID : item.ID;
        }

        protected override bool DoStartCoolDown(ItemData item)
        {
            if (!item.TryGetModule<CoolDownModule>(out var cool)) return false;
            var timer = Timer.Create(default(System.Action), cool.Time, true);
            timers[ID(item)] = timer;
            return timer != null;
        }

        protected override bool DoHasCooled(ItemData item)
        {
            if (timers.TryGetValue(ID(item), out var timer)) return timer.IsStop;
            else return true;
        }
    }
}