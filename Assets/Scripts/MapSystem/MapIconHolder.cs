using UnityEngine;
using System.Collections;

public class MapIconHolder : MonoBehaviour
{
    public Sprite icon;

    public Vector2 iconSize = new Vector2(48, 48);

    public bool drawOnWorldMap = true;

    public bool keepOnMap = true;

    [SerializeField, Tooltip("小于零时表示显示状态不受距离影响。游戏运行时修改无效。")]
    private float maxValidDistance = -1;

    [HideInInspector]
    public float distanceSqr;

    public bool forceHided;

    public bool showRange;

    public Color rangeColor = new Color(1, 1, 1, 0.5f);

    public float rangeSize = 144;

    public MapIconType iconType;

    public MapIcon iconInstance;

    public bool AutoHide => maxValidDistance > 0;

    private void Awake()
    {
        distanceSqr = maxValidDistance * maxValidDistance;
        StartCoroutine(UpdateSizeAndColor());
    }

    void Start()
    {
        if (MapManager.Instance) MapManager.Instance.CreateMapIcon(this);
    }

    public void SetIconValidDistance(float distance)
    {
        maxValidDistance = distance;
        distanceSqr = maxValidDistance * maxValidDistance;
    }

    public void ShowIcon(float zoom)
    {
        if (forceHided) return;
        if (iconInstance)
        {
            if (iconInstance.iconImage) ZetanUtil.SetActive(iconInstance.iconImage.gameObject, true);
            if (iconInstance.iconRange)
                if (showRange)
                {
                    ZetanUtil.SetActive(iconInstance.iconRange.gameObject, true);
                    iconInstance.iconRange.color = rangeColor;
                    if (iconInstance.iconRange) iconInstance.iconRange.rectTransform.sizeDelta = new Vector2(rangeSize, rangeSize) * zoom;
                }
                else ZetanUtil.SetActive(iconInstance.iconRange.gameObject, false);
        }
    }
    public void HideIcon()
    {
        if (iconInstance)
        {
            if (iconInstance.iconImage) ZetanUtil.SetActive(iconInstance.iconImage.gameObject, false);
            if (iconInstance.iconRange) ZetanUtil.SetActive(iconInstance.iconRange.gameObject, false);
        }
    }

    readonly WaitForSeconds WaitForSeconds = new WaitForSeconds(0.2f);
    private IEnumerator UpdateSizeAndColor()
    {
        while (true)
        {
            if (iconInstance)
            {
                iconInstance.iconImage.overrideSprite = icon;
                iconInstance.iconImage.rectTransform.sizeDelta = iconSize;
                iconInstance.iconType = iconType;
                yield return WaitForSeconds;
            }
            else yield return new WaitUntil(() => iconInstance);
        }
    }

    private void OnDestroy()
    {
        if (MapManager.Instance) MapManager.Instance.RemoveMapIcon(this);
    }
}
