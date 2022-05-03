using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("建造图纸"), Require(typeof(UsableModule))]
    public class StructureBlueprintModule : ItemModule
    {
        [field: SerializeField, DisplayName("设施")]
        public StructureInformation Structure { get; protected set; }

        public override bool IsValid => Structure;
    }
}