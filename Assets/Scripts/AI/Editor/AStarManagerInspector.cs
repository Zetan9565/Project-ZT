using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStarManager))]
public class AStarInspector : Editor
{
    SerializedProperty unwalkableLayer;
    SerializedProperty gizmosGrid;
    SerializedProperty gizmosCast;
    SerializedProperty gridColor;
    SerializedProperty castColor;
    SerializedProperty gizmosEdge;
    SerializedProperty edgeColor;
    SerializedProperty worldSize;
    SerializedProperty threeD;
    SerializedProperty sizeWidth;
    SerializedProperty sizeHeight;
    SerializedProperty baseCellSize;
    SerializedProperty unitSizes;
    SerializedProperty worldHeight;
    SerializedProperty maxUnitHeight;
    SerializedProperty groundLayer;
    SerializedProperty castRadiusMultiple;
    SerializedProperty castCheckType;

    private void OnEnable()
    {
        unwalkableLayer = serializedObject.FindProperty("unwalkableLayer");
        gizmosGrid = serializedObject.FindProperty("gizmosGrid");
        gizmosCast = serializedObject.FindProperty("gizmosCast");
        castColor = serializedObject.FindProperty("castColor");
        gridColor = serializedObject.FindProperty("gridColor");
        gizmosEdge = serializedObject.FindProperty("gizmosEdge");
        edgeColor = serializedObject.FindProperty("edgeColor");
        worldSize = serializedObject.FindProperty("worldSize");
        baseCellSize = serializedObject.FindProperty("baseCellSize");
        unitSizes = serializedObject.FindProperty("unitSizes");
        threeD = serializedObject.FindProperty("threeD");
        sizeWidth = worldSize.FindPropertyRelative("x");
        sizeHeight = worldSize.FindPropertyRelative("y");
        worldHeight = serializedObject.FindProperty("worldHeight");
        maxUnitHeight = serializedObject.FindProperty("maxUnitHeight");
        groundLayer = serializedObject.FindProperty("groundLayer");
        castRadiusMultiple = serializedObject.FindProperty("castRadiusMultiple");
        castCheckType = serializedObject.FindProperty("castCheckType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Gizmos", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(gizmosEdge, new GUIContent("显示边界"));
        if (gizmosEdge.boolValue) EditorGUILayout.PropertyField(edgeColor, new GUIContent("边界颜色"));
        EditorGUILayout.PropertyField(gizmosGrid, new GUIContent("显示网格"));
        if (gizmosGrid.boolValue)
        {
            EditorGUILayout.PropertyField(gridColor, new GUIContent("网格颜色"));
            EditorGUILayout.PropertyField(gizmosCast, new GUIContent("显示检测"));
            if (gizmosCast.boolValue) EditorGUILayout.PropertyField(castColor, new GUIContent("检测颜色"));
        }
        EditorGUILayout.EndVertical();
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(worldSize, new GUIContent("世界平面尺寸"));
            if (sizeWidth.floatValue < 1) sizeWidth.floatValue = 1;
            if (sizeHeight.floatValue < 1) sizeHeight.floatValue = 1;
            EditorGUILayout.PropertyField(threeD, new GUIContent("基于3D空间"));
            if (threeD.boolValue)
            {
                EditorGUILayout.PropertyField(groundLayer, new GUIContent("地面检测层"));
                EditorGUILayout.PropertyField(worldHeight, new GUIContent("世界高度"));
                if (worldHeight.floatValue < 1) worldHeight.floatValue = 1;
                maxUnitHeight.intValue = EditorGUILayout.IntSlider("寻路单位最大高度", maxUnitHeight.intValue, 1, Mathf.RoundToInt(worldHeight.floatValue / baseCellSize.floatValue));
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(unwalkableLayer, new GUIContent("不可行检测层"));
            EditorGUILayout.PropertyField(castCheckType, new GUIContent("可行性检测形状"));
            EditorGUILayout.PropertyField(castRadiusMultiple, new GUIContent("检测半径倍数"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(baseCellSize, new GUIContent("基本单元格大小"));
            EditorGUILayout.PropertyField(unitSizes, new GUIContent("常用寻路单位规格"), true);
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.PropertyField(worldSize, new GUIContent("世界平面尺寸"));
            if (sizeWidth.floatValue < 1) sizeWidth.floatValue = 1;
            if (sizeHeight.floatValue < 1) sizeHeight.floatValue = 1;
            EditorGUILayout.PropertyField(threeD, new GUIContent("基于3D空间"));
            if (threeD.boolValue)
            {
                EditorGUILayout.PropertyField(groundLayer, new GUIContent("地面检测层"));
                EditorGUILayout.PropertyField(worldHeight, new GUIContent("世界高度"));
                if (worldHeight.floatValue < 1) worldHeight.floatValue = 1;
                maxUnitHeight.intValue = EditorGUILayout.IntSlider("寻路单位最大高度", maxUnitHeight.intValue, 1, Mathf.RoundToInt(worldHeight.floatValue / baseCellSize.floatValue));
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(unwalkableLayer, new GUIContent("不可行检测层"));
            EditorGUILayout.PropertyField(castCheckType, new GUIContent("可行性检测形状"));
            EditorGUILayout.PropertyField(castRadiusMultiple, new GUIContent("检测半径倍数"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(baseCellSize, new GUIContent("基本单元格大小"));
            EditorGUILayout.PropertyField(unitSizes, new GUIContent("常用寻路单位规格"), true);
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
