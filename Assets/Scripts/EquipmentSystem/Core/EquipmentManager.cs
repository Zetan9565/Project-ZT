using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio
{
    using Character;
    using ItemSystem;
    using ItemSystem.Module;

    public static class EquipmentManager
    {
        public static List<RoleAttribute> attributes = new List<RoleAttribute>();
        public static List<RoleProperty> properties = new List<RoleProperty>();

        public static Dictionary<EquipmentType, ItemData> equiped = new Dictionary<EquipmentType, ItemData>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            foreach (var e in RoleAttributeEnum.Instance.Enum)
            {
                var a = new RoleAttribute(e);
                switch (a.ValueType)
                {
                    case RoleValueType.Integer:
                        a.IntValue = 100;
                        break;
                    case RoleValueType.Float:
                        a.FloatValue = 1f;
                        break;
                    case RoleValueType.Boolean:
                        a.BoolValue = true;
                        break;
                    default:
                        break;
                }
                attributes.Add(a);
            }
            foreach (var e in RolePropertyEnum.Instance.Enum)
            {
                var p = new RoleProperty(e);
                p.AddOperandSet(attributes);
                properties.Add(p);
            }
            //properties.Sort(RoleProperty.Comparer.Default);
        }

        public static bool Equip(ItemData item, out ItemData equiped)
        {
            equiped = null;
            if (!item) return false;
            if (!item.TryGetModule<EquipableModule>(out var equipable) || !item.TryGetModuleData<AttributeData>(out var data)) return false;
            EquipmentManager.equiped.TryGetValue(equipable.Type, out equiped);
            Unequip(equiped, false);
            EquipmentManager.equiped[equipable.Type] = item;
            IEnumerable<ItemProperty> affixes = item.GetModuleData<AffixData>()?.affixes;
            foreach (var prop in properties)
            {
                prop.AddOperandSet(data.properties);
                prop.AddOperandSet(affixes);
            }
            properties.ForEach(x => x.CalculateValue());
            BackpackManager.Instance.Inventory.SwapHiddenItem(equiped, item);
            return true;
        }
        public static bool Unequip(ItemData item)
        {
            if (!item || !item.TryGetModule<EquipableModule>(out var equipable)) return false;
            if (!EquipmentManager.equiped.TryGetValue(equipable.Type, out var equiped)) return false;
            return Unequip(equiped, true);
        }
        private static bool Unequip(ItemData item, bool unhide)
        {
            if (!item || !item.TryGetModule<EquipableModule>(out var equipable)) return false;
            if (!EquipmentManager.equiped.TryGetValue(equipable.Type, out var equiped)) return false;
            if (unhide)
            {
                if (BackpackManager.Instance.Inventory.CanUnhide()) BackpackManager.Instance.Inventory.UnhideItem(equiped);
                else
                {
                    MessageManager.Instance.New("空间不足");
                    return false;
                }
            }
            if (!item.TryGetModuleData<AttributeData>(out var data)) return false;
            IEnumerable<ItemProperty> affixes = item.GetModuleData<AffixData>()?.affixes;
            foreach (var prop in properties)
            {
                prop.RemoveOperandSet(data.properties);
                prop.RemoveOperandSet(affixes);
            }
            properties.ForEach(x => x.CalculateValue());
            EquipmentManager.equiped.Remove(equipable.Type);
            return true;
        }
    }
}