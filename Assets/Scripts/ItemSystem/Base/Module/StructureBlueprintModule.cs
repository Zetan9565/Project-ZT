using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("建造图纸"), Require(typeof(UsableModule))]
    public class StructureBlueprintModule : ItemModule
    {
        [field: SerializeField, Label("设施"), ObjectSelector("_name", title: "设施")]
        public StructureInformation Structure { get; protected set; }

        public override bool IsValid => Structure;
    }
}