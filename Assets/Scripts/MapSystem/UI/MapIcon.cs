using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class MapIcon : MonoBehaviour, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public Image iconImage;

    [HideInInspector]
    public MapIconRange iconRange;

    public CanvasGroup ImageCanvas { get; private set; }

    [HideInInspector]
    public MapIconType iconType;

    private bool forceHided;
    public bool ForceHided => holder ? holder.forceHided : forceHided;

    private bool removeAble;
    public bool RemoveAble
    {
        get
        {
            return holder ? holder.removeAble : removeAble;
        }

        set
        {
            removeAble = value;
        }
    }

    private string textToDisplay;
    public string TextToDisplay
    {
        get
        {
            return holder ? holder.textToDisplay : textToDisplay;
        }

        set
        {
            textToDisplay = value;
        }
    }

    //[HideInInspector]
    public MapIconHolder holder;

    public void Show(bool showRange)
    {
        if (ForceHided) return;
        ZetanUtility.SetActive(iconImage.gameObject, true);
        if (iconRange) ZetanUtility.SetActive(iconRange.gameObject, showRange);
    }
    public void Hide()
    {
        ZetanUtility.SetActive(iconImage.gameObject, false);
        if (iconRange) ZetanUtility.SetActive(iconRange.gameObject, false);
    }

    public void Recycle()
    {
        if (ObjectPool.Instance)
        {
            if (holder)
            {
                holder.iconInstance = null;
                holder = null;
            }
            iconImage.raycastTarget = true;
            RemoveAble = true;
            if (!string.IsNullOrEmpty(TextToDisplay)) TipsManager.Instance.Hide();
            TextToDisplay = string.Empty;
            if (iconRange) ObjectPool.Instance.Put(iconRange.gameObject);
            iconRange = null;
            ObjectPool.Instance.Put(gameObject);
        }
        else
        {
            if (iconRange) DestroyImmediate(iconRange.gameObject);
            if (this) DestroyImmediate(gameObject);
        }
    }

    private void OnRightClick()
    {
        if (MapManager.Instance) MapManager.Instance.RemoveMapIcon(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
#if UNITY_STANDALONE
            if (holder) holder.OnMouseClick?.Invoke();
#elif UNITY_ANDROID
            if (holder) holder.OnFingerClick?.Invoke();
            if (!string.IsNullOrEmpty(TextToDisplay)) TipsManager.Instance.ShowText(transform.position, TextToDisplay, 2);
#endif
        }
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
#if UNITY_STANDALONE
        if (holder) holder.OnMouseEnter?.Invoke();
        if(!string.IsNullOrEmpty(textToDisplay)) TipsManager.Instance.ShowText(transform.position, textToDisplay);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
#if UNITY_STANDALONE
        if (holder) holder.OnMouseExit?.Invoke();
        if(!string.IsNullOrEmpty(textToDisplay)) TipsManager.Instance.HideText();
#endif
    }

    private void Awake()
    {
        iconImage = GetComponent<Image>();
        ImageCanvas = iconImage.GetComponent<CanvasGroup>();
        if (!ImageCanvas) ImageCanvas = iconImage.gameObject.AddComponent<CanvasGroup>();
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
    Objective
}