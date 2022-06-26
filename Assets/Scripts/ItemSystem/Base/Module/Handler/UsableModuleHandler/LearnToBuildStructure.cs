using UnityEngine;
using ZetanStudio.StructureSystem;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "learn to build structure", menuName = "Zetan Studio/道具/用途/学习建造设施")]
    public sealed class LearnToBuildStructure : ItemUsage
    {
        public LearnToBuildStructure()
        {
            _name = "学习建造设施";
        }

        protected override bool Use(ItemData item)
        {
            if (!item.TryGetModule<StructureBlueprintModule>(out var module)) return false;
            else return StructureManager.Learn(module.Structure);
        }
    }
}