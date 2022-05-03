using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("冷却"), Require(typeof(UsableModule))]
    public class CoolDownModule : ItemModule
    {
        [field: SerializeField, Min(0)]
        public float Time { get; protected set; }

        public override bool IsValid => Time > 0;
    }
}