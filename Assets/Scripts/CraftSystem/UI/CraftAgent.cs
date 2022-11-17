using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.CraftSystem.UI
{
    using ItemSystem;
    using ItemSystem.UI;
    using ZetanStudio.UI;

    [RequireComponent(typeof(Button))]
    public class CraftAgent : ListItem<CraftAgent, Item>
    {
        [SerializeField]
        private ItemSlot icon;

        [SerializeField]
        private Text nameText;

        [SerializeField]
        private GameObject selected;

        public override void Refresh()
        {
            icon.SetItem(Data);
            nameText.text = ItemFactory.GetColorName(Data);
        }

        protected override void RefreshSelected()
        {
            Utility.SetActive(selected, isSelected);
        }

        public override void Clear()
        {
            base.Clear();
            nameText.text = string.Empty;
            Data = null;
            icon.Vacate();
        }
    }
}