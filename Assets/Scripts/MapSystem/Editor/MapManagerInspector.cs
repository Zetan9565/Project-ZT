using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class MapManagerInspector : Editor
{
    SerializedProperty UI;
    SerializedProperty updateMode;
    SerializedProperty player;
    SerializedProperty playerIcon;
    SerializedProperty playerIconSize;
    SerializedProperty camera;
    SerializedProperty targetTexture;
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
        UI = serializedObject.FindProperty("UI");
        updateMode = serializedObject.FindProperty("updateMode");
        player = serializedObject.FindProperty("player");
        playerIcon = serializedObject.FindProperty("playerIcon");
        playerIconSize = serializedObject.FindProperty("playerIconSize");
        camera = serializedObject.FindProperty("camera");
        targetTexture = serializedObject.FindProperty("targetTexture");
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
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(use2D, new GUIContent("2D"));
        EditorGUILayout.PropertyField(rotateMap, new GUIContent("旋转地图"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(UI);
        EditorGUILayout.PropertyField(updateMode, new GUIContent("更新方式"));
        if (UI.objectReferenceValue)
        {
            int mode = circle.boolValue ? 1 : 0;
            int index = EditorGUILayout.IntPopup("边框形状", mode, new string[] { "矩形", "圆形" }, new int[] { 0, 1 });
            circle.boolValue = index == 0 ? false : true;
            if (!circle.boolValue) EditorGUILayout.PropertyField(edgeSize, new GUIContent("边框厚度"));
            if (circle.boolValue) EditorGUILayout.PropertyField(radius, new GUIContent("半径"));
        }
        EditorGUILayout.PropertyField(player, new GUIContent("跟随对象"));
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(playerIcon, new GUIContent("主图标"));
        EditorGUILayout.PropertyField(playerIconSize, new GUIContent("主图标大小"));
        EditorGUILayout.PropertyField(defaultMarkIcon, new GUIContent("默认标记图标"));
        EditorGUILayout.PropertyField(defaultMarkSize, new GUIContent("默认标记大小"));
        if (Application.isPlaying) GUI.enabled = true;
        if (playerIcon.objectReferenceValue || defaultMarkIcon.objectReferenceValue)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.x, rect.y + rect.height * 1.5f, rect.width, rect.height), "主图标");
            EditorGUI.LabelField(new Rect(rect.x + rect.width / 2, rect.y + rect.height * 1.5f, rect.width, rect.height), "默认标记图标");
            GUI.enabled = false;
            if (playerIcon.objectReferenceValue) EditorGUI.ObjectField(new Rect(rect.x - rect.width / 2f, rect.y, rect.width, rect.height * 4),
                 string.Empty, playerIcon.objectReferenceValue as Sprite, typeof(Texture2D), false);
            if (defaultMarkIcon.objectReferenceValue) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, rect.height * 4),
                 string.Empty, defaultMarkIcon.objectReferenceValue as Sprite, typeof(Texture2D), false);
            GUI.enabled = true;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(camera, new GUIContent("地图相机"));
        EditorGUILayout.PropertyField(targetTexture, new GUIContent("采样贴图"));
        if (Application.isPlaying) GUI.enabled = true;
        EditorGUILayout.PropertyField(mapRenderMask, new GUIContent("地图相机可视层"));
        if (camera.objectReferenceValue && targetTexture.objectReferenceValue)
            (camera.objectReferenceValue as Camera).targetTexture = targetTexture.objectReferenceValue as RenderTexture;
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(worldEdgeSize, new GUIContent("大地图边框厚度"));
        EditorGUILayout.PropertyField(dragSensitivity, new GUIContent("大地图拖拽灵敏度"));
        EditorGUILayout.PropertyField(animationSpeed, new GUIContent("动画速度"));
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(isViewingWorldMap, new GUIContent("当前是大地图模式"));
        if (Application.isPlaying) GUI.enabled = true;
        DrawModeInfo(miniModeInfo, true);
        DrawModeInfo(worldModeInfo, false);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    private void DrawModeInfo(SerializedProperty modeInfo, bool mini)
    {
        MapUI UI = this.UI.objectReferenceValue as MapUI;
        if (UI)
        {
            SerializedProperty sizeOfCam = modeInfo.FindPropertyRelative("sizeOfCam");
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
                    if (camera.objectReferenceValue) sizeOfCam.floatValue = (camera.objectReferenceValue as Camera).orthographicSize;
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
                if (mini) (target as MapManager).ToMiniMap();
                else (target as MapManager).ToWorldMap();
            }
            if (mini && !isViewingWorldMap.boolValue || !mini && isViewingWorldMap.boolValue) GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(modeInfo, new GUIContent(mini ? "小地图模式信息" : "大地图模式信息"));
            if (modeInfo.isExpanded)
            {
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("sizeOfCam"), new GUIContent("地图相机视野大小"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("windowAnchoreMin"), new GUIContent("窗口锚点最小值"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("windowAnchoreMax"), new GUIContent("窗口锚点最大值"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("mapAnchoreMin"), new GUIContent("地图锚点最小值"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("mapAnchoreMax"), new GUIContent("地图锚点最大值"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("anchoredPosition"), new GUIContent("窗口修正位置"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("sizeOfWindow"), new GUIContent("窗口矩形大小"));
                EditorGUILayout.PropertyField(modeInfo.FindPropertyRelative("sizeOfMap"), new GUIContent("地图矩形大小"));
            }
            EditorGUILayout.EndVertical();
        }
    }
}