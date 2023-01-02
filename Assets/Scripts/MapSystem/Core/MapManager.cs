using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio
{
    using SavingSystem;
    using ZetanStudio.Extension;

    [DisallowMultipleComponent]
    [AddComponentMenu("Zetan Studio/管理器/地图管理器")]
    public class MapManager : SingletonMonoBehaviour<MapManager>
    {
        [SerializeField]
        private MapMiniUI miniUI;
        [SerializeField]
        private MapWorldUI worldUI;

        //private IMapUI UI;

        //public RectTransform MapMaskRect => UI?.MapMaskRect;

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
        [SerializeField]
        private Sprite defaultMarkIcon;
        [SerializeField]
        private Vector2 defaultMarkSize = new Vector2(64, 64);

        [SerializeField]
        private MapIcon iconPrefab;
        [SerializeField]
        private MapIconRange iconRangePrefab;

        [SerializeField]
        private MapCamera cameraPrefab;
        [SerializeField]
        private MapCamera mapCamera;
        public Camera MapCamera
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
        private Vector2Int textureSize = new Vector2Int(1024, 1024);
        [SerializeField]
        private RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32;
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

        [SerializeField]
        private bool worldCircle;
        [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
        private float worldEdgeSize;
        [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0.5f, 1)]
        private float worldRadius = 1;
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

        private readonly Dictionary<IMapUI, MapIcon> playerIconInstances = new Dictionary<IMapUI, MapIcon>();
        private readonly Dictionary<MapIconHolder, MapIconData> iconsWithHolder = new Dictionary<MapIconHolder, MapIconData>();
        public List<MapIconData> NormalIcons { get; private set; } = new List<MapIconData>();

        private bool isInit;

        #region 地图图标相关
        #region 创建
        public void CreateMapIcon(MapIconHolder holder)
        {
            var data = new MapIconData(holder);
            iconsWithHolder[holder] = data;
            data.CollectEntity(miniUI, CreateMapIcon(holder, miniUI));
            data.CollectEntity(worldUI, CreateMapIcon(holder, worldUI));
            if (holder.showRange)
            {
                data.CollectRange(miniUI, CreateMapIconRange(miniUI, holder.rangeSize, holder.rangeColor));
                data.CollectRange(worldUI, CreateMapIconRange(worldUI, holder.rangeSize, holder.rangeColor));
            }
        }
        private MapIconRange CreateMapIconRange(IMapUI UI, float radius, Color? color = null)
        {
            if (UI == null || !UI.gameObject) return null;
            MapIconRange icon = ObjectPool.Get(iconRangePrefab.gameObject, UI.RangeParent).GetComponent<MapIconRange>();
            icon.Init(radius, color);
            return icon;
        }
        private MapIcon CreateMapIcon(MapIconHolder holder, IMapUI UI)
        {
            if (UI == null || !UI.gameObject || !holder.icon) return null;
            MapIcon icon = ObjectPool.Get(iconPrefab.gameObject, SelectParent(UI, holder.iconType)).GetComponent<MapIcon>();
            icon.Init(holder);
            return icon;
        }
        public MapIconData CreateMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap,
            MapIconType iconType, bool removeAble, string textToDisplay = null)
        {
            var data = new MapIconData(worldPosition, keepOnMap, iconType, removeAble, textToDisplay);
            NormalIcons.Add(data);
            data.CollectEntity(miniUI, CreateMapIcon(data, miniUI, iconSprite, size));
            data.CollectEntity(worldUI, CreateMapIcon(data, worldUI, iconSprite, size));
            return data;
        }
        public MapIconData CreateMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap, float rangeSize,
            MapIconType iconType, bool removeAble, string textToDisplay = null)
        {
            var data = new MapIconData(worldPosition, keepOnMap, iconType, removeAble, textToDisplay);
            NormalIcons.Add(data);
            data.CollectEntity(miniUI, CreateMapIcon(data, miniUI, iconSprite, size));
            data.CollectEntity(worldUI, CreateMapIcon(data, worldUI, iconSprite, size));
            if (rangeSize > 0)
            {
                data.CollectRange(miniUI, CreateMapIconRange(miniUI, rangeSize));
                data.CollectRange(worldUI, CreateMapIconRange(worldUI, rangeSize));
            }
            return data;
        }
        private MapIcon CreateMapIcon(MapIconData data, IMapUI UI, Sprite iconSprite, Vector2 size)
        {
            if (UI == null || !UI.gameObject || !iconSprite) return null;
            MapIcon icon = ObjectPool.Get(iconPrefab.gameObject, SelectParent(UI, data.iconType)).GetComponent<MapIcon>();
            icon.Init(data, iconSprite, size);
            return icon;
        }

        public MapIconData CreateDefaultMark(Vector3 worldPosition, bool keepOnMap, bool removeAble, string textToDisplay = null)
        {
            return CreateMapIcon(defaultMarkIcon, defaultMarkSize, worldPosition, keepOnMap, MapIconType.Mark, removeAble, textToDisplay);
        }
        public MapIconData CreateDefaultMarkAtMapPoint(Vector3 mapPoint)
        {
            return CreateDefaultMark(MapPointToWorldPoint(mapPoint), true, true);
        }
        #endregion

        #region 移除
        public void RemoveMapIcon(MapIconHolder holder, bool force = false)
        {
            if (!holder || !holder.removeAble && !force) { Debug.Log("return1" + holder); return; }
            //Debug.Log("remove");
            iconsWithHolder.TryGetValue(holder, out MapIconData iconFound);
            if (!iconFound && holder.iconInstance) iconFound = holder.iconInstance;
            if (iconFound) iconFound.Recycle();
            iconsWithHolder.Remove(holder);
        }
        public void RemoveMapIcon(MapIconData icon, bool force = false)
        {
            if (!icon || !icon.RemoveAble && !force) return;
            if (icon.holder) RemoveMapIcon(icon.holder, force);
            else
            {
                NormalIcons.Remove(icon);
                icon.Recycle();
            }
        }
        public void DestroyMapIcon(MapIconData icon)
        {
            if (!icon) return;
            if (icon.holder)
            {
                RemoveMapIcon(icon.holder, true);
            }
            else
            {
                if (!icon.holder) NormalIcons.Remove(icon);
                else iconsWithHolder.Remove(icon.holder);
            }
            icon.Destroy();
        }
        #endregion

        private void DrawMapIcons(IMapUI UI)
        {
            if (UI == null || !UI.gameObject || !player) return;
            if (!MapCamera.orthographic) MapCamera.orthographic = true;
            if (MapCamera.cullingMask != mapRenderMask) MapCamera.cullingMask = mapRenderMask;
            foreach (var iconKvp in iconsWithHolder)
            {
                MapIconHolder holder = iconKvp.Key;
                float distance = Vector3.Magnitude(holder.transform.position - player.position);
                if (iconKvp.Key.isActiveAndEnabled && !iconKvp.Value.ForceHided && (isViewingWorldMap && holder.drawOnWorldMap || !isViewingWorldMap && (!holder.AutoHide
                   || holder.AutoHide && holder.maxValidDistance >= distance)))
                {
                    var screenPos = TargetScreenPos(UI, holder.transform.position + new Vector3(holder.offset.x, use2D ? holder.offset.y : 0, use2D ? 0 : holder.offset.y));
                    if (screenPos == null) continue;
                    MapIcon mapIcon = iconKvp.Value.entities[UI];
                    if (!Inside(UI, screenPos.Value))
                    {
                        if (!holder.keepOnMap)
                        {
                            mapIcon.Hide();
                            continue;
                        }
                    }
                    mapIcon.Show();
                    iconKvp.Value.ShowRange(holder.showRange);
                    iconKvp.Value.UpdateRange(holder.rangeSize * CameraZoom, holder.rangeColor);
                    DrawMapIcon(UI, screenPos.Value, mapIcon, iconKvp.Value.GetRange(UI), holder.keepOnMap);
                    if (!IsViewingWorldMap && distance > holder.maxValidDistance * 0.9f && distance < holder.maxValidDistance)
                        iconKvp.Value.UpdateAlpha((holder.maxValidDistance - distance) / (holder.maxValidDistance * 0.1f));
                    else iconKvp.Value.UpdateAlpha(1);
                }
                else holder.HideIcon();
            }
            foreach (var icon in NormalIcons)
            {
                var screenPos = TargetScreenPos(UI, icon.Position);
                if (screenPos != null)
                {
                    if (Inside(UI, screenPos.Value))
                    {
                        icon.entities[UI].Show();
                        DrawMapIcon(UI, screenPos.Value, icon.entities[UI], icon.GetRange(UI), icon.KeepOnMap);
                    }
                    else icon.entities[UI].Hide();
                }
            }
        }
        private void DrawMapIcon(IMapUI UI, Vector2 screenPos, MapIcon icon, MapIconRange range, bool keepOnMap)
        {
            if (!icon || UI == null || !UI.gameObject) return;
            Vector3 rangePos = screenPos;
            if (keepOnMap)
            {
                //以遮罩的Rect为范围基准而不是地图的
                Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapMaskRect);
                float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f;//地图的一半尺寸
                Vector3[] corners = new Vector3[4];
                UI.MapWindowRect.GetWorldCorners(corners);
                if (circle && !isViewingWorldMap || worldCircle && isViewingWorldMap)
                {
                    //以下不使用UI.mapMaskRect.position，是因为该position值会受轴心(UI.mapMaskRect.pivot)位置的影响而使得最后的结果出现偏移
                    Vector3 realCenter = Utility.CenterBetween(corners[0], corners[2]);
                    float radius = (isViewingWorldMap ? worldRadius : this.radius) * size;
                    Vector3 positionOffset = Vector3.ClampMagnitude((Vector3)screenPos - realCenter, radius);
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
            if (range) range.transform.position = rangePos;
        }

        private Vector2? TargetScreenPos(IMapUI UI, Vector3 worldPosition)
        {
            if (UI == null || !UI.gameObject) return null;
            //把相机视野内的世界坐标归一化为一个裁剪正方体中的坐标，其边长为1，就是说所有视野内的坐标都变成了x、z、y分量都在(0,1)以内的裁剪坐标
            Vector3 viewportPoint = MapCamera.WorldToViewportPoint(worldPosition);
            //这一步用于修正UI因设备分辨率不一样，在进行缩放后实际Rect信息变了而产生的问题
            Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapRect);
            //获取四个顶点的位置，顶点序号
            //  1 ┏━┓ 2
            //  0 ┗━┛ 3
            Vector3[] corners = new Vector3[4];
            UI.MapRect.GetWorldCorners(corners);
            //根据归一化的裁剪坐标，转化为相对于地图的坐标
            return new Vector2(viewportPoint.x * screenSpaceRect.width + corners[0].x, viewportPoint.y * screenSpaceRect.height + corners[0].y);
        }
        private bool Inside(IMapUI UI, Vector2 screenPos)
        {
            Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapMaskRect);
            float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f;//地图的一半尺寸
            Vector3[] corners = new Vector3[4];
            UI.MapWindowRect.GetWorldCorners(corners);
            if (circle && !isViewingWorldMap || worldCircle && isViewingWorldMap)
            {
                //以下不使用UI.mapMaskRect.position，是因为该position值会受轴心(UI.mapMaskRect.pivot)位置的影响而使得最后的结果出现偏移
                Vector3 realCenter = Utility.CenterBetween(corners[0], corners[2]);
                float radius = (isViewingWorldMap ? worldRadius : this.radius) * size;
                return Vector3.Magnitude((Vector3)screenPos - realCenter) <= radius;
            }
            else
            {
                float edgeSize = (isViewingWorldMap ? worldEdgeSize : this.edgeSize) * size;
                return screenPos.x >= corners[0].x + edgeSize && screenPos.x <= corners[2].x - edgeSize
                    && screenPos.y >= corners[0].y + edgeSize && screenPos.y <= corners[1].y - edgeSize;
            }
        }
        private void InitPlayerIcon(IMapUI UI)
        {
            if (UI == null || !UI.gameObject) return;
            playerIconInstances.TryGetValue(UI, out var playerIconInstance);
            if (!playerIconInstance) playerIconInstances[UI] = playerIconInstance = ObjectPool.Get(iconPrefab.gameObject, SelectParent(UI, MapIconType.Main)).GetComponent<MapIcon>();
            playerIconInstance.iconImage.overrideSprite = playerIcon;
            playerIconInstance.rectTransform.sizeDelta = playerIconSize;
            playerIconInstance.iconImage.raycastTarget = false;
        }
        #endregion

        #region 地图切换相关
        public void SwitchMapMode()
        {
            if (worldUI)
            {
                if (!isViewingWorldMap)
                {
                    MapCamera.targetTexture = worldUI.MapImage.texture as RenderTexture;
                    worldUI.Open();
                    ApplyCamera(worldModeInfo);
                    worldUI.onClose += () =>
                    {
                        MapCamera.targetTexture = miniUI.MapImage.texture as RenderTexture;
                        isViewingWorldMap = false;
                        ApplyCamera(miniModeInfo);
                    };
                }
                else worldUI.Close();
                isViewingWorldMap = !isViewingWorldMap;
            }
            else
            {
                var UI = miniUI;
                if (UI == null || !UI.gameObject) return;
                isViewingWorldMap = !isViewingWorldMap;
                isMovingCamera = false;
                if (!isViewingWorldMap)//从大向小切换
                {
                    if (animationSpeed > 0)
                    {
                        UI.MapWindowRect.anchorMin = miniModeInfo.windowAnchoreMin;
                        UI.MapWindowRect.anchorMax = miniModeInfo.windowAnchoreMax;
                        UI.MapRect.anchorMin = miniModeInfo.mapAnchoreMin;
                        UI.MapRect.anchorMax = miniModeInfo.mapAnchoreMax;
                    }
                    else ToMiniMap();
                }
                else
                {
                    if (animationSpeed > 0)
                    {
                        UI.MapWindowRect.anchorMin = worldModeInfo.windowAnchoreMin;
                        UI.MapWindowRect.anchorMax = worldModeInfo.windowAnchoreMax;
                        UI.MapRect.anchorMin = worldModeInfo.mapAnchoreMin;
                        UI.MapRect.anchorMax = worldModeInfo.mapAnchoreMax;
                    }
                    else ToWorldMap();
                }
                if (animationSpeed > 0)
                {
                    isSwitching = true;
                    switchTime = 0;
                    startSizeOfCamForMap = MapCamera.orthographicSize;
                    startPosOfCamForMap = MapCamera.transform.position;
                    startPositionOfMap = UI.MapWindowRect.anchoredPosition;
                    startSizeOfMapWindow = UI.MapWindowRect.rect.size;
                    startSizeOfMap = UI.MapRect.rect.size;
                }
            }
        }

        private void AnimateSwitching()
        {
            var UI = miniUI;
            if (UI == null || !UI.gameObject || !AnimateAble) return;
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
                    Vector3 newCamPos = new Vector3(player.position.x, use2D ? player.position.y : MapCamera.transform.position.y, use2D ? MapCamera.transform.position.z : player.position.z);
                    MapCamera.transform.position = Vector3.Lerp(startPosOfCamForMap, newCamPos, switchTime);
                    AnimateTo(miniModeInfo);
                }
                else ToMiniMap();
            }
        }
        private void AnimateTo(MapModeInfo modeInfo)
        {
            var UI = miniUI;
            if (UI == null || !UI.gameObject) return;
            MapCamera.orthographicSize = Mathf.Lerp(startSizeOfCamForMap, modeInfo.currentSizeOfCam, switchTime);
            UI.MapWindowRect.anchoredPosition = Vector3.Lerp(startPositionOfMap, modeInfo.anchoredPosition, switchTime);
            UI.MapRect.sizeDelta = Vector2.Lerp(startSizeOfMap, modeInfo.sizeOfMap, switchTime);
            UI.MapWindowRect.sizeDelta = Vector2.Lerp(startSizeOfMapWindow, modeInfo.sizeOfWindow, switchTime);
        }
        private bool IsAnimaComplete(bool toWorldMode)
        {
            var UI = miniUI;
            if (toWorldMode) return UI.MapRect.sizeDelta.x >= worldModeInfo.sizeOfMap.x && UI.MapRect.sizeDelta.y >= worldModeInfo.sizeOfMap.y;
            else return UI.MapRect.sizeDelta.x <= miniModeInfo.sizeOfMap.x && UI.MapRect.sizeDelta.y <= miniModeInfo.sizeOfMap.y;
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
            var UI = miniUI;
            if (UI == null || !UI.gameObject) return;
            ApplyCamera(modeInfo);
            UI.MapWindowRect.anchorMin = modeInfo.windowAnchoreMin;
            UI.MapWindowRect.anchorMax = modeInfo.windowAnchoreMax;
            UI.MapRect.anchorMin = modeInfo.mapAnchoreMin;
            UI.MapRect.anchorMax = modeInfo.mapAnchoreMax;
            UI.MapWindowRect.anchoredPosition = modeInfo.anchoredPosition;
            UI.MapRect.sizeDelta = modeInfo.sizeOfMap;
            UI.MapWindowRect.sizeDelta = modeInfo.sizeOfWindow;
        }

        private void ApplyCamera(MapModeInfo modeInfo)
        {
            MapCamera.orthographicSize = modeInfo.currentSizeOfCam;
            zoomLimit.x = modeInfo.minZoomOfCam;
            zoomLimit.y = modeInfo.maxZoomOfCam;
        }

        public void SetCurrentAsMiniMap()
        {
            var UI = miniUI;
            if (UI == null || !UI.gameObject || isViewingWorldMap) return;
            if (MapCamera)
            {
                miniModeInfo.defaultSizeOfCam = MapCamera.orthographicSize;
                miniModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
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
            var UI = miniUI;
            if (UI == null || !UI.gameObject || !isViewingWorldMap) return;
            if (MapCamera)
            {
                worldModeInfo.defaultSizeOfCam = MapCamera.orthographicSize;
                worldModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
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
            var UI = miniUI;
            if (UI == null || !UI.gameObject) return;
            modeInfo.windowAnchoreMin = UI.MapWindowRect.anchorMin;
            modeInfo.windowAnchoreMax = UI.MapWindowRect.anchorMax;
            modeInfo.mapAnchoreMin = UI.MapRect.anchorMin;
            modeInfo.mapAnchoreMax = UI.MapRect.anchorMax;
            modeInfo.anchoredPosition = UI.MapWindowRect.anchoredPosition;
            modeInfo.sizeOfWindow = UI.MapWindowRect.sizeDelta;
            modeInfo.sizeOfMap = UI.MapRect.sizeDelta;
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
            if (mapCamera) ObjectPool.Put(mapCamera);
            mapCamera = Instantiate(cameraPrefab, transform);
            mapCamera.Camera.targetTexture = targetTexture;
            //DontDestroyOnLoad(mapCamera);
        }
        private void FollowPlayer(IMapUI UI, MapIcon playerIconInstance)
        {
            if (!player || !playerIconInstance) return;
            DrawMapIcon(UI, TargetScreenPos(UI, isViewingWorldMap ? player.position : MapCamera.transform.position).Value, playerIconInstance, null, true);
            playerIconInstance.transform.SetSiblingIndex(playerIconInstance.transform.childCount - 1);
            if (!rotateMap)
            {
                if (use2D)
                {
                    playerIconInstance.transform.eulerAngles = new Vector3(playerIconInstance.transform.eulerAngles.x, playerIconInstance.transform.eulerAngles.y, player.eulerAngles.z);
                    MapCamera.transform.eulerAngles = Vector3.zero;
                }
                else
                {
                    playerIconInstance.transform.eulerAngles = new Vector3(playerIconInstance.transform.eulerAngles.x, playerIconInstance.transform.eulerAngles.y, -player.eulerAngles.y);
                    MapCamera.transform.eulerAngles = Vector3.right * 90;
                }
            }
            else
            {
                if (use2D)
                {
                    playerIconInstance.transform.eulerAngles = Vector3.zero;
                    MapCamera.transform.eulerAngles = new Vector3(0, 0, player.eulerAngles.z);
                }
                else
                {
                    playerIconInstance.transform.eulerAngles = Vector3.zero;
                    MapCamera.transform.eulerAngles = new Vector3(MapCamera.transform.eulerAngles.x, player.eulerAngles.y, MapCamera.transform.eulerAngles.z);
                }
            }
            if (!isViewingWorldMap && !isSwitching && !isMovingCamera)
            {
                //2D模式，则跟随目标的Y坐标，相机的Z坐标不动；3D模式，则跟随目标的Z坐标，相机的Y坐标不动。
                Vector3 newCamPos = new Vector3(player.position.x + offset.x,
                    use2D ? player.position.y + offset.y : MapCamera.transform.position.y,
                    use2D ? MapCamera.transform.position.z : player.position.z + offset.y);
                MapCamera.transform.position = newCamPos;
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
            Vector3 newCamPos = new Vector3(worldPosition.x, use2D ? worldPosition.y : MapCamera.transform.position.y, use2D ? MapCamera.transform.position.z : worldPosition.z);
            startPosOfCamForMap = MapCamera.transform.position;
            camMoveDestination = newCamPos;
            isMovingCamera = true;
            cameraMovingTime = 0;
        }

        public Vector3 MapPointToWorldPoint(Vector3 mapViewportPoint)
        {
            Vector3 worldPosition = MapCamera.ViewportToWorldPoint(mapViewportPoint);
            return use2D ? new Vector3(worldPosition.x, worldPosition.y) : worldPosition;
        }

        public void DragWorldMap(Vector2 direction)
        {
            if (!isViewingWorldMap || isSwitching) return;
            if (direction == default) return;
            isMovingCamera = false;
            cameraMovingTime = 0;
            float mag = new Vector2(Screen.width, Screen.height).magnitude;
            direction = new Vector2(direction.x * 1000 / mag, direction.y * 1000 / mag) * (MapCamera.orthographicSize / worldModeInfo.currentSizeOfCam);
            MapCamera.transform.Translate(new Vector3(direction.x, use2D ? direction.y : 0, use2D ? 0 : direction.y) * dragSensitivity / CameraZoom, Space.World);
        }

        public void Zoom(float value)
        {
            if (isSwitching || value == 0) return;
            MapCamera.orthographicSize = Mathf.Clamp(MapCamera.orthographicSize - value, zoomLimit.x, zoomLimit.y);
            if (IsViewingWorldMap) worldModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
            else miniModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
        }

        public float CameraZoom => IsViewingWorldMap ? worldModeInfo.defaultSizeOfCam / MapCamera.orthographicSize : miniModeInfo.defaultSizeOfCam / MapCamera.orthographicSize;
        #endregion

        #region MonoBehaviour
        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (!isInit) return;
            if (updateMode == UpdateMode.Update)
            {
                DrawMapIcons(miniUI);
                DrawMapIcons(worldUI);
            }
            if (isSwitching) AnimateSwitching();
            if (isMovingCamera)
            {
                cameraMovingTime += Time.deltaTime * 5;
                if (camMoveDestination != MapCamera.transform.position)
                    MapCamera.transform.position = Vector3.Lerp(startPosOfCamForMap, camMoveDestination, cameraMovingTime);
                else
                {
                    isMovingCamera = false;
                    cameraMovingTime = 0;
                }
            }
        }
        private void LateUpdate()
        {
            if (!isInit) return;
            if (updateMode == UpdateMode.LateUpdate)
            {
                DrawMapIcons(miniUI);
                DrawMapIcons(worldUI);
            }
        }
        private void FixedUpdate()
        {
            if (!isInit) return;
            if (updateMode == UpdateMode.FixedUpdate)
            {
                DrawMapIcons(miniUI);
                DrawMapIcons(worldUI);
            }
            playerIconInstances.ForEach(x => FollowPlayer(x.Key, x.Value)); //放在FixedUpdate()可以有效防止主图标抖动
        }

        private void OnDrawGizmos()
        {
            var UI = miniUI;
            if (UI == null || !UI.gameObject || !UI.MapMaskRect) return;
            Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapMaskRect);
            Vector3[] corners = new Vector3[4];
            UI.MapMaskRect.GetWorldCorners(corners);
            if (circle && !isViewingWorldMap)
            {
                float radius = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f * this.radius;
                Utility.Editor.DrawGizmosCircle(Utility.CenterBetween(corners[0], corners[2]), radius);
            }
            else
            {
                float edgeSize = isViewingWorldMap ? worldEdgeSize : this.edgeSize;
                Vector3 size = new Vector3(screenSpaceRect.width - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height),
                    screenSpaceRect.height - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height), 0);
                Gizmos.DrawWireCube(Utility.CenterBetween(corners[0], corners[2]), size);
            }
        }
        #endregion

        #region 其它
        public void Init()
        {
            InitPlayerIcon(miniUI);
            InitPlayerIcon(worldUI);
            //foreach (var icon in iconsWithHolder)
            //    icon.Value.Recycle();
            //iconsWithHolder.Clear();
            //foreach (var iconWoH in NormalIcons)
            //    iconWoH.Recycle();
            //NormalIcons.Clear();
            if (targetTexture) targetTexture.Release();
            targetTexture = new RenderTexture(textureSize.x, textureSize.y, 24, textureFormat)
            {
                name = "MapTexture"
            };
            MapCamera.targetTexture = targetTexture;
            if (!MapCamera.CompareTag("MapCamera")) MapCamera.tag = "MapCamera";
            miniUI.MapImage.texture = targetTexture;
            if (worldUI)
            {
                var worldTexture = new RenderTexture(Mathf.Abs((int)worldUI.MapRect.rect.x), Mathf.Abs((int)worldUI.MapRect.rect.y), 24, textureFormat)
                {
                    name = "MapWorldTexture"
                };
                worldUI.MapImage.texture = worldTexture;
            }
            miniModeInfo.currentSizeOfCam = miniModeInfo.defaultSizeOfCam;
            worldModeInfo.currentSizeOfCam = worldModeInfo.defaultSizeOfCam;
            ToMiniMap();
            isInit = true;
        }
        private void ClearMarks()
        {
            var marks = NormalIcons.FindAll(x => x.iconType == MapIconType.Mark);
            foreach (var iconWoH in marks)
                RemoveMapIcon(iconWoH, true);
        }

        public void DrawIconGizmos(MapIconHolder holder)
        {
            DrawIconGizmos(miniUI, holder);
            DrawIconGizmos(worldUI, holder);
        }
        private void DrawIconGizmos(IMapUI UI, MapIconHolder holder)
        {
            if (UI == null || !UI.gameObject || !UI.MapMaskRect) return;
            var rect = Utility.GetScreenSpaceRect(UI.MapMaskRect);
            Gizmos.DrawCube(UI.MapMaskRect.position, holder.iconSize * rect.width / UI.MapMaskRect.rect.width);
            if (holder.showRange)
                Utility.Editor.DrawGizmosCircle(UI.MapMaskRect.position, holder.rangeSize * rect.width / UI.MapMaskRect.rect.width, Vector3.forward, holder.rangeColor);
        }

        private RectTransform SelectParent(IMapUI UI, MapIconType iconType)
        {
            switch (iconType)
            {
                case MapIconType.Normal:
                    return UI.IconsParent;
                case MapIconType.Main:
                    return UI.MainParent;
                case MapIconType.Mark:
                    return UI.MarksParent;
                case MapIconType.Quest:
                    return UI.QuestsParent;
                case MapIconType.Objective:
                    return UI.ObjectivesParent;
                default:
                    return null;
            }
        }

        [SaveMethod]
        public void SaveData(SaveData saveData)
        {
            var data = saveData.Write("markData", new GenericData());
            foreach (var iconWoH in NormalIcons)
                if (iconWoH.iconType == MapIconType.Mark)
                {
                    var md = data.Write(new GenericData());
                    md["worldPosX"] = iconWoH.Position.x;
                    md["worldPosY"] = iconWoH.Position.y;
                    md["worldPosZ"] = iconWoH.Position.z;
                    md["keepOnMap"] = iconWoH.KeepOnMap;
                    md["removeAble"] = iconWoH.RemoveAble;
                    md["textToDisplay"] = iconWoH.TextToDisplay;
                }
        }

        public void LoadData(SaveData saveData)
        {
            ClearMarks();
            if (saveData.TryReadData("markData", out var data))
                foreach (var md in data.ReadDataList())
                    CreateDefaultMark(new Vector3(md.ReadFloat("worldPosX"), md.ReadFloat("worldPosY"), md.ReadFloat("worldPosZ")), md.ReadBool("keepOnMap"), md.ReadBool("removeAble"), md.ReadString("textToDisplay"));
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
}