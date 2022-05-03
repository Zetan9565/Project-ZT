using ZetanStudio.Item.Module;

namespace ZetanStudio.Item
{
    public class LockableData : ItemModuleData<LockableModule>
    {
        public bool isLocked;

        public LockableData(ItemData item, LockableModule model) : base(item, model)
        {
        }
    }
}