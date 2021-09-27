using System;
using UnityEditor;
using UnityEngine;

public class ItemEditor : ConfigurationEditor<ItemBase>
{
    [MenuItem("Zetan Studio/配置管理/道具")]
    private static void CreateWindow()
    {
        ItemEditor window = GetWindowWithRect<ItemEditor>(new Rect(0, 0, 450, 720), false, "道具管理器");
        window.Show();
    }

    protected override void MakeDropDownMenu(GenericMenu menu)
    {
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            if (type != ItemType.Other)
                menu.AddItem(new GUIContent($"增加新{ZetanUtility.GetInspectorName(type)}"), false, CreateNewConfig, ItemBase.ItemTypeToClassType(type));
        }
    }

    protected override string GetNewFileName(Type subType)
    {
        if (subType == typeof(MedicineItem))
            return "medicine";
        else if (subType == typeof(WeaponItem))
            return "weapon";
        else if (subType == typeof(ArmorItem))
            return "armor";
        else if (subType == typeof(BoxItem))
            return "box";
        else if (subType == typeof(MaterialItem))
            return "material";
        else if (subType == typeof(QuestItem))
            return "quest item";
        else if (subType == typeof(GemItem))
            return "gem";
        else if (subType == typeof(BagItem))
            return "bag";
        else if (subType == typeof(SeedItem))
            return "seed";
        else if (subType == typeof(CurrencyItem))
            return "currency";
        else
            return "item";
    }

    protected override bool CompareKey(ItemBase element, out string remark)
    {
        remark = string.Empty;
        if (!element) return false;
        if (element.ID.Contains(keyWords))
        {
            remark = $"识别码：{ZetanEditorUtility.TrimContentByKey(element.Name, keyWords, 16)}";
            return true;
        }
        else if (element.Name.Contains(keyWords))
        {
            remark = $"名称：{ZetanEditorUtility.TrimContentByKey(element.Name, keyWords, 16)}";
            return true;
        }
        else if (element.Description.Contains(keyWords))
        {
            remark = $"描述：{ZetanEditorUtility.TrimContentByKey(element.Description, keyWords, 20)}";
            return true;
        }
        return false;
    }

    protected override string GetConfigurationName()
    {
        return "道具";
    }

    protected override string GetElementName(ItemBase element)
    {
        return element.Name;
    }
}