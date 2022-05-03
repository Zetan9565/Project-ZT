using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("出售")]
    public class SellableModule : ItemModule
    {
        [field: SerializeField, Min(1)]
        public int Price { get; protected set; }

        public override bool IsValid => Price > 0;
    }
}