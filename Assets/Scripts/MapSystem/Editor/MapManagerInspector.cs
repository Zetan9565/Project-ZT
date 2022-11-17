using UnityEngine;
using UnityEditor;
using ZetanStudio;

[CustomEditor(typeof(MapManager))]
public class MapManagerInspector : SingletonMonoBehaviourInspector
{
    MapManager manager;

    SerializedProperty UI;
    SerializedProperty updateMode;
    SerializedProperty player;
    SerializedProperty offset;
    SerializedProperty playerIcon;
    SerializedProperty playerIconSize;
    SerializedProperty mapCamera;
    SerializedProperty cameraPrefab;
    SerializedProperty textueSize;
    SerializedProperty textueFormat;
    SerializedProperty mapRenderMask;
    SerializedProperty use2D;
    SerializedProperty rotateMap;
    SerializedProperty edgeSize;
    SerializedProperty worldEdgeSize;
    SerializedProperty circle;
    SerializedProperty radius;
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

        UI = serializedObject.FindProperty("UI");
        updateMode = serializedObject.FindProperty("updateMode");
        player = serializedObject.FindProperty("player");
        offset = serializedObject.FindProperty("offset");
        playerIcon = serializedObject.FindProperty("playerIcon");
        playerIconSize = serializedObject.FindProperty("playerIconSize");
        mapCamera = serializedObject.FindProperty("mapCamera");
        cameraPrefab = serializedObject.FindProperty("cameraPrefab");
        textueSize = serializedObject.FindProperty("textureSize");
        textueFormat = serializedObject.FindProperty("textureFormat");
        mapRenderMask = serializedObject.FindProperty("mapRenderMask");
        use2D = serializedObject.FindProperty("use2D");
        rotateMap = serializedObject.FindProperty("rotateMap");
        edgeSize = serializedObject.FindProperty("edgeSize");
        worldEdgeSize = serializedObject.FindProperty("worldEdgeSize");
        circle = serializedObject.FindProperty("circle");
        radius = serializedObject.FindProperty("radius");
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
        EditorGUILayout.PropertyField(UI);
        if (UI.objectReferenceValue)
        {
            int mode = circle.boolValue ? 1 : 0;
            int index = EditorGUILayout.IntPopup("边框形状", mode, new string[] { "矩形", "圆形" }, new int[] { 0, 1 });
            circle.boolValue = index == 0 ? false : true;
            if (!circle.boolValue) EditorGUILayout.PropertyField(edgeSize, new GUIContent("边框厚度"));
            if (circle.boolValue) EditorGUILayout.PropertyField(radius, new GUIContent("半径"));

            if (Application.isPlaying) GUI.enabled = false;
            playerIcon.objectReferenceValue = EditorGUILayout.ObjectField("主图标", playerIcon.objectReferenceValue as Sprite, typeof(Sprite), false);
            EditorGUILayout.PropertyField(playerIconSize, new GUIContent("主图标大小"));
            defaultMarkIcon.objectReferenceValue = EditorGUILayout.ObjectField("默认标记图标", defaultMarkIcon.objectReferenceValue as Sprite, typeof(Sprite), false);
            EditorGUILayout.PropertyField(defaultMarkSize, new GUIContent("默认标记大小"));
            EditorGUILayout.PropertyField(cameraPrefab, new GUIContent("地图相机预制件"));
            if (!Application.isPlaying) GUI.enabled = false;
            EditorGUILayout.PropertyField(mapCamera, new GUIContent("地图相机"));
            if (!Application.isPlaying) GUI.enabled = true;
            EditorGUILayout.PropertyField(textueSize, new GUIContent("相机采样分辨率"));
            EditorGUILayout.PropertyField(textueFormat, new GUIContent("相机采样格式"));
            if (Application.isPlaying) GUI.enabled = true;
            EditorGUILayout.PropertyField(mapRenderMask, new GUIContent("地图相机可视层"));
            if (mapCamera.objectReferenceValue)
            {
                Camera cam = (mapCamera.objectReferenceValue as MapCamera).Camera;
                cam.cullingMask = mapRenderMask.intValue;
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(worldEdgeSize, new GUIContent("大地图边框厚度"));
            EditorGUILayout.PropertyField(dragSensitivity, new GUIContent("大地图拖拽灵敏度"));
            EditorGUILayout.PropertyField(animationSpeed, new GUIContent("动画速度"));
            if (Application.isPlaying) GUI.enabled = false;
            EditorGUILayout.PropertyField(isViewingWorldMap, new GUIContent("当前是大地图模式"));
            if (Application.isPlaying) GUI.enabled = true;
            DrawModeInfo(miniModeInfo, true);
            DrawModeInfo(worldModeInfo, false);
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    private void DrawModeInfo(SerializedProperty modeInfo, bool mini)
    {
        MapUI UI = this.UI.objectReferenceValue as MapUI;
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
            EditorGUILayout.BeginHorizontal();
            if (Application.isPlaying || mini && isViewingWorldMap.boolValue || !mini && !isViewingWorldMap.boolValue) GUI.enabled = false;
            if (GUILayout.Button("以当前状态作为" + (mini ? "小地图" : "大地图")))
            {
                if (!(mini && isViewingWorldMap.boolValue))
                {
                    if (mapCamera.objectReferenceValue)
                    {
                        defaultSizeOfCam.floatValue = (mapCamera.objectReferenceValue as MapCamera).Camera.orthographicSize;
                        currentSizeOfCam.floatValue = (mapCamera.objectReferenceValue as MapCamera).Camera.orthographicSize;
                    }
                    windowAnchoreMin.vector2Value = UI.mapWindowRect.anchorMin;
                    windowAnchoreMax.vector2Value = UI.mapWindowRect.anchorMax;
                    mapAnchoreMin.vector2Value = UI.mapRect.anchorMin;
                    mapAnchoreMax.vector2Value = UI.mapRect.anchorMax;
                    anchoredPosition.vector2Value = UI.mapWindowRect.anchoredPosition;
                    sizeOfWindow.vector2Value = UI.mapWindowRect.sizeDelta;
                    sizeOfMap.vector2Value = UI.mapRect.sizeDelta;
                    if (mini) isViewingWorldMap.boolValue = false;
                    else isViewingWorldMap.boolValue = true;
                }
            }
            if (Application.isPlaying || mini && isViewingWorldMap.boolValue || !mini && !isViewingWorldMap.boolValue) GUI.enabled = true;
            if (mini && !isViewingWorldMap.boolValue || !mini && isViewingWorldMap.boolValue) GUI.enabled = false;
            if (GUILayout.Button("切换至" + (mini ? "小地图" : "大地图") + "模式"))
            {
                if (mini) manager.ToMiniMap();
                else manager.ToWorldMap();
            }
            if (mini && !isViewingWorldMap.boolValue || !mini && isViewingWorldMap.boolValue) GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(modeInfo, new GUIContent(mini ? "小地图模式信息" : "大地图模式信息"), false);
            if (modeInfo.isExpanded)
            {
                EditorGUILayout.PropertyField(defaultSizeOfCam, new GUIContent("默认相机视野大小"));                
                EditorGUILayout.LabelField("当前相机视野大小", currentSizeOfCam.floatValue.ToString());
                EditorGUILayout.Slider(minZoomOfCam, mini ? 1 : miniModeInfo.FindPropertyRelative("defaultSizeOfCam").floatValue, defaultSizeOfCam.floatValue, new GUIContent("相机视野大小下限"));
                EditorGUILayout.Slider(maxZoomOfCam, defaultSizeOfCam.floatValue, mini ? worldModeInfo.FindPropertyRelative("defaultSizeOfCam").floatValue : 255, new GUIContent("相机视野大小上限"));
                EditorGUILayout.PropertyField(windowAnchoreMin, new GUIContent("窗口锚点最小值"));
                EditorGUILayout.PropertyField(windowAnchoreMax, new GUIContent("窗口锚点最大值"));
                EditorGUILayout.PropertyField(mapAnchoreMin, new GUIContent("地图锚点最小值"));
                EditorGUILayout.PropertyField(mapAnchoreMax, new GUIContent("地图锚点最大值"));
                EditorGUILayout.PropertyField(anchoredPosition, new GUIContent("窗口修正位置"));
                EditorGUILayout.PropertyField(sizeOfWindow, new GUIContent("窗口矩形大小"));
                EditorGUILayout.PropertyField(sizeOfMap, new GUIContent("地图矩形大小"));
            }
            EditorGUILayout.EndVertical();
        }
    }
}