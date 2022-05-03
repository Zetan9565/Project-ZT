using UnityEngine;

namespace ZetanStudio.Item.Module
{

    [Name("背包空间扩张"), Require(typeof(UsableModule))]
    public sealed class SpaceExpandModule : ItemModule
    {
        [field: SerializeField, Min(1)]
        public int Space { get; private set; } = 1;

        public override bool IsValid => Space > 0;
    }
}