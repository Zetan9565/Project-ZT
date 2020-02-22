using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapIconHolder))]
public class MapIconHolderInspector : Editor
{
    MapIconHolder holder;

    SerializedProperty icon;
    SerializedProperty iconSize;
    SerializedProperty iconType;
    SerializedProperty drawOnWorldMap;
    SerializedProperty keepOnMap;
    SerializedProperty maxValidDistance;
    SerializedProperty forceHided;
    SerializedProperty removeAble;
    SerializedProperty showRange;
    SerializedProperty rangeColor;
    SerializedProperty rangeSize;
    SerializedProperty textToDisplay;
    SerializedProperty iconEvents;
    SerializedProperty onFingerClick;
    SerializedProperty onMouseClick;
    SerializedProperty onMouseEnter;
    SerializedProperty onMouseExit;
    SerializedProperty gizmos;

    private void OnEnable()
    {
        holder = target as MapIconHolder;

        icon = serializedObject.FindProperty("icon");
        iconSize = serializedObject.FindProperty("iconSize");
        iconType = serializedObject.FindProperty("iconType");
        drawOnWorldMap = serializedObject.FindProperty("drawOnWorldMap");
        keepOnMap = serializedObject.FindProperty("keepOnMap");
        maxValidDistance = serializedObject.FindProperty("maxValidDistance");
        forceHided = serializedObject.FindProperty("forceHided");
        removeAble = serializedObject.FindProperty("removeAble");
        showRange = serializedObject.FindProperty("showRange");
        rangeColor = serializedObject.FindProperty("rangeColor");
        rangeSize = serializedObject.FindProperty("rangeSize");
        textToDisplay = serializedObject.FindProperty("textToDisplay");
        iconEvents = serializedObject.FindProperty("iconEvents");
        onFingerClick = iconEvents.FindPropertyRelative("onFingerClick");
        onMouseClick = iconEvents.FindPropertyRelative("onMouseClick");
        onMouseEnter = iconEvents.FindPropertyRelative("onMouseEnter");
        onMouseExit = iconEvents.FindPropertyRelative("onMouseExit");
        gizmos = serializedObject.FindProperty("gizmos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        if (Application.isPlaying) if (GUILayout.Button("重新生成")) holder.CreateIcon();
        icon.objectReferenceValue = EditorGUILayout.ObjectField("图标", icon.objectReferenceValue as Sprite, typeof(Sprite), false);
        EditorGUILayout.PropertyField(iconSize, new GUIContent("图标大小"));
        EditorGUILayout.IntPopup(iconType, new GUIContent[] { new GUIContent("普通"), new GUIContent("任务") }, new int[] { 0, 3 }, new GUIContent("图标类型"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(keepOnMap, new GUIContent("保持显示"));
        EditorGUILayout.PropertyField(drawOnWorldMap, new GUIContent("在大地图上显示"));
        EditorGUILayout.EndHorizontal();
        //if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(maxValidDistance, new GUIContent("最大有效显示距离"));
        EditorGUILayout.PropertyField(textToDisplay, new GUIContent("显示内容"));
        //if (Application.isPlaying) GUI.enabled = true;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(forceHided, new GUIContent("强制隐藏"));
        EditorGUILayout.PropertyField(removeAble, new GUIContent("可移除"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(showRange, new GUIContent("显示范围"));
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(gizmos, new GUIContent("显示预览"));
        if (Application.isPlaying) GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        if (showRange.boolValue)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(rangeColor, new GUIContent("范围颜色"));
            EditorGUILayout.PropertyField(rangeSize, new GUIContent("范围半径"));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.PropertyField(iconEvents, new GUIContent("事件"), false);
        if (iconEvents.isExpanded)
        {
            EditorGUILayout.PropertyField(onFingerClick, new GUIContent("手指点击事件"));
            EditorGUILayout.PropertyField(onMouseClick, new GUIContent("鼠标点击事件"));
            EditorGUILayout.PropertyField(onMouseEnter, new GUIContent("鼠标滑入事件"));
            EditorGUILayout.PropertyField(onMouseExit, new GUIContent("鼠标滑出事件"));
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying) holder.SetIconValidDistance(maxValidDistance.floatValue);
        }
    }
}