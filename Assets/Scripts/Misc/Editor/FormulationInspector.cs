using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[CustomEditor(typeof(Formulation))]
public class FormulationInspector : Editor
{
    Formulation formulation;

    SerializedProperty remark;
    SerializedProperty materials;

    MaterialListDrawer listDrawer;

    Formulation[] formulations;

    private void OnEnable()
    {
        formulation = target as Formulation;
        formulations = Resources.LoadAll<Formulation>("Configuration");
        remark = serializedObject.FindProperty("remark");
        materials = serializedObject.FindProperty("materials");
        listDrawer = new MaterialListDrawer(serializedObject, materials, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight + 2);
    }

    public override void OnInspectorGUI()
    {
        var other = Array.Find(formulations, x => x != formulation && Formulation.CheckMaterialsDuplicate(formulation, x));
        if (other) EditorGUILayout.HelpBox($"�䷽�����ظ�������·����\n{AssetDatabase.GetAssetPath(other)}", MessageType.Error);
        for (int i = 0; i < formulation.Materials.Count; i++)
        {
            bool bre = false;
            var left = formulation.Materials[i];
            if (!left.IsValid)
            {
                EditorGUILayout.HelpBox("���䷽δ��������", MessageType.Warning);
                break;
            }
            for (int j = 0; j < formulation.Materials.Count; j++)
            {
                var right = formulation.Materials[j];
                if (i != j && left.MakingType == right.MakingType)
                {
                    if (left.MakingType == MakingType.SingleItem && left.Item == right.Item || left.MakingType == MakingType.SameType && left.MaterialType == right.MaterialType)
                    {
                        EditorGUILayout.HelpBox($"��[{i + 1}]�͵�[{j + 1}]�������ظ���", MessageType.Error);
                        bre = true;
                    }
                }
                if (bre) break;
            }
            if (bre) break;
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(remark, new GUIContent("��ע"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        listDrawer.DoLayoutDraw();
    }
}
