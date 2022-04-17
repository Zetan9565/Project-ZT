﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStarUnit))]
[CanEditMultipleObjects]
public class AStarUnitInspector : Editor
{
    AStarUnit unit;

    new SerializedProperty target;
    SerializedProperty unitSize;
    SerializedProperty targetFootOffset;
    SerializedProperty targetFollowStartDistance;
    SerializedProperty repathRate;
    SerializedProperty footOffset;
    SerializedProperty fixedOffset;
    SerializedProperty moveMode;
    SerializedProperty rigidbody;
    SerializedProperty rigidbody2D;
    SerializedProperty controller;
    SerializedProperty turnSpeed;
    SerializedProperty moveSpeed;
    SerializedProperty slopeLimit;
    SerializedProperty stopDistance;
    SerializedProperty drawGizmos;
    SerializedProperty lineColor;
    SerializedProperty pointColor;
    SerializedProperty pathRenderer;
    SerializedProperty animator;
    SerializedProperty animaHorizontal;
    SerializedProperty animaVertical;
    SerializedProperty animaMagnitude;

    AStarManager manager;

    private void OnEnable()
    {
        unit = base.target as AStarUnit;
        unitSize = serializedObject.FindProperty("unitSize");
        target = serializedObject.FindProperty("target");
        targetFootOffset = serializedObject.FindProperty("targetFootOffset");
        targetFollowStartDistance = serializedObject.FindProperty("targetFollowStartDistance");
        footOffset = serializedObject.FindProperty("footOffset");
        fixedOffset = serializedObject.FindProperty("fixedOffset");
        moveMode = serializedObject.FindProperty("moveMode");
        rigidbody = serializedObject.FindProperty("rigidbody");
        rigidbody2D = serializedObject.FindProperty("rigidbody2D");
        controller = serializedObject.FindProperty("controller");
        turnSpeed = serializedObject.FindProperty("turnSpeed");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        slopeLimit = serializedObject.FindProperty("slopeLimit");
        stopDistance = serializedObject.FindProperty("stopDistance");
        repathRate = serializedObject.FindProperty("repathRate");
        drawGizmos = serializedObject.FindProperty("drawGizmos");
        lineColor = serializedObject.FindProperty("lineColor");
        pointColor = serializedObject.FindProperty("pointColor");
        pathRenderer = serializedObject.FindProperty("pathRenderer");
        animator = serializedObject.FindProperty("animator");
        animaHorizontal = serializedObject.FindProperty("animaHorizontal");
        animaVertical = serializedObject.FindProperty("animaVertical");
        animaMagnitude = serializedObject.FindProperty("animaMagnitude");
        manager = FindObjectOfType<AStarManager>();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        if (manager) EditorGUILayout.LabelField("寻路单元格大小", manager.BaseCellSize.ToString());
        else EditorGUILayout.HelpBox("未找到A*管理器对象!", MessageType.Warning);
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(unitSize, new GUIContent("单位大小"));
        if (unitSize.vector2IntValue.x < 1) unitSize.vector2IntValue = new Vector2Int(1, unitSize.vector2IntValue.y);
        if (unitSize.vector2IntValue.y < 1) unitSize.vector2IntValue = new Vector2Int(unitSize.vector2IntValue.x, 1);
        if (manager)
            EditorGUILayout.LabelField("单位实际大小", string.Format("宽: {0} 高: {1}",
                (manager.BaseCellSize * unitSize.vector2IntValue.x).ToString(), (manager.BaseCellSize * unitSize.vector2IntValue.y).ToString()));
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(footOffset, new GUIContent("自身脚部偏移"));
        EditorGUILayout.PropertyField(fixedOffset, new GUIContent("近似位置修正值"));
        if (fixedOffset.floatValue < 0) fixedOffset.floatValue = 0;
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("Box");
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(target, new GUIContent("跟随目标(可选)"));
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.PropertyField(target, new GUIContent("跟随目标(可选)"));
        }
        if (target.objectReferenceValue)
        {
            if ((target.objectReferenceValue as Transform) != unit.transform)
            {
                EditorGUILayout.PropertyField(targetFootOffset, new GUIContent("目标脚部偏移"));
                EditorGUILayout.PropertyField(targetFollowStartDistance, new GUIContent("目标开始跟随距离"));
                if (Application.isPlaying) GUI.enabled = false;
                EditorGUILayout.PropertyField(repathRate, new GUIContent("目标位置刷新频率(秒)"));
                if (Application.isPlaying) GUI.enabled = true;
            }
            else EditorGUILayout.HelpBox("跟随目标是自身！", MessageType.Warning);
        }
        if (targetFollowStartDistance.floatValue < 0) targetFollowStartDistance.floatValue = 0;
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("Box");
        if (Application.isPlaying)
            GUI.enabled = false;
        EditorGUILayout.PropertyField(moveMode, new GUIContent("移动方式"));
        if (Application.isPlaying)
            GUI.enabled = true;
        if (moveMode.enumValueIndex == 1)
        {
            if (!manager)
            {
                if (!rigidbody2D.objectReferenceValue)
                {
                    EditorGUILayout.PropertyField(rigidbody, new GUIContent("刚体"));
                    if (rigidbody.objectReferenceValue)
                    {
                        EditorGUILayout.PropertyField(turnSpeed, new GUIContent("转向速度"));
                        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
                    }
                }
                if (!rigidbody.objectReferenceValue)
                {
                    EditorGUILayout.PropertyField(rigidbody2D, new GUIContent("2D 刚体"));
                    if (rigidbody2D.objectReferenceValue)
                        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
                }
            }
            else
            {
                if (manager.ThreeD)
                {
                    EditorGUILayout.PropertyField(rigidbody, new GUIContent("刚体"));
                    if (rigidbody.objectReferenceValue)
                    {
                        EditorGUILayout.PropertyField(turnSpeed, new GUIContent("转向速度"));
                        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(rigidbody2D, new GUIContent("2D 刚体"));
                    if (rigidbody2D.objectReferenceValue)
                        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
                }
            }
        }
        else if (moveMode.enumValueIndex == 2)
        {
            EditorGUILayout.PropertyField(controller, new GUIContent("控制器"));
            if (controller.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(turnSpeed, new GUIContent("转向速度"));
                EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
            }
        }
        else
        {
            if (manager && manager.ThreeD || !manager)
                EditorGUILayout.PropertyField(turnSpeed, new GUIContent("转向速度"));
            EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
        }
        if (moveSpeed.floatValue < 0.01f) moveSpeed.floatValue = 0.01f;
        if (manager && manager.ThreeD || !manager)
            slopeLimit.floatValue = EditorGUILayout.Slider("最大移动坡度", slopeLimit.floatValue, 0, 90);
        EditorGUILayout.PropertyField(stopDistance, new GUIContent("提前停止距离"));
        if (stopDistance.floatValue < 0) stopDistance.floatValue = 0;
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("Box");
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(animator, new GUIContent("动画控制器(可选)"));
        if (animator.objectReferenceValue)
        {
            EditorGUILayout.PropertyField(animaHorizontal, new GUIContent("动画水平参数"));
            EditorGUILayout.PropertyField(animaVertical, new GUIContent("动画控垂直参数"));
            EditorGUILayout.PropertyField(animaMagnitude, new GUIContent("动画模参数"));
        }
        if (Application.isPlaying) GUI.enabled = true;
        EditorGUILayout.EndVertical();


        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(pathRenderer, new GUIContent("路线渲染器(可选)"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(drawGizmos, new GUIContent("绘制Gizmos"));
        if (drawGizmos.boolValue)
        {
            EditorGUILayout.PropertyField(lineColor, new GUIContent("连线颜色"));
            EditorGUILayout.PropertyField(pointColor, new GUIContent("折点颜色"));
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}