using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [CreateAssetMenu(fileName = "learn to build structure", menuName = "Zetan Studio/道具/用途/学习建造设施")]
    public sealed class LearnToBuildStructure : ItemUsage
    {
        public override string Name => "学习建造设施";

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<StructureBlueprintModule>() is not StructureBlueprintModule module) return false;
            else return StructureManager.Instance.Learn(module.Structure);
        }
    }
}