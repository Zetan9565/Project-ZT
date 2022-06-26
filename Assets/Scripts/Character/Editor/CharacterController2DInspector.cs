using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterController2D))]
public class CharacterController2DInspector : Editor
{
    CharacterController2D contoller;
    Type type;
    FieldInfo fieldLastDir;
    SerializedProperty rigidbody;
    SerializedProperty raycastLayer;
    SerializedProperty distanceOffset;

    SerializedProperty moveSpeed;
    SerializedProperty flashForce;
    SerializedProperty defaultDirection;
    SerializedProperty input;

    private void OnEnable()
    {
        contoller = target as CharacterController2D;
        type = contoller.GetType();
        fieldLastDir = type.GetField("latestDirection", BindingFlags.Instance | BindingFlags.NonPublic);
        rigidbody = serializedObject.FindProperty("mRigidbody");
        raycastLayer = serializedObject.FindProperty("raycastLayer");
        distanceOffset = serializedObject.FindProperty("distanceOffset");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        defaultDirection = serializedObject.FindProperty("defaultDirection");
        flashForce = serializedObject.FindProperty("dashForce");
        input = serializedObject.FindProperty("input");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(rigidbody, new GUIContent("刚体(可选)"));
        if (!rigidbody.objectReferenceValue || (rigidbody.objectReferenceValue as Rigidbody2D).collisionDetectionMode == CollisionDetectionMode2D.Discrete)
        {
            EditorGUILayout.PropertyField(raycastLayer, new GUIContent("碰撞检测层"));
            EditorGUILayout.PropertyField(distanceOffset, new GUIContent("检测距离修正量"));
        }
        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("移动速度"));
        EditorGUILayout.PropertyField(flashForce, new GUIContent("闪现力度"));
        EditorGUILayout.PropertyField(defaultDirection, new GUIContent("初始翻滚/闪现方向"));
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(input, new GUIContent("当前方向"));
        EditorGUILayout.Vector2Field("上次方向", (Vector2)fieldLastDir.GetValue(target));
        EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}