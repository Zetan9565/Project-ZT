using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class Map : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
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
        canZoom = true;
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        canZoom = false;
#endif
        isClick = false;
        clickCount = 0;
        clickTime = 0;
    }

    private void Update()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 2)
        {
            var first = Touchscreen.current.touches[0];
            var second = Touchscreen.current.touches[1];
            if (first.press.wasPressedThisFrame && second.press.wasPressedThisFrame)
            {
                var firstPos = first.position.ReadValue();
                var secondPos = second.position.ReadValue();
                var curDis = (firstPos - secondPos).magnitude;

                var firstPrePos = first.position.ReadValue() - first.delta.ReadValue();
                var secondPrePos = second.position.ReadValue() - second.delta.ReadValue();
                var preDis = (firstPrePos - secondPrePos).magnitude;

                var zoomValue = curDis - preDis;

                MapManager.Instance.Zoom(zoomValue);
            }
        }
        else if (canZoom)
        {
            MapManager.Instance.Zoom(Mouse.current.scroll.ReadValue().y * 0.01f);
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
#endif
}