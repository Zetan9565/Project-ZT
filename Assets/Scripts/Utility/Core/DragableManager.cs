using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DragableManager : SingletonMonoBehaviour<DragableManager>
{
    public IDraggable Current { get; private set; }

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
        eventData.position = Input.mousePosition;
        MoveIcon();
    }

    public void MoveIcon()
    {
        if (Current != null)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetTouchDown(1)) CancelDrag();
            if (Input.GetPointerUp()) EndDrag();
            icon.transform.position = eventData.position;
        }
    }

    public void BeginDrag(IDraggable dragable, Action<PointerEventData> endDragAction, float width = 100, float height = 100)
    {
        if (!dragable.DraggableIcon) return;
        iconSortCanvas.sortingOrder = 999;
        Current = dragable;
        icon.overrideSprite = dragable.DraggableIcon;
        icon.color = Color.white;
        onEndDrag = endDragAction;
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        Update();
        ZetanUtility.SetActive(icon.gameObject, true);
    }

    public void ResetIcon()
    {
        Current = null;
        onEndDrag = null;
        raycastResults.Clear();
        ZetanUtility.SetActive(icon, false);
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