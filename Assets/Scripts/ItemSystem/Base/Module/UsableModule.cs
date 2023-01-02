using System;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("使用")]
    public class UsableModule : ItemModule
    {
        [field: SerializeField, Label("使用动作称呼")]
        public string UseActionName { get; protected set; } = "使用";

        [field: SerializeField, Label("每次消耗量"), Range(0, 1), Tooltip("为0时可以无限使用")]
        public int Cost { get; protected set; } = 1;

        [field: SerializeField, Label("用途")]
        public ItemUsage Usage { get; protected set; }

        public override bool IsValid => Usage;

        public override ItemModuleData CreateData(ItemData item)
        {
            return new UsableData(item, this);
        }
    }

    public class UsableData : ItemModuleData<UsableModule>
    {
        public event Func<ItemData, bool> canUse;
        public event Func<ItemData, string> canUseWithMsg;

        public UsableData(ItemData item, UsableModule module) : base(item, module)
        {
        }

        public bool CanUse(ItemData item) => canUse?.Invoke(item) ?? true;
        public string CanUseWithMsg(ItemData item) => canUseWithMsg?.Invoke(item) ?? string.Empty;

        public override GenericData GenerateSaveData() => null;

        public override void LoadSaveData(GenericData data) { }
    }
}