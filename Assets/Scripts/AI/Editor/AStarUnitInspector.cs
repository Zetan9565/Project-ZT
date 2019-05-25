using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStarUnit))]
[CanEditMultipleObjects]
public class AStarUnitInspector : Editor
{
    new SerializedProperty target;
    SerializedProperty unitSize;
    SerializedProperty targetFootOffset;
    SerializedProperty targetFollowStartDistance;
    SerializedProperty footOffset;
    SerializedProperty fixedOffset;
    SerializedProperty moveType;
    SerializedProperty rigidbody;
    SerializedProperty rigidbody2D;
    SerializedProperty turnSpeed;
    SerializedProperty moveSpeed;
    SerializedProperty maxMoveSlope;
    SerializedProperty stopDistance;
    SerializedProperty drawGizmos;
    SerializedProperty pathRenderer;

    AStarManager AStar;

    private void OnEnable()
    {
        unitSize = serializedObject.FindProperty("unitSize");
        target = serializedObject.FindProperty("target");
        targetFootOffset = serializedObject.FindProperty("targetFootOffset");
        targetFollowStartDistance = serializedObject.FindProperty("targetFollowStartDistance");
        footOffset = serializedObject.FindProperty("footOffset");
        fixedOffset = serializedObject.FindProperty("fixedOffset");
        moveType = serializedObject.FindProperty("moveType");
        rigidbody = serializedObject.FindProperty("rigidbody");
        rigidbody2D = serializedObject.FindProperty("rigidbody2D");
        turnSpeed = serializedObject.FindProperty("turnSpeed");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        maxMoveSlope = serializedObject.FindProperty("maxMoveSlope");
        stopDistance = serializedObject.FindProperty("stopDistance");
        drawGizmos = serializedObject.FindProperty("drawGizmos");
        pathRenderer = serializedObject.FindProperty("pathRenderer");
        AStar = FindObjectOfType<AStarManager>();
    }

    public override void OnInspectorGUI()
    {
        if (AStar) EditorGUILayout.LabelField("寻路单元格大小", AStar.BaseCellSize.ToString());
        else EditorGUILayout.HelpBox("未找到A*对象!", MessageType.Warning);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(unitSize, new GUIContent("单位大小"));
        if (unitSize.intValue < 1) unitSize.intValue = 1;
        if (AStar) EditorGUILayout.LabelField("单位实际大小", (AStar.BaseCellSize * unitSize.intValue).ToString());
        EditorGUILayout.PropertyField(footOffset, new GUIContent("自身脚部偏移"));
        EditorGUILayout.PropertyField(fixedOffset, new GUIContent("近似位置修正值"));
        if (fixedOffset.floatValue < 0) fixedOffset.floatValue = 0;
        EditorGUILayout.Space();
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(target, new GUIContent("目标"));
            EditorGUILayout.PropertyField(targetFootOffset, new GUIContent("目标脚部偏移"));
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.PropertyField(target, new GUIContent("目标"));
            EditorGUILayout.PropertyField(targetFootOffset, new GUIContent("目标脚部偏移"));
        }
        EditorGUILayout.PropertyField(targetFollowStartDistance, new GUIContent("目标跟随距离修正值"));
        if (targetFollowStartDistance.floatValue < 0) targetFollowStartDistance.floatValue = 0;
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(moveType, new GUIContent("移动方式"));
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(moveType, new GUIContent("移动方式"));
        }
        if (moveType.enumValueIndex != 0)
        {
            if (!AStarManager.Instance)
            {
                if (!rigidbody2D.objectReferenceValue)
                {
                    EditorGUILayout.PropertyField(rigidbody, new GUIContent("刚体"));
                    if (rigidbody.objectReferenceValue) EditorGUILayout.PropertyField(turnSpeed, new GUIContent("转向速度"));
                }
                if (!rigidbody.objectReferenceValue) EditorGUILayout.PropertyField(rigidbody2D, new GUIContent("2D 刚体"));
            }
            else
            {
                if (AStarManager.Instance.ThreeD)
                {
                    EditorGUILayout.PropertyField(rigidbody, new GUIContent("刚体"));
                    if (rigidbody.objectReferenceValue) EditorGUILayout.PropertyField(turnSpeed, new GUIContent("转向速度"));
                }
                else EditorGUILayout.PropertyField(rigidbody2D, new GUIContent("2D 刚体"));
            }
        }
        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
        if (moveSpeed.floatValue < 0.01f) moveSpeed.floatValue = 0.01f;
        if (AStarManager.Instance && AStarManager.Instance.ThreeD || !AStarManager.Instance) maxMoveSlope.floatValue = EditorGUILayout.Slider("最大移动坡度", maxMoveSlope.floatValue, 0, 90);
        EditorGUILayout.PropertyField(stopDistance, new GUIContent("提前停止距离"));
        if (stopDistance.floatValue < 0) stopDistance.floatValue = 0;
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(pathRenderer, new GUIContent("路线渲染器"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(drawGizmos, new GUIContent("绘制Gizmos"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}