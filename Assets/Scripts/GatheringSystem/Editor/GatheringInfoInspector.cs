using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceInformation))]
public class GatheringInfoInspector : Editor
{
    ResourceInformation info;

    SerializedProperty _ID;
    SerializedProperty _name;
    SerializedProperty gatherType;
    SerializedProperty gatherTime;
    SerializedProperty refreshTime;
    SerializedProperty lootPrefab;
    SerializedProperty productItems;

    DropItemListDrawer dropList;
    float lineHeight;
    float lineHeightSpace;

    ResourceInformation[] gathering;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;

        info = target as ResourceInformation;

        _ID = serializedObject.FindProperty("_ID");
        _name = serializedObject.FindProperty("_name");
        gatherType = serializedObject.FindProperty("gatherType");
        gatherTime = serializedObject.FindProperty("gatherTime");
        refreshTime = serializedObject.FindProperty("refreshTime");
        lootPrefab = serializedObject.FindProperty("lootPrefab");
        productItems = serializedObject.FindProperty("productItems");
        dropList = new DropItemListDrawer(serializedObject, productItems, lineHeight, lineHeightSpace);

        gathering = Resources.LoadAll<ResourceInformation>("Configuration");
    }

    public override void OnInspectorGUI()
    {
        if (!CheckEditComplete())
            EditorGUILayout.HelpBox("该采集物存在未补全信息。", MessageType.Warning);
        else
        {
            EditorGUILayout.HelpBox("该采集物信息已完整。", MessageType.Info);
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            else
                EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_name, new GUIContent("名称"));
        EditorGUILayout.PropertyField(gatherType, new GUIContent("采集方法"));
        EditorGUILayout.PropertyField(gatherTime, new GUIContent("采集耗时"));
        EditorGUILayout.PropertyField(refreshTime, new GUIContent("刷新时间"));
        EditorGUILayout.PropertyField(lootPrefab, new GUIContent("掉落预制件"));
        EditorGUILayout.PropertyField(productItems, new GUIContent("产出道具"), false);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        if (productItems.isExpanded)
        {
            serializedObject.Update();
            dropList.DoLayoutDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }

    private bool CheckEditComplete()
    {
        bool editCmplt = true;

        editCmplt &= !string.IsNullOrEmpty(_ID.stringValue);
        editCmplt &= !string.IsNullOrEmpty(_name.stringValue);
        editCmplt &= lootPrefab.objectReferenceValue != null;
        editCmplt &= !info.ProductItems.Exists(x => x.Item == null || x.OnlyDropForQuest && x.BindedQuest == null);

        return editCmplt;
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        for (int i = 1; i < 1000; i++)
        {
            newID = "GATH" + i.ToString().PadLeft(3, '0');
            if (!Array.Exists(gathering, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        ResourceInformation find = Array.Find(gathering, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != info || (find == info && Array.FindAll(gathering, x => x.ID == _ID.stringValue).Length > 1);
    }
}