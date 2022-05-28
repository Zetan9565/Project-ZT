using UnityEngine;

namespace ZetanStudio.Item.Module
{
    public class CoolDownData : ItemModuleData<CoolDownModule>
    {
        public float Time => Module.Time - Module.Cooler.GetTime(Item);

        public float NormalizeTime => Module.Cooler.GetTime(Item) / Module.Time;

        public bool Available => Time <= 0;

        public CoolDownData(ItemData item, CoolDownModule module) : base(item, module)
        {
        }
    }
}