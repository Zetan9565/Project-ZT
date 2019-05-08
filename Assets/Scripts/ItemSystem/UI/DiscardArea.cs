using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardArea : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (DragableHandler.Instance.IsDraging && eventData.button == PointerEventData.InputButton.Left)
        {
            ItemAgent source = DragableHandler.Instance.Current as ItemAgent;
            if (source)
            {
                BackpackManager.Instance.DiscardItem(source.MItemInfo);
                AmountHandler.Instance.SetPosition(eventData.position);
                source.FinishDrag();
            }
        }
    }
}
