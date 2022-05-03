using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    public class DurabilityData : ItemModuleData<DurabilityModule>
    {
        public int currentDurability;
        public int maxDurability;

        public DurabilityData(ItemData item, DurabilityModule model) : base(item, model)
        {
            maxDurability = model.Durability;
        }
    }
}