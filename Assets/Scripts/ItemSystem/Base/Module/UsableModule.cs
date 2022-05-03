using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("使用")]
    public class UsableModule : ItemModule
    {
        [field: SerializeField, DisplayName("使用动作称呼")]
        public string UseActionName { get; protected set; } = "使用";

        [field: SerializeField, DisplayName("每次消耗量"), Range(0, 1)]
        public int Cost { get; protected set; } = 1;

        [field: SerializeField, DisplayName("用途"), ObjectDropDown(typeof(ItemUsage), "Name")]
        public ItemUsage Usage { get; protected set; }

        public override bool IsValid => Usage;
    }
}