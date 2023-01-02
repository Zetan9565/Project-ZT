using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.InventorySystem;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("宝石镶嵌"), Require(typeof(EquipableModule))]
    public class GemSlotModule : ItemModule, IItemWindowModifier
    {
        [field: SerializeField, Label("插槽数"), Min(1)]
        public int SlotCount { get; protected set; } = 1;

        public override bool IsValid => SlotCount > 0;

        public void ModifyItemWindow(ItemInfoDisplayer displayer)
        {
            var data = displayer.Item.GetModuleData<GemSlotData>();
            for (int i = 0; i < SlotCount; i++)
            {
                var gem = data && i < data.gems.Count ? data?.gems?[i] : null;
                if (!string.IsNullOrEmpty(gem) && BackpackManager.Instance.Inventory.HiddenItems.TryGetValue(gem, out var find))
                    displayer.AddGem(find.Model);
                else displayer.AddGem(null);
            }
        }

        public override ItemModuleData CreateData(ItemData item)
        {
            return new GemSlotData(item, this);
        }
    }

    public class GemSlotData : ItemModuleData<GemSlotModule>
    {
        public List<string> gems = new List<string>();

        public GemSlotData(ItemData item, GemSlotModule module) : base(item, module)
        {
        }
        public override GenericData GenerateSaveData()
        {
            var data = new GenericData();
            foreach (var gem in gems)
            {
                data[gem] = gem;
            }
            return data;
        }
        public override void LoadSaveData(GenericData data)
        {
            foreach (var kvp in data.ReadStringDict())
            {
                gems.Add(kvp.Key);
            }
        }
    }
}