namespace ZetanStudio.Item.Module
{
    [Name("可锁定")]
    public class LockableModule : ItemModule
    {
        public override bool IsValid => true;

        public override ItemModuleData CreateData(ItemData item)
        {
            return new LockableData(item, this);
        }
    }
}