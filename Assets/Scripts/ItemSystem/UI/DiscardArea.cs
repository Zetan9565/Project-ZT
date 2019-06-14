using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardArea : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (DragableManager.Instance.IsDraging && eventData.button == PointerEventData.InputButton.Left)
        {
            ItemAgent source = DragableManager.Instance.Current as ItemAgent;
            if (source)
            {
                BackpackManager.Instance.DiscardItem(source.MItemInfo);
                AmountManager.Instance.SetPosition(eventData.position);
                source.FinishDrag();
            }
        }
    }
}
