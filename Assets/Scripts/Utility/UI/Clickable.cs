using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Clickable : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    IPointerExitHandler
{
    public bool isEnabled = true;

    [Min(0.2f)]
    public float doubleClickInterval = 0.2f;
    [Min(0.3f)]
    public float longPressTime = 0.5f;
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onRightClick = new UnityEvent();
    public UnityEvent onDoubleClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();

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
            if (clickTime >= doubleClickInterval)
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
        if (isEnabled && isActiveAndEnabled)
            if (eventData.button == PointerEventData.InputButton.Left)
            {
#if UNITY_STANDALONE
                if (eventData.clickCount > 1)
                    onDoubleClick?.Invoke();
                else onClick?.Invoke();
#elif UNITY_ANDROID
                if (clickCount < 1)
                {
                    if (clickCoroutine != null) StopCoroutine(clickCoroutine);
                    clickCoroutine = StartCoroutine(Click());
                }
                if (clickTime <= doubleClickInterval) clickCount++;
                if (clickCount > 1)
                {
                    onDoubleClick?.Invoke();
                    clickCount = 0;
                    clickTime = 0;
                    if (clickCoroutine != null) StopCoroutine(clickCoroutine);
                    clickCoroutine = null;
                }
                else if (clickCount == 1 && pressTime < longPressTime)
                {
                    onClick?.Invoke();
                }
#endif
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
                onRightClick?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEnabled && isActiveAndEnabled && eventData.button == PointerEventData.InputButton.Left)
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