using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "product selectable items", menuName = "Zetan Studio/道具/用途/选择性产出")]
    public class ProductSelectableItems : ItemUsage
    {
        public ProductSelectableItems()
        {
            _name = "选择性产出道具";
        }

        protected override bool Use(ItemData item)
        {
            if (item.GetModule<SelectableProductModule>() is null) return false;
            return WindowsManager.OpenWindow<UI.ProductSelectionWindow>(item);
        }

        protected override bool Complete(ItemData item, int cost)
        {
            return true;
        }

        protected override void Nodify(ItemData item) { }
    }
}
