using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WarehouseAgent))]
public class WarehouseAgentInspector : Editor
{
    WarehouseAgent agent;

    SerializedProperty warehouse;

    private void OnEnable()
    {
        agent = target as WarehouseAgent;
        warehouse = serializedObject.FindProperty("warehouse");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("识别码", agent.ID);
        SerializedProperty warehouseSize = warehouse.FindPropertyRelative("warehouseSize");
        warehouseSize.FindPropertyRelative("max").intValue = EditorGUILayout.IntSlider("默认仓库容量(格)",
            warehouseSize.FindPropertyRelative("max").intValue, 100, 500);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
