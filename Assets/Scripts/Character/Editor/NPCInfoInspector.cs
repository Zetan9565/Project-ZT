using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

[CustomEditor(typeof(NPCInfomation))]
public class NPCInfoInspector : Editor
{
    NPCInfomation npc;
    ReorderableList favoriteItemList;
    ReorderableList hateItemList;

    SerializedProperty _ID;
    SerializedProperty _Name;
    SerializedProperty favoriteItems;
    SerializedProperty hateItems;

    float lineHeight;
    float lineHeightSpace;

    bool showfavoriteItemList = true;
    bool showHateItemList = true;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        npc = target as NPCInfomation;
        _ID = serializedObject.FindProperty("_ID");
        _Name = serializedObject.FindProperty("_Name");
        favoriteItems = serializedObject.FindProperty("favoriteItems");
        hateItems = serializedObject.FindProperty("hateItems");

        HandlingFavoriteItemList();
        HandlingHateItemList();
    }

    public override void OnInspectorGUI()
    {
        if (string.IsNullOrEmpty(npc.Name) || string.IsNullOrEmpty(npc.ID) || npc.FavoriteItems.Exists(x => !x) || npc.HateItems.Exists(x => !x))
            EditorGUILayout.HelpBox("该NPC信息未补全。", MessageType.Warning);
        else if (npc.FavoriteItems.Intersect(npc.HateItems).Where(x => x != null).Count() > 0)
            EditorGUILayout.HelpBox("喜爱道具与讨厌道具存在冲突！", MessageType.Warning);
        else EditorGUILayout.HelpBox("该NPC信息已完整。", MessageType.Info);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_Name, new GUIContent("名字"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        showfavoriteItemList = EditorGUILayout.Toggle("显示喜爱道具列表", showfavoriteItemList);
        if (showfavoriteItemList)
        {
            serializedObject.Update();
            favoriteItemList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        showHateItemList = EditorGUILayout.Toggle("显示讨厌道具列表", showHateItemList);
        if (showHateItemList)
        {
            serializedObject.Update();
            hateItemList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }

    void HandlingFavoriteItemList()
    {
        favoriteItemList = new ReorderableList(serializedObject, favoriteItems, true, true, true, true);
        showfavoriteItemList = true;
        favoriteItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (npc.FavoriteItems[index] != null)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), npc.FavoriteItems[index].Name);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty item = favoriteItems.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        favoriteItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            npc.FavoriteItems.Add(null);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        favoriteItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
            {
                npc.FavoriteItems.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        favoriteItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount =npc.FavoriteItems.FindAll(x => !x).Count;
            EditorGUI.LabelField(rect, "喜爱道具列表", "数量：" + npc.FavoriteItems.Count.ToString() +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        favoriteItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingHateItemList()
    {
        hateItemList = new ReorderableList(serializedObject, hateItems, true, true, true, true);
        showfavoriteItemList = true;
        hateItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (npc.HateItems[index] != null)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), npc.HateItems[index].Name);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty item = hateItems.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        hateItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            npc.HateItems.Add(null);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        hateItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
            {
                npc.HateItems.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        hateItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount =npc.HateItems.FindAll(x => !x).Count;
            EditorGUI.LabelField(rect, "讨厌道具列表", "数量：" + npc.HateItems.Count.ToString() +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        hateItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        NPCInfomation[] npcs = Resources.LoadAll<NPCInfomation>("");
        for (int i = 1; i < 1000; i++)
        {
            newID = "NPC" + i.ToString().PadLeft(3, '0');
            if (!Array.Exists(npcs, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        List<NPCInfomation> npcs = new List<NPCInfomation>();
        foreach (NPCInfomation npc in Resources.LoadAll<NPCInfomation>(""))
        {
            npcs.Add(npc);
        }

        NPCInfomation find = npcs.Find(x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != npc || (find == npc && npcs.FindAll(x => x.ID == _ID.stringValue).Count > 1);
    }
}
