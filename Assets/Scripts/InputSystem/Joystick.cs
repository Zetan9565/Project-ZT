using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using ZetanStudio.Extension;

[RequireComponent(typeof(EmptyGraphic))]
public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
{
    public RectTransform background;
    public OnScreenStick handle;

    [HideWhenPlaying(true)]
    public bool autoHide = true;
    [HideIf("autoHide", false), HideWhenPlaying(true)]
    public bool fade = true;
    [HideIf(new string[] { "autoHide", "fade" }, new object[] { false, false }, false)]
    public float duration = 0.5f;

    private RectTransform baseRect;
    private Image bgImg;
    private Image hdImg;
    private readonly Dictionary<Image, Coroutine> fadeCoroutines = new Dictionary<Image, Coroutine>();

    private void Awake()
    {
        baseRect = GetComponent<RectTransform>();
        bgImg = background.GetComponent<Image>();
        if (bgImg) bgImg.raycastTarget = false;
        hdImg = handle.GetComponent<Image>();
        if (hdImg) hdImg.raycastTarget = false;
        fadeCoroutines.Add(bgImg, null);
        fadeCoroutines.Add(hdImg, null);
        Stop();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        handle.OnPointerDown(eventData);
        handle.OnDrag(eventData);
        OnStart();
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
                if (bgImg) CrossFadeAlpha(bgImg, 1, 0);
                if (hdImg) CrossFadeAlpha(hdImg, 1, 0);
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
                if (bgImg) CrossFadeAlpha(bgImg, 0, duration);
                if (hdImg) CrossFadeAlpha(hdImg, 0, duration);
            }
            else ZetanUtility.SetActive(background, false);
        }
    }

    private void CrossFadeAlpha(Image image, float alpha, float duration)
    {
        if (fadeCoroutines[image] != null) StopCoroutine(fadeCoroutines[image]);
        fadeCoroutines[image] = StartCoroutine(Fade(image, alpha, duration));

        static IEnumerator Fade(Image target, float alpha, float duration)
        {
            float time = 0;
            while (time < duration)
            {
                yield return null;
                if (time <= duration) target.color = new Color(target.color.r, target.color.g, target.color.b, target.color.a + (alpha - target.color.a) * Time.unscaledDeltaTime / (duration - time));
                time += Time.unscaledDeltaTime;
            }
            target.color = new Color(target.color.r, target.color.g, target.color.b, alpha);
        }
    }
}
