using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using ZetanStudio;

[RequireComponent(typeof(RectTransform))]
public class Map : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

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
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out var localPoint))
            {
                MapManager.Instance.CreateDefaultMarkAtMapPoint(new Vector2((localPoint.x + rectTransform.rect.width / 2) / rectTransform.rect.width,
                    (localPoint.y + rectTransform.rect.height / 2) / rectTransform.rect.height));
            }
        }
#elif UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (clickCount < 1) isClick = true;
            if (clickTime <= 0.2f) clickCount++;
            if (clickCount > 1)
            {
                if (MapManager.Instance.IsViewingWorldMap)
                {
                    Rect screenSpaceRect = Utility.GetScreenSpaceRect(rectTransform);
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);
                    Vector2 mapViewportPoint = new Vector2((eventData.position.x - corners[0].x) / screenSpaceRect.width, (eventData.position.y - corners[0].y) / screenSpaceRect.height);
                    MapManager.Instance.CreateDefaultMarkAtMapPoint(mapViewportPoint);
                }
                isClick = false;
                clickCount = 0;
                clickTime = 0;
            }
        }
#endif
    }

#pragma warning disable IDE0044 // 添加只读修饰符
    bool canZoom;
#pragma warning restore IDE0044 // 添加只读修饰符
    public void OnPointerEnter(PointerEventData eventData)
    {
        canZoom = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        canZoom = false;
    }

    private void Update()
    {
        if (touchCount == 2 && Touchscreen.current != null && Touchscreen.current.touches.Count > 2)
        {
            List<TouchControl> touches = new List<TouchControl>();
            foreach (var touch in Touchscreen.current.touches)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touch.position.ReadValue()))
                    touches.Add(touch);
            }
            if (touches.Count == 2)
            {
                var first = touches[0];
                var second = touches[1];
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
        }
        else if (canZoom)
        {
            MapManager.Instance.Zoom(InputManager.GetAsix("Mouse ScrollWheel") * 0.01f);
        }
    }

#if UNITY_ANDROID
    private float clickTime;
    private int clickCount;
    private bool isClick;

    private void LateUpdate()
    {
        if (isClick)
        {
            clickTime += Time.deltaTime;
            if (clickTime > 0.2f)
            {
                isClick = false;
                clickCount = 0;
                clickTime = 0;
            }
        }
    }

    int touchCount;

    public void OnPointerDown(PointerEventData eventData)
    {
        touchCount++;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        touchCount--;
    }
#endif
}