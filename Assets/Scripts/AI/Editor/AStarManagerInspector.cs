using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Editor
{
    [CustomEditor(typeof(AStarManager))]
    public class AStarInspector : SingletonMonoBehaviourInspector
    {
        SerializedProperty unwalkableLayer;
        SerializedProperty gizmosGrid;
        SerializedProperty gizmosUnitSize;
        SerializedProperty gridColor;
        SerializedProperty gizmosPriview;
        SerializedProperty expendGraphs;
        SerializedProperty gizmosEdge;
        SerializedProperty edgeColor;
        SerializedProperty worldSize;
        SerializedProperty threeD;
        SerializedProperty sizeWidth;
        SerializedProperty sizeHeight;
        SerializedProperty baseCellSize;
        SerializedProperty unitSizes;
        SerializedProperty worldHeight;
        SerializedProperty groundLayer;
        SerializedProperty castRadiusMultiple;
        SerializedProperty castCheckType;
        SerializedProperty pathLog;

        AstarPath pathFinder;

        private void OnEnable()
        {
            unwalkableLayer = serializedObject.FindProperty("unwalkableLayer");
            gizmosGrid = serializedObject.FindProperty("gizmosGrid");
            gizmosUnitSize = serializedObject.FindProperty("gizmosUnitSize");
            gizmosPriview = serializedObject.FindProperty("gizmosPriview");
            expendGraphs = serializedObject.FindProperty("expendGraphs");
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
            groundLayer = serializedObject.FindProperty("groundLayer");
            castRadiusMultiple = serializedObject.FindProperty("castRadiusMultiple");
            castCheckType = serializedObject.FindProperty("castCheckType");
            pathLog = serializedObject.FindProperty("pathLog");
            pathFinder = (target as AStarManager).GetComponent<AstarPath>();
        }

        public override void OnInspectorGUI()
        {
            if (!CheckValid(out string text))
                EditorGUILayout.HelpBox(text, MessageType.Error);
            else
            {
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Gizmos", new GUIStyle() { fontStyle = FontStyle.Bold });
                EditorGUILayout.PropertyField(gizmosPriview, new GUIContent("显示预览"));
                if (gizmosPriview.boolValue)
                {
                    EditorGUILayout.PropertyField(gizmosEdge, new GUIContent("显示边界"));
                    if (gizmosEdge.boolValue) EditorGUILayout.PropertyField(edgeColor, new GUIContent("边界颜色"));
                }
                EditorGUILayout.PropertyField(gizmosGrid, new GUIContent("显示网格"));
                if (gizmosGrid.boolValue)
                {
                    EditorGUILayout.PropertyField(gizmosUnitSize, new GUIContent("预览寻路单位大小"));
                    if (gizmosUnitSize.vector2IntValue.x < 1) gizmosUnitSize.vector2IntValue = new Vector2Int(1, gizmosUnitSize.vector2IntValue.y);
                    if (gizmosUnitSize.vector2IntValue.y < 1) gizmosUnitSize.vector2IntValue = new Vector2Int(gizmosUnitSize.vector2IntValue.x, 1);
                    EditorGUILayout.PropertyField(gridColor, new GUIContent("网格颜色"));
                    if (pathFinder && pathFinder.graphs.Length > 0)
                    {
                        EditorGUILayout.PropertyField(expendGraphs, new GUIContent("显示网格列表"));
                        pathFinder.showGraphs = expendGraphs.boolValue;
                        if (expendGraphs.boolValue)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            foreach (NavGraph graph in pathFinder.graphs)
                            {
                                if (graph == null) continue;
                                EditorGUI.BeginChangeCheck();
                                bool draw = EditorGUILayout.Toggle("显示网格" + graph.name, graph.drawGizmos);
                                if (EditorGUI.EndChangeCheck())
                                    graph.drawGizmos = draw;
                            }
                            EditorGUILayout.EndVertical();
                        }
                        else pathFinder.showGraphs = false;
                    }
                }
                if (pathFinder)
                {
                    pathFinder.showNavGraphs = gizmosGrid.boolValue;
                    pathFinder.colorSettings._SolidColor = gridColor.colorValue;
                }
                EditorGUILayout.EndVertical();
                if (Application.isPlaying) GUI.enabled = false;
                EditorGUILayout.PropertyField(worldSize, new GUIContent("世界平面尺寸"));
                if (sizeWidth.floatValue < 1) sizeWidth.floatValue = 1;
                if (sizeHeight.floatValue < 1) sizeHeight.floatValue = 1;
                EditorGUILayout.PropertyField(threeD, new GUIContent("基于3D空间"));
                if (threeD.boolValue)
                {
                    EditorGUILayout.PropertyField(groundLayer, new GUIContent("地面检测层"));
                    EditorGUILayout.PropertyField(worldHeight, new GUIContent("世界高度"));
                    if (worldHeight.floatValue < 1) worldHeight.floatValue = 1;
                }
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(unwalkableLayer, new GUIContent("不可行走检测层"));
                if (threeD.boolValue)
                    castCheckType.enumValueIndex = EditorGUILayout.IntPopup("可行性检测形状", castCheckType.enumValueIndex,
                        new string[] { "球体", "胶囊体", "射线" }, new int[] { 0, 1, 2 });
                else castCheckType.enumValueIndex = EditorGUILayout.IntPopup("可行性检测形状", castCheckType.enumValueIndex,
                         new string[] { "圆形", "点" }, new int[] { 0, 2 });
                EditorGUILayout.PropertyField(castRadiusMultiple, new GUIContent("检测半径倍数"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(baseCellSize, new GUIContent("基本单元格大小"));
                EditorGUILayout.PropertyField(unitSizes, new GUIContent("常用寻路单位规格"), true);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(pathLog, new GUIContent("路径Log内容"));
                if (Application.isPlaying) GUI.enabled = true;
                if (pathFinder) pathFinder.logPathResults = (PathLog)pathLog.enumValueIndex;
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
        }
    }
}