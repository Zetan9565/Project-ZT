using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("种子")]
    public sealed class SeedModule : ItemModule
    {
        [field: SerializeField, Label("作物")]
        public CropInformation Crop { get; private set; }

        public override bool IsValid => Crop;
    }
}