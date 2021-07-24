using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class ItemInspector
{
    BoxItem box;

    SerializedProperty boxItems;

    ReorderableList boxItemList;

    void HandlingBoxItemList()
    {
        boxItemList = new ReorderableList(serializedObject, boxItems, true, true, true, true);
        boxItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            SerializedProperty itemInfo = boxItems.GetArrayElementAtIndex(index);
            if (box.ItemsInBox[index] != null && box.ItemsInBox[index].Item != null)
                EditorGUI.PropertyField(new Rect(rect.x + 8f, rect.y, rect.width / 2f, lineHeight), itemInfo, new GUIContent(box.ItemsInBox[index].ItemName));
            else
                EditorGUI.PropertyField(new Rect(rect.x + 8f, rect.y, rect.width / 2f, lineHeight), itemInfo, new GUIContent("(空)"));
            EditorGUI.BeginChangeCheck();
            SerializedProperty item = itemInfo.FindPropertyRelative("item");
            SerializedProperty minAmount = itemInfo.FindPropertyRelative("minAmount");
            SerializedProperty maxAmount = itemInfo.FindPropertyRelative("maxAmount");
            SerializedProperty dropRate = itemInfo.FindPropertyRelative("dropRate");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            if (itemInfo.isExpanded)
            {
                int lineCount = 1;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    dropRate, new GUIContent("产出概率百分比"));
                if (dropRate.floatValue < 0) dropRate.floatValue = 0.0f;
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2f, lineHeight),
                    minAmount, new GUIContent("最少产出"));
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y + lineHeightSpace * lineCount, rect.width / 2f, lineHeight),
                    maxAmount, new GUIContent("最多产出"));
                if (minAmount.intValue < 1) minAmount.intValue = 1;
                if (maxAmount.intValue < 1) maxAmount.intValue = 1;
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        boxItemList.elementHeightCallback = (int index) =>
        {
            SerializedProperty itemInfo = boxItems.GetArrayElementAtIndex(index);
            return lineHeightSpace * (itemInfo.isExpanded ? 3f : 1f);
        };

        boxItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            box.ItemsInBox.Add(new DropItemInfo());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        boxItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
            {
                box.ItemsInBox.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        boxItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = box.ItemsInBox.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "盒内道具列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
        };

        boxItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void BoxItemEnable()
    {
        boxItems = serializedObject.FindProperty("itemsInBox");
        HandlingBoxItemList();
    }

    void DrawBoxItem()
    {
        EditorGUI.BeginChangeCheck();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        EditorGUILayout.PropertyField(boxItems, new GUIContent("盒内道具\t\t" + (boxItems.arraySize > 0 ? "数量：" + boxItems.arraySize : "无")), false);
        if (boxItems.isExpanded)
        {
            EditorGUILayout.HelpBox("目前只设计8个容量。", MessageType.Info);
            serializedObject.Update();
            boxItemList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (box.ItemsInBox.Count >= 8)
                boxItemList.displayAdd = false;
            else boxItemList.displayAdd = true;
        }
    }

    bool CheckBoxEditCmlt()
    {
        return !box.ItemsInBox.Exists(x => x.Item == null);
    }
}