using System.Linq;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("产出"), Require(typeof(UsableModule))]
    public class ProductModule : ItemModule, IItemWindowModifier
    {
        [SerializeField]
        private DropItemInfo[] product = { };
        public ReadOnlyCollection<DropItemInfo> Product => ProductInfo && product.Length < 1 ? ProductInfo.Products : new ReadOnlyCollection<DropItemInfo>(product);

        [field: SerializeField, Label("公共产出表")]
        public ProductInformation ProductInfo { get; protected set; }

        public void ModifyItemWindow(ItemInfoDisplayer displayer)
        {
            if (Product.Any(x => !x.Definite)) displayer.AddTitle(L.Tr(typeof(ItemModule).Name, "可能获得:"));
            else displayer.AddTitle(L.Tr(typeof(ItemModule).Name, "可获得:"));
            displayer.AddContent(DropItemInfo.GetDropInfoString(Product));
        }

        public override bool IsValid => ProductInfo && ProductInfo.IsValid || product.Length > 0 && !System.Array.Exists(product, x => !x.IsValid);
    }
}