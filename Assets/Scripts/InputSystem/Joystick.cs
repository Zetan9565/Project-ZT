using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using ZetanExtends;

[RequireComponent(typeof(Image))]
public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
{
    public RectTransform background;
    public OnScreenStick handle;

    public bool autoHide = true;
    public bool fade = true;
    public float duration = 0.5f;

    private RectTransform baseRect;
    private Image bgImg;
    private Image hdImg;

    private void Awake()
    {
        baseRect = GetComponent<RectTransform>();
        bgImg = background.GetComponent<Image>();
        hdImg = handle.GetComponent<Image>();
        Stop();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnStart();
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        handle.OnPointerDown(eventData);
        handle.OnDrag(eventData);
    }

    private Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, null, out Vector2 localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        handle.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.OnPointerUp(eventData);
        Stop();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnStart();
        handle.OnDrag(eventData);
    }

    public void Enable()
    {
        ZetanUtility.SetActive(gameObject, true);
    }

    public void Disable()
    {
        ZetanUtility.SetActive(gameObject, false);
    }

    private void OnStart()
    {
        if (autoHide)
        {
            ZetanUtility.SetActive(background, true);
            if (fade)
            {
                bgImg.CrossFadeAlpha(1, 0, true);
                hdImg.CrossFadeAlpha(1, 0, true);
            }
        }
    }

    public void Stop()
    {
        handle.GetRectTransform().anchoredPosition = Vector2.zero;
        if (autoHide)
        {
            if (fade)
            {
                if (bgImg) bgImg.CrossFadeAlpha(0, duration, true);
                if (hdImg) hdImg.CrossFadeAlpha(0, duration, true);
            }
            else
            {
                ZetanUtility.SetActive(background, false);
            }
        }
    }
}
