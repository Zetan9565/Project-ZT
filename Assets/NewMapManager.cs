using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.MapSystem
{
    public class NewMapManager : SingletonMonoBehaviour<NewMapManager>
    {
        private readonly Dictionary<MapIconHolder, MapIconData> iconsWithHolder = new Dictionary<MapIconHolder, MapIconData>();
        private readonly List<MapIconData> icons = new List<MapIconData>();

        public ReadOnlyDictionary<MapIconHolder, MapIconData> IconsWithHolder => new ReadOnlyDictionary<MapIconHolder, MapIconData>(iconsWithHolder);
        public ReadOnlyCollection<MapIconData> Icons => new ReadOnlyCollection<MapIconData>(icons);

        public event Action<MapIconData> OnIconCreated;
        public event Action<MapIconData> OnIconRemoved;

        public Transform player;
        public MapCamera cameraPrefab;
        public LayerMask mapRenderMask = ~0;

        public MapIconData CreateIcon(MapIconHolder holder)
        {
            var data = new MapIconData(holder);
            iconsWithHolder[holder] = data;
            OnIconCreated?.Invoke(data);
            return data;
        }
        public MapIconData CreateIcon(Vector3 position, Sprite icon, Vector2 size, bool keepOnMap, MapIconType type, bool removeAble, string text = null)
        {
            var data = new MapIconData(position, icon, size, keepOnMap, type, removeAble, text);
            icons.Add(data);
            OnIconCreated?.Invoke(data);
            return data;
        }
        public MapIconData CreateIcon(Vector3 position, Sprite icon, Vector2 size, bool keepOnMap, MapIconType type, bool removeAble, float rangeSize, Color? rangeColor = null, string text = null)
        {
            var data = new MapIconData(position, icon, size, keepOnMap, type, removeAble, rangeSize, rangeColor, text);
            OnIconCreated?.Invoke(data);
            return data;
        }
        public void RemoveIcon(MapIconHolder holder)
        {
            if (iconsWithHolder.TryGetValue(holder, out var data))
            {
                iconsWithHolder.Remove(holder);
                OnIconRemoved?.Invoke(data);
            }
        }
        public void RemoveIcon(MapIconData data)
        {
            OnIconRemoved?.Invoke(data);
        }
    }

    //public interface IMapUI<T> where T : MonoBehaviour
    //{
    //    public RectTransform MapWindowRect { get; }
    //    public RectTransform MapMaskRect { get; }

    //    public RectTransform IconsParent { get; }
    //    public RectTransform MainParent { get; }
    //    public RectTransform RangeParent { get; }

    //    public RectTransform MarksParent { get; }
    //    public RectTransform ObjectivesParent { get; }
    //    public RectTransform QuestsParent { get; }

    //    public RectTransform MapRect { get; }
    //    public RawImage MapImage { get; }

    //    public T component { get; }
    //    public bool circle { get; }
    //    public Sprite playerIcon { get; }
    //    public Vector2 playerIconSize { get; }

    //    public float radius { get; }
    //    public float edgeSize { get; }
    //    protected MapCamera mapCamera { get; set; }
    //    public Camera MapCamera
    //    {
    //        get
    //        {
    //            if (!mapCamera) mapCamera = UnityEngine.Object.Instantiate(NewMapManager.Instance.cameraPrefab, component.transform);
    //            return mapCamera.Camera;
    //        }
    //    }

    //    public Dictionary<MapIconHolder, MapIconData> iconsWithHolder { get; }
    //    public List<MapIconData> icons { get; }

    //    protected MapIcon iconPrefab { get; }
    //    protected MapIcon playerIconInstance { get; set; }
    //    public float CameraZoom { get; }
    //    public Transform player { get; }
    //    public bool use2D { get; }
    //    private void DrawMapIcons(IMapUI UI)
    //    {
    //        if (UI == null || !UI.gameObject || !player) return;
    //        if (!MapCamera.orthographic) MapCamera.orthographic = true;
    //        if (MapCamera.cullingMask != NewMapManager.Instance.mapRenderMask) MapCamera.cullingMask = NewMapManager.Instance.mapRenderMask;
    //        foreach (var iconKvp in iconsWithHolder)
    //        {
    //            MapIconHolder holder = iconKvp.Key;
    //            float distance = Vector3.Magnitude(holder.transform.position - player.position);
    //            if (iconKvp.Key.isActiveAndEnabled && !iconKvp.Value.ForceHided && (isViewingWorldMap && holder.drawOnWorldMap || !isViewingWorldMap && (!holder.AutoHide
    //               || holder.AutoHide && holder.maxValidDistance >= distance)))
    //            {
    //                var screenPos = TargetScreenPos(UI, holder.transform.position + new Vector3(holder.offset.x, use2D ? holder.offset.y : 0, use2D ? 0 : holder.offset.y));
    //                if (screenPos == null) continue;
    //                MapIcon mapIcon = iconKvp.Value.entities[UI];
    //                if (!Inside(UI, screenPos.Value))
    //                {
    //                    if (!holder.keepOnMap)
    //                    {
    //                        mapIcon.Hide();
    //                        continue;
    //                    }
    //                }
    //                mapIcon.Show();
    //                iconKvp.Value.ShowRange(holder.showRange);
    //                iconKvp.Value.UpdateRange(holder.rangeSize * CameraZoom, holder.rangeColor);
    //                DrawMapIcon(UI, screenPos.Value, mapIcon, iconKvp.Value.GetRange(UI), holder.keepOnMap);
    //                if (!IsViewingWorldMap && distance > holder.maxValidDistance * 0.9f && distance < holder.maxValidDistance)
    //                    iconKvp.Value.UpdateAlpha((holder.maxValidDistance - distance) / (holder.maxValidDistance * 0.1f));
    //                else iconKvp.Value.UpdateAlpha(1);
    //            }
    //            else holder.HideIcon();
    //        }
    //        foreach (var icon in icons)
    //        {
    //            var screenPos = TargetScreenPos(UI, icon.Position);
    //            if (screenPos != null)
    //            {
    //                if (Inside(UI, screenPos.Value))
    //                {
    //                    icon.entities[UI].Show();
    //                    DrawMapIcon(UI, screenPos.Value, icon.entities[UI], icon.GetRange(UI), icon.KeepOnMap);
    //                }
    //                else icon.entities[UI].Hide();
    //            }
    //        }
    //    }
    //    private void DrawMapIcon(IMapUI UI, Vector2 screenPos, MapIcon icon, MapIconRange range, bool keepOnMap)
    //    {
    //        if (!icon || UI == null || !UI.gameObject) return;
    //        Vector3 rangePos = screenPos;
    //        if (keepOnMap)
    //        {
    //            //以遮罩的Rect为范围基准而不是地图的
    //            Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapMaskRect);
    //            float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f;//地图的一半尺寸
    //            Vector3[] corners = new Vector3[4];
    //            UI.MapWindowRect.GetWorldCorners(corners);
    //            if (circle)
    //            {
    //                //以下不使用UI.mapMaskRect.position，是因为该position值会受轴心(UI.mapMaskRect.pivot)位置的影响而使得最后的结果出现偏移
    //                Vector3 realCenter = Utility.CenterBetween(corners[0], corners[2]);
    //                float radius = this.radius * size;
    //                Vector3 positionOffset = Vector3.ClampMagnitude((Vector3)screenPos - realCenter, radius);
    //                screenPos = realCenter + positionOffset;
    //            }
    //            else
    //            {
    //                float edgeSize = this.edgeSize * size;
    //                screenPos.x = Mathf.Clamp(screenPos.x, corners[0].x + edgeSize, corners[2].x - edgeSize);
    //                screenPos.y = Mathf.Clamp(screenPos.y, corners[0].y + edgeSize, corners[1].y - edgeSize);
    //            }
    //        }
    //        icon.transform.position = screenPos;
    //        if (range) range.transform.position = rangePos;
    //    }

    //    private Vector2? TargetScreenPos(IMapUI UI, Vector3 worldPosition)
    //    {
    //        if (UI == null || !UI.gameObject) return null;
    //        //把相机视野内的世界坐标归一化为一个裁剪正方体中的坐标，其边长为1，就是说所有视野内的坐标都变成了x、z、y分量都在(0,1)以内的裁剪坐标
    //        Vector3 viewportPoint = MapCamera.WorldToViewportPoint(worldPosition);
    //        //这一步用于修正UI因设备分辨率不一样，在进行缩放后实际Rect信息变了而产生的问题
    //        Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapRect);
    //        //获取四个顶点的位置，顶点序号
    //        //  1 ┏━┓ 2
    //        //  0 ┗━┛ 3
    //        Vector3[] corners = new Vector3[4];
    //        UI.MapRect.GetWorldCorners(corners);
    //        //根据归一化的裁剪坐标，转化为相对于地图的坐标
    //        return new Vector2(viewportPoint.x * screenSpaceRect.width + corners[0].x, viewportPoint.y * screenSpaceRect.height + corners[0].y);
    //    }
    //    private bool Inside(IMapUI UI, Vector2 screenPos)
    //    {
    //        Rect screenSpaceRect = Utility.GetScreenSpaceRect(UI.MapMaskRect);
    //        float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f;//地图的一半尺寸
    //        Vector3[] corners = new Vector3[4];
    //        UI.MapWindowRect.GetWorldCorners(corners);
    //        if (circle)
    //        {
    //            //以下不使用UI.mapMaskRect.position，是因为该position值会受轴心(UI.mapMaskRect.pivot)位置的影响而使得最后的结果出现偏移
    //            Vector3 realCenter = Utility.CenterBetween(corners[0], corners[2]);
    //            float radius = this.radius * size;
    //            return Vector3.Magnitude((Vector3)screenPos - realCenter) <= radius;
    //        }
    //        else
    //        {
    //            float edgeSize = this.edgeSize * size;
    //            return screenPos.x >= corners[0].x + edgeSize && screenPos.x <= corners[2].x - edgeSize
    //                && screenPos.y >= corners[0].y + edgeSize && screenPos.y <= corners[1].y - edgeSize;
    //        }
    //    }
    //    private void InitPlayerIcon()
    //    {
    //        if (!component) return;
    //        if (playerIconInstance) UnityEngine.Object.Destroy(playerIconInstance.gameObject);
    //        playerIconInstance = ObjectPool.Get(iconPrefab.gameObject, SelectParent(MapIconType.Main)).GetComponent<MapIcon>();
    //        playerIconInstance.iconImage.overrideSprite = playerIcon;
    //        playerIconInstance.rectTransform.sizeDelta = playerIconSize;
    //        playerIconInstance.iconImage.raycastTarget = false;
    //    }

    //    private RectTransform SelectParent(MapIconType iconType)
    //    {
    //        switch (iconType)
    //        {
    //            case MapIconType.Normal:
    //                return IconsParent;
    //            case MapIconType.Main:
    //                return MainParent;
    //            case MapIconType.Mark:
    //                return MarksParent;
    //            case MapIconType.Quest:
    //                return QuestsParent;
    //            case MapIconType.Objective:
    //                return ObjectivesParent;
    //            default:
    //                return null;
    //        }
    //    }
    //}
}
