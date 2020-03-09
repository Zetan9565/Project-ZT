using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ItemWindowManager : WindowHandler<ItemWindowUI, ItemWindowManager>
{
    public bool IsHeld { get; private set; }

    private ItemAgent itemAgent;
    public ItemInfo MItemInfo { get; private set; }

    private ItemAgentType itemAgentType;

    #region UI相关
    public void SetItemAndOpenWindow(ItemAgent itemAgent)
    {
        //UI.windowsRect.position = new Vector3(Input.mousePosition.x - UI.windowsRect.sizeDelta.x, UI.windowsRect.position.y);
        ZetanUtility.KeepInsideScreen(UI.window.transform as RectTransform);
        if (itemAgent == null || itemAgent.IsEmpty || this.itemAgent == itemAgent) return;
        itemAgent.Select();
        if (this.itemAgent) this.itemAgent.DeSelect();
        this.itemAgent = itemAgent;
        LeftOrRight(itemAgent.transform.position);
        animated = false;
        OpenWindow();
        MItemInfo = itemAgent.MItemInfo;
        itemAgentType = itemAgent.agentType;
        UI.icon.overrideSprite = MItemInfo.item.Icon;
        UI.nameText.text = MItemInfo.ItemName;
        UI.nameText.color = GameManager.QualityToColor(MItemInfo.item.Quality);
        UI.typeText.text = ItemBase.GetItemTypeString(MItemInfo.item.ItemType);
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
                ZetanUtility.SetActive(UI.gemstone_1.gameObject, weapon.GemSlotAmout > 0);
                ZetanUtility.SetActive(UI.gemstone_2.gameObject, weapon.GemSlotAmout > 1);
                ZetanUtility.SetActive(UI.durability.gameObject, true);
                if (PlayerManager.Instance.PlayerInfo.HasPrimaryWeapon) OpenSubItemWindow(PlayerManager.Instance.PlayerInfo.primaryWeapon);
                else if (PlayerManager.Instance.PlayerInfo.HasSecondaryWeapon) OpenSubItemWindow(PlayerManager.Instance.PlayerInfo.secondaryWeapon);
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
        //UI.window.alpha = 1;
#if UNITY_STANDALONE
        UI.window.blocksRaycasts = false;
#elif UNITY_ANDROID
        //UI.window.blocksRaycasts = true;
        ZetanUtility.SetActive(UI.buttonsArea, true);
        UI.mulFunButton.onClick.RemoveAllListeners();
        ZetanUtility.SetActive(UI.closeButton.gameObject, true);
        UI.priceTitle.text = "贩卖价格";
#endif
        switch (itemAgentType)
        {
            case ItemAgentType.Backpack:
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.discardButton.gameObject, MItemInfo.item.DiscardAble);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, false);
                UI.mulFunButton.onClick.RemoveAllListeners();
                if (!WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen && !ItemSelectionManager.Instance.IsUIOpen)
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
                else if (ItemSelectionManager.Instance.IsUIOpen)
                {
                    switch (ItemSelectionManager.Instance.SelectionType)
                    {
                        case ItemSelectionType.Discard:
                            if (MItemInfo.item.DiscardAble)
                            {
                                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                                UI.mulFunButton.GetComponentInChildren<Text>().text = "选取";
                                UI.mulFunButton.onClick.AddListener(delegate
                                {
                                    if (ItemSelectionManager.Instance.Place(MItemInfo)) CloseWindow();
                                });
                            }
                            break;
                        case ItemSelectionType.Gift:
                            break;
                        case ItemSelectionType.Making:
                            break;
                        case ItemSelectionType.None:
                        default:
                            break;
                    }
                    ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                }
#endif
                break;
            case ItemAgentType.Warehouse:
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(TakeOutCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "取出";
#endif
                break;
            case ItemAgentType.Making:
#if UNITY_ANDROID
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
                else CloseWindow();
#if UNITY_ANDROID
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
                else CloseWindow();
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, BackpackManager.Instance.GetItemAmount(MItemInfo.ItemID) > 0);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(SellOrPurchaseCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "出售";
#endif
                break;
            case ItemAgentType.Loot:
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(TakeCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "拾取";
#endif
                break;
            case ItemAgentType.Selection:
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, true);
                UI.mulFunButton.onClick.RemoveAllListeners();
                UI.mulFunButton.onClick.AddListener(TakeOutCurrentItem);
                UI.mulFunButton.GetComponentInChildren<Text>().text = "取出";
#endif
                break;
            default:
#if UNITY_ANDROID
                ZetanUtility.SetActive(UI.buttonsArea.gameObject, false);
                ZetanUtility.SetActive(UI.closeButton.gameObject, true);
#endif
                ZetanUtility.SetActive(UI.discardButton.gameObject, false);
                ZetanUtility.SetActive(UI.mulFunButton.gameObject, false);
                UI.mulFunButton.onClick.RemoveAllListeners();
                break;
        }
    }

    private void OpenSubItemWindow(ItemInfo equipped)
    {
        if (equipped == null || !equipped.item || IsPausing) return;
        UI.subUI.icon.overrideSprite = equipped.item.Icon;
        UI.subUI.nameText.text = equipped.ItemName;
        UI.subUI.nameText.color = GameManager.QualityToColor(equipped.item.Quality);
        UI.subUI.typeText.text = ItemBase.GetItemTypeString(equipped.item.ItemType);
        UI.subUI.priceTitle.text = "贩卖价格";
        UI.subUI.priceText.text = equipped.item.SellAble ? equipped.item.SellPrice + GameManager.CoinName : "不可出售";
        UI.subUI.weightText.text = "重量：" + equipped.item.Weight.ToString("F2") + "WL";
        UI.subUI.descriptionText.text = equipped.item.Description;

        switch (equipped.item.ItemType)
        {
            case ItemType.Weapon:
                WeaponItem weapon = equipped.item as WeaponItem;
                UI.subUI.effectText.text = (weapon.CutATK > 0 ? "斩击攻击力+" + weapon.CutATK + "\n" : string.Empty) +
                    (weapon.PunATK > 0 ? "刺击攻击力+" + weapon.PunATK + "\n" : string.Empty) +
                    (weapon.BluATK > 0 ? "钝击攻击力+" + weapon.BluATK + "\n" : string.Empty) +
                    (weapon.DEF > 0 ? "防御力力+" + weapon.DEF + "\n" : string.Empty) +
                    (weapon.Hit > 0 ? "命中+" + weapon.Hit + "\n" : string.Empty);
                if (weapon.Powerup.IsEffective)
                {
                    ZetanUtility.SetActive(UI.subUI.mulFunTitle.gameObject, true);
                    ZetanUtility.SetActive(UI.subUI.mulFunText.gameObject, true);
                    UI.subUI.mulFunTitle.text = "-附加能力";
                    UI.subUI.mulFunText.text = weapon.Powerup.ToString();
                }
                else
                {
                    ZetanUtility.SetActive(UI.subUI.mulFunTitle.gameObject, false);
                    ZetanUtility.SetActive(UI.subUI.mulFunText.gameObject, false);
                    UI.subUI.mulFunTitle.text = string.Empty;
                    UI.subUI.mulFunText.text = string.Empty;
                }
                ZetanUtility.SetActive(UI.subUI.gemstone_1.gameObject, weapon.GemSlotAmout > 0);
                ZetanUtility.SetActive(UI.subUI.gemstone_2.gameObject, weapon.GemSlotAmout > 1);
                ZetanUtility.SetActive(UI.subUI.durability.gameObject, true);
                break;
            default:
                UI.subUI.effectText.text = string.Empty;
                ZetanUtility.SetActive(UI.subUI.mulFunTitle.gameObject, false);
                ZetanUtility.SetActive(UI.subUI.mulFunText.gameObject, false);
                ZetanUtility.SetActive(UI.subUI.gemstone_1.gameObject, false);
                ZetanUtility.SetActive(UI.subUI.gemstone_2.gameObject, false);
                ZetanUtility.SetActive(UI.subUI.durability.gameObject, false);
                break;
        }
        UI.subUI.window.alpha = 1;
        UI.subUI.window.blocksRaycasts = false;
        UI.subUI.windowCanvas.sortingOrder = UI.windowCanvas.sortingOrder;
        //isSubUIOpen = true;
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (itemAgent) itemAgent.DeSelect();
        itemAgent = null;
        MItemInfo = null;
        UI.nameText.text = null;
        UI.descriptionText.text = string.Empty;
        UI.priceText.text = string.Empty;
        ZetanUtility.SetActive(UI.durability.gameObject, false);
        CloseSubWindow();
#if UNITY_ANDROID
        ZetanUtility.SetActive(UI.buttonsArea, false);
        ZetanUtility.SetActive(UI.closeButton.gameObject, false);
#endif
    }

    private void CloseSubWindow()
    {
        UI.subUI.window.alpha = 0;
        UI.subUI.window.blocksRaycasts = false;
        UI.subUI.nameText.text = null;
        UI.subUI.descriptionText.text = string.Empty;
        UI.subUI.priceText.text = string.Empty;
        UI.subUI.descriptionText.text = string.Empty;
        ZetanUtility.SetActive(UI.subUI.durability.gameObject, false);
        //isSubUIOpen = false;
    }
    #endregion

    private void UseCurrenItem()
    {
        BackpackManager.Instance.UseItem(MItemInfo);
        CloseWindow();
    }

    public void DiscardCurrentItem()
    {
        BackpackManager.Instance.DiscardItem(MItemInfo);
        AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
        CloseWindow();
    }

    private void StoreCurrentItem()
    {
        WarehouseManager.Instance.StoreItem(MItemInfo);
        CloseWindow();
    }

    private void TakeCurrentItem()
    {
        LootManager.Instance.TakeItem(MItemInfo);
        CloseWindow();
    }

    private void TakeOutCurrentItem()
    {
        if (itemAgentType == ItemAgentType.Warehouse)
            WarehouseManager.Instance.TakeOutItem(MItemInfo);
        else if (itemAgentType == ItemAgentType.Selection)
            ItemSelectionManager.Instance.TakeOut(MItemInfo);
        CloseWindow();
    }

    private void SellOrPurchaseCurrentItem()
    {
        MerchandiseAgent agent = ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo);
        if (agent) agent.OnSellOrPurchase();
        else ShopManager.Instance.PurchaseItem(MItemInfo);
        CloseWindow();
    }

    private void MakeCurrentItem()
    {
        MakingManager.Instance.MakeCurrent();
        CloseWindow();
    }

    //private bool isSubUIOpen;
    //public void PauseShowing(bool pause)
    //{
    //    if (!IsUIOpen || IsPausing == pause) return;
    //    //Debug.Log("pause from+" + IsPausing + " to: " + pause);
    //    if (!IsPausing && pause)//从非暂停变成暂停
    //    {
    //        UI.window.alpha = 0;
    //        UI.window.blocksRaycasts = false;
    //        if (isSubUIOpen)
    //        {
    //            UI.subUI.window.alpha = 0;
    //            UI.subUI.window.blocksRaycasts = false;
    //        }
    //    }
    //    else if (IsPausing && !pause)//从暂停变成非暂停
    //    {
    //        UI.window.alpha = 1;
    //        UI.window.blocksRaycasts = true;
    //        if (isSubUIOpen)
    //        {
    //            UI.subUI.window.alpha = 1;
    //            UI.subUI.window.blocksRaycasts = true;
    //        }
    //    }
    //    IsPausing = pause;
    //}

    public void Hold()
    {
        IsHeld = true;
    }

    public void Dehold()
    {
        IsHeld = false;
    }

    private void LeftOrRight(Vector2 position)
    {
        if (Screen.width * 0.5f < position.x)//在屏幕右半边
        {
            UI.subUI.window.transform.SetAsFirstSibling();
            UI.buttonsArea.transform.SetAsLastSibling();
            Rect rectAgent = ZetanUtility.GetScreenSpaceRect(itemAgent.GetComponent<RectTransform>());
            Rect rectWin = ZetanUtility.GetScreenSpaceRect(UI.subUI.GetComponent<RectTransform>());
            Rect rectButton = ZetanUtility.GetScreenSpaceRect(UI.buttonsArea.GetComponent<RectTransform>());
            UI.window.transform.position = new Vector2(position.x - rectAgent.width * 0.5f - rectWin.width - rectButton.width, UI.window.transform.position.y);
        }
        else
        {
            UI.subUI.window.transform.SetAsLastSibling();
            UI.buttonsArea.transform.SetAsFirstSibling();
            Rect rectAgent = ZetanUtility.GetScreenSpaceRect(itemAgent.GetComponent<RectTransform>());
            Rect rectWin = ZetanUtility.GetScreenSpaceRect(UI.subUI.GetComponent<RectTransform>());
            Rect rectButton = ZetanUtility.GetScreenSpaceRect(UI.buttonsArea.GetComponent<RectTransform>());
            UI.window.transform.position = new Vector2(position.x + rectAgent.width * 0.5f + rectWin.width + rectButton.width, UI.window.transform.position.y);
        }
        ZetanUtility.KeepInsideScreen(UI.window.GetComponent<RectTransform>(), true, true, false, false);
    }

    public override void SetUI(ItemWindowUI UI)
    {
        this.UI = UI;
        this.UI.subUI = UI.subUI;
    }
}