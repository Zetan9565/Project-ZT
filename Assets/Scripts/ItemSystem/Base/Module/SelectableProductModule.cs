using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("可选产出")]
    public sealed class SelectableProductModule : ItemModule, IItemWindowModifier
    {
        public override bool IsValid => Amount > 0 && product.Length > 0 && product.All(x => x.Item && x.Amount > 0);

        [field: SerializeField, Min(1)]
        public int Amount { get; private set; } = 1;

        [SerializeField]
        private ItemInfo[] product = { new ItemInfo() };
        public ReadOnlyCollection<ItemInfo> Product => new ReadOnlyCollection<ItemInfo>(product);

        public void ModifyItemWindow(ItemInfoDisplayer displayer)
        {
            displayer.AddTitle(LM.Tr(typeof(ItemModule).Name, "以下道具选择{0}种:", Amount));
            displayer.AddContent(ItemInfo.GetItemInfoString(Product));
        }
    }
}