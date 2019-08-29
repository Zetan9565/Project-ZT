using UnityEngine;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((Application.platform == RuntimePlatform.Android) || eventData.button == PointerEventData.InputButton.Right)
            MapManager.Instance.DragWorldMap(eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount > 1)
        {
            MapManager.Instance.CreateMarkByMousePosition(Input.mousePosition);
        }
    }
}
