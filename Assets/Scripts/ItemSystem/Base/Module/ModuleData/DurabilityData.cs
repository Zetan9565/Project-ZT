using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    public class DurabilityData : ItemModuleData<DurabilityModule>
    {
        public int currentDurability;
        public int maxDurability;

        public DurabilityData(ItemData item, DurabilityModule module) : base(item, module)
        {
            maxDurability = module.Durability;
        }
    }
}