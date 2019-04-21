using UnityEngine;
using System.Collections;

public class ItemWindowManager : MonoBehaviour
{
    private static ItemWindowManager instance;
    public static ItemWindowManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<ItemWindowManager>();
            return instance;
        }
    }

    [SerializeField]
    private ItemWindowUI UI;

    private bool IsPause;

    public bool IsHeld { get; private set; }

    public ItemInfo ItemInfo { get; private set; } = null;

    public ItemAgentType ItemAgentType { get; private set; } = ItemAgentType.None;

    public void OpenItemWindow(ItemAgent itemAgent)
    {
        if (itemAgent.itemInfo == null || !itemAgent.itemInfo.Item || IsPause) return;
        ItemInfo = itemAgent.itemInfo;
        UI.icon.overrideSprite = ItemInfo.Item.Icon;
        UI.nameText.text = ItemInfo.ItemName;
        UI.nameText.color = itemAgent.currentQualityColor;
        UI.typeText.text = GetItemTypeString(ItemInfo.Item.ItemType);
        UI.priceText.text = ItemInfo.Item.SellAble ? ItemInfo.Item.SellPrice + "文" : "不可出售";
        UI.weightText.text = "重量：" + ItemInfo.Item.Weight.ToString("F2") + "WL";
        UI.descriptionText.text = ItemInfo.Item.Description;
        switch (itemAgent.agentType)
        {
            case ItemAgentType.None: break;
            default: break;
        }
        switch (ItemInfo.Item.ItemType)
        {
            case ItemType.Weapon:
                WeaponItem weapon = ItemInfo.Item as WeaponItem;
                UI.effectText.text = (weapon.ATK > 0 ? "攻击力+" + weapon.ATK + "\n" : string.Empty) +
                    (weapon.DEF > 0 ? "防御力力+" + weapon.DEF + "\n" : string.Empty) +
                    (weapon.Hit > 0 ? "命中+" + weapon.Hit + "\n" : string.Empty);
                if (weapon.Powerup.IsEffective)
                {
                    MyTools.SetActive(UI.mulFunTitle.gameObject, true);
                    MyTools.SetActive(UI.mulFunText.gameObject, true);
                    UI.mulFunTitle.text = "附加能力";
                    UI.mulFunText.text = weapon.Powerup.ToString();
                }
                else
                {
                    MyTools.SetActive(UI.mulFunTitle.gameObject, false);
                    MyTools.SetActive(UI.mulFunText.gameObject, false);
                    UI.mulFunTitle.text = string.Empty;
                    UI.mulFunText.text = string.Empty;
                }
                if (weapon.GemSlotAmout > 0)
                    MyTools.SetActive(UI.gemstone_1.gameObject, true);
                else
                    MyTools.SetActive(UI.gemstone_1.gameObject, false);
                if (weapon.GemSlotAmout > 1)
                    MyTools.SetActive(UI.gemstone_2.gameObject, true);
                else
                    MyTools.SetActive(UI.gemstone_2.gameObject, false);
                MyTools.SetActive(UI.durability.gameObject, true);
                break;
            default:
                UI.effectText.text = string.Empty;
                MyTools.SetActive(UI.mulFunTitle.gameObject, false);
                MyTools.SetActive(UI.mulFunText.gameObject, false);
                MyTools.SetActive(UI.gemstone_1.gameObject, false);
                MyTools.SetActive(UI.gemstone_2.gameObject, false);
                MyTools.SetActive(UI.durability.gameObject, false);
                break;
        }
        UI.itemWindow.alpha = 1;
        UI.itemWindow.blocksRaycasts = false;
#if UNITY_ANDROID
        UI.mulFunButton.onClick.RemoveAllListeners();
        if (ItemAgentType == ItemAgentType.Backpack)
        {
            MyTools.SetActive(UI.mulFunButton.gameObject, ItemInfo.Item.Useable);
            UI.mulFunButton.onClick.AddListener(UseCurrenItem);
        }
#endif
    }

    public static string GetItemTypeString(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Jewelry:
            case ItemType.Tool:
                return "装备";
            case ItemType.Quest:
            case ItemType.Valuables:
                return "特殊";
            case ItemType.Material:
                return "材料";
            case ItemType.Box:
            case ItemType.Medicine:
            case ItemType.DanMedicine:
            case ItemType.Cuisine:
                return "消耗品";
            default: return "普通";
        }
    }

    public void CloseItemWindow()
    {
        UI.itemWindow.alpha = 0;
        UI.itemWindow.blocksRaycasts = false;
        ItemInfo = null;
        UI.nameText.text = null;
        UI.descriptionText.text = string.Empty;
        UI.priceText.text = string.Empty;
        UI.descriptionText.text = string.Empty;
        MyTools.SetActive(UI.durability.gameObject, false);
    }

    public void UseCurrenItem()
    {
        BackpackManager.Instance.GetItemAgentByInfo(ItemInfo).OnUse();
    }

    public void DiscardCurrentItem()
    {
        BackpackManager.Instance.LoseItem(ItemInfo);
        if (ItemInfo.Amount < 1) CloseItemWindow();
    }

    public void PauseShowing(bool value)
    {
        IsPause = value;
    }

    public void Hold()
    {
        IsHeld = true;
    }

    public void Dehold()
    {
        IsHeld = false;
    }
}