using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("宝石镶嵌"), Require(typeof(EquipableModule))]
    public class GemSlotModule : ItemModule
    {
        [field: SerializeField, Min(1)]
        public int SlotCount { get; protected set; } = 1;

        public override bool IsValid => SlotCount > 0;
    }
}