using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    public class ProductSelectionAgent : ListItem<ProductSelectionAgent, ItemInfo>
    {
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private Image icon;
        [SerializeField]
        private Text _name;

        public override void Refresh()
        {
            _name.text = ItemFactory.GetColorName(Data.Item);
            icon.overrideSprite = Data.Item.Icon;
        }

        protected override void RefreshSelected()
        {
            toggle.SetIsOnWithoutNotify(IsSelected);
        }
    }
}
