using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("冷却"), Require(typeof(UsableModule))]
    public sealed class CoolDownModule : ItemModule, IItemWindowModifier
    {
        [field: SerializeField, Label("冷却器")]
        public ItemCooler Cooler { get; private set; }

        [SerializeField, HideIf("typeof(Cooler)", typeof(GroupCoolDown)), Min(1)]
        private float time = 1;
        public float Time => Cooler is GroupCoolDown cool ? cool.Time : time;

        [field: SerializeField]
        public string Message { get; private set; } = "冷却中";

        public override bool IsValid => Time > 0 && Cooler;

        public override ItemModuleData CreateData(ItemData item)
        {
            return new CoolDownData(item, this);
        }

        public void ModifyItemWindow(ItemInfoDisplayer displayer)
        {
            displayer.AddTitledContent($"-{LM.Tr(typeof(Item).Name, "冷却时间")}: ", LM.Tr(typeof(Item).Name, "{0}秒", MiscFuntion.SecondsToSortTime(Time)));
        }
    }

    public class CoolDownData : ItemModuleData<CoolDownModule>
    {
        public float NormalizeTime => Mathf.Clamp01(Time / Module.Time);

        public bool Available => Module.Cooler.HasCooled(Item);

        public float Time => Module.Cooler.GetTime(Item);

        public CoolDownData(ItemData item, CoolDownModule module) : base(item, module)
        {
            item.GetModuleData<UsableData>().canUse += module.Cooler.Handle;
            item.GetModuleData<UsableData>().canUseWithMsg += i => Available ? "" : module.Message;
        }

        public override GenericData GetSaveData()
        {
            var data = new GenericData();
            data["time"] = Time;
            return data;
        }
        public override void LoadSaveData(GenericData data)
        {
            Module.Cooler.SetTime(Item, data.ReadFloat("time"));
        }
    }
}