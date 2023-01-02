using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("耐久度")]
    public class DurabilityModule : ItemModule
    {
        [field: SerializeField, Label("初始值"), Min(0)]
        public int Durability { get; protected set; }

        public override bool IsValid => Durability > 0;

        public override ItemModuleData CreateData(ItemData item)
        {
            return new DurabilityData(item, this);
        }
    }

    public class DurabilityData : ItemModuleData<DurabilityModule>
    {
        public int currentDurability;
        public int maxDurability;

        public DurabilityData(ItemData item, DurabilityModule module) : base(item, module)
        {
            maxDurability = module.Durability;
            currentDurability = maxDurability;
        }

        public override GenericData GenerateSaveData()
        {
            var data = new GenericData();
            data["currentDurability"] = currentDurability;
            data["maxDurability"] = maxDurability;
            return data;
        }
        public override void LoadSaveData(GenericData data)
        {
            currentDurability = data.ReadInt("currentDurability");
            maxDurability = data.ReadInt("maxDurability");
        }
    }
}