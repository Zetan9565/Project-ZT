using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ItemWindowManager : SingletonMonoBehaviour<ItemWindowManager>
{
    [SerializeField]
    private ItemWindowUI UI;

    [SerializeField]
    private ItemWindowBaseUI subUI;

    private bool IsPause;

    public bool IsHeld { get; private set; }

    public ItemInfo MItemInfo { get; private set; }

    public ItemAgentType ItemAgentType { get; private set; }

    public void OpenItemWindow(ItemAgent itemAgent)
    {
        //UI.windowsRect.position = new Vector3(Input.mousePosition.x - UI.windowsRect.sizeDelta.x, UI.windowsRect.position.y);
        if (itemAgent.MItemInfo == null || !itemAgent.MItemInfo.item || IsPause) return;
        MItemInfo = itemAgent.MItemInfo;
        ItemAgentType = itemAgent.agentType;
        UI.icon.overrideSprite = MItemInfo.item.Icon;
        UI.nameText.text = MItemInfo.ItemName;
        UI.nameText.color = itemAgent.currentQualityColor;
        UI.typeText.text = GetItemTypeString(MItemInfo.item.ItemType);
        UI.priceText.text = MItemInfo.item.SellAble ? MItemInfo.item.SellPrice + GameManager.CoinName : "不可出售";
        UI.weightText.text = "重量：" + MItemInfo.item.Weight.ToString("F2") + "WL";
        UI.descriptionText.text = MItemInfo.item.Description;
        switch (MItemInfo.item.ItemType)
        {
            case ItemType.Weapon:
                WeaponItem weapon = MItemInfo.item as WeaponItem;
                UI.effectText.text = (weapon.CutATK > 0 ? "斩击攻击力+" + weapon.CutATK + "\n" : string.Empty) +
                    (weapon.PunATK > 0 ? "刺击攻击力+" + weapon.PunATK + "\n" : string.Empty) +
                    (weapon.BluATK > 0 ? "钝击攻击力+" + weapon.BluATK + "\n" : string.Empty) +
                    (weapon.DEF > 0 ? "防御力力+" + weapon.DEF + "\n" : string.Empty) +
                    (weapon.Hit > 0 ? "命中+" + weapon.Hit + "\n" : string.Empty);
                if (weapon.Powerup.IsEffective)
                {
                    ZetanUtility.SetActive(UI.mulFunTitle.gameObject, true);
                    ZetanUtility.SetActive(UI.mulFunText.gameObject, true);
                    UI.mulFunTitle.text = "-附加能力";
                    UI.mulFunText.text = weapon.Powerup.ToString();
                }
                else
                {
                    ZetanUtility.SetActive(UI.mulFunTitle.gameObject, false);
                    ZetanUtility.SetActive(UI.mulFunText.gameObject, false);
                    UI.mulFunTitle.text = string.Empty;
                    UI.mulFunText.text = string.Empty;
                }
                if (weapon.GemSlotAmout > 0)
                    ZetanUtility.SetActive(UI.gemstone_1.gameObject, true);
                else
                    ZetanUtility.SetActive(UI.gemstone_1.gameObject, false);
                if (weapon.GemSlotAmout > 1)
                    ZetanUtility.SetActive(UI.gemstone_2.gameObject, true);
                else
                    ZetanUtility.SetActive(UI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(UI.durability.gameObject, true);
                if (PlayerManager.Instance.PlayerInfo.HasPrimaryWeapon)
                    OpenSubItemWindow(PlayerManager.Instance.PlayerInfo.primaryWeapon);
                else if (PlayerManager.Instance.PlayerInfo.HasSecondaryWeapon)
                    OpenSubItemWindow(PlayerManager.Instance.PlayerInfo.secondaryWeapon);
                break;
            case ItemType.Bag:
                UI.effectText.text = GameManager.BackpackName + "容量+" + (MItemInfo.item as BagItem).ExpandSize;
                ZetanUtility.SetActive(UI.mulFunTitle.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunText.gameObject, false);
                ZetanUtility.SetActive(UI.gemstone_1.gameObject, false);
                ZetanUtility.SetActive(UI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(UI.durability.gameObject, false);
                CloseSubWindow();
                break;
            case ItemType.Box:
                UI.effectText.text = string.Empty;
                BoxItem box = MItemInfo.item as BoxItem;
                UI.mulFunTitle.text = "-内含物品";
                ZetanUtility.SetActive(UI.mulFunTitle.gameObject, true);
                System.Text.StringBuilder itemsInfo = new System.Text.StringBuilder();
                for (int i = 0; i < box.ItemsInBox.Count; i++)
                {
                    itemsInfo.Append("[" + box.ItemsInBox[i].ItemName + "] × " + box.ItemsInBox[i].Amount);
                    if (i != box.ItemsInBox.Count - 1) itemsInfo.Append("\n");
                }
                UI.mulFunText.text = itemsInfo.ToString();
                ZetanUtility.SetActive(UI.mulFunText.gameObject, true);
                ZetanUtility.SetActive(UI.gemstone_1.gameObject, false);
                ZetanUtility.SetActive(UI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(UI.durability.gameObject, false);
                CloseSubWindow();
                break;
            default:
                UI.effectText.text = string.Empty;
                ZetanUtility.SetActive(UI.mulFunTitle.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunText.gameObject, false);
                ZetanUtility.SetActive(UI.gemstone_1.gameObject, false);
                ZetanUtility.SetActive(UI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(UI.durability.gameObject, false);
                CloseSubWindow();
                break;
        }
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = false;
#if UNITY_ANDROID
        UI.window.blocksRaycasts = true;
        UI.buttonAreaCanvas.alpha = 1;
        UI.buttonAreaCanvas.blocksRaycasts = true;
        UI.mulFunButton.onClick.RemoveAllListeners();
        ZetanUtility.SetActive(UI.closeButton.gameObject, true);
#endif
        switch (itemAgent.agentType)
        {
            case ItemAgentType.Backpack:
                UI.priceTitle.text = "贩卖价格";
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea, true);
                ZetanUtility.SetActive(UI.discardButton.gameObject, MItemInfo.item.DiscardAble);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, false);
                UI.mulFunButton.onClick.RemoveAllListeners();
                if (!WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen)
                {
                    if (MItemInfo.item.Usable)
                    {
                        ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                        UI.mulFunButton.GetComponentInChildren<Text>().text = MItemInfo.item.IsEquipment ? "装备" : "使用";
                        UI.mulFunButton.onClick.AddListener(UseCurrenItem);
                    }
                }
                else if (WarehouseManager.Instance.IsUIOpen)
                {
                    ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                    UI.mulFunButton.GetComponentInChildren<Text>().text = "存入";
                    UI.mulFunButton.onClick.AddListener(StoreCurrentItem);
                }
                else if (ShopManager.Instance.IsUIOpen)
                {
                    if (MItemInfo.item.SellAble)
                    {
                        ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                        UI.mulFunButton.GetComponentInChildren<Text>().text = "出售";
                        UI.mulFunButton.onClick.AddListener(SellOrPurchaseCurrentItem);
                    }
                    ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                }
#endif
                break;
            case ItemAgentType.Warehouse:
                UI.priceTitle.text = "贩卖价格";
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea, true);
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(TakeOutCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "取出";
#endif
                break;
            case ItemAgentType.Making:
                UI.priceTitle.text = "贩卖价格";
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea, true);
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, MItemInfo.Amount > 0);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(MakeCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "制作";
#endif
                break;
            case ItemAgentType.Selling:
                UI.priceTitle.text = "售价";
                if (ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo))
                    UI.priceText.text = ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo.SellPrice.ToString() + GameManager.CoinName;
                else CloseItemWindow();
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea, true);
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(SellOrPurchaseCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "购买";
#endif
                break;
            case ItemAgentType.Purchasing:
                UI.priceTitle.text = "收购价";
                if (ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo))
                    UI.priceText.text = ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo.PurchasePrice.ToString() + GameManager.CoinName;
                else CloseItemWindow();
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea, true);
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, BackpackManager.Instance.GetItemAmount(MItemInfo.ItemID) > 0);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(SellOrPurchaseCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "出售";
#endif
                break;
            case ItemAgentType.Loot:
                UI.priceTitle.text = "贩卖价格";
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea, true);
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(TakeCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "拾取";
#endif
                break;
            default:
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.closeButton.gameObject, true);
#endif
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, false);
                UI.mulFunButton.onClick.RemoveAllListeners();
                break;
        }
        UI.windowCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
    }

    public void OpenSubItemWindow(ItemInfo equiped)
    {
        if (equiped == null || !equiped.item || IsPause) return;
        subUI.icon.overrideSprite = equiped.item.Icon;
        subUI.nameText.text = equiped.ItemName;
        if (GameManager.QualityColors.Count >= 5)
            subUI.nameText.color = GameManager.QualityColors[(int)equiped.item.Quality];
        subUI.typeText.text = GetItemTypeString(equiped.item.ItemType);
        subUI.priceTitle.text = "贩卖价格";
        subUI.priceText.text = equiped.item.SellAble ? equiped.item.SellPrice + GameManager.CoinName : "不可出售";
        subUI.weightText.text = "重量：" + equiped.item.Weight.ToString("F2") + "WL";
        subUI.descriptionText.text = equiped.item.Description;
        switch (equiped.item.ItemType)
        {
            case ItemType.Weapon:
                WeaponItem weapon = equiped.item as WeaponItem;
                subUI.effectText.text = (weapon.CutATK > 0 ? "攻击力+" + weapon.CutATK + "\n" : string.Empty) +
                    (weapon.DEF > 0 ? "防御力力+" + weapon.DEF + "\n" : string.Empty) +
                    (weapon.Hit > 0 ? "命中+" + weapon.Hit + "\n" : string.Empty);
                if (weapon.Powerup.IsEffective)
                {
                    ZetanUtility.SetActive(subUI.mulFunTitle.gameObject, true);
                    ZetanUtility.SetActive(subUI.mulFunText.gameObject, true);
                    subUI.mulFunTitle.text = "-附加能力";
                    subUI.mulFunText.text = weapon.Powerup.ToString();
                }
                else
                {
                    ZetanUtility.SetActive(subUI.mulFunTitle.gameObject, false);
                    ZetanUtility.SetActive(subUI.mulFunText.gameObject, false);
                    subUI.mulFunTitle.text = string.Empty;
                    subUI.mulFunText.text = string.Empty;
                }
                if (weapon.GemSlotAmout > 0)
                    ZetanUtility.SetActive(subUI.gemstone_1.gameObject, true);
                else
                    ZetanUtility.SetActive(subUI.gemstone_1.gameObject, false);
                if (weapon.GemSlotAmout > 1)
                    ZetanUtility.SetActive(subUI.gemstone_2.gameObject, true);
                else
                    ZetanUtility.SetActive(subUI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(subUI.durability.gameObject, true);
                break;
            default:
                subUI.effectText.text = string.Empty;
                ZetanUtility.SetActive(subUI.mulFunTitle.gameObject, false);
                ZetanUtility.SetActive(subUI.mulFunText.gameObject, false);
                ZetanUtility.SetActive(subUI.gemstone_1.gameObject, false);
                ZetanUtility.SetActive(subUI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(subUI.durability.gameObject, false);
                break;
        }
        subUI.window.alpha = 1;
        subUI.window.blocksRaycasts = false;
        subUI.windowCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
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
            case ItemType.Bag:
            case ItemType.Medicine:
            case ItemType.Elixir:
            case ItemType.Cuisine:
                return "消耗品";
            default: return "普通";
        }
    }

    public void CloseItemWindow()
    {
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        MItemInfo = null;
        UI.nameText.text = null;
        UI.descriptionText.text = string.Empty;
        UI.priceText.text = string.Empty;
        UI.descriptionText.text = string.Empty;
        ZetanUtility.SetActive(UI.durability.gameObject, false);
        CloseSubWindow();
#if UNITY_ANDROID
        UI.buttonAreaCanvas.alpha = 0;
        UI.buttonAreaCanvas.blocksRaycasts = false;
        ZetanUtility.SetActive(UI.closeButton.gameObject, false);
#endif
    }

    public void CloseSubWindow()
    {
        subUI.window.alpha = 0;
        subUI.window.blocksRaycasts = false;
        subUI.nameText.text = null;
        subUI.descriptionText.text = string.Empty;
        subUI.priceText.text = string.Empty;
        subUI.descriptionText.text = string.Empty;
        ZetanUtility.SetActive(subUI.durability.gameObject, false);
    }

    public void UseCurrenItem()
    {
        BackpackManager.Instance.UseItem(MItemInfo);
        CloseItemWindow();
    }

    public void DiscardCurrentItem()
    {
        BackpackManager.Instance.DiscardItem(MItemInfo);
        AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
        CloseItemWindow();
    }

    public void StoreCurrentItem()
    {
        WarehouseManager.Instance.StoreItem(MItemInfo);
        CloseItemWindow();
    }

    public void TakeCurrentItem()
    {
        LootManager.Instance.TakeItem(MItemInfo);
        CloseItemWindow();
    }

    public void TakeOutCurrentItem()
    {
        WarehouseManager.Instance.TakeOutItem(MItemInfo);
        CloseItemWindow();
    }

    public void SellOrPurchaseCurrentItem()
    {
        MerchandiseAgent agent = ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo);
        if (agent) agent.OnSellOrPurchase();
        else ShopManager.Instance.PurchaseItem(MItemInfo);
        CloseItemWindow();
    }

    public void MakeCurrentItem()
    {
        MakingManager.Instance.MakeCurrent();
        CloseItemWindow();
    }

    public void PauseShowing(bool pause)
    {
        IsPause = pause;
        CloseItemWindow();
    }

    public void Hold()
    {
        IsHeld = true;
    }

    public void Dehold()
    {
        IsHeld = false;
    }

    public void SetUI(ItemWindowUI UI, ItemWindowBaseUI subUI)
    {
        this.UI = UI;
        this.subUI = subUI;
    }
}