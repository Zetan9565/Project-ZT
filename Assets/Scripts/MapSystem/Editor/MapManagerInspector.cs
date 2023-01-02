using UnityEngine;
using UnityEditor;
using ZetanStudio;

[CustomEditor(typeof(MapManager))]
public class MapManagerInspector : SingletonMonoBehaviourInspector
{
    MapManager manager;

    SerializedProperty miniUI;
    SerializedProperty worldUI;
    SerializedProperty updateMode;
    SerializedProperty player;
    SerializedProperty offset;
    SerializedProperty playerIcon;
    SerializedProperty playerIconSize;
    SerializedProperty mapCamera;
    SerializedProperty iconPrefab;
    SerializedProperty iconRangePrefab;
    SerializedProperty cameraPrefab;
    SerializedProperty textueSize;
    SerializedProperty textueFormat;
    SerializedProperty mapRenderMask;
    SerializedProperty use2D;
    SerializedProperty rotateMap;
    SerializedProperty edgeSize;
    SerializedProperty worldEdgeSize;
    SerializedProperty circle;
    SerializedProperty worldCircle;
    SerializedProperty radius;
    SerializedProperty worldRadius;
    SerializedProperty animationSpeed;
    SerializedProperty isViewingWorldMap;
    SerializedProperty dragSensitivity;
    SerializedProperty miniModeInfo;
    SerializedProperty worldModeInfo;
    SerializedProperty defaultMarkIcon;
    SerializedProperty defaultMarkSize;

    private void OnEnable()
    {
        manager = target as MapManager;

        miniUI = serializedObject.FindProperty("miniUI");
        worldUI = serializedObject.FindProperty("worldUI");
        updateMode = serializedObject.FindProperty("updateMode");
        player = serializedObject.FindProperty("player");
        offset = serializedObject.FindProperty("offset");
        playerIcon = serializedObject.FindProperty("playerIcon");
        playerIconSize = serializedObject.FindProperty("playerIconSize");
        mapCamera = serializedObject.FindProperty("mapCamera");
        iconPrefab = serializedObject.FindProperty("iconPrefab");
        iconRangePrefab = serializedObject.FindProperty("iconRangePrefab");
        cameraPrefab = serializedObject.FindProperty("cameraPrefab");
        textueSize = serializedObject.FindProperty("textureSize");
        textueFormat = serializedObject.FindProperty("textureFormat");
        mapRenderMask = serializedObject.FindProperty("mapRenderMask");
        use2D = serializedObject.FindProperty("use2D");
        rotateMap = serializedObject.FindProperty("rotateMap");
        edgeSize = serializedObject.FindProperty("edgeSize");
        worldEdgeSize = serializedObject.FindProperty("worldEdgeSize");
        circle = serializedObject.FindProperty("circle");
        worldCircle = serializedObject.FindProperty("worldCircle");
        radius = serializedObject.FindProperty("radius");
        worldRadius = serializedObject.FindProperty("worldRadius");
        animationSpeed = serializedObject.FindProperty("animationSpeed");
        isViewingWorldMap = serializedObject.FindProperty("isViewingWorldMap");
        dragSensitivity = serializedObject.FindProperty("dragSensitivity");
        miniModeInfo = serializedObject.FindProperty("miniModeInfo");
        worldModeInfo = serializedObject.FindProperty("worldModeInfo");
        defaultMarkIcon = serializedObject.FindProperty("defaultMarkIcon");
        defaultMarkSize = serializedObject.FindProperty("defaultMarkSize");
    }

    public override void OnInspectorGUI()
    {
        if (!CheckValid(out string text))
        {
            EditorGUILayout.HelpBox(text, MessageType.Error);
            return;
        }
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(player, new GUIContent("跟随目标"));
        EditorGUILayout.PropertyField(offset, new GUIContent("位置偏移量"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(use2D, new GUIContent("2D场景"));
        EditorGUILayout.PropertyField(rotateMap, new GUIContent("旋转地图"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(updateMode, new GUIContent("更新方式"));
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUILayout.PropertyField(miniUI, new GUIContent("小地图"));
        EditorGUILayout.PropertyField(worldUI, new GUIContent("大地图", "若设置独立大地图，将不会把小地图缩放为大地图"));
        EditorGUI.EndDisabledGroup();
        if (miniUI.objectReferenceValue)
        {
            int mode = circle.boolValue ? 1 : 0;
            int index = EditorGUILayout.IntPopup("形状", mode, new string[] { "矩形", "圆形" }, new int[] { 0, 1 });
            circle.boolValue = index != 0;
            if (!circle.boolValue) EditorGUILayout.PropertyField(edgeSize, new GUIContent("边框厚度"));
            else EditorGUILayout.PropertyField(radius, new GUIContent("半径"));
            if (!worldUI.objectReferenceValue)
            {
                mode = worldCircle.boolValue ? 1 : 0;
                index = EditorGUILayout.IntPopup("大地图形状", mode, new string[] { "矩形", "圆形" }, new int[] { 0, 1 });
                worldCircle.boolValue = index != 0;
                if (!worldCircle.boolValue) EditorGUILayout.PropertyField(worldEdgeSize, new GUIContent("大地图边框厚度"));
                else EditorGUILayout.PropertyField(worldRadius, new GUIContent("大地图半径"));
            }

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(iconPrefab, new GUIContent("图标预制件"));
            EditorGUILayout.PropertyField(iconRangePrefab, new GUIContent("图标范围预制件"));
            playerIcon.objectReferenceValue = EditorGUILayout.ObjectField("主图标", playerIcon.objectReferenceValue as Sprite, typeof(Sprite), false);
            EditorGUILayout.PropertyField(playerIconSize, new GUIContent("主图标大小"));
            defaultMarkIcon.objectReferenceValue = EditorGUILayout.ObjectField("默认标记图标", defaultMarkIcon.objectReferenceValue as Sprite, typeof(Sprite), false);
            EditorGUILayout.PropertyField(defaultMarkSize, new GUIContent("默认标记大小"));
            EditorGUILayout.PropertyField(cameraPrefab, new GUIContent("地图相机预制件"));
            EditorGUILayout.PropertyField(mapCamera, new GUIContent("地图相机"));
            EditorGUILayout.PropertyField(textueSize, new GUIContent("相机采样分辨率"));
            EditorGUILayout.PropertyField(textueFormat, new GUIContent("相机采样格式"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(mapRenderMask, new GUIContent("地图相机可视层"));
            if (mapCamera.objectReferenceValue)
            {
                Camera cam = (mapCamera.objectReferenceValue as MapCamera).Camera;
                cam.cullingMask = mapRenderMask.intValue;
                if (!Application.isPlaying)
                {
                    cam.orthographicSize = EditorGUILayout.Slider("相机视野大小", cam.orthographicSize, min(), max());

                    float min() => isViewingWorldMap.boolValue ? miniModeInfo.FindPropertyRelative("minZoomOfCam").floatValue : worldModeInfo.FindPropertyRelative("minZoomOfCam").floatValue;
                    float max() => isViewingWorldMap.boolValue ? miniModeInfo.FindPropertyRelative("maxZoomOfCam").floatValue : worldModeInfo.FindPropertyRelative("maxZoomOfCam").floatValue;
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(dragSensitivity, new GUIContent("大地图拖拽灵敏度"));
            EditorGUILayout.PropertyField(animationSpeed, new GUIContent("动画速度"));
            EditorGUI.BeginDisabledGroup(Application.isPlaying || worldUI.objectReferenceValue);
            EditorGUILayout.PropertyField(isViewingWorldMap, new GUIContent("当前是大地图模式"));
            EditorGUI.EndDisabledGroup();
            DrawModeInfo(miniModeInfo, true);
            DrawModeInfo(worldModeInfo, false);
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    private void DrawModeInfo(SerializedProperty modeInfo, bool mini)
    {
        MapMiniUI UI = this.miniUI.objectReferenceValue as MapMiniUI;
        if (UI)
        {
            SerializedProperty defaultSizeOfCam = modeInfo.FindPropertyRelative("defaultSizeOfCam");
            SerializedProperty currentSizeOfCam = modeInfo.FindPropertyRelative("currentSizeOfCam");
            SerializedProperty minZoomOfCam = modeInfo.FindPropertyRelative("minZoomOfCam");
            SerializedProperty maxZoomOfCam = modeInfo.FindPropertyRelative("maxZoomOfCam");
            SerializedProperty windowAnchoreMin = modeInfo.FindPropertyRelative("windowAnchoreMin");
            SerializedProperty windowAnchoreMax = modeInfo.FindPropertyRelative("windowAnchoreMax");
            SerializedProperty mapAnchoreMin = modeInfo.FindPropertyRelative("mapAnchoreMin");
            SerializedProperty mapAnchoreMax = modeInfo.FindPropertyRelative("mapAnchoreMax");
            SerializedProperty anchoredPosition = modeInfo.FindPropertyRelative("anchoredPosition");
            SerializedProperty sizeOfWindow = modeInfo.FindPropertyRelative("sizeOfWindow");
            SerializedProperty sizeOfMap = modeInfo.FindPropertyRelative("sizeOfMap");
            EditorGUILayout.BeginVertical("Box");
            if (!worldUI.objectReferenceValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(Application.isPlaying || mini && isViewingWorldMap.boolValue || !mini && !isViewingWorldMap.boolValue);
                if (GUILayout.Button("以当前状态作为" + (mini ? "小地图" : "大地图")))
                {
                    if (!(mini && isViewingWorldMap.boolValue))
                    {
                        if (mapCamera.objectReferenceValue)
                        {
                            defaultSizeOfCam.floatValue = (mapCamera.objectReferenceValue as MapCamera).Camera.orthographicSize;
                            currentSizeOfCam.floatValue = (mapCamera.objectReferenceValue as MapCamera).Camera.orthographicSize;
                        }
                        windowAnchoreMin.vector2Value = UI.MapWindowRect.anchorMin;
                        windowAnchoreMax.vector2Value = UI.MapWindowRect.anchorMax;
                        mapAnchoreMin.vector2Value = UI.MapRect.anchorMin;
                        mapAnchoreMax.vector2Value = UI.MapRect.anchorMax;
                        anchoredPosition.vector2Value = UI.MapWindowRect.anchoredPosition;
                        sizeOfWindow.vector2Value = UI.MapWindowRect.sizeDelta;
                        sizeOfMap.vector2Value = UI.MapRect.sizeDelta;
                        if (mini) isViewingWorldMap.boolValue = false;
                        else isViewingWorldMap.boolValue = true;
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(mini && !isViewingWorldMap.boolValue || !mini && isViewingWorldMap.boolValue);
                if (GUILayout.Button("切换至" + (mini ? "小地图" : "大地图") + "模式"))
                {
                    if (mini) manager.ToMiniMap();
                    else manager.ToWorldMap();
                    if (!Application.isPlaying && mapCamera.objectReferenceValue)
                    {
                        Camera cam = (mapCamera.objectReferenceValue as MapCamera).Camera;
                        currentSizeOfCam.floatValue = cam.orthographicSize = defaultSizeOfCam.floatValue;
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.PropertyField(modeInfo, new GUIContent(mini ? "小地图模式信息" : "大地图模式信息"), false);
            if (modeInfo.isExpanded)
            {
                EditorGUILayout.PropertyField(defaultSizeOfCam, new GUIContent("默认相机视野大小"));
                EditorGUILayout.LabelField("当前相机视野大小", currentSizeOfCam.floatValue.ToString());
                EditorGUILayout.Slider(minZoomOfCam, mini ? 1 : miniModeInfo.FindPropertyRelative("defaultSizeOfCam").floatValue, defaultSizeOfCam.floatValue, new GUIContent("相机视野大小下限"));
                EditorGUILayout.Slider(maxZoomOfCam, defaultSizeOfCam.floatValue, mini ? worldModeInfo.FindPropertyRelative("defaultSizeOfCam").floatValue : 255, new GUIContent("相机视野大小上限"));
                if (!worldUI.objectReferenceValue)
                {
                    EditorGUILayout.PropertyField(windowAnchoreMin, new GUIContent("窗口锚点最小值"));
                    EditorGUILayout.PropertyField(windowAnchoreMax, new GUIContent("窗口锚点最大值"));
                    EditorGUILayout.PropertyField(mapAnchoreMin, new GUIContent("地图锚点最小值"));
                    EditorGUILayout.PropertyField(mapAnchoreMax, new GUIContent("地图锚点最大值"));
                    EditorGUILayout.PropertyField(anchoredPosition, new GUIContent("窗口修正位置"));
                    EditorGUILayout.PropertyField(sizeOfWindow, new GUIContent("窗口矩形大小"));
                    EditorGUILayout.PropertyField(sizeOfMap, new GUIContent("地图矩形大小"));
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}