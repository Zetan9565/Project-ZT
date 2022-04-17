using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DragableManager : SingletonMonoBehaviour<DragableManager>
{
    public IDragAble Current { get; private set; }

    public bool IsDraging
    {
        get
        {
            return Current != null;
        }
    }

    private Image icon;

    private Canvas iconSortCanvas;
    private Action<PointerEventData> onEndDrag;
    private PointerEventData eventData;
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        icon = GetComponent<Image>();
        if (!icon.GetComponent<GraphicRaycaster>()) icon.gameObject.AddComponent<GraphicRaycaster>();
        iconSortCanvas = icon.GetComponent<Canvas>();
        iconSortCanvas.overrideSorting = true;
    }

    private void Update()
    {
        eventData = new PointerEventData(EventSystem.current);
        eventData.position = InputManager.mousePosition;
        MoveIcon();
    }

    public void MoveIcon()
    {
        if (Current != null)
        {
            if (InputManager.GetMouseButtonDown(1) || UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.touches.Count > 0
                && UnityEngine.InputSystem.Touchscreen.current.touches[1].press.wasPressedThisFrame)
                CancelDrag();
            if (InputManager.GetMouseButtonDown(0) || UnityEngine.InputSystem.Pointer.current.press.wasReleasedThisFrame)
                EndDrag();
            icon.transform.position = eventData.position;
        }
    }

    public void BeginDrag(IDragAble dragable, Action<PointerEventData> endDragAction, float width = 100, float height = 100)
    {
        if (!dragable.DragAbleIcon) return;
        iconSortCanvas.sortingOrder = 999;
        Current = dragable;
        icon.overrideSprite = dragable.DragAbleIcon;
        icon.color = Color.white;
        ZetanUtility.SetActive(icon.gameObject, true);
        onEndDrag = endDragAction;
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        MoveIcon();
    }

    public void ResetIcon()
    {
        Current = null;
        onEndDrag = null;
        raycastResults.Clear();
        ZetanUtility.SetActive(icon.gameObject, false);
    }

    public void CancelDrag()
    {
        ResetIcon();
    }

    public void EndDrag()
    {
        EventSystem.current.RaycastAll(eventData, raycastResults);
        eventData.button = PointerEventData.InputButton.Left;
        if (raycastResults.Count > 0) eventData.pointerCurrentRaycast = raycastResults[0];
        else eventData.pointerCurrentRaycast = new RaycastResult();
        onEndDrag?.Invoke(eventData);
        ResetIcon();
    }
}