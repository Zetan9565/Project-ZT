using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(ItemBase), true)]
[CanEditMultipleObjects]
public partial class ItemInspector : Editor
{
    protected ItemBase item;

    float lineHeight;
    float lineHeightSpace;

    SerializedProperty _ID;
    SerializedProperty _Name;
    SerializedProperty itemType;
    SerializedProperty quality;
    SerializedProperty weight;
    SerializedProperty sellAble;
    SerializedProperty sellPrice;
    SerializedProperty buyPrice;
    SerializedProperty icon;
    SerializedProperty description;
    SerializedProperty stackAble;
    SerializedProperty discardAble;
    SerializedProperty lockAble;
    SerializedProperty usable;
    SerializedProperty inexhaustible;
    SerializedProperty maxDurability;
    SerializedProperty makingMethod;
    SerializedProperty minYield;
    SerializedProperty maxYield;
    SerializedProperty formulation;

    SerializedProperty materialType;

    ItemBase[] items;

    SerializedProperty attribute;
    RoleAttributeGroupDrawer attrDrawer;

    private void OnEnable()
    {
        items = Resources.LoadAll<ItemBase>("Configuration");

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;

        item = target as ItemBase;
        _ID = serializedObject.FindProperty("_ID");
        _Name = serializedObject.FindProperty("_Name");
        itemType = serializedObject.FindProperty("itemType");
        quality = serializedObject.FindProperty("quality");
        weight = serializedObject.FindProperty("weight");
        sellAble = serializedObject.FindProperty("sellAble");
        sellPrice = serializedObject.FindProperty("sellPrice");
        buyPrice = serializedObject.FindProperty("buyPrice");
        icon = serializedObject.FindProperty("icon");
        description = serializedObject.FindProperty("description");
        stackAble = serializedObject.FindProperty("stackAble");
        discardAble = serializedObject.FindProperty("discardAble");
        lockAble = serializedObject.FindProperty("lockAble");
        usable = serializedObject.FindProperty("usable");
        inexhaustible = serializedObject.FindProperty("inexhaustible");
        maxDurability = serializedObject.FindProperty("maxDurability");
        formulation = serializedObject.FindProperty("formulation");
        makingMethod = serializedObject.FindProperty("makingMethod");
        minYield = serializedObject.FindProperty("minYield");
        maxYield = serializedObject.FindProperty("maxYield");

        box = target as BoxItem;
        if (box)
        {
            BoxItemEnable();
        }

        if (target is EquipmentItem)
        {
            attribute = serializedObject.FindProperty("attribute");
            attrDrawer = new RoleAttributeGroupDrawer(serializedObject, attribute, lineHeight, lineHeightSpace);
        }

        materialType = serializedObject.FindProperty("materialType");

        FixType();
    }

    public override void OnInspectorGUI()
    {
        if (!CheckEditComplete())
            EditorGUILayout.HelpBox("该道具存在未补全信息。", MessageType.Warning);
        else
        {
            EditorGUILayout.HelpBox("该道具信息已完整。", MessageType.Info);
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID() || string.IsNullOrEmpty(Regex.Replace(_ID.stringValue, @"[^0-9]+", "")) || !Regex.IsMatch(_ID.stringValue, @"(\d+)$"))
        {
            if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            else if (!string.IsNullOrEmpty(_ID.stringValue) && (string.IsNullOrEmpty(Regex.Replace(_ID.stringValue, @"[^0-9]+", "")) || !Regex.IsMatch(_ID.stringValue, @"(\d+)$")))
                EditorGUILayout.HelpBox("此识别码非法！", MessageType.Error);
            else EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_Name, new GUIContent("名称"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("道具类型");
        GUI.enabled = false;
        EditorGUILayout.PropertyField(itemType, new GUIContent(string.Empty));
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        if (!item.IsCurrency) EditorGUILayout.PropertyField(materialType, new GUIContent("作为材料时的类型"));
        if (item.ItemType != ItemType.Quest)
            EditorGUILayout.PropertyField(quality, new GUIContent("道具品质"));
        EditorGUILayout.PropertyField(weight, new GUIContent("道具重量"));
        if (weight.floatValue < 0) weight.floatValue = 0;
        EditorGUILayout.PropertyField(buyPrice, new GUIContent("购买价格"));
        if (buyPrice.intValue < 0) buyPrice.intValue = 0;
        if (item.ItemType != ItemType.Quest)
        {
            EditorGUILayout.PropertyField(sellAble, new GUIContent("可出售"));
            if (sellAble.boolValue) EditorGUILayout.PropertyField(sellPrice, new GUIContent("贩卖价格"));
            if (sellPrice.intValue < 0) sellPrice.intValue = 0;
        }
        EditorGUILayout.PropertyField(description, new GUIContent("描述"));
        if (!item.IsEquipment && !item.IsBag)
            EditorGUILayout.PropertyField(stackAble, new GUIContent("可叠加"));
        if (!item.IsForQuest) EditorGUILayout.PropertyField(discardAble, new GUIContent("可丢弃"));
        EditorGUILayout.PropertyField(lockAble, new GUIContent("可上锁"));
        if (item.IsForQuest) EditorGUILayout.PropertyField(usable, new GUIContent("可使用"));
        if (item.IsForQuest && item.Usable) EditorGUILayout.PropertyField(inexhaustible, new GUIContent("可无限使用"));
        icon.objectReferenceValue = EditorGUILayout.ObjectField("图标", item.Icon, typeof(Sprite), false);
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("附加信息", new GUIStyle() { fontStyle = FontStyle.Bold });
        HandlingItemType();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        if (!(item is BoxItem) && !(item is BookItem) && !(item is CurrencyItem))
        {
            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(makingMethod, new GUIContent("制作方法"));
            if (makingMethod.enumValueIndex != (int)MakingMethod.None)
            {
                minYield.intValue = EditorGUILayout.IntSlider("最小产量", minYield.intValue, 1, maxYield.intValue);
                EditorGUILayout.PropertyField(maxYield, new GUIContent("最大产量"));
            }
            if (minYield.intValue < 1) minYield.intValue = 1;
            if (maxYield.intValue < 1) maxYield.intValue = 1;
            if (minYield.intValue > maxYield.intValue) minYield.intValue = maxYield.intValue;
            if (makingMethod.enumValueIndex != (int)MakingMethod.None)
            {
                EditorGUILayout.PropertyField(formulation, new GUIContent("制作配方"), false);
                if (!formulation.objectReferenceValue)
                {
                    if (GUILayout.Button("新建"))
                    {
                        string folder = EditorUtility.OpenFolderPanel("选择保存文件夹", ZetanEditorUtility.GetDirectoryName(target), "");
                        if (!string.IsNullOrEmpty(folder))
                        {
                            try
                            {
                                Formulation formuInstance = CreateInstance<Formulation>();
                                AssetDatabase.CreateAsset(formuInstance, AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/Configuration/Formulation/formulation.asset"));
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                formulation.objectReferenceValue = formuInstance;

                                EditorUtility.OpenPropertyEditor(formuInstance);
                            }
                            catch
                            {
                                EditorUtility.DisplayDialog("新建失败", "请选择Assets目录以下的文件夹。", "确定");
                            }
                        }
                    }
                    EditorGUILayout.HelpBox("未设置配方", MessageType.Error);
                }
                else
                {
                    if (GUILayout.Button("编辑"))
                        EditorUtility.OpenPropertyEditor(formulation.objectReferenceValue as Formulation);
                    if (!(formulation.objectReferenceValue as Formulation).IsValid)
                        EditorGUILayout.HelpBox("配方信息不完整。", MessageType.Error);
                    else if (item.Formulation)
                    {
                        var other = Array.Find(items, x => x.MakingMethod != MakingMethod.None && x.MakingMethod == item.MakingMethod && x != item && item.Formulation == x.Formulation);
                        if (other) EditorGUILayout.HelpBox($"其它道具与此道具的制作材料重复！配置路径：\n{AssetDatabase.GetAssetPath(other)}", MessageType.Error);
                        GUI.enabled = false;
                        EditorGUILayout.TextArea(item.Formulation.ToString());
                        GUI.enabled = true;
                    }
                }
            }
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
        if (box)
        {
            DrawBoxItem();
        }
        EditorGUILayout.EndHorizontal();
    }

    void HandlingItemType()
    {
        switch (item.ItemType)
        {
            case ItemType.Medicine:

                break;
            case ItemType.Weapon:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gemSlotAmount"), new GUIContent("默认宝石槽数"));
                goto case ItemType.Gemstone;
            case ItemType.Armor:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("armorType"), new GUIContent("防具类型"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("gemSlotAmount"), new GUIContent("默认宝石槽数"));
                goto case ItemType.Gemstone;
            case ItemType.Gemstone:
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("powerup"), new GUIContent("附加效果"), true);
                break;
            case ItemType.Book:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bookType"), new GUIContent("书籍/图纸类型"), true);
                switch ((item as BookItem).BookType)
                {
                    case BookType.Building:
                        SerializedProperty building = serializedObject.FindProperty("buildingToLearn");
                        EditorGUILayout.PropertyField(building, new GUIContent("可学设施"), true);
                        if (building.objectReferenceValue)
                        {
                            EditorGUILayout.LabelField("设施名称", (building.objectReferenceValue as BuildingInformation).name);
                        }
                        break;
                    case BookType.Making:
                        SerializedProperty item = serializedObject.FindProperty("itemToLearn");
                        EditorGUILayout.PropertyField(item, new GUIContent("可学道具"), true);
                        if (item.objectReferenceValue)
                        {
                            if ((item.objectReferenceValue as ItemBase).MakingMethod == MakingMethod.None)
                            {
                                EditorGUILayout.HelpBox("不可制作的道具！", MessageType.Error);
                            }
                            else EditorGUILayout.LabelField("道具名称", (item.objectReferenceValue as ItemBase).name);
                        }
                        break;
                    default: break;
                }
                break;
            case ItemType.Bag:
                serializedObject.FindProperty("expandSize").intValue = EditorGUILayout.IntSlider(new GUIContent("背包扩张数量"), serializedObject.FindProperty("expandSize").intValue, 1, 192);
                break;
            case ItemType.Quest:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerName"), new GUIContent("触发器名称"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stateToSet"), new GUIContent("触发器状态"));
                break;
            case ItemType.Seed:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("crop"), new GUIContent("农作物信息"));
                break;
            default: break;
        }
        if (item.IsEquipment)
        {
            EditorGUILayout.PropertyField(attribute, new GUIContent("装备属性"), false);
            if (attribute.isExpanded)
                attrDrawer?.DoLayoutDraw();
            EditorGUILayout.PropertyField(maxDurability, new GUIContent("最大耐久度"));
            if (maxDurability.intValue < 1) maxDurability.intValue = 1;
        }
        else if (item.IsCurrency)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("valueEach"), new GUIContent("面额"));
        }
    }

    void FixType()
    {
        if (item.IsWeapon && item.ItemType != ItemType.Weapon)
        {
            itemType.enumValueIndex = (int)ItemType.Weapon;
        }
        else if (item.IsBox && item.ItemType != ItemType.Box)
        {
            itemType.enumValueIndex = (int)ItemType.Box;
        }
        else if (item.IsBag && item.ItemType != ItemType.Bag)
        {
            itemType.enumValueIndex = (int)ItemType.Bag;
        }
        else if (item.IsBook && item.ItemType != ItemType.Book)
        {
            itemType.enumValueIndex = (int)ItemType.Book;
        }
        else if (item.IsMaterial && item.ItemType != ItemType.Material)
        {
            itemType.enumValueIndex = (int)ItemType.Material;
        }
        else if (item.IsForQuest && item.ItemType != ItemType.Quest)
        {
            itemType.enumValueIndex = (int)ItemType.Quest;
        }
        else if (item.IsMedicine && item.ItemType != ItemType.Medicine)
        {
            itemType.enumValueIndex = (int)ItemType.Medicine;
        }
        else if (item.IsSeed && item.ItemType != ItemType.Seed)
        {
            itemType.enumValueIndex = (int)ItemType.Seed;
        }
    }

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !(string.IsNullOrEmpty(item.ID) || string.IsNullOrEmpty(item.name) ||
            string.IsNullOrEmpty(item.Description) || item.Icon == null ||
            ExistsID() || string.IsNullOrEmpty(Regex.Replace(item.ID, @"[^0-9]+", "")) || !Regex.IsMatch(item.ID, @"(\d+)$") ||
            (item.IsBook && (item as BookItem).BookType == BookType.Building && !(item as BookItem).BuildingToLearn) ||
            (item.IsBook && (item as BookItem).BookType == BookType.Making && (!(item as BookItem).ItemToLearn ||
            (item as BookItem).ItemToLearn.MakingMethod == MakingMethod.None)) ||
            (item.IsSeed && (item as SeedItem).Crop == null)
            );

        if (box)
            editComplete &= CheckBoxEditCmlt();

        if (item.MakingMethod != MakingMethod.None)
        {
            editComplete &= formulation.objectReferenceValue && (formulation.objectReferenceValue as Formulation).IsValid;
        }

        return editComplete;
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        ItemBase[] items = Resources.LoadAll<ItemBase>("Configuration");
        for (int i = 1; i < 1000; i++)
        {
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    newID = "WEPN" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Armor:
                    newID = "ARMR" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Box:
                    newID = "IBOX" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Material:
                    newID = "MATR" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Quest:
                    newID = "QSTI" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Gemstone:
                    newID = "GEMS" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Book:
                    newID = "BOOK" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Bag:
                    newID = "IBAG" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Medicine:
                    newID = "MADC" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Seed:
                    newID = "SEED" + i.ToString().PadLeft(3, '0');
                    break;
                case ItemType.Currency:
                    newID = "CURR" + i.ToString().PadLeft(3, '0');
                    break;
                default:
                    newID = "ITEM" + i.ToString().PadLeft(3, '0');
                    break;
            }
            if (!Array.Exists(items, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        ItemBase find = Array.Find(items, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != item || (find == item && Array.FindAll(items, x => x.ID == _ID.stringValue).Length > 1);
    }
}