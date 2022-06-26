using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "equip item", menuName = "Zetan Studio/道具/用途/装备")]
    public class Equip : ItemUsage
    {
        public Equip()
        {
            _name = "装备";
        }

        protected override ItemHandler Instance => Instantiate(this);

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<EquipableModule>() is null) return false;
            return EquipmentManager.Equip(item, out _);
        }

        protected override bool Prepare(ItemData item, int cost)
        {
            return BackpackManager.Instance.ContainsItem(item);
        }
        protected override bool Complete(ItemData item, int cost)
        {
            return true;
        }
    }
}