using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [CreateAssetMenu(fileName = "product items", menuName = "Zetan Studio/道具/用途/产出道具")]
    public sealed class ProductItems : ItemUsage
    {
        public override string Name => "产出道具";

        private ItemWithAmount[] product;

        protected override ItemHandler Instance => Instantiate(this);

        protected override bool Use(ItemData item)
        {
            return item.GetModule<ProductModule>() is not null;
        }

        protected override bool Prepare(ItemData item, int cost)
        {
            if (item.GetModule<ProductModule>() is not ProductModule module) return false;
            product = module.ProductInfo ? module.ProductInfo.DoDrop().ToArray() : DropItemInfo.Drop(module.Product).ToArray();
            return BackpackManager.Instance.CanLose(item, cost, product);
        }

        protected override bool Complete(ItemData item, int cost)
        {
            return BackpackManager.Instance.LoseItem(item, cost, product);
        }
    }
}