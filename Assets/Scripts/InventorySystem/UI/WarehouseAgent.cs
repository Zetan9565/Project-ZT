using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.UI;

namespace ZetanStudio.InventorySystem.UI
{
    public class WarehouseAgent : ListItem<WarehouseAgent, IWarehouseKeeper>
    {
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private Image icon;
        [SerializeField]
        private Text _name;

        public override void Refresh()
        {
            _name.text = Data.WarehouseName;
            icon.overrideSprite = null;
        }

        protected override void RefreshSelected()
        {
            toggle.SetIsOnWithoutNotify(IsSelected);
        }
    }
}