using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Talker), true)]
public class TalkerInspector : Editor
{
    Talker talker;
    QuestGiver questGiver;

    SerializedProperty info;
    SerializedProperty isWarehouseAgent;
    SerializedProperty warehouse;
    SerializedProperty isVendor;
    SerializedProperty shop;
    SerializedProperty commodities;
    SerializedProperty acquisitions;

    SerializedProperty questsStored;
    SerializedProperty questInstances;

    ReorderableList commodityList;
    ReorderableList acquisitionList;
    ReorderableList questList;

    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        talker = target as Talker;
        info = serializedObject.FindProperty("info");
        isWarehouseAgent = serializedObject.FindProperty("isWarehouseAgent");
        isVendor = serializedObject.FindProperty("isVendor");
        warehouse = serializedObject.FindProperty("warehouse");
        shop = serializedObject.FindProperty("shop");
        commodities = shop.FindPropertyRelative("commodities");
        acquisitions = shop.FindPropertyRelative("acquisitions");
        HandlingCommodityList();
        HandlingAcquisitionList();
        if (talker is QuestGiver)
        {
            questsStored = serializedObject.FindProperty("questsStored");
            questInstances = serializedObject.FindProperty("questInstances");

            questGiver = talker as QuestGiver;

            HandlingQuestList();
        }
        else questGiver = null;
    }

    public override void OnInspectorGUI()
    {
        if (talker.Info)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("NPC名字：" + talker.TalkerName);
            EditorGUILayout.LabelField("NPC识别码：" + talker.TalkerID);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("NPC信息为空！", MessageType.Error);
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(info, new GUIContent("信息"));
        if (!isVendor.boolValue) EditorGUILayout.PropertyField(isWarehouseAgent, new GUIContent("是仓库管理员"));
        if (isWarehouseAgent.boolValue && !isVendor.boolValue)
        {
            SerializedProperty warehouseSize = warehouse.FindPropertyRelative("warehouseSize");
            warehouseSize.FindPropertyRelative("max").intValue = EditorGUILayout.IntSlider("默认仓库容量(格)",
                warehouseSize.FindPropertyRelative("max").intValue, 100, 500);
        }
        if (!isWarehouseAgent.boolValue) EditorGUILayout.PropertyField(isVendor, new GUIContent("是商贩"));
        if (isVendor.boolValue && !isWarehouseAgent.boolValue)
            EditorGUILayout.PropertyField(shop, new GUIContent("商铺信息"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (shop.isExpanded && talker.IsVendor)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shop.FindPropertyRelative("shopName"), new GUIContent("商店名称"));
            EditorGUILayout.PropertyField(commodities, new GUIContent("--商品列表--\t\t" + (commodities.arraySize > 0 ? "数量：" + commodities.arraySize : "无")));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (commodities.isExpanded)
            {
                serializedObject.Update();
                commodityList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.PropertyField(acquisitions, new GUIContent("--收购品列表--\t\t" + (acquisitions.arraySize > 0 ? "数量：" + acquisitions.arraySize : "无")));
            if (acquisitions.isExpanded)
            {
                serializedObject.Update();
                acquisitionList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }
        if (questGiver && talker.Info)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(questsStored, new GUIContent("持有任务\t\t" + (questsStored.arraySize > 0 ? "数量：" + questsStored.arraySize : "无")));
            if (questsStored.isExpanded)
            {
                serializedObject.Update();
                questList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
            if (questInstances.arraySize > 0)
            {
                EditorGUILayout.PropertyField(questInstances, new GUIContent("任务实例\t\t数量：" + questInstances.arraySize));
                GUI.enabled = false;
                if (questInstances.isExpanded)
                    for (int i = 0; i < questGiver.QuestInstances.Count; i++)
                        EditorGUILayout.LabelField(questGiver.QuestInstances[i].Title);
                GUI.enabled = true;
            }
        }
    }

    void HandlingCommodityList()
    {
        commodityList = new ReorderableList(serializedObject, commodities, true, true, true, true);

        commodityList.drawElementCallback = (rect, index, isActive, isFocusd) =>
         {
             serializedObject.Update();
             EditorGUI.BeginChangeCheck();
             SerializedProperty commodity = commodities.GetArrayElementAtIndex(index);
             SerializedProperty item = commodity.FindPropertyRelative("item");
             SerializedProperty maxAmount = commodity.FindPropertyRelative("maxAmount");
             SerializedProperty priceMultiple = commodity.FindPropertyRelative("priceMultiple");
             SerializedProperty emptyAble = commodity.FindPropertyRelative("emptyAble");
             SerializedProperty refreshTime = commodity.FindPropertyRelative("refreshTime");
             SerializedProperty minRefreshAmount = commodity.FindPropertyRelative("minRefreshAmount");
             SerializedProperty maxRefreshAmount = commodity.FindPropertyRelative("maxRefreshAmount");
             string label = "(空)";
             if (talker.shop.Commodities[index].Item) label = talker.shop.Commodities[index].Item.name;
             EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 2 - 8, lineHeight), commodity, new GUIContent(label));
             EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, lineHeight),
                 item, new GUIContent(string.Empty));
             int lineCount = 1;
             if (commodity.isExpanded && item.objectReferenceValue)
             {
                 EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                     priceMultiple, new GUIContent("价格倍数"));
                 if (priceMultiple.floatValue <= 0.01f) priceMultiple.floatValue = 0.01f;
                 lineCount++;
                 if (maxAmount.intValue <= 1) maxAmount.intValue = 1;
                 EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                     emptyAble, new GUIContent("会售罄"));
                 lineCount++;
                 if (emptyAble.boolValue)
                 {
                     EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         maxAmount, new GUIContent("最大库存量"));
                     lineCount++;
                     EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         refreshTime, new GUIContent("补货时间"));
                     if (refreshTime.floatValue <= -1.0f) refreshTime.floatValue = -1.0f;
                     lineCount++;
                     minRefreshAmount.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         "最少补货量", minRefreshAmount.intValue, 0, maxRefreshAmount.intValue - 1);
                     lineCount++;
                     maxRefreshAmount.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         "最多补货量", maxRefreshAmount.intValue, minRefreshAmount.intValue + 1, maxAmount.intValue);
                     if (maxRefreshAmount.intValue > maxAmount.intValue) maxRefreshAmount.intValue = maxAmount.intValue;
                     lineCount++;
                 }
             }
             if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
         };

        commodityList.elementHeightCallback = (index) =>
        {
            int lineCount = 1;
            SerializedProperty commodity = commodities.GetArrayElementAtIndex(index);
            SerializedProperty item = commodity.FindPropertyRelative("item");
            SerializedProperty emptyAble = commodity.FindPropertyRelative("emptyAble");
            if (commodity.isExpanded && item.objectReferenceValue)
            {
                lineCount += 2;//倍数、会售罄
                if (emptyAble.boolValue)
                {
                    lineCount += 4;//最大库存、补货时间、最少补货、最多补货
                }
            }
            return lineCount * lineHeightSpace;
        };

        commodityList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            talker.shop.Commodities.Add(new MerchandiseInfo());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        commodityList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个商品吗？", "确定", "取消"))
                talker.shop.Commodities.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        commodityList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = talker.shop.Commodities.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "商品列表", notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty);
        };

        commodityList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingAcquisitionList()
    {
        acquisitionList = new ReorderableList(serializedObject, acquisitions, true, true, true, true);

        acquisitionList.drawElementCallback = (rect, index, isActive, isFocusd) =>
         {
             serializedObject.Update();
             EditorGUI.BeginChangeCheck();
             SerializedProperty acquisition = acquisitions.GetArrayElementAtIndex(index);
             SerializedProperty item = acquisition.FindPropertyRelative("item");
             SerializedProperty maxAmount = acquisition.FindPropertyRelative("maxAmount");
             SerializedProperty priceMultiple = acquisition.FindPropertyRelative("priceMultiple");
             SerializedProperty emptyAble = acquisition.FindPropertyRelative("emptyAble");
             SerializedProperty refreshTime = acquisition.FindPropertyRelative("refreshTime");
             SerializedProperty minRefreshAmount = acquisition.FindPropertyRelative("minRefreshAmount");
             SerializedProperty maxRefreshAmount = acquisition.FindPropertyRelative("maxRefreshAmount");
             string label = "(空)";
             if (talker.shop.Acquisitions[index].Item) label = talker.shop.Acquisitions[index].Item.name;
             EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 2 - 8, lineHeight), acquisition, new GUIContent(label));
             EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, lineHeight),
                 item, new GUIContent(string.Empty));
             int lineCount = 1;
             if (acquisition.isExpanded && item.objectReferenceValue && (item.objectReferenceValue as ItemBase).SellAble)
             {
                 EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                     priceMultiple, new GUIContent("价格倍数"));
                 if (priceMultiple.floatValue <= 0.01f) priceMultiple.floatValue = 0.01f;
                 lineCount++;
                 EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                     emptyAble, new GUIContent("会购满"));
                 lineCount++;
                 if (emptyAble.boolValue)
                 {
                     EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         maxAmount, new GUIContent("最大收购量"));
                     lineCount++;
                     if (maxAmount.intValue <= 1) maxAmount.intValue = 1;
                     EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         refreshTime, new GUIContent("需求刷新时间"));
                     if (refreshTime.floatValue <= -1.0f) refreshTime.floatValue = -1.0f;
                     lineCount++;
                     minRefreshAmount.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         "最少新需求量", minRefreshAmount.intValue, 0, maxRefreshAmount.intValue - 1);
                     lineCount++;
                     maxRefreshAmount.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                         "最多新需求量", maxRefreshAmount.intValue, minRefreshAmount.intValue + 1, maxAmount.intValue);
                     if (maxRefreshAmount.intValue > maxAmount.intValue) maxRefreshAmount.intValue = maxAmount.intValue;
                     lineCount++;
                 }
             }
             else if (!(item.objectReferenceValue as ItemBase).SellAble)
                 EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "该物品不可出售！");
             if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
         };

        acquisitionList.elementHeightCallback = (index) =>
        {
            int lineCount = 1;
            SerializedProperty acquisition = acquisitions.GetArrayElementAtIndex(index);
            SerializedProperty item = acquisition.FindPropertyRelative("item");
            SerializedProperty emptyAble = acquisition.FindPropertyRelative("emptyAble");
            if (acquisition.isExpanded && item.objectReferenceValue && (item.objectReferenceValue as ItemBase).SellAble)
            {
                lineCount += 2;//倍数、会购满
                if (emptyAble.boolValue)
                {
                    lineCount += 4;//最大需求、需求时间、最少需求、最多需求
                }
            }
            else if (!(item.objectReferenceValue as ItemBase).SellAble) lineCount++;
            return lineCount * lineHeightSpace;
        };

        acquisitionList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            talker.shop.Acquisitions.Add(new MerchandiseInfo());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acquisitionList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个收购品吗？", "确定", "取消"))
                talker.shop.Acquisitions.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acquisitionList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = talker.shop.Acquisitions.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "收购品列表", notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty);
        };

        acquisitionList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingQuestList()
    {
        questList = new ReorderableList(serializedObject, questsStored, true, true, true, true);

        questList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (questGiver.QuestsStored[index])
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), questGiver.QuestsStored[index].Title);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            SerializedProperty quest = questsStored.GetArrayElementAtIndex(index);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), quest, new GUIContent(string.Empty));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.elementHeightCallback = (int index) =>
        {
            return 2 * lineHeightSpace;
        };

        questList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            questGiver.QuestsStored.Add(null);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个任务吗？", "确定", "取消"))
                questGiver.QuestsStored.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = questGiver.QuestsStored.FindAll(x => !x).Count;
            EditorGUI.LabelField(rect, "任务列表", notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty);
        };

        questList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }
}