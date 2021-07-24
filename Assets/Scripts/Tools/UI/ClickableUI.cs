using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ClickableUI : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    IPointerExitHandler
{
    public bool isEnabled = true;

    public float doubleClickDelay = 0.2f;
    public float longPressTime = 0.5f;

    public UnityEvent onClick;
    public UnityEvent onDoubleClick;
    public UnityEvent onLongPress;

    private float clickTime;
    private int clickCount;
    private Coroutine clickCoroutine;
    private readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    private Coroutine pressCoroutine;
    private float pressTime = 0;
    private IEnumerator Press()
    {
        pressTime = 0;
        while (true)
        {
            pressTime += Time.fixedDeltaTime;
            if (pressTime >= longPressTime)
            {
                onLongPress?.Invoke();
                pressCoroutine = null;
                yield break;
            }
            yield return WaitForFixedUpdate;
        }
    }
    private IEnumerator Click()
    {
        clickTime = 0;
        while (true)
        {
            clickTime += Time.fixedDeltaTime;
            if (clickTime >= doubleClickDelay)
            {
                clickCount = 0;
                clickTime = 0;
                clickCoroutine = null;
                yield break;
            }
            yield return WaitForFixedUpdate;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEnabled && eventData.button == PointerEventData.InputButton.Left)
        {
            if (clickCount < 1)
            {
                if (clickCoroutine != null) StopCoroutine(clickCoroutine);
                clickCoroutine = StartCoroutine(Click());
            }
            if (clickTime <= 0.2f) clickCount++;
            if (clickCount > 1)
            {
                onDoubleClick?.Invoke();
                clickCount = 0;
                clickTime = 0;
                if (clickCoroutine != null) StopCoroutine(clickCoroutine);
                clickCoroutine = null;
            }
            else if (clickCount == 1 && pressTime < 0.5f)
            {
                onClick?.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEnabled && eventData.button == PointerEventData.InputButton.Left)
        {
            if (pressCoroutine != null) StopCoroutine(pressCoroutine);
            pressCoroutine = StartCoroutine(Press());
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
    }
}