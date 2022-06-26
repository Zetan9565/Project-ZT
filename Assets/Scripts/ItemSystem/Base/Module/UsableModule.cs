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
    }
}