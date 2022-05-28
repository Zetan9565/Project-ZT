using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.Item.Module
{
    public class EquipmentData : ItemModuleData<EquipableModule>
    {
        public List<ItemData> gems;

        public EquipmentData(ItemData item, EquipableModule module) : base(item, module)
        {
        }
    }
}