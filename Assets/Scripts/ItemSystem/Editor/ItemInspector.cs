using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditorInternal;
using System.Text.RegularExpressions;

[CustomEditor(typeof(ItemBase), true)]
[CanEditMultipleObjects]
public class ItemInspector : Editor
{
    protected ItemBase item;
    BoxItem box;
    MaterialItem material;

    ReorderableList boxItemList;
    ReorderableList materialList;

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
    SerializedProperty useable;
    SerializedProperty inexhaustible;
    SerializedProperty maxDurability;
    SerializedProperty processMethod;
    SerializedProperty materials;

    SerializedProperty boxItems;

    SerializedProperty materialType;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

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
        useable = serializedObject.FindProperty("useable");
        inexhaustible = serializedObject.FindProperty("inexhaustible");
        maxDurability = serializedObject.FindProperty("maxDurability");
        materials = serializedObject.FindProperty("materials");
        processMethod = serializedObject.FindProperty("processMethod");
        HandlingMaterialItemList();

        box = target as BoxItem;
        if (box)
        {
            boxItems = serializedObject.FindProperty("itemsInBox");
            HandlingBoxItemList();
        }

        material = target as MaterialItem;
        if (material)
            materialType = serializedObject.FindProperty("materialType");

        FixType();
    }

    public override void OnInspectorGUI()
    {
        if (item.Materials.Exists(x => (x.ProcessType == ProcessType.SingleItem && item.Materials.FindAll(y => y.ProcessType == ProcessType.SingleItem && y.Item == x.Item).Count > 1) ||
                                       (x.ProcessType == ProcessType.SameType && item.Materials.FindAll(y => y.ProcessType == ProcessType.SameType && y.MaterialType == x.MaterialType).Count > 1)))
        {
            EditorGUILayout.HelpBox("制作材料存在重复。", MessageType.Warning);
        }
        else if (!CheckEditComplete())
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
            {
                EditorGUILayout.HelpBox("此识别码非法！", MessageType.Error);
            }
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_Name, new GUIContent("名称"));
        HandlingItemType();
        if (item is MaterialItem)
            EditorGUILayout.PropertyField(materialType, new GUIContent("材料类型"));
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
        EditorGUILayout.PropertyField(icon, new GUIContent("图标"));
        if (item.Icon)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField(new GUIContent(string.Empty), item.Icon, typeof(Texture2D), false);
            GUI.enabled = true;
        }
        if (!item.IsEquipment && !item.IsBag)
            EditorGUILayout.PropertyField(stackAble, new GUIContent("可叠加"));
        if (!item.IsForQuest) EditorGUILayout.PropertyField(discardAble, new GUIContent("可丢弃"));
        if (item.IsForQuest) EditorGUILayout.PropertyField(useable, new GUIContent("可使用"));
        if (item.IsForQuest && item.Useable) EditorGUILayout.PropertyField(inexhaustible, new GUIContent("可无限使用"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (!(item is BoxItem) && !(item is BookItem))
        {
            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(processMethod, new GUIContent("制作方法"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (processMethod.enumValueIndex != 0)
            {
                EditorGUILayout.PropertyField(materials, new GUIContent("制作材料\t\t" + (materials.arraySize > 0 ? "数量：" + materials.arraySize : "无")));
                if (materials.isExpanded)
                {
                    serializedObject.Update();
                    materialList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        EditorGUILayout.Space();
        if (box)
        {
            EditorGUILayout.PropertyField(boxItems, new GUIContent("盒内道具\t\t" + (boxItems.arraySize > 0 ? "数量：" + boxItems.arraySize : "无")));
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
    }

    void HandlingBoxItemList()
    {
        boxItemList = new ReorderableList(serializedObject, boxItems, true, true, true, true);
        boxItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (box.ItemsInBox[index] != null && box.ItemsInBox[index].Item != null)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), box.ItemsInBox[index].Item.Name);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty itemInfo = boxItems.GetArrayElementAtIndex(index);
            SerializedProperty item = itemInfo.FindPropertyRelative("item");
            SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                amount, new GUIContent("数量"));
            if (amount.intValue < 1) amount.intValue = 1;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        boxItemList.elementHeightCallback = (int index) =>
        {
            return 2 * lineHeightSpace;
        };

        boxItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            box.ItemsInBox.Add(new ItemInfo());
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

    void HandlingMaterialItemList()
    {
        materialList = new ReorderableList(serializedObject, materials, true, true, true, true);
        materialList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (this.item.Materials[index] != null && this.item.Materials[index].Item != null && this.item.Materials[index].ProcessType == ProcessType.SingleItem)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), this.item.Materials[index].Item.Name);
            else if (this.item.Materials[index] != null && this.item.Materials[index].ProcessType == ProcessType.SameType)
            {
                switch (this.item.Materials[index].MaterialType)
                {
                    case MaterialType.Cloth: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "布料"); break;
                    case MaterialType.Fruit: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "水果"); break;
                    case MaterialType.Fur: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "皮毛"); break;
                    case MaterialType.Meat: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "肉类"); break;
                    case MaterialType.Metal: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "金属"); break;
                    case MaterialType.Ore: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "矿石"); break;
                    case MaterialType.Plant: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "植物"); break;
                    default: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "未定义"); break;
                }
            }
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty itemInfo = materials.GetArrayElementAtIndex(index);
            SerializedProperty processType = itemInfo.FindPropertyRelative("processType");
            SerializedProperty item = itemInfo.FindPropertyRelative("item");
            SerializedProperty materialType = itemInfo.FindPropertyRelative("materialType");
            SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight), processType, new GUIContent(string.Empty));
            if (processType.enumValueIndex == (int)ProcessType.SameType)
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), materialType, new GUIContent("所需种类"));
            else
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), item, new GUIContent("所需材料"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight),
                amount, new GUIContent("所需数量"));
            if (amount.intValue < 1) amount.intValue = 1;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        materialList.elementHeightCallback = (int index) =>
        {
            return 3 * lineHeightSpace;
        };

        materialList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            item.Materials.Add(new MatertialInfo() { Amount = 1 });
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        materialList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
            {
                item.Materials.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        materialList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = item.Materials.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "制作材料列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
        };

        materialList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingItemType()
    {
        string typeName = "未定义";
        switch (item.ItemType)
        {
            case ItemType.Weapon: typeName = "武器"; break;
            case ItemType.Armor: typeName = "防具"; break;
            case ItemType.Box: typeName = "箱子"; break;
            case ItemType.Material: typeName = "制作材料"; break;
            case ItemType.Quest: typeName = "任务道具"; break;
            case ItemType.Gemstone: typeName = "宝石"; break;
            case ItemType.Book: typeName = "书籍/图纸"; break;
            case ItemType.Bag: typeName = "扩张用袋子"; break;
            default: break;
        }
        EditorGUILayout.LabelField("道具类型", typeName);
        switch (item.ItemType)
        {
            case ItemType.Weapon:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_ATK"), new GUIContent("攻击力"));
                if (serializedObject.FindProperty("_ATK").intValue < 0) serializedObject.FindProperty("_ATK").intValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_DEF"), new GUIContent("防御力"));
                if (serializedObject.FindProperty("_DEF").intValue < 0) serializedObject.FindProperty("_DEF").intValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("hit"), new GUIContent("命中力"));
                if (serializedObject.FindProperty("hit").intValue < 0) serializedObject.FindProperty("hit").intValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("gemSlotAmount"), new GUIContent("默认宝石槽数"));
                goto case ItemType.Gemstone;
            case ItemType.Armor:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("armorType"), new GUIContent("防具类型"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_DEF"), new GUIContent("防御力"));
                if (serializedObject.FindProperty("_DEF").intValue < 0) serializedObject.FindProperty("_DEF").intValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("hit"), new GUIContent("命中力"));
                if (serializedObject.FindProperty("hit").intValue < 0) serializedObject.FindProperty("hit").intValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("dodge"), new GUIContent("闪避力"));
                if (serializedObject.FindProperty("dodge").intValue < 0) serializedObject.FindProperty("dodge").intValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("gemSlotAmount"), new GUIContent("默认宝石槽数"));
                goto case ItemType.Gemstone;
            case ItemType.Gemstone:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("powerup"), new GUIContent("附加效果"), true);
                break;
            case ItemType.Book:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bookType"), new GUIContent("书籍/图纸类型"), true);
                switch ((item as BookItem).BookType)
                {
                    case BookType.Building:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("buildingInfo"), new GUIContent("可学设施"), true);
                        if (serializedObject.FindProperty("buildingInfo").objectReferenceValue)
                        {
                            EditorGUILayout.LabelField("设施名称", (item as BookItem).BuildingInfo.Name);
                        }
                        break;
                    default: break;
                }
                break;
            case ItemType.Bag:
                serializedObject.FindProperty("expandSize").intValue = EditorGUILayout.IntSlider(new GUIContent("背包扩张数量"), serializedObject.FindProperty("expandSize").intValue, 1, 192);
                break;
            default: break;
        }
        if (item.IsEquipment)
        {
            EditorGUILayout.PropertyField(maxDurability, new GUIContent("最大耐久度"));
            if (maxDurability.intValue < 1) maxDurability.intValue = 1;
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
    }

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !(string.IsNullOrEmpty(item.ID) || string.IsNullOrEmpty(item.Name) ||
            string.IsNullOrEmpty(item.Description) || item.Icon == null ||
            ExistsID() || string.IsNullOrEmpty(Regex.Replace(item.ID, @"[^0-9]+", "")) || !Regex.IsMatch(item.ID, @"(\d+)$")
            );

        if (box)
            editComplete &= !box.ItemsInBox.Exists(x => x.Item == null);

        if (item.ProcessMethod != ProcessMethod.None)
            editComplete &= !item.Materials.Exists(x => x.ProcessType == ProcessType.SingleItem && x.Item == null);

        return editComplete;
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        ItemBase[] items = Resources.LoadAll<ItemBase>("");
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
        List<ItemBase> items = new List<ItemBase>();
        foreach (ItemBase item in Resources.LoadAll<ItemBase>(""))
        {
            items.Add(item);
        }

        ItemBase find = items.Find(x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != item || (find == item && items.FindAll(x => x.ID == _ID.stringValue).Count > 1);
    }
}