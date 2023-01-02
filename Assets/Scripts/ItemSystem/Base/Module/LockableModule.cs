namespace ZetanStudio.ItemSystem.Module
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

    public class LockableData : ItemModuleData<LockableModule>
    {
        public bool isLocked;

        public LockableData(ItemData item, LockableModule module) : base(item, module)
        {
        }
        public override GenericData GenerateSaveData()
        {
            var data = new GenericData();
            data["locked"] = isLocked;
            return data;
        }
        public override void LoadSaveData(GenericData data)
        {
            isLocked = data.ReadBool("locked");
        }
    }
}