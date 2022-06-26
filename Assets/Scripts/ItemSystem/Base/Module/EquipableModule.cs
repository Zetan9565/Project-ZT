using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("装备"), Require(typeof(AttributeModule), typeof(UsableModule))]
    public class EquipableModule : ItemModule, IItemWindowModifier
    {
        [SerializeField, Enum(typeof(EquipmentType))]
        private int type;
        public EquipmentType Type => EquipmentTypeEnum.Instance[type];

        public override bool IsValid => type >= 0;

        public void ModifyItemWindow(ItemInfoDisplayer displayer)
        {
            if (!displayer.Item.TryGetModule<EquipableModule>(out var equipable)) return;
            if (!EquipmentManager.equiped.ContainsValue(displayer.Item) && EquipmentManager.equiped.TryGetValue(equipable.Type, out var equiped))
                displayer.Window.SetContrast(equiped);
        }

        public static bool SameType(ItemData item1, ItemData item2)
        {
            if (!item1 || !item2) return false;
            if (!item1.TryGetModule<EquipableModule>(out var equipable1) || !item2.TryGetModule<EquipableModule>(out var equipable2)) return false;
            return equipable1.type == equipable2.type;
        }
        public static bool SameType(EquipmentType type, ItemData item)
        {
            if (!item) return false;
            if (!item.TryGetModule<EquipableModule>(out var equipable)) return false;
            return type == equipable.Type;
        }
    }
}