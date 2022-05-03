using System;
using UnityEditor;
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
        if (other) EditorGUILayout.HelpBox($"配方材料重复！配置路径：\n{AssetDatabase.GetAssetPath(other)}", MessageType.Error);
        for (int i = 0; i < formulation.Materials.Count; i++)
        {
            bool bre = false;
            var left = formulation.Materials[i];
            if (!left.IsValid)
            {
                EditorGUILayout.HelpBox("该配方未补充完整", MessageType.Warning);
                break;
            }
            for (int j = 0; j < formulation.Materials.Count; j++)
            {
                var right = formulation.Materials[j];
                if (i != j && left.MakingType == right.MakingType)
                {
                    if (left.MakingType == MakingType.SingleItem && left.Item == right.Item || left.MakingType == MakingType.SameType && left.MaterialType == right.MaterialType)
                    {
                        EditorGUILayout.HelpBox($"第[{i + 1}]和第[{j + 1}]个材料重复！", MessageType.Error);
                        bre = true;
                    }
                }
                if (bre) break;
            }
            if (bre) break;
        }
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(remark, new GUIContent("备注"));
        listDrawer.DoLayoutDraw();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
