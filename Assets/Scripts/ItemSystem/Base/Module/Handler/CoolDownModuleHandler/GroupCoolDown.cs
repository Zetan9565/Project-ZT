using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "group cool down", menuName = "Zetan Studio/道具/冷却/成组冷却")]
    public sealed class GroupCoolDown : ItemCooler
    {
        public GroupCoolDown()
        {
            _name = "成组冷却";
        }

        [field: SerializeField]
        public float Time { get; private set; } = 5;

        private Timer timer;

        protected override float DoGetTime(ItemData item)
        {
            if (timer == null) return Time;
            else return timer.Time;
        }
        protected override void DoSetTime(ItemData item, float time)
        {
            if (timer == null) timer = Timer.Create(default(System.Action), Time, true);
            timer.Restart(time);
        }

        protected override bool DoStartCoolDown(ItemData item)
        {
            if (timer == null) timer = Timer.Create(default(System.Action), Time, true);
            else if (timer.IsStop) timer.Restart();
            return true;
        }

        protected override bool DoHasCooled(ItemData item)
        {
            return timer == null || timer.IsStop;
        }
    }
}