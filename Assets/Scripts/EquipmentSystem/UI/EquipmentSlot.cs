using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.ItemSystem;

namespace ZetanStudio
{
    public class EquipmentSlot : ItemSlotEx
    {
        [SerializeField, Enum(typeof(EquipmentType))]
        private int slotType;
        public EquipmentType SlotType => EquipmentTypeEnum.Instance[slotType];
    }
}
