using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterMotion2D))]
public class CharacterMotion2DInspector : Editor
{
    CharacterMotion2D motion;
    Type type;
    FieldInfo fieldMoveDir;
    FieldInfo fieldLastDir;
    SerializedProperty rigidbody;
    SerializedProperty raycastLayer;
    SerializedProperty distanceOffset;

    SerializedProperty moveSpeed;
    SerializedProperty dashForce;
    SerializedProperty rollForce;
    SerializedProperty rollStopSpeed;
    SerializedProperty rollSpeedDownMult;
    SerializedProperty defaultDirection;

    private void OnEnable()
    {
        motion = target as CharacterMotion2D;
        type = motion.GetType();
        fieldMoveDir = type.GetField("moveDirection", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldLastDir = type.GetField("latestDirection", BindingFlags.Instance | BindingFlags.NonPublic);
        rigidbody = serializedObject.FindProperty("mRigidbody");
        raycastLayer = serializedObject.FindProperty("raycastLayer");
        distanceOffset = serializedObject.FindProperty("distanceOffset");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        rollForce = serializedObject.FindProperty("rollForce");
        rollStopSpeed = serializedObject.FindProperty("rollStopSpeed");
        rollSpeedDownMult = serializedObject.FindProperty("rollSpeedDownMult");
        defaultDirection = serializedObject.FindProperty("defaultDirection");
        dashForce = serializedObject.FindProperty("dashForce");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(rigidbody, new GUIContent("刚体(可选)"));
        if (!rigidbody.objectReferenceValue || (rigidbody.objectReferenceValue as Rigidbody2D).collisionDetectionMode == CollisionDetectionMode2D.Discrete)
        {
            EditorGUILayout.PropertyField(raycastLayer, new GUIContent("碰撞检测层"));
            EditorGUILayout.PropertyField(distanceOffset, new GUIContent("检测距离修正量"));
        }
        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
        EditorGUILayout.PropertyField(dashForce, new GUIContent("闪现力度"));
        EditorGUILayout.PropertyField(rollForce, new GUIContent("翻滚力度"));
        EditorGUILayout.PropertyField(rollStopSpeed, new GUIContent("翻滚停止阈值"));
        EditorGUILayout.PropertyField(rollSpeedDownMult, new GUIContent("翻滚速度下降幅度"));
        EditorGUILayout.PropertyField(defaultDirection, new GUIContent("初始翻滚/闪现方向"));
        GUI.enabled = false;
        EditorGUILayout.Vector2Field("当前方向", (Vector2)fieldMoveDir.GetValue(target));
        EditorGUILayout.Vector2Field("上次方向", (Vector2)fieldLastDir.GetValue(target));
        GUI.enabled = true;
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}