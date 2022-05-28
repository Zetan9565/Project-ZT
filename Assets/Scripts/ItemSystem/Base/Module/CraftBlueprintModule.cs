using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("制作图纸"), Require(typeof(UsableModule))]
    public class CraftBlueprintModule : ItemModule
    {
        [field: SerializeField, Label("制作道具"), ItemFilter(typeof(CraftableModule))]
        public Item Product { get; protected set; }

        public override bool IsValid => Product;
    }
}