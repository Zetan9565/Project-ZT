using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("种子")]
    public sealed class SeedModule : ItemModule
    {
        [field: SerializeField, DisplayName("作物")]
        public CropInformation Crop { get; private set; }

        public override bool IsValid => Crop;
    }
}