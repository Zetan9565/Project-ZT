using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class MapIcon : MonoBehaviour, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public Image iconImage;

    [HideInInspector]
    public Image iconRange;

    [HideInInspector]
    public UnityEvent onClick = new UnityEvent();

    [HideInInspector]
    public UnityEvent onEnter = new UnityEvent();

    [HideInInspector]
    public MapIconType iconType;

    private void OnRightClick()
    {
        if (iconType == MapIconType.Mark)
            MapManager.Instance.RemoveMapIcon(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) onClick?.Invoke();
        if (eventData.button == PointerEventData.InputButton.Right) OnRightClick();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (pressCoroutine != null) StopCoroutine(pressCoroutine);
            pressCoroutine = StartCoroutine(Press());
        }
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
    }

    private void Awake()
    {
        iconImage = transform.Find("Icon").GetComponent<Image>();
        iconRange = transform.Find("Range").GetComponent<Image>();
        if (iconRange) iconRange.raycastTarget = false;
    }

#if UNITY_ANDROID
    readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    Coroutine pressCoroutine;
    IEnumerator Press()
    {
        float touchTime = 0;
        bool isPress = true;
        while (isPress)
        {
            touchTime += Time.fixedDeltaTime;
            if (touchTime >= 0.5f)
            {
                OnRightClick();
                yield break;
            }
            yield return WaitForFixedUpdate;
        }
    }
#endif
}
public enum MapIconType
{
    Normal,
    Main,
    Mark,
    Quest,
}