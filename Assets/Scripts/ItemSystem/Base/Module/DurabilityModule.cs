using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("耐久度")]
    public class DurabilityModule : ItemModule
    {
        [field: SerializeField, Min(0)]
        public int Durability { get; protected set; }

        public override bool IsValid => Durability > 0;
    }
}