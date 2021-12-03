using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[AddComponentMenu("Zetan Studio/地图图标生成器")]
public class MapIconHolder : MonoBehaviour
{
    public Sprite icon;

    public Vector2 iconSize = new Vector2(48, 48);

    public Vector2 offset;

    public bool drawOnWorldMap = true;

    public bool keepOnMap = true;

    [SerializeField, Tooltip("小于零时表示显示状态不受距离影响。")]
    public float maxValidDistance = -1;

    public bool forceHided;

    public bool removeAble;

    public bool showRange;

    public Color rangeColor = new Color(1, 1, 1, 0.5f);

    public float rangeSize = 144;

    public MapIconType iconType;

    public MapIcon iconInstance;

    public bool gizmos = true;

    public bool AutoHide => maxValidDistance > 0;

    public string textToDisplay;

    public MapIconEvents iconEvents;

    public UnityEvent OnFingerClick => iconEvents.onFingerClick;
    public UnityEvent OnMouseClick => iconEvents.onMouseClick;

    public UnityEvent OnMouseEnter => iconEvents.onMouseEnter;
    public UnityEvent OnMouseExit => iconEvents.onMouseExit;

    public void CreateIcon()
    {
        if (MapManager.Instance)
        {
            if (iconInstance && iconInstance.gameObject)
            {
                //Debug.Log(gameObject.name + " remove");
                MapManager.Instance.RemoveMapIcon(this, true);
            }
            MapManager.Instance.CreateMapIcon(this);
            //Debug.Log(gameObject.name);
        }
    }

    public void ShowIcon(float zoom)
    {
        if (forceHided) return;
        if (iconInstance)
        {
            iconInstance.Show(showRange);
            if (iconInstance.iconRange)
                if (showRange)
                {
                    if (iconInstance.iconRange)
                    {
                        if (iconInstance.iconRange.Color != rangeColor) iconInstance.iconRange.Color = rangeColor;
                        iconInstance.iconRange.rectTransform.sizeDelta = new Vector2(rangeSize * 2, rangeSize * 2) * zoom;
                    }
                }
                else ZetanUtility.SetActive(iconInstance.iconRange.gameObject, false);
        }
    }
    public void HideIcon()
    {
        if (iconInstance) iconInstance.Hide();
    }

    readonly WaitForSeconds WaitForSeconds = new WaitForSeconds(0.5f);
    private IEnumerator UpdateIcon()
    {
        while (true)
        {
            if (iconInstance)
            {
                if (iconInstance.iconImage.overrideSprite != icon) iconInstance.iconImage.overrideSprite = icon;
                if (iconInstance.iconImage.rectTransform.rect.size != iconSize) iconInstance.iconImage.rectTransform.sizeDelta = iconSize;
                iconInstance.iconType = iconType;
                yield return WaitForSeconds;
            }
            else yield return new WaitUntil(() => iconInstance);
        }
    }

    #region MonoBehaviour
    void Start()
    {
        CreateIcon();
    }

    private void Awake()
    {
        StartCoroutine(UpdateIcon());
    }

    private void OnDrawGizmosSelected()
    {
        if (gizmos && MapManager.Instance && !Application.isPlaying)
        {
            if (MapManager.Instance.MapMaskRect)
            {
                var rect = ZetanUtility.GetScreenSpaceRect(MapManager.Instance.MapMaskRect);
                Gizmos.DrawCube(MapManager.Instance.MapMaskRect.position, iconSize * rect.width / MapManager.Instance.MapMaskRect.rect.width);
                if (showRange)
                    ZetanUtility.DrawGizmosCircle(MapManager.Instance.MapMaskRect.position, rangeSize * rect.width / MapManager.Instance.MapMaskRect.rect.width,
                        Vector3.forward, rangeColor);
            }
        }
    }

    private void OnDestroy()
    {
        if (MapManager.Instance) MapManager.Instance.RemoveMapIcon(this, true);
    }
    #endregion
}
[System.Serializable]
public class MapIconEvents
{
    public UnityEvent onFingerClick = new UnityEvent();
    public UnityEvent onMouseClick = new UnityEvent();

    public UnityEvent onMouseEnter = new UnityEvent();
    public UnityEvent onMouseExit = new UnityEvent();

    public void RemoveAllListner()
    {
        onFingerClick.RemoveAllListeners();
        onMouseClick.RemoveAllListeners();
        onMouseEnter.RemoveAllListeners();
        onMouseExit.RemoveAllListeners();
    }
}