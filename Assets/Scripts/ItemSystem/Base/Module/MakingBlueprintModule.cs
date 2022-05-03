using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("制作图纸"), Require(typeof(UsableModule))]
    public class MakingBlueprintModule : ItemModule
    {
        [field: SerializeField, DisplayName("制作道具")]
        public ItemNew Product { get; protected set; }

        public override bool IsValid => Product;
    }
}