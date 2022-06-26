using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "product items", menuName = "Zetan Studio/道具/用途/产出道具")]
    public sealed class ProductItems : ItemUsage
    {
        public ProductItems()
        {
            _name = "产出道具";
        }

        private CountedItem[] product;

        protected override ItemHandler Instance => Instantiate(this);

        protected override bool Use(ItemData item)
        {
            return item.GetModule<ProductModule>();
        }

        protected override bool Prepare(ItemData item, int cost)
        {
            if (!item.TryGetModule<ProductModule>(out var module) || !module.IsValid) return false;
            product = module.Product.Count < 1 ? module.ProductInfo.DoDrop().ToArray() : DropItemInfo.Drop(module.Product).ToArray();
            return BackpackManager.Instance.CanLose(item, cost, product);
        }

        protected override bool Complete(ItemData item, int cost)
        {
            return BackpackManager.Instance.Lose(item, cost, product);
        }
    }
}