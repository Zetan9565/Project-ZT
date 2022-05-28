using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("冷却"), Require(typeof(UsableModule))]
    public sealed class CoolDownModule : ItemModule
    {
        [field: SerializeField, Min(0.01f)]
        public float Time { get; private set; } = 1;

        [field: SerializeField]
        public string Message { get; private set; } = "冷却中";

        [field: SerializeField, Label("冷却器")]
        public ItemCooler Cooler { get; private set; }

        public override bool IsValid => Time > 0 && Cooler;

        public override ItemModuleData CreateData(ItemData item)
        {
            return new CoolDownData(item, this);
        }
    }
}