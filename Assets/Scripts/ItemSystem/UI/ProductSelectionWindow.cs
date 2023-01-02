using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    using Module;
    using ZetanStudio.InventorySystem;

    public class ProductSelectionWindow : Window
    {
        [SerializeField]
        private Text title;
        [SerializeField]
        private Button confirmButton;
        [SerializeField]
        private ProductSelectionList list;
        private ItemData item;
        private SelectableProductModule module;
        private int cost;

        protected override void OnAwake()
        {
            list.Selectable = true;
            list.SetSelectCallback((IEnumerable<ProductSelectionAgent> list) =>
            {
                confirmButton.interactable = System.Linq.Enumerable.Count(list) == module.Amount;
            });
            confirmButton.onClick.AddListener(Confirm);
        }

        public void Confirm()
        {
            if (list.SelectedIndices.Count < 1)
                MessageManager.Instance.New(Tr("未选择任何道具"));
            else if (list.SelectedIndices.Count < module.Amount)
                MessageManager.Instance.New(Tr("未选择足够的道具"));
            else
            {
                if (BackpackManager.Instance.Get(list.SelectedDatas, new CountedItem(item, cost)))
                    Close();
            }
        }

        protected override bool OnOpen(params object[] args)
        {
            if (args.Length == 0 || args[0] is not ItemData item
                || !item.TryGetModule<UsableModule>(out var usable)
                || !item.TryGetModule<SelectableProductModule>(out var selectable))
                return false;
            this.item = item;
            module = selectable;
            cost = usable.Cost;
            list.Refresh(module.Product);
            list.SelectionLimit = module.Amount;
            title.text = Tr("选择{0}种道具", module.Amount);
            confirmButton.interactable = false;
            return true;
        }
    }
}
