using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(ShopInformation))]
public class ShopInfoInspector : Editor
{
    ShopInformation shop;

    SerializedProperty shopName;
    SerializedProperty commodities;
    SerializedProperty acquisitions;

    ReorderableList commodityList;
    ReorderableList acquisitionList;

    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        shop = target as ShopInformation;
        shopName = serializedObject.FindProperty("shopName");
        commodities = serializedObject.FindProperty("commodities");
        acquisitions = serializedObject.FindProperty("acquisitions");
        HandlingCommodityList();
        HandlingAcquisitionList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(shopName, new GUIContent("商店名称"));
        EditorGUILayout.PropertyField(commodities, new GUIContent("在售品列表\t\t" + (commodities.arraySize > 0 ? "数量：" + commodities.arraySize : "无")), false);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (commodities.isExpanded)
        {
            serializedObject.Update();
            commodityList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(acquisitions, new GUIContent("收购品列表\t\t" + (acquisitions.arraySize > 0 ? "数量：" + acquisitions.arraySize : "无")), false);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (acquisitions.isExpanded)
        {
            serializedObject.Update();
            acquisitionList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
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
            if (shop.Commodities[index].Item) label = shop.Commodities[index].Item.name;
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
            shop.Commodities.Add(new MerchandiseInfo());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        commodityList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个商品吗？", "确定", "取消"))
                shop.Commodities.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        commodityList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = shop.Commodities.FindAll(x => !x.Item).Count;
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
            if (shop.Acquisitions[index].Item) label = shop.Acquisitions[index].Item.name;
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
            else if (item.objectReferenceValue && !(item.objectReferenceValue as ItemBase).SellAble)
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
            else if (item.objectReferenceValue && !(item.objectReferenceValue as ItemBase).SellAble) lineCount++;
            return lineCount * lineHeightSpace;
        };

        acquisitionList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            shop.Acquisitions.Add(new MerchandiseInfo());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acquisitionList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个收购品吗？", "确定", "取消"))
                shop.Acquisitions.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acquisitionList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = shop.Acquisitions.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "收购品列表", notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty);
        };

        acquisitionList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }
}