using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZetanStudio.Extension;
using ZetanStudio;
using ZetanStudio.UI;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System;

[RequireComponent(typeof(RectTransform))]
public class MapIcon : MonoBehaviour, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public new Transform transform { get; private set; }

    public Image iconImage;

    public CanvasGroup ImageCanvas { get; private set; }

    public RectTransform rectTransform { get; private set; }

    private FloatTipsPanel tips;
    private MapIconData data;

    public void Init(MapIconHolder holder)
    {
        Init(holder.iconInstance, holder.icon, holder.iconSize);
    }

    public void Init(MapIconData data, Sprite iconSprite, Vector2 size)
    {
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        iconImage.overrideSprite = iconSprite;
        this.data = data;
    }

    public void Show()
    {
        if (data.active) Utility.SetActive(iconImage.gameObject, true);
    }
    public void Hide()
    {
        Utility.SetActive(iconImage.gameObject, false);
    }

    public void Recycle()
    {
        iconImage.raycastTarget = true;
        if (!string.IsNullOrEmpty(data.TextToDisplay) && tips && tips.openBy is MapIcon icon && icon == this) tips.Close();
        data = null;
        ObjectPool.Put(gameObject);
    }

    private void OnRightClick()
    {
        if (MapManager.Instance) MapManager.Instance.RemoveMapIcon(data, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
#if UNITY_STANDALONE
            if (holder) holder.OnMouseClick?.Invoke();
#elif UNITY_ANDROID
            if (data.holder) data.holder.OnFingerClick?.Invoke();
            if (!string.IsNullOrEmpty(data.TextToDisplay))
            {
                tips = WindowsManager.OpenWindowBy<FloatTipsPanel>(transform.position, data.TextToDisplay, 2, false);
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
        if(!string.IsNullOrEmpty(textToDisplay)) FloatTipsPanel.ShowText(transform.position, textToDisplay);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
#if UNITY_STANDALONE
        if (holder) holder.OnMouseExit?.Invoke();
        if(!string.IsNullOrEmpty(textToDisplay)) WindowsManager.CloseWindow<FloatTipsPanel>();
#endif
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ImageCanvas = this.GetOrAddComponent<CanvasGroup>();
        if (!ImageCanvas) ImageCanvas = iconImage.gameObject.AddComponent<CanvasGroup>();
        transform = base.transform;
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

public class MapIconData
{
    public bool active = true;

    public MapIconType iconType;

#pragma warning disable IDE0044 // 添加只读修饰符
    private bool forceHided;
#pragma warning restore IDE0044 // 添加只读修饰符
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

    public MapIconHolder holder;

    private Vector3 position;
    private readonly Sprite icon;
    private readonly Vector2 size;

    public Vector3 Position => holder ? holder.transform.position : position;

    private bool keepOnMap;
    public bool KeepOnMap => holder ? holder.keepOnMap : keepOnMap;

    private float rangeSize;
    private readonly Color? rangeColor;

    public float RangeSize => holder ? holder.rangeSize : rangeSize;

    public Dictionary<IMapUI, MapIconRange> ranges = new Dictionary<IMapUI, MapIconRange>();
    public Dictionary<IMapUI, MapIcon> entities = new Dictionary<IMapUI, MapIcon>();

    public void CollectRange(IMapUI UI, MapIconRange range)
    {
        if (UI == null || !range) return;
        ranges[UI] = range;
    }
    public MapIconRange GetRange(IMapUI UI)
    {
        if (ranges.TryGetValue(UI, out var range)) return range;
        return null;
    }
    public void CollectEntity(IMapUI UI, MapIcon entity)
    {
        if (UI == null || !entity) return;
        entities[UI] = entity;
    }

    public void UpdateAlpha(float alpha)
    {
        entities.ForEach(x => x.Value.ImageCanvas.alpha = alpha);
    }
    public void UpdateIcon(Sprite icon)
    {
        entities.ForEach(x => x.Value.iconImage.overrideSprite = icon);
    }
    public void UpdateRange(float radius, Color color)
    {
        Vector2 size = new Vector2(radius * 2, radius * 2);
        ranges.ForEach(x =>
        {
            if (x.Value)
            {
                x.Value.Color = color;
                if (x.Value.rectTransform.sizeDelta != size)
                    x.Value.rectTransform.sizeDelta = size;
            }
        });
    }
    public void UpdateRange(float radius)
    {
        Vector2 size = new Vector2(radius * 2, radius * 2);
        ranges.ForEach(x => { if (x.Value.rectTransform.sizeDelta != size) x.Value.rectTransform.sizeDelta = size; });
    }
    public void UpdateRange(Color color)
    {
        ranges.ForEach(x => x.Value.Color = color);
    }
    public void UpdateSize(Vector2 size)
    {
        entities.ForEach(x => { if (x.Value.rectTransform.sizeDelta != size) x.Value.rectTransform.sizeDelta = size; });
    }
    public void UpdatePosition(Vector3 worldPosition)
    {
        position = worldPosition;
    }

    public void SetClickable(bool enable)
    {
        entities.ForEach(x => x.Value.iconImage.raycastTarget = enable);
    }

    public void ShowRange(bool value)
    {
        if (value) ranges.ForEach(x => x.Value.Show(RangeSize));
        else ranges.ForEach(x => x.Value.Hide());
    }

    public void Show(bool showRange = false)
    {
        if (ForceHided) return;
        entities.ForEach(x => x.Value.Show());
        if (showRange) ranges.ForEach(x => x.Value.Show(RangeSize));
        else ranges.ForEach(x => x.Value.Hide());
    }
    public void Hide()
    {
        entities.ForEach(x => x.Value.Hide());
    }
    public void SetActive(bool active)
    {
        this.active = active;
        if (!active) Hide();
    }

    public MapIconData(MapIconHolder holder)
    {
        iconType = holder.iconType;
        holder.iconInstance = this;
        this.holder = holder;
    }

    public MapIconData(Vector3 worldPosition, Sprite icon, Vector2 size, bool keepOnMap, MapIconType iconType, bool removeAble, string textToDisplay = null)
    {
        this.iconType = iconType;
        position = worldPosition;
        this.icon = icon;
        this.size = size;
        this.keepOnMap = keepOnMap;
        this.removeAble = removeAble;
        this.textToDisplay = textToDisplay;
    }
    public MapIconData(Vector3 worldPosition, Sprite icon, Vector2 size, bool keepOnMap, MapIconType iconType, bool removeAble, float rangeSize, Color? rangeColor, string textToDisplay = null)
    {
        this.iconType = iconType;
        position = worldPosition;
        this.icon = icon;
        this.size = size;
        this.keepOnMap = keepOnMap;
        this.removeAble = removeAble;
        this.rangeSize = rangeSize;
        this.rangeColor = rangeColor;
        this.textToDisplay = textToDisplay;
    }
    public MapIconData(Vector3 worldPosition, bool keepOnMap, MapIconType iconType, bool removeAble, string textToDisplay = null)
    {
        this.iconType = iconType;
        position = worldPosition;
        this.keepOnMap = keepOnMap;
        this.removeAble = removeAble;
        this.textToDisplay = textToDisplay;
    }
    public MapIconData(Vector3 worldPosition, bool keepOnMap, MapIconType iconType, bool removeAble, float rangeSize, string textToDisplay = null)
    {
        this.iconType = iconType;
        position = worldPosition;
        this.keepOnMap = keepOnMap;
        this.removeAble = removeAble;
        this.rangeSize = rangeSize;
        this.textToDisplay = textToDisplay;
    }

    public void Recycle()
    {
        if (holder)
        {
            holder.iconInstance = null;
            holder = null;
        }
        removeAble = true;
        textToDisplay = string.Empty;
        entities.ForEach(x => { if (x.Value) x.Value.Recycle(); });
        ranges.ForEach(x => { if (x.Value) x.Value.Recycle(); });
        entities.Clear();
        ranges.Clear();
    }

    public void Destroy()
    {
        entities.ForEach(x => UnityEngine.Object.Destroy(x.Value));
        ranges.ForEach(x => UnityEngine.Object.Destroy(x.Value));
        entities.Clear();
        ranges.Clear();
    }

    public static implicit operator bool(MapIconData obj) => obj != null;
}

public enum MapIconType
{
    Normal,
    Main,
    Mark,
    Quest,
    Objective
}