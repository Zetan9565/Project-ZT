using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ZetanStudio.InventorySystem.UI;
using ZetanStudio.UI;

namespace ZetanStudio.ItemSystem.UI
{
    public class DiscardButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private InventoryWindow window;

        public void SetWindow(InventoryWindow window)
        {
            this.window = window;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
#if UNITY_STANDALONE
                if (DragableManager.Instance.IsDraging)
                {
                    ItemSlotEx source = DragableManager.Instance.Current as ItemSlotEx;
                    if (source)
                    {
                        if (source.Item && source.Item.Discardable)
                            AmountWindow.StartInput(discard, window.Handler.GetAmount(source.Item));
                        source.FinishDrag();

                        void discard(long count)
                        {
                            if (window.Handler.CanLose(source.Item, (int)count))
                                window.Handler.Lose(source.Item, (int)count);
                        }
                    }
                }
                else
                {
                    OpenDiscardWindow();
                }
#elif UNITY_ANDROID
                OpenDiscardWindow();
                FloatTipsPanel.ShowText(transform.position, "将物品拖拽到此按钮丢弃，或者点击该按钮进行多选。", 3);
#endif
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
#if UNITY_STANDALONE
            if (!DragableManager.Instance.IsDraging)
                FloatTipsPanel.ShowText(transform.position, "将物品放置到此按钮丢弃，或者点击该按钮进行多选。", 3);
#endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if UNITY_STANDALONE
            WindowsManager.CloseWindow<FloatTipsPanel>();
#endif
        }

        private void OpenDiscardWindow()
        {
            static bool canSelect(ItemData item)
            {
                return item?.Model.Discardable ?? false;
            }
            void discardItems(IEnumerable<CountedItem> items)
            {
                if (items == null) return;
                foreach (var item in items)
                    window.Handler.Lose(item.source, item.amount);
            }
            ItemSelectionWindow.StartSelection(ItemSelectionType.SelectAll, window.Grid as ISlotContainer, window.Handler, discardItems, "丢弃", "确定要丢掉这些道具吗？", selectCondition: canSelect);
        }
    }
}