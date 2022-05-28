using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZetanStudio.Extension;

[RequireComponent(typeof(RectTransform))]
public class MapIcon : MonoBehaviour, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public new Transform transform { get; private set; }

    public Image iconImage;

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
    }

    private string textToDisplay;
    public string TextToDisplay
    {
        get
        {
            return holder ? holder.textToDisplay : textToDisplay;
        }
    }

    [HideInInspector]
    public MapIconHolder holder;

    private Vector3 position;
    public Vector3 Position => holder ? holder.transform.position : position;

    private bool keepOnMap;
    public bool KeepOnMap => holder ? holder.keepOnMap : keepOnMap;

    public RectTransform rectTransform { get; private set; }

    private FloatTipsPanel tips;

    public void Init(MapIconHolder holder)
    {
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = holder.iconSize;
        iconImage.overrideSprite = holder.icon;
        iconType = holder.iconType;
        holder.iconInstance = this;
        this.holder = holder;
        if (holder.showRange) ZetanUtility.SetActive(iconRange, true);
        else ZetanUtility.SetActive(iconRange, false);
    }

    public void Init(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap,
        MapIconType iconType, bool removeAble, string textToDisplay = null)
    {
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        iconImage.overrideSprite = iconSprite;
        this.iconType = iconType;
        ZetanUtility.SetActive(iconRange.gameObject, false);
        position = worldPosition;
        this.keepOnMap = keepOnMap;
        this.removeAble = removeAble;
        this.textToDisplay = textToDisplay;
    }

    public void Init(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap, float rangeSize,
        MapIconType iconType, bool removeAble, string textToDisplay = null)
    {
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        iconImage.overrideSprite = iconSprite;
        this.iconType = iconType;
        if (rangeSize > 0)
        {
            ZetanUtility.SetActive(iconRange.gameObject, true);
            iconRange.rectTransform.sizeDelta = new Vector2(rangeSize, rangeSize);
        }
        else ZetanUtility.SetActive(iconRange.gameObject, false);
        position = worldPosition;
        this.keepOnMap = keepOnMap;
        this.removeAble = removeAble;
        this.textToDisplay = textToDisplay;
    }

    public void Show(bool showRange = false)
    {
        if (ForceHided) return;
        ZetanUtility.SetActive(iconImage.gameObject, true);
        ZetanUtility.SetActive(iconRange.gameObject, showRange);
    }
    public void Hide()
    {
        ZetanUtility.SetActive(iconImage.gameObject, false);
        if (iconRange) ZetanUtility.SetActive(iconRange.gameObject, false);
    }

    public void Recycle()
    {
        if (holder)
        {
            holder.iconInstance = null;
            holder = null;
        }
        iconImage.raycastTarget = true;
        removeAble = true;
        if (!string.IsNullOrEmpty(TextToDisplay) && tips && tips.openBy is MapIcon icon && icon == this) tips.Close();
        textToDisplay = string.Empty;
        ObjectPool.Put(gameObject);
    }

    private void OnRightClick()
    {
        if (MapManager.Instance) MapManager.Instance.RemoveMapIcon(this, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
#if UNITY_STANDALONE
            if (holder) holder.OnMouseClick?.Invoke();
#elif UNITY_ANDROID
            if (holder) holder.OnFingerClick?.Invoke();
            if (!string.IsNullOrEmpty(TextToDisplay))
            {
                tips = WindowsManager.OpenWindowBy<FloatTipsPanel>(transform.position, TextToDisplay, 2, false);
                if (tips) tips.onClose += () => tips = null;
            }
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
        if(!string.IsNullOrEmpty(textToDisplay)) TipsManager.Instance.Hide();
#endif
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ImageCanvas = this.GetOrAddComponent<CanvasGroup>();
        if (!ImageCanvas) ImageCanvas = iconImage.gameObject.AddComponent<CanvasGroup>();
        transform = base.transform;
    }

    public void UpdatePosition(Vector3 worldPosition)
    {
        position = worldPosition;
    }

#if UNITY_ANDROID
    readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    Coroutine pressCoroutine;
    internal object worldPosition;

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