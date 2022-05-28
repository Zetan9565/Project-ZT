using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("产出"), Require(typeof(UsableModule))]
    public class ProductModule : ItemModule
    {
        [SerializeField]
        private DropItemInfo[] product = { };
        public ReadOnlyCollection<DropItemInfo> Product => ProductInfo ? ProductInfo.Products : new ReadOnlyCollection<DropItemInfo>(product);

        [field: SerializeField, Label("公共产出表")]
        public ProductInformation ProductInfo { get; protected set; }

        public override bool IsValid => ProductInfo && ProductInfo.IsValid || product.Length > 0 && !System.Array.Exists(product, x => !x.IsValid);
    }
}