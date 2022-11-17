using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio
{
    using ItemSystem;
    using UnityEngine.EventSystems;
    using Extension;
    using ZetanStudio.ItemSystem.UI;
    using ZetanStudio.InventorySystem;
    using ZetanStudio.UI;

    public class MaterialAgent : ListItem<MaterialAgent, MaterialInfo>, IPointerClickHandler
    {
        [SerializeField]
        protected Image icon;
        [SerializeField]
        protected Text nameText;
        [SerializeField]
        protected Text amountText;
        [SerializeField]
        protected Color enoughColor = Color.green;
        [SerializeField]
        protected Color lackColor = Color.red;

        public InventoryHandler handler;

        protected override void OnAwake()
        {
            gameObject.GetOrAddComponent<Clickable>();
        }

        public override void Refresh()
        {
            switch (Data.CostType)
            {
                case MaterialCostType.SingleItem:
                    icon.overrideSprite = Data.Item.Icon;
                    nameText.text = ItemFactory.GetColorName(Data.Item);
                    break;
                case MaterialCostType.SameType:
                    icon.overrideSprite = Data.MaterialType.Icon;
                    nameText.text = $"[{LM.Tr(typeof(MaterialType).Name, Data.MaterialType.Name)}]";
                    break;
            }
            int have = handler?.GetMaterialsAmount(Data) ?? 0;
            amountText.text = $"{Utility.ColorText(have.ToString(), have < Data.Amount ? lackColor : enoughColor)}/{Data.Amount}";
        }

        public override void Clear()
        {
            icon.overrideSprite = null;
            nameText.text = string.Empty;
            amountText.text = string.Empty;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                WindowsManager.OpenWindow<ItemWindow>(Data.Item);
        }
    }
}
