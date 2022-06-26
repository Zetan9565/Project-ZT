using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
                ItemSlot source = DragableManager.Instance.Current as ItemSlot;
                if (source)
                {
                    InventoryUtility.DiscardItem(window.Handler, source.Item);
                    source.FinishDrag();
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
        static bool canSelect(ItemSlot slot)
        {
            return slot && slot.Item && slot.Item.Model.Discardable;
        }
        void discardItems(IEnumerable<CountedItem> items)
        {
            if (items == null) return;
            foreach (var item in items)
                window.Handler.Lose(item.source, item.amount);
        }
        ItemSelectionWindow.StartSelection(ItemSelectionType.SelectAll, window.Grid as ISlotContainer, window.Handler, discardItems, $"从{window.Handler.Name}中丢弃物品", "确定要丢掉这些道具吗？", selectCondition: canSelect);
    }
}