using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "product selectable items", menuName = "Zetan Studio/����/��;/ѡ���Բ���")]
    public class ProductSelectableItems : ItemUsage
    {
        public ProductSelectableItems()
        {
            _name = "ѡ���Բ�������";
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
