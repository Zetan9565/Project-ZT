﻿using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    using FarmingSystem;

    [Name("种子")]
    public sealed class SeedModule : ItemModule
    {
        [field: SerializeField, Label("作物")]
        public CropInformation Crop { get; private set; }

        public override bool IsValid => Crop;
    }
}