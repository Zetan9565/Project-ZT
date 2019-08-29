using UnityEngine;
using System.Collections.Generic;
using System;

[DisallowMultipleComponent]
public class MapManager : SingletonMonoBehaviour<MapManager>
{
    [SerializeField]
    private MapUI UI;

    [SerializeField]
    private UpdateMode updateMode;

    [SerializeField]
    private Transform player;
    [SerializeField]
    private Sprite playerIcon;
    [SerializeField]
    private Vector2 playerIconSize = new Vector2(64, 64);
    private MapIcon playerIconInsatance;
    [SerializeField]
    private Sprite defaultMarkIcon;
    [SerializeField]
    private Vector2 defaultMarkSize = new Vector2(64, 64);

    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private RenderTexture targetTexture;
    [SerializeField]
    private LayerMask mapRenderMask = ~0;

    [SerializeField]
    private bool use2D = true;

    [SerializeField, Tooltip("否则旋转图标。")]
    private bool rotateMap;

    [SerializeField]
    private bool circle;
    [SerializeField, Tooltip("此值为地图Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
    private float edgeSize;
    [SerializeField, Tooltip("此值为地图Rect宽度、高度两者中较小值的倍数。"), Range(0.5f, 1)]
    private float radius = 1;

    [SerializeField, Tooltip("此值为地图Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
    private float worldEdgeSize;
    [SerializeField]
    private bool isViewingWorldMap;
    [SerializeField]
    private float dragSensitivity = 0.135f;

    [SerializeField, Tooltip("小于等于 0 时表示不动画。")]
    private float animationSpeed = 5;

    private bool AnimateAble => animationSpeed > 0 && miniModeInfo.mapAnchoreMax == worldModeInfo.mapAnchoreMax && miniModeInfo.mapAnchoreMin == worldModeInfo.mapAnchoreMin
        && miniModeInfo.windowAnchoreMax == worldModeInfo.windowAnchoreMax && miniModeInfo.windowAnchoreMin == worldModeInfo.windowAnchoreMin;

    private bool isSwitching;
    private float switchTime;
    private float startSizeOfCamForMap;
    private Vector3 startPosOfCamForMap;
    private Vector2 startPositionOfMap;
    private Vector2 startSizeOfMapWindow;
    private Vector2 startSizeOfMap;

    [SerializeField]
    private MapModeInfo miniModeInfo = new MapModeInfo();

    [SerializeField]
    private MapModeInfo worldModeInfo = new MapModeInfo();

    private readonly Dictionary<MapIconHolder, MapIcon> iconsWithHolder = new Dictionary<MapIconHolder, MapIcon>();
    private readonly List<MapIconWithoutHolder> iconsWithoutHolder = new List<MapIconWithoutHolder>();

    #region 地图图标相关
    public MapIcon CreateMapIcon(MapIconHolder holder)
    {
        if (!UI || !UI.gameObject) return null;
        MapIcon icon = ObjectPool.Instance.Get(UI.iconPrefb.gameObject, UI.iconsParent).GetComponent<MapIcon>();
        icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        icon.iconImage.overrideSprite = holder.icon;
        icon.iconImage.rectTransform.sizeDelta = holder.iconSize;
        holder.iconInstance = icon;
        iconsWithHolder.TryGetValue(holder, out MapIcon iconFound);
        if (iconFound != null) holder.iconInstance = icon;
        else iconsWithHolder.Add(holder, icon);
        return icon;
    }
    public MapIcon CearteMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap)
    {
        if (!UI || !UI.gameObject) return null;
        MapIcon icon = ObjectPool.Instance.Get(UI.iconPrefb.gameObject, UI.iconsParent).GetComponent<MapIcon>();
        icon.iconImage.overrideSprite = iconSprite;
        icon.iconImage.rectTransform.sizeDelta = size;
        iconsWithoutHolder.Add(new MapIconWithoutHolder(worldPosition, icon, keepOnMap));
        return icon;
    }
    public MapIcon CreateMark(Vector3 worldPosition, bool keepOnMap)
    {
        return CearteMapIcon(defaultMarkIcon, defaultMarkSize, worldPosition, keepOnMap);
    }
    public MapIcon CreateMarkByMousePosition(Vector3 mousePosition)
    {
        return CreateMark(MapPointToWorldPoint(mousePosition), true);
    }

    public void RemoveMapIcon(MapIconHolder holder)
    {
        if (!holder) return;
        holder.iconInstance = null;
        iconsWithHolder.TryGetValue(holder, out MapIcon iconFound);
        if (iconFound != null)
        {
            if (ObjectPool.Instance) ObjectPool.Instance.Put(iconFound.gameObject);
            iconsWithHolder.Remove(holder);
        }
    }
    public void RemoveMapIcon(MapIcon icon)
    {
        iconsWithoutHolder.RemoveAll(x => x.mapIcon == icon);
        ObjectPool.Instance.Put(icon.gameObject);
    }
    public void RemoveMapIcon(Vector3 worldPosition)
    {
        foreach (var icon in iconsWithoutHolder)
        {
            if (icon.worldPosition == worldPosition)
                ObjectPool.Instance.Put(icon.mapIcon.gameObject);
        }
        iconsWithoutHolder.RemoveAll(x => x.worldPosition == worldPosition);
    }

    private void DrawMapIcons()
    {
        if (!UI || !UI.gameObject) return;
        camera.orthographic = true;
        camera.tag = "MapCamera";
        camera.cullingMask = mapRenderMask;
        foreach (var iconKvp in iconsWithHolder)
            if (!iconKvp.Key.forceHided && (isViewingWorldMap && iconKvp.Key.drawOnWorldMap || !isViewingWorldMap && (iconKvp.Key.maxValidDistance <= 0
                || iconKvp.Key.maxValidDistance > 0 && iconKvp.Key.distanceSqr >= Vector3.SqrMagnitude(iconKvp.Key.transform.position - player.position))))
            {
                iconKvp.Key.ShowIcon();
                DrawMapIcon(iconKvp.Key.transform.position, iconKvp.Value.transform, iconKvp.Key.keepOnMap);
            }
            else iconKvp.Key.HideIcon();
        foreach (var icon in iconsWithoutHolder)
            DrawMapIcon(icon.worldPosition, icon.mapIcon.transform, icon.keepOnMap);
    }
    private void DrawMapIcon(Vector3 worldPosition, Transform iconTrans, bool keepOnMap)
    {
        if (!UI || !UI.gameObject) return;
        //把相机视野内的世界坐标归一化为一个裁剪正方体中的坐标，其边长为1，就是说所有视野内的坐标都变成了x、z、y分量都在(0,1)以内的裁剪坐标
        //（图形学基础，不知所云的读者得加强一下）
        Vector3 viewportPoint = camera.WorldToViewportPoint(worldPosition);
        //这一步用于修正UI因设备分辨率不一样而进行缩放后实际Rect信息变了从而产生的问题
        Rect screenSpaceRect = ZetanUtilities.GetScreenSpaceRect(UI.mapRect);
        Vector3[] corners = new Vector3[4];
        UI.mapRect.GetWorldCorners(corners);
        //获取四个顶点的位置，顶点序号
        //  1 ┏━┓ 2
        //  0 ┗━┛ 3
        //根据归一化的裁剪坐标，转化为相对于地图的坐标
        Vector3 screenPos = new Vector3(viewportPoint.x * screenSpaceRect.width + corners[0].x, viewportPoint.y * screenSpaceRect.height + corners[0].y, 0);
        if (keepOnMap)
        {
            //以窗口的Rect为范围基准而不是地图的
            screenSpaceRect = ZetanUtilities.GetScreenSpaceRect(UI.mapWindowRect);
            float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) / 2;//地图的一半尺寸
            UI.mapWindowRect.GetWorldCorners(corners);
            if (circle && !isViewingWorldMap)
            {
                //以下不使用UI.mapWindowRect.position，是因为该position值会受轴心(UI.mapWindowRect.pivot)位置的影响而使得最后的结果出现偏移
                Vector3 realCenter = ZetanUtilities.CenterBetween(corners[0], corners[2]);
                Vector3 positionOffset = Vector3.ClampMagnitude(screenPos - realCenter, radius * size);
                screenPos = realCenter + positionOffset;
            }
            else
            {
                float edgeSize = (isViewingWorldMap ? worldEdgeSize : this.edgeSize) * size;
                screenPos.x = Mathf.Clamp(screenPos.x, corners[0].x + edgeSize, corners[2].x - edgeSize);
                screenPos.y = Mathf.Clamp(screenPos.y, corners[0].y + edgeSize, corners[1].y - edgeSize);
            }
        }
        iconTrans.position = screenPos;
    }

    private void FollowPlayer()
    {
        if (!player || !playerIconInsatance) return;
        DrawMapIcon(isViewingWorldMap ? player.position : camera.transform.position, playerIconInsatance.transform, true);
        playerIconInsatance.transform.SetSiblingIndex(playerIconInsatance.transform.childCount - 1);
        if (!rotateMap)
        {
            if (use2D)
                playerIconInsatance.transform.eulerAngles = new Vector3(playerIconInsatance.transform.eulerAngles.x, playerIconInsatance.transform.eulerAngles.y, player.eulerAngles.z);
            else
                playerIconInsatance.transform.eulerAngles = new Vector3(playerIconInsatance.transform.eulerAngles.x, player.eulerAngles.y, playerIconInsatance.transform.eulerAngles.z);
        }
        else
        {
            if (use2D) camera.transform.eulerAngles = new Vector3(0, 0, player.eulerAngles.z);
            else camera.transform.eulerAngles = new Vector3(camera.transform.eulerAngles.x, player.eulerAngles.y, camera.transform.eulerAngles.z);
        }
        Vector3 newCamPos = new Vector3(player.position.x, use2D ? player.position.y : camera.transform.position.y, use2D ? camera.transform.position.z : player.position.z);
        if (!isViewingWorldMap && !isSwitching) camera.transform.position = newCamPos;
    }

    public void Test()
    {
        UnityEditor.Undo.RecordObject(this, "Undo sososos");
        tag = "NPC";
    }

    public Vector3 MapPointToWorldPoint(Vector3 mousePosition)
    {
        Rect screenSpaceRect = ZetanUtilities.GetScreenSpaceRect(UI.mapRect);
        Vector3[] corners = new Vector3[4];
        UI.mapRect.GetWorldCorners(corners);
        Vector2 viewportPoint = new Vector2((mousePosition.x - corners[0].x) / screenSpaceRect.width, (mousePosition.y - corners[0].y) / screenSpaceRect.height);
        Vector3 worldPosition = camera.ViewportToWorldPoint(viewportPoint);
        return use2D ? new Vector3(worldPosition.x, worldPosition.y) : worldPosition;
    }
    #endregion

    #region 地图切换相关
    public void SwitchMapMode()
    {
        if (!UI || !UI.gameObject) return;
        isViewingWorldMap = !isViewingWorldMap;
        if (!isViewingWorldMap)//从大向小切换
        {
            if (animationSpeed > 0)
            {
                UI.mapWindowRect.anchorMin = miniModeInfo.windowAnchoreMin;
                UI.mapWindowRect.anchorMax = miniModeInfo.windowAnchoreMax;
                UI.mapRect.anchorMin = miniModeInfo.mapAnchoreMin;
                UI.mapRect.anchorMax = miniModeInfo.mapAnchoreMax;
            }
            else ToMiniMap();
        }
        else
        {
            if (animationSpeed > 0)
            {
                UI.mapWindowRect.anchorMin = worldModeInfo.windowAnchoreMin;
                UI.mapWindowRect.anchorMax = worldModeInfo.windowAnchoreMax;
                UI.mapRect.anchorMin = worldModeInfo.mapAnchoreMin;
                UI.mapRect.anchorMax = worldModeInfo.mapAnchoreMax;
            }
            else ToWorldMap();
        }
        if (animationSpeed > 0)
        {
            isSwitching = true;
            switchTime = 0;
            startSizeOfCamForMap = camera.orthographicSize;
            startPosOfCamForMap = camera.transform.position;
            startPositionOfMap = UI.mapWindowRect.anchoredPosition;
            startSizeOfMapWindow = UI.mapWindowRect.rect.size;
            startSizeOfMap = UI.mapRect.rect.size;
        }
    }
    private void AnimateSwitching()
    {
        if (!UI || !UI.gameObject || !AnimateAble) return;
        switchTime += Time.deltaTime * animationSpeed;
        if (isViewingWorldMap)//从小向大切换
        {
            if (camera.orthographicSize < worldModeInfo.sizeOfCam) AnimateTo(worldModeInfo);
            else ToWorldMap();
        }
        else
        {
            if (camera.orthographicSize > miniModeInfo.sizeOfCam)
            {
                Vector3 newCamPos = new Vector3(player.position.x, use2D ? player.position.y : camera.transform.position.y, use2D ? camera.transform.position.z : player.position.z);
                camera.transform.position = Vector3.Lerp(startPosOfCamForMap, newCamPos, switchTime);
                AnimateTo(miniModeInfo);
            }
            else ToMiniMap();
        }
    }
    private void AnimateTo(MapModeInfo modeInfo)
    {
        if (!UI || !UI.gameObject) return;
        camera.orthographicSize = Mathf.Lerp(startSizeOfCamForMap, modeInfo.sizeOfCam, switchTime);
        UI.mapWindowRect.anchoredPosition = Vector3.Lerp(startPositionOfMap, modeInfo.anchoredPosition, switchTime);
        UI.mapRect.sizeDelta = Vector2.Lerp(startSizeOfMap, modeInfo.sizeOfMap, switchTime);
        UI.mapWindowRect.sizeDelta = Vector2.Lerp(startSizeOfMapWindow, modeInfo.sizeOfWindow, switchTime);
    }

    public void ToMiniMap()
    {
        isSwitching = false;
        switchTime = 0;
        isViewingWorldMap = false;
        SetInfoFrom(miniModeInfo);
    }
    public void ToWorldMap()
    {
        isSwitching = false;
        switchTime = 0;
        isViewingWorldMap = true;
        SetInfoFrom(worldModeInfo);
    }
    private void SetInfoFrom(MapModeInfo modeInfo)
    {
        if (!UI || !UI.gameObject) return;
        camera.orthographicSize = modeInfo.sizeOfCam;
        UI.mapWindowRect.anchorMin = modeInfo.windowAnchoreMin;
        UI.mapWindowRect.anchorMax = modeInfo.windowAnchoreMax;
        UI.mapRect.anchorMin = modeInfo.mapAnchoreMin;
        UI.mapRect.anchorMax = modeInfo.mapAnchoreMax;
        UI.mapWindowRect.anchoredPosition = modeInfo.anchoredPosition;
        UI.mapRect.sizeDelta = modeInfo.sizeOfMap;
        UI.mapWindowRect.sizeDelta = modeInfo.sizeOfWindow;
    }

    public void SetCurrentAsMiniMap()
    {
        if (!UI || !UI.gameObject || isViewingWorldMap) return;
        if (camera) miniModeInfo.sizeOfCam = camera.orthographicSize;
        else Debug.LogError("地图相机不存在！");
        if (UI && UI.mapWindowRect) CopyInfoTo(miniModeInfo);
        else Debug.LogError("地图UI不存在或未编辑完整！");
    }
    public void SetCurrentAsWorldMap()
    {
        if (!UI || !UI.gameObject || !isViewingWorldMap) return;
        if (camera) worldModeInfo.sizeOfCam = camera.orthographicSize;
        else Debug.LogError("地图相机不存在！");
        if (UI && UI.mapWindowRect) CopyInfoTo(worldModeInfo);
        else Debug.LogError("地图UI不存在或未编辑完整！");
    }
    private void CopyInfoTo(MapModeInfo modeInfo)
    {
        if (!UI || !UI.gameObject) return;
        modeInfo.windowAnchoreMin = UI.mapWindowRect.anchorMin;
        modeInfo.windowAnchoreMax = UI.mapWindowRect.anchorMax;
        modeInfo.mapAnchoreMin = UI.mapRect.anchorMin;
        modeInfo.mapAnchoreMax = UI.mapRect.anchorMax;
        modeInfo.anchoredPosition = UI.mapWindowRect.anchoredPosition;
        modeInfo.sizeOfWindow = UI.mapWindowRect.sizeDelta;
        modeInfo.sizeOfMap = UI.mapRect.sizeDelta;
    }

    public void DragWorldMap(Vector2 dir)
    {
        float mag = new Vector2(Screen.width, Screen.height).magnitude;
        dir = new Vector2(dir.x * 1000 / mag, dir.y * 1000 / mag);
        if (isViewingWorldMap)
            camera.transform.Translate(new Vector3(dir.x, use2D ? dir.y : 0, use2D ? 0 : dir.y) * -dragSensitivity / (Application.platform == RuntimePlatform.Android ? 2 : 1));
    }
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        playerIconInsatance = ObjectPool.Instance.Get(UI.iconPrefb.gameObject, UI.iconsParent).GetComponent<MapIcon>();
        playerIconInsatance.iconImage.overrideSprite = playerIcon;
        playerIconInsatance.iconImage.rectTransform.sizeDelta = playerIconSize;
        camera.targetTexture = targetTexture;
        UI.mapImage.texture = targetTexture;
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update) DrawMapIcons();
        if (isSwitching) AnimateSwitching();
    }
    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate) DrawMapIcons();
    }
    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate) DrawMapIcons();
        FollowPlayer();//放在FixedUpdate()可以有效防止主图标抖动
    }

    private void OnDrawGizmos()
    {
        if (!UI || !UI.gameObject || !UI.mapWindowRect) return;
        Rect screenSpaceRect = ZetanUtilities.GetScreenSpaceRect(UI.mapWindowRect);
        Vector3[] corners = new Vector3[4];
        UI.mapWindowRect.GetWorldCorners(corners);
        if (circle && !isViewingWorldMap)
        {
            float radius = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) / 2 * this.radius;
            ZetanUtilities.DrawGizmosCircle(ZetanUtilities.CenterBetween(corners[0], corners[2]), radius, radius / 1000, Color.white, false);
        }
        else
        {
            float edgeSize = isViewingWorldMap ? worldEdgeSize : this.edgeSize;
            Vector3 size = new Vector3(screenSpaceRect.width - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height),
                screenSpaceRect.height - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height), 0);
            Gizmos.DrawWireCube(ZetanUtilities.CenterBetween(corners[0], corners[2]), size);
        }
    }
    #endregion

    private class MapIconWithoutHolder
    {
        public Vector3 worldPosition;
        public MapIcon mapIcon;
        public bool keepOnMap;

        public MapIconWithoutHolder(Vector3 worldPosition, MapIcon mapIcon, bool keepOnMap)
        {
            this.worldPosition = worldPosition;
            this.mapIcon = mapIcon;
            this.keepOnMap = keepOnMap;
        }
    }

    [System.Serializable]
    public class MapModeInfo
    {
        public float sizeOfCam;
        public Vector2 windowAnchoreMin;
        public Vector2 windowAnchoreMax;
        public Vector2 mapAnchoreMin;
        public Vector2 mapAnchoreMax;
        public Vector2 anchoredPosition;
        public Vector2 sizeOfWindow;
        public Vector2 sizeOfMap;
    }
}