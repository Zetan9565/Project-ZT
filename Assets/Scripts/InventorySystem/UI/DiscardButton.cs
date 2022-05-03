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
#if UNITY_STANDARD
            if (DragableManager.Instance.IsDraging)
            {
                ItemSlotAgent source = DragableManager.Instance.Current as ItemSlotAgent;
                if (source)
                {
                    InventoryUtility.DiscardItem(Window.Handler, source.Item);
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
            TipsManager.Instance.ShowText(transform.position, "将物品放置到此按钮丢弃，或者点击该按钮进行多选。", 3);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        TipsManager.Instance.Hide();
#endif
    }

    private void OpenDiscardWindow()
    {
        static bool canSelect(ItemSlotBase slot)
        {
            return slot && slot.Item && slot.Item.Model_old.DiscardAble;
        }
        void discardItems(IEnumerable<ItemWithAmount> items)
        {
            if (items == null) return;
            foreach (var item in items)
                window.Handler.LoseItem(item.source, item.amount);
        }
        ItemSelectionWindow.StartSelection(ItemSelectionType.SelectAll, window.Grid as ISlotContainer, window.Handler, discardItems, "丢弃物品", "确定要丢掉这些道具吗？", selectCondition: canSelect);
    }
}