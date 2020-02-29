using UnityEngine;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        if ((Application.platform == RuntimePlatform.Android) || eventData.button == PointerEventData.InputButton.Right)
            MapManager.Instance.DragWorldMap(-eventData.delta);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!MapManager.Instance) return;
#if UNITY_STANDALONE
        if (eventData.clickCount > 1)
            MapManager.Instance.CreateDefaultMarkAtMousePos(eventData.position);
#elif UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (clickCount < 1) isClick = true;
            if (clickTime <= 0.2f) clickCount++;
            if (clickCount > 1)
            {
                if (MapManager.Instance.IsViewingWorldMap) MapManager.Instance.CreateDefaultMarkAtMousePos(eventData.position);
                isClick = false;
                clickCount = 0;
                clickTime = 0;
            }
        }
#endif
    }

    bool canZoom;
    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_STANDALONE
#endif
        canZoom = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
#endif
        canZoom = false;
    }

    private void Update()
    {
        if (canZoom) MapManager.Instance.ZoomMap(Input.mouseScrollDelta.y);
    }

#if UNITY_ANDROID
    private float clickTime;
    private int clickCount;
    private bool isClick;

    private void FixedUpdate()
    {
        if (isClick)
        {
            clickTime += Time.fixedDeltaTime;
            if (clickTime > 0.2f)
            {
                isClick = false;
                clickCount = 0;
                clickTime = 0;
            }
        }
    }
#endif
}