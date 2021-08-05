using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
#if UNITY_STANDALONE
            if (DragableManager.Instance.IsDraging)
            {
                ItemAgent source = DragableManager.Instance.Current as ItemAgent;
                if (source)
                {
                    BackpackManager.Instance.DiscardItem(source.MItemInfo);
                    AmountManager.Instance.SetPosition(eventData.position);
                    source.FinishDrag();
                }
            }
            else
            {
                BackpackManager.Instance.OpenDiscardWindow();
            }
#elif UNITY_ANDROID
            BackpackManager.Instance.OpenDiscardWindow();
            TipsManager.Instance.ShowText(transform.position, "将物品拖拽到此按钮丢弃，或者点击该按钮进行多选。", 3);
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
}