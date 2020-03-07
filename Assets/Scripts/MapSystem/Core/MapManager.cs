using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/地图管理器")]
public class MapManager : SingletonMonoBehaviour<MapManager>
{
    [SerializeField]
    private MapUI UI;

    public RectTransform MapMaskRect => UI ? UI.mapMaskRect : null;

    [SerializeField]
    private UpdateMode updateMode;

    [SerializeField]
    private Transform player;
    [SerializeField]
    private Vector2 offset;
    [SerializeField]
    private Sprite playerIcon;
    [SerializeField]
    private Vector2 playerIconSize = new Vector2(64, 64);
    private MapIcon playerIconInstance;
    [SerializeField]
    private Sprite defaultMarkIcon;
    [SerializeField]
    private Vector2 defaultMarkSize = new Vector2(64, 64);

    [SerializeField]
    private MapCamera mapCamera;
    [SerializeField]
    private MapCamera cameraPrefab;
    public Camera Camera
    {
        get
        {
            if (!mapCamera) mapCamera = Instantiate(cameraPrefab, transform);
            return mapCamera.Camera;
        }

    }

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
    [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
    private float edgeSize;
    [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0.5f, 1)]
    private float radius = 1;

    [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
    private float worldEdgeSize;
    [SerializeField]
    private bool isViewingWorldMap;
    public bool IsViewingWorldMap => isViewingWorldMap;

    [SerializeField, Range(0.05f, 0.5f)]
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

    private bool isMovingCamera;
    private float cameraMovingTime;
    private Vector3 camMoveDestination;

    private Vector2 zoomLimit;

    [SerializeField]
    private MapModeInfo miniModeInfo = new MapModeInfo();

    [SerializeField]
    private MapModeInfo worldModeInfo = new MapModeInfo();

    private readonly Dictionary<MapIconHolder, MapIcon> iconsWithHolder = new Dictionary<MapIconHolder, MapIcon>();
    public Dictionary<MapIcon, MapIconWithoutHolder> IconsWithoutHolder { get; private set; } = new Dictionary<MapIcon, MapIconWithoutHolder>();

    #region 地图图标相关
    public void CreateMapIcon(MapIconHolder holder)
    {
        if (!UI || !UI.gameObject || !holder.icon) return;
        MapIcon icon = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(holder.iconType)).GetComponent<MapIcon>();
        InitIcon();
        iconsWithHolder.TryGetValue(holder, out MapIcon iconFound);
        if (iconFound != null)
        {
            holder.iconInstance = icon;
            iconsWithHolder[holder] = icon;
        }
        else iconsWithHolder.Add(holder, icon);
        return;

        void InitIcon()
        {
            icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            icon.iconImage.overrideSprite = holder.icon;
            icon.iconImage.rectTransform.sizeDelta = holder.iconSize;
            icon.iconType = holder.iconType;
            icon.RemoveAble = holder.removeAble;
            holder.iconInstance = icon;
            icon.holder = holder;
            if (holder.showRange) icon.iconRange = ObjectPool.Get(UI.rangePrefab.gameObject, UI.rangesParent).GetComponent<MapIconRange>();
        }
    }
    public MapIcon CreateMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap,
        MapIconType iconType, bool removeAble, string textToDisplay = "")
    {
        if (!UI || !UI.gameObject || !iconSprite) return null;
        MapIcon icon = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(iconType)).GetComponent<MapIcon>();
        InitIcon();
        IconsWithoutHolder.Add(icon, new MapIconWithoutHolder(worldPosition, icon, keepOnMap, removeAble, textToDisplay));
        return icon;

        void InitIcon()
        {
            icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            icon.iconImage.overrideSprite = iconSprite;
            icon.iconImage.rectTransform.sizeDelta = size;
            icon.iconType = iconType;
            icon.RemoveAble = removeAble;
            icon.TextToDisplay = textToDisplay;
        }
    }
    public MapIcon CreateMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap, float rangeSize,
        MapIconType iconType, bool removeAble, string textToDisplay = "")
    {
        if (!UI || !UI.gameObject || !iconSprite) return null;
        MapIcon icon = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(iconType)).GetComponent<MapIcon>();
        InitIcon();
        IconsWithoutHolder.Add(icon, new MapIconWithoutHolder(worldPosition, icon, keepOnMap, removeAble, textToDisplay));
        return icon;

        void InitIcon()
        {
            icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            icon.iconImage.overrideSprite = iconSprite;
            icon.iconImage.rectTransform.sizeDelta = size;
            icon.iconType = iconType;
            if (rangeSize > 0)
            {
                icon.iconRange = ObjectPool.Get(UI.rangePrefab.gameObject, UI.rangesParent).GetComponent<MapIconRange>();
                ZetanUtility.SetActive(icon.iconRange.gameObject, true);
                icon.iconRange.RectTransform.sizeDelta = new Vector2(rangeSize, rangeSize);
            }
            icon.RemoveAble = removeAble;
            icon.TextToDisplay = textToDisplay;
        }
    }

    public MapIcon CreateDefaultMark(Vector3 worldPosition, bool keepOnMap, bool removeAble, string textToDisplay = "")
    {
        return CreateMapIcon(defaultMarkIcon, defaultMarkSize, worldPosition, keepOnMap, MapIconType.Mark, removeAble, textToDisplay);
    }
    public MapIcon CreateDefaultMarkAtMousePos(Vector3 mousePosition)
    {
        return CreateDefaultMark(MapPointToWorldPoint(mousePosition), true, true);
    }

    public void RemoveMapIcon(MapIconHolder holder, bool force = false)
    {
        if (!holder || !holder.removeAble && !force) { Debug.Log("return1"); return; }
        //Debug.Log("remove");
        iconsWithHolder.TryGetValue(holder, out MapIcon iconFound);
        if (!iconFound && holder.iconInstance) iconFound = holder.iconInstance;
        if (iconFound) iconFound.Recycle();
        iconsWithHolder.Remove(holder);
    }
    public void RemoveMapIcon(MapIcon icon, bool force)
    {
        if (!icon || !icon.RemoveAble && !force) return;
        if (icon.holder) RemoveMapIcon(icon.holder);
        else
        {
            IconsWithoutHolder.Remove(icon);
            icon.Recycle();
        }
    }
    public void RemoveMapIcon(Vector3 worldPosition, bool force = false)
    {
        foreach (var iconWoH in IconsWithoutHolder.Values.ToList())
            if (iconWoH.worldPosition == worldPosition && (iconWoH.removeAble || force))
            {
                IconsWithoutHolder.Remove(iconWoH.mapIcon);
                if (iconWoH.mapIcon) iconWoH.mapIcon.Recycle();
                iconWoH.mapIcon = null;
            }
    }
    public void DestroyMapIcon(MapIcon icon)
    {
        if (!icon) return;
        if (icon.holder) RemoveMapIcon(icon.holder);
        else
        {
            if (!icon.holder) IconsWithoutHolder.Remove(icon);
            else iconsWithHolder.Remove(icon.holder);
            Destroy(icon);
        }
    }

    private void DrawMapIcons()
    {
        if (!UI || !UI.gameObject) return;
        if (!Camera.orthographic) Camera.orthographic = true;
        if (!Camera.CompareTag("MapCamera")) Camera.tag = "MapCamera";
        if (Camera.cullingMask != mapRenderMask) Camera.cullingMask = mapRenderMask;
        foreach (var iconKvp in iconsWithHolder)
        {
            MapIconHolder holder = iconKvp.Key;
            float sqrDistance = Vector3.SqrMagnitude(holder.transform.position - player.position);
            if (!iconKvp.Value.ForceHided && (isViewingWorldMap && holder.drawOnWorldMap || !isViewingWorldMap && (!holder.AutoHide
               || holder.AutoHide && holder.DistanceSqr >= sqrDistance)))
            {
                if (holder.showRange && !iconKvp.Value.iconRange)
                    iconKvp.Value.iconRange = ObjectPool.Get(UI.rangePrefab.gameObject, UI.rangesParent).GetComponent<MapIconRange>();
                holder.ShowIcon(IsViewingWorldMap ? (worldModeInfo.currentSizeOfCam / Camera.orthographicSize) : (miniModeInfo.currentSizeOfCam / Camera.orthographicSize));
                DrawMapIcon(holder.transform.position + new Vector3(holder.offset.x, use2D ? holder.offset.y : 0, use2D ? 0 : holder.offset.y), iconKvp.Value, holder.keepOnMap);
                if (!IsViewingWorldMap && sqrDistance > holder.DistanceSqr * 0.81f && sqrDistance < holder.DistanceSqr)
                    iconKvp.Value.ImageCanvas.alpha = (holder.DistanceSqr - sqrDistance) / (holder.DistanceSqr * 0.19f);
                else iconKvp.Value.ImageCanvas.alpha = 1;
            }
            else holder.HideIcon();
        }
        foreach (var icon in IconsWithoutHolder.Values)
            DrawMapIcon(icon.worldPosition, icon.mapIcon, icon.keepOnMap);
    }
    private void DrawMapIcon(Vector3 worldPosition, MapIcon icon, bool keepOnMap)
    {
        if (!icon || !UI || !UI.gameObject) return;
        //把相机视野内的世界坐标归一化为一个裁剪正方体中的坐标，其边长为1，就是说所有视野内的坐标都变成了x、z、y分量都在(0,1)以内的裁剪坐标
        Vector3 viewportPoint = Camera.WorldToViewportPoint(worldPosition);
        //这一步用于修正UI因设备分辨率不一样，在进行缩放后实际Rect信息变了而产生的问题
        Rect screenSpaceRect = ZetanUtility.GetScreenSpaceRect(UI.mapRect);
        //获取四个顶点的位置，顶点序号
        //  1 ┏━┓ 2
        //  0 ┗━┛ 3
        Vector3[] corners = new Vector3[4];
        UI.mapRect.GetWorldCorners(corners);
        //根据归一化的裁剪坐标，转化为相对于地图的坐标
        Vector3 screenPos = new Vector3(viewportPoint.x * screenSpaceRect.width + corners[0].x, viewportPoint.y * screenSpaceRect.height + corners[0].y, 0);
        Vector3 rangePos = screenPos;
        if (keepOnMap)
        {
            //以遮罩的Rect为范围基准而不是地图的
            screenSpaceRect = ZetanUtility.GetScreenSpaceRect(MapMaskRect);
            float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f;//地图的一半尺寸
            UI.mapWindowRect.GetWorldCorners(corners);
            if (circle && !isViewingWorldMap)
            {
                //以下不使用UI.mapMaskRect.position，是因为该position值会受轴心(UI.mapMaskRect.pivot)位置的影响而使得最后的结果出现偏移
                Vector3 realCenter = ZetanUtility.CenterBetween(corners[0], corners[2]);
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
        icon.transform.position = screenPos;
        if (icon.iconRange) icon.iconRange.transform.position = rangePos;
    }

    private void InitPlayerIcon()
    {
        if (playerIconInstance) return;
        playerIconInstance = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(MapIconType.Main)).GetComponent<MapIcon>();
        playerIconInstance.iconImage.overrideSprite = playerIcon;
        playerIconInstance.iconImage.rectTransform.sizeDelta = playerIconSize;
        playerIconInstance.iconType = MapIconType.Main;
        playerIconInstance.iconImage.raycastTarget = false;
    }
    #endregion

    #region 地图切换相关
    public void SwitchMapMode()
    {
        if (!UI || !UI.gameObject) return;
        isViewingWorldMap = !isViewingWorldMap;
        isMovingCamera = false;
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
            startSizeOfCamForMap = Camera.orthographicSize;
            startPosOfCamForMap = Camera.transform.position;
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
            if (!IsAnimaComplete(true))
            {
                AnimateTo(worldModeInfo);
            }
            else ToWorldMap();
        }
        else
        {
            if (!IsAnimaComplete(false))
            {
                Vector3 newCamPos = new Vector3(player.position.x, use2D ? player.position.y : Camera.transform.position.y, use2D ? Camera.transform.position.z : player.position.z);
                Camera.transform.position = Vector3.Lerp(startPosOfCamForMap, newCamPos, switchTime);
                AnimateTo(miniModeInfo);
            }
            else ToMiniMap();
        }
    }
    private void AnimateTo(MapModeInfo modeInfo)
    {
        if (!UI || !UI.gameObject) return;
        Camera.orthographicSize = Mathf.Lerp(startSizeOfCamForMap, modeInfo.currentSizeOfCam, switchTime);
        UI.mapWindowRect.anchoredPosition = Vector3.Lerp(startPositionOfMap, modeInfo.anchoredPosition, switchTime);
        UI.mapRect.sizeDelta = Vector2.Lerp(startSizeOfMap, modeInfo.sizeOfMap, switchTime);
        UI.mapWindowRect.sizeDelta = Vector2.Lerp(startSizeOfMapWindow, modeInfo.sizeOfWindow, switchTime);
    }
    private bool IsAnimaComplete(bool toWorldMode)
    {
        if (toWorldMode) return UI.mapRect.sizeDelta.x >= worldModeInfo.sizeOfMap.x && UI.mapRect.sizeDelta.y >= worldModeInfo.sizeOfMap.y;
        else return UI.mapRect.sizeDelta.x <= miniModeInfo.sizeOfMap.x && UI.mapRect.sizeDelta.y <= miniModeInfo.sizeOfMap.y;
    }

    public void ToMiniMap()
    {
        isSwitching = false;
        isMovingCamera = false;
        switchTime = 0;
        cameraMovingTime = 0;
        isViewingWorldMap = false;
        SetInfoFrom(miniModeInfo);
    }
    public void ToWorldMap()
    {
        isSwitching = false;
        isMovingCamera = false;
        switchTime = 0;
        cameraMovingTime = 0;
        isViewingWorldMap = true;
        SetInfoFrom(worldModeInfo);
    }
    private void SetInfoFrom(MapModeInfo modeInfo)
    {
        if (!UI || !UI.gameObject) return;
        Camera.orthographicSize = modeInfo.currentSizeOfCam;
        zoomLimit.x = modeInfo.minZoomOfCam;
        zoomLimit.y = modeInfo.maxZoomOfCam;
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
        if (Camera)
        {
            miniModeInfo.defaultSizeOfCam = Camera.orthographicSize;
            miniModeInfo.currentSizeOfCam = Camera.orthographicSize;
        }
        else Debug.LogError("地图相机不存在！");
        try
        {
            CopyInfoTo(miniModeInfo);
        }
        catch
        {
            Debug.LogError("地图UI不存在或UI存在空缺！");
        }
    }
    public void SetCurrentAsWorldMap()
    {
        if (!UI || !UI.gameObject || !isViewingWorldMap) return;
        if (Camera)
        {
            worldModeInfo.defaultSizeOfCam = Camera.orthographicSize;
            worldModeInfo.currentSizeOfCam = Camera.orthographicSize;
        }
        else Debug.LogError("地图相机不存在！");
        try
        {
            CopyInfoTo(worldModeInfo);
        }
        catch
        {
            Debug.LogError("地图UI不存在或UI存在空缺！");
        }
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
    #endregion

    #region 相机相关
    public void SetPlayer(Transform player)
    {
        this.player = player;
    }
    public void RemakeCamera()
    {
        if (!cameraPrefab) return;
        mapCamera = Instantiate(cameraPrefab, transform);
        DontDestroyOnLoad(mapCamera);
    }
    private void FollowPlayer()
    {
        if (!player || !playerIconInstance) return;
        DrawMapIcon(isViewingWorldMap ? player.position : Camera.transform.position, playerIconInstance, true);
        playerIconInstance.transform.SetSiblingIndex(playerIconInstance.transform.childCount - 1);
        if (!rotateMap)
        {
            if (use2D)
            {
                playerIconInstance.transform.eulerAngles = new Vector3(playerIconInstance.transform.eulerAngles.x, playerIconInstance.transform.eulerAngles.y, player.eulerAngles.z);
                Camera.transform.eulerAngles = Vector3.zero;
            }
            else
            {
                playerIconInstance.transform.eulerAngles = new Vector3(playerIconInstance.transform.eulerAngles.x, playerIconInstance.transform.eulerAngles.y, -player.eulerAngles.y);
                Camera.transform.eulerAngles = Vector3.right * 90;
            }
        }
        else
        {
            if (use2D)
            {
                playerIconInstance.transform.eulerAngles = Vector3.zero;
                Camera.transform.eulerAngles = new Vector3(0, 0, player.eulerAngles.z);
            }
            else
            {
                playerIconInstance.transform.eulerAngles = Vector3.zero;
                Camera.transform.eulerAngles = new Vector3(Camera.transform.eulerAngles.x, player.eulerAngles.y, Camera.transform.eulerAngles.z);
            }
        }
        if (!isViewingWorldMap && !isSwitching && !isMovingCamera)
        {
            //2D模式，则跟随目标的Y坐标，相机的Z坐标不动；3D模式，则跟随目标的Z坐标，相机的Y坐标不动。
            Vector3 newCamPos = new Vector3(player.position.x + offset.x,
                use2D ? player.position.y + offset.y : Camera.transform.position.y,
                use2D ? Camera.transform.position.z : player.position.z + offset.y);
            Camera.transform.position = newCamPos;
        }
        playerIconInstance.transform.SetAsLastSibling();
    }
    public void LocatePlayer()
    {
        MoveCameraTo(player.position);
    }

    public void MoveCameraTo(Vector3 worldPosition)
    {
        if (isSwitching || !isViewingWorldMap) return;
        Vector3 newCamPos = new Vector3(worldPosition.x, use2D ? worldPosition.y : Camera.transform.position.y, use2D ? Camera.transform.position.z : worldPosition.z);
        startPosOfCamForMap = Camera.transform.position;
        camMoveDestination = newCamPos;
        isMovingCamera = true;
        cameraMovingTime = 0;
    }

    public Vector3 MapPointToWorldPoint(Vector3 mousePosition)
    {
        Rect screenSpaceRect = ZetanUtility.GetScreenSpaceRect(UI.mapRect);
        Vector3[] corners = new Vector3[4];
        UI.mapRect.GetWorldCorners(corners);
        Vector2 mapViewportPoint = new Vector2((mousePosition.x - corners[0].x) / screenSpaceRect.width, (mousePosition.y - corners[0].y) / screenSpaceRect.height);
        Vector3 worldPosition = Camera.ViewportToWorldPoint(mapViewportPoint);
        return use2D ? new Vector3(worldPosition.x, worldPosition.y) : worldPosition;
    }

    public void DragWorldMap(Vector2 direction)
    {
        if (!isViewingWorldMap || isSwitching) return;
        if (direction == default) return;
        isMovingCamera = false;
        cameraMovingTime = 0;
        float mag = new Vector2(Screen.width, Screen.height).magnitude;
        direction = new Vector2(direction.x * 1000 / mag, direction.y * 1000 / mag) * (Camera.orthographicSize / worldModeInfo.currentSizeOfCam);
        //direction = new Vector2(direction.x / Screen.dpi, direction.y / Screen.dpi) * (Camera.orthographicSize / worldModeInfo.currentSizeOfCam);
        Camera.transform.Translate(new Vector3(direction.x, use2D ? direction.y : 0, use2D ? 0 : direction.y) * dragSensitivity);
    }

    public void Zoom(float value)
    {
        if (isSwitching || value == 0) return;
        Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize - value, zoomLimit.x, zoomLimit.y);
        if (IsViewingWorldMap) worldModeInfo.currentSizeOfCam = Camera.orthographicSize;
        else miniModeInfo.currentSizeOfCam = Camera.orthographicSize;
    }
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update) DrawMapIcons();
        if (isSwitching) AnimateSwitching();
        if (isMovingCamera)
        {
            cameraMovingTime += Time.deltaTime * 5;
            if (camMoveDestination != Camera.transform.position)
                Camera.transform.position = Vector3.Lerp(startPosOfCamForMap, camMoveDestination, cameraMovingTime);
            else
            {
                isMovingCamera = false;
                cameraMovingTime = 0;
            }
        }
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
        if (!UI || !UI.gameObject || !MapMaskRect) return;
        Rect screenSpaceRect = ZetanUtility.GetScreenSpaceRect(MapMaskRect);
        Vector3[] corners = new Vector3[4];
        UI.mapMaskRect.GetWorldCorners(corners);
        if (circle && !isViewingWorldMap)
        {
            float radius = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f * this.radius;
            //ZetanUtility.DrawGizmosCircle(ZetanUtility.CenterBetween(corners[0], corners[2]), radius, radius / 1000, Color.white, false);
            ZetanUtility.DrawGizmosCircle(ZetanUtility.CenterBetween(corners[0], corners[2]), radius, Vector3.forward);
        }
        else
        {
            float edgeSize = isViewingWorldMap ? worldEdgeSize : this.edgeSize;
            Vector3 size = new Vector3(screenSpaceRect.width - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height),
                screenSpaceRect.height - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height), 0);
            Gizmos.DrawWireCube(ZetanUtility.CenterBetween(corners[0], corners[2]), size);
        }
    }
    #endregion

    #region 其它
    public void Init()
    {
        InitPlayerIcon();
        Camera.targetTexture = targetTexture;
        UI.mapImage.texture = targetTexture;
        miniModeInfo.currentSizeOfCam = miniModeInfo.defaultSizeOfCam;
        worldModeInfo.currentSizeOfCam = worldModeInfo.defaultSizeOfCam;
        ToMiniMap();
    }

    private void ClearMarks()
    {
        MapIconWithoutHolder[] iconWoHs = new MapIconWithoutHolder[IconsWithoutHolder.Count];
        IconsWithoutHolder.Values.CopyTo(iconWoHs, 0);
        foreach (var iconWoH in iconWoHs)
            if (iconWoH.mapIcon.iconType == MapIconType.Mark) RemoveMapIcon(iconWoH.mapIcon, true);
    }

    private RectTransform SelectParent(MapIconType iconType)
    {
        switch (iconType)
        {
            case MapIconType.Normal:
                return UI.iconsParent;
            case MapIconType.Main:
                return UI.mainParent;
            case MapIconType.Mark:
                return UI.marksParent;
            case MapIconType.Quest:
                return UI.questsParent;
            case MapIconType.Objective:
                return UI.objectivesParent;
            default:
                return null;
        }
    }

    public void SaveData(SaveData data)
    {
        foreach (var iconWoH in IconsWithoutHolder.Values)
            if (iconWoH.mapIcon.iconType == MapIconType.Mark) data.markDatas.Add(new MapMarkData(iconWoH));
    }

    public void LoadData(SaveData data)
    {
        ClearMarks();
        foreach (var md in data.markDatas)
            CreateDefaultMark(new Vector3(md.worldPosX, md.worldPosY, md.worldPosZ), md.keepOnMap, md.removeAble, md.textToDisplay);
    }

    public class MapIconWithoutHolder
    {
        public Vector3 worldPosition;
        public MapIcon mapIcon;
        public bool keepOnMap;
        public bool removeAble;
        public string textToDisplay;

        public MapIconWithoutHolder(Vector3 worldPosition, MapIcon mapIcon, bool keepOnMap, bool removeAble, string textToDisplay)
        {
            this.worldPosition = worldPosition;
            this.mapIcon = mapIcon;
            this.keepOnMap = keepOnMap;
            this.removeAble = removeAble;
            this.textToDisplay = textToDisplay;
        }
    }

    [Serializable]
    public class MapModeInfo
    {
        public float defaultSizeOfCam;
        public float currentSizeOfCam;
        public float minZoomOfCam;
        public float maxZoomOfCam;
        public Vector2 windowAnchoreMin;
        public Vector2 windowAnchoreMax;
        public Vector2 mapAnchoreMin;
        public Vector2 mapAnchoreMax;
        public Vector2 anchoredPosition;
        public Vector2 sizeOfWindow;
        public Vector2 sizeOfMap;
    }
    #endregion
}