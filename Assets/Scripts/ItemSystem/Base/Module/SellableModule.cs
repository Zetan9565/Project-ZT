using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("出售")]
    public class SellableModule : ItemModule
    {
        [field: SerializeField, Label("价格"), Min(1)]
        public int Price { get; protected set; } = 1;

        public override bool IsValid => Price > 0;
    }
}