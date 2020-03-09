using UnityEngine;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        if (touchCount < 2 && ((Application.platform == RuntimePlatform.Android) || eventData.button == PointerEventData.InputButton.Right))
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
            if (clickTime <= 0.2f && Input.touchCount < 2) clickCount++;
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
        if (touchCount > 1 && Input.touchCount == 2)
        {
            var first = Input.GetTouch(0);
            var second = Input.GetTouch(1);

            var firstPos = (Vector2)Camera.main.WorldToViewportPoint(first.position);
            var secondPos = (Vector2)Camera.main.WorldToViewportPoint(second.position);
            var curDis = (firstPos - secondPos).magnitude;

            var firstPrePos = (Vector2)Camera.main.WorldToViewportPoint(first.position - first.deltaPosition);
            var secondPrePos = (Vector2)Camera.main.WorldToViewportPoint(second.position - second.deltaPosition);
            var preDis = (firstPrePos - secondPrePos).magnitude;

            var zoomValue = curDis - preDis;

            MapManager.Instance.Zoom(zoomValue);
        }
        else if (canZoom)
        {
            MapManager.Instance.Zoom(Input.mouseScrollDelta.y);
        }
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

    private int touchCount;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (touchCount < 2) touchCount++;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (touchCount > 0) touchCount--;
    }
#endif
}