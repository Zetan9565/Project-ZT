using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/背包管理器")]
public class BackpackManager : WindowHandler<BackpackUI, BackpackManager>, IOpenCloseAbleWindow
{
    [SerializeField]
    private Color overColor = Color.yellow;
    [SerializeField]
    private Color maxColor = Color.red;

#if UNITY_ANDROID
    public DiscardButton DiscardButton { get { return UI.discardButton; } }
#endif

    public bool IsInputFocused => UI ? IsUIOpen && UI.searchInput.isFocused : false;

    public Image GridMask { get { return UI.gridMask; } }

    public delegate void ItemAmountListener(string id, int leftAmount);
    public event ItemAmountListener OnGetItemEvent;
    public event ItemAmountListener OnLoseItemEvent;

    public List<ItemAgent> itemAgents = new List<ItemAgent>();

    private Backpack Backpack => PlantManager.Instance ? PlayerManager.Instance.Backpack : null;

    //TODO 临时字段，影响美观，到时再改
    public List<ItemInfo> Seeds => Backpack ? Backpack.Items.FindAll(x => x.item.IsSeed) : null;

    public long Money => Backpack ? Backpack.Money : 0;

    public void Init()
    {
        if (!UI || !UI.gameObject) return;
        if (Backpack != null)
        {
            foreach (ItemAgent ia in itemAgents)
                ia.Empty();
            while (Backpack.size.Max > itemAgents.Count)
            {
                ItemAgent ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                itemAgents.Add(ia);
                ia.Init(ItemAgentType.Backpack, itemAgents.Count - 1, UI.gridScrollRect);
            }
            while (Backpack.size.Max < itemAgents.Count)
            {
                itemAgents[itemAgents.Count - 1].Clear(true);
                itemAgents.RemoveAt(itemAgents.Count - 1);
            }
            UpdateUI();
        }
        UI.pageSelector.SetValueWithoutNotify(0);
    }

    #region 道具获得相关
    /// <summary>
    /// 求取道具最大可获取数量
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">目标获取量</param>
    /// <returns>实际获取量</returns>
    public int TryGetItem_Integer(ItemBase item, int amount = 1)
    {
        if (Backpack == null || !item || amount < 1) return 0;
        if (!item.StackAble && Backpack.IsFull)
        {
            return 0;
        }
        int finalGet = amount;
        if (!item.StackAble)
        {
            if (amount > Backpack.size.Rest)
                finalGet = Backpack.size.Rest;
        }
        if (Backpack.weight + finalGet * item.Weight > Backpack.weight.Max)
            for (int i = 0; i <= finalGet; i++)
            {
                if (Backpack.weight + i * item.Weight <= Backpack.weight.Max &&
                    Backpack.weight + (i + 1) * item.Weight > Backpack.weight.Max)
                {
                    finalGet = i;
                    break;
                }
            }
        return finalGet;
    }
    /// <summary>
    /// 求取道具最大可获取数量
    /// </summary>
    /// <param name="info">道具信息</param>
    /// <param name="amount">目标获取量</param>
    /// <returns>实际获取量</returns>
    public int TryGetItem_Integer(ItemInfo info, int amount)
    {
        return TryGetItem_Integer(info.item, amount);
    }
    /// <summary>
    /// 求取道具最大可获取数量
    /// </summary>
    /// <param name="info">道具信息（包括数量）</param>
    /// <returns>实际获取量</returns>
    public int TryGetItem_Integer(ItemInfo info)
    {
        return TryGetItem_Integer(info, info.Amount);
    }

    /// <summary>
    /// 尝试可否获取道具
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">获取数量</param>
    /// <param name="simulLoseItems">会同时失去的道具</param>
    /// <returns>可否获取</returns>
    public bool TryGetItem_Boolean(ItemBase item, int amount, params ItemInfo[] simulLoseItems)
    {
        if (Backpack == null || !item)
        {
            MessageManager.Instance.New("无效的道具");
            return false;
        }
        if (amount < 1)
        {
            if (amount < 0) MessageManager.Instance.New("无效的数量");
            return false;
        }
        if (!item.StackAble)//不可叠加，则看剩余空间是否足够放下
        {
            int vacateSize = 0;
            if (HasItemToLose())
                foreach (var info in simulLoseItems)
                {
                    if (!TryLoseItem_Boolean(info.item, info.Amount)) return false;//有一个要失去的道具不能失去，则直接不能获取该道具
                    if (info.item.StackAble && info.Amount - GetItemAmount(info.item) == 0)//只要该可叠加道具会全部失去，则能留出一个位置
                        vacateSize++;
                    else if (!info.item.StackAble)//若该道具不可叠加，则失去多少个就能空出多少位置
                        vacateSize += info.Amount;
                }
            if (amount > Backpack.size.Rest + vacateSize)//如果留出位置还不能放下
            {
                MessageManager.Instance.New($"请至少多留出{(amount - Backpack.size.Rest - vacateSize).ToString()}个{GameManager.BackpackName}空间");
                return false;
            }
        }
        float vacateWeightload = 0;
        if (HasItemToLose())
            foreach (var info in simulLoseItems)
            {
                if (!TryLoseItem_Boolean(info.item, info.Amount)) return false;
                vacateWeightload += info.item.Weight * info.Amount;
            }
        if (Backpack.weight - vacateWeightload + amount * item.Weight > Backpack.WeightLimit)//如果留出负重还不能放下
        {
            MessageManager.Instance.New("这些物品太重了");
            return false;
        }
        return true;

        bool HasItemToLose()
        {
            return simulLoseItems != null && simulLoseItems.Length > 0;
        }
    }
    /// <summary>
    /// 尝试可否获取道具
    /// </summary>
    /// <param name="info">道具信息</param>
    /// <param name="amount">获取数量</param>
    /// <param name="simulLoseItems">会同时失去的道具</param>
    /// <returns>可否获取</returns>
    public bool TryGetItem_Boolean(ItemInfo info, int amount, params ItemInfo[] simulLoseItems)
    {
        if (info == null || amount < 1) return false;
        return TryGetItem_Boolean(info.item, amount, simulLoseItems);
    }
    /// <summary>
    /// 尝试可否获取道具
    /// </summary>
    /// <param name="info">道具信息（包括数量）</param>
    /// <param name="simulLoseItems">会同时失去的道具</param>
    /// <returns>可否获取</returns>
    public bool TryGetItem_Boolean(ItemInfo info, params ItemInfo[] simulLoseItems)
    {
        if (info == null) return false;
        return TryGetItem_Boolean(info, info.Amount, simulLoseItems);
    }

    /// <summary>
    /// 获取道具
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">获取数量</param>
    /// <param name="simulLoseItems">会同时失去的道具</param>
    /// <returns>是否成功</returns>
    public bool GetItem(ItemBase item, int amount, params ItemInfo[] simulLoseItems)
    {
        if (Backpack == null || !item || amount < 1) return false;
        if (!TryGetItem_Boolean(item, amount, simulLoseItems)) return false;
        if (simulLoseItems != null)
            foreach (var si in simulLoseItems)
                LoseItem(si.item, si.Amount);
        if (item.StackAble)
        {
            Backpack.GetItemSimple(item, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && (x.MItemInfo.item == item || x.MItemInfo.ItemID == item.ID));
            if (ia) ia.UpdateInfo();
            else//如果找不到，说明该物品是新的，是原来背包里没有的
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.SetItem(Backpack.LatestInfo);
                else
                {
                    MessageManager.Instance.New("发生内部错误！");
                    Debug.Log("[Get Item Error: Can't find ItemAgent] ID: " + item.ID + "[" + System.DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                Backpack.GetItemSimple(item);
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.SetItem(Backpack.LatestInfo);
                        break;
                    }
            }
        OnGetItemEvent?.Invoke(item.ID, GetItemAmount(item));
        UpdateUI();
        return true;
    }
    /// <summary>
    /// 获取道具
    /// </summary>
    /// <param name="info">道具信息（包括数量）</param>
    /// <param name="simulLoseItems">会同时失去的道具</param>
    /// <returns>是否成功</returns>
    public bool GetItem(ItemInfo info, params ItemInfo[] simulLoseItems)
    {
        return GetItem(info.item, info.Amount, simulLoseItems);
    }
    /// <summary>
    /// 仓库、装备专用获取道具
    /// </summary>
    /// <param name="info">道具信息</param>
    /// <param name="amount">获取数量</param>
    /// <param name="simulLoseItems">会同时失去的道具</param>
    /// <returns>是否成功</returns>
    public bool GetItem(ItemInfo info, int amount, params ItemInfo[] simulLoseItems)//仓库、装备专用
    {
        if (Backpack == null || info == null || !info.item || amount < 1) return false;
        if (!TryGetItem_Boolean(info, amount, simulLoseItems)) return false;
        if (simulLoseItems != null)
            foreach (var si in simulLoseItems)
                LoseItem(si.item, si.Amount);
        if (info.item.StackAble)
        {
            Backpack.GetItemSimple(info, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && (x.MItemInfo.item == info.item || x.MItemInfo.ItemID == info.ItemID));
            if (ia) ia.UpdateInfo();
            else//如果找不到，说明该物品是新的，原来背包里没有的
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.SetItem(Backpack.LatestInfo);
                else
                {
                    MessageManager.Instance.New("发生内部错误！");
                    Debug.Log("[Get Item Error: Can't find ItemAgent] ID: " + info.item.ID + "[" + System.DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                Backpack.GetItemSimple(info);
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.SetItem(Backpack.LatestInfo);
                        break;
                    }
            }
        OnGetItemEvent?.Invoke(info.ItemID, amount);
        UpdateUI();
        return true;
    }
    #endregion

    #region 道具失去相关
    /// <summary>
    /// 尝试可否失去道具
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">失去数量</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>可否失去</returns>
    public bool TryLoseItem_Boolean(ItemBase item, int amount, params ItemInfo[] simulGetItems)
    {
        if (Backpack == null || !item || amount < 1) return false;
        if (simulGetItems != null)
            foreach (var si in simulGetItems)
                if (!TryGetItem_Boolean(si, new ItemInfo(item, amount))) return false;
        if (GetItemAmount(item) < amount)
        {
            MessageManager.Instance.New($"{GameManager.BackpackName}中没有这么多的 [{item.name}]");
            return false;
        }
        if (QuestManager.Instance.HasQuestRequiredItem(item, GetItemAmount(item) - amount))
        {
            MessageManager.Instance.New($"[{item.name}] 为任务所需");
            return false;
        }
        return true;
    }
    /// <summary>
    /// 尝试可否失去道具（精确到背包中的道具）
    /// </summary>
    /// <param name="info">背包中的道具信息</param>
    /// <param name="amount">失去数量</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>可否失去</returns>
    public bool TryLoseItem_Boolean(ItemInfo info, int amount, params ItemInfo[] simulGetItems)
    {
        if (!info) return false;
        if (Backpack.Items.Contains(info))
            return TryLoseItem_Boolean(info.item, amount, simulGetItems);
        else return false;
    }
    /// <summary>
    /// 尝试可否失去道具
    /// </summary>
    /// <param name="info">道具信息（包括数量）</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>可否失去</returns>
    public bool TryLoseItem_Boolean(ItemInfo info, params ItemInfo[] simulGetItems)
    {
        return TryLoseItem_Boolean(info.item, info.Amount, simulGetItems);
    }
    public bool TryLoseItems_Boolean(IEnumerable<ItemInfo> infos)
    {
        foreach (ItemInfo info in infos)
        {
            if (!TryLoseItem_Boolean(info))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 失去道具
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">失去数量</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>是否成功</returns>
    public bool LoseItem(ItemBase item, int amount, params ItemInfo[] simulGetItems)
    {
        if (item.StackAble)
        {
            return LoseItem(Backpack.Find(item), amount, simulGetItems);
        }
        else
        {
            if (simulGetItems != null)
                foreach (var si in simulGetItems)
                    if (!TryGetItem_Boolean(si, new ItemInfo(item, amount))) return false;
            ItemInfo[] finds = Backpack.FindAll(item).ToArray();
            if (finds.Length < 1)
            {
                MessageManager.Instance.New($"该物品已不在{GameManager.BackpackName}中");
                return false;
            }
            for (int i = 0; i < amount; i++)
                if (!LoseItem(finds[i], 1))
                    return false;
            if (simulGetItems != null)
                foreach (var si in simulGetItems)
                    GetItem(si);
            return true;
        }
    }
    /// <summary>
    /// 失去道具（精确到个例）
    /// </summary>
    /// <param name="info">道具信息</param>
    /// <param name="amount">失去数量</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>是否成功</returns>
    public bool LoseItem(ItemInfo info, int amount, params ItemInfo[] simulGetItems)
    {
        if (Backpack == null || info == null || !info.item || amount < 1) return false;
        if (!TryLoseItem_Boolean(info, amount)) return false;
        if (simulGetItems != null)
            foreach (var si in simulGetItems)
                if (!TryGetItem_Boolean(si, new ItemInfo(info.item, amount))) return false;
        Backpack.LoseItemSimple(info, amount);
        ItemAgent ia = itemAgents.Find(x => x.MItemInfo == info);
        if (ia) ia.UpdateInfo();
        OnLoseItemEvent?.Invoke(info.ItemID, GetItemAmount(info.item));
        if (ItemWindowManager.Instance.MItemInfo == info && info.Amount < 1) ItemWindowManager.Instance.CloseWindow();
        if (simulGetItems != null)
            foreach (var si in simulGetItems)
                GetItem(si);
        UpdateUI();
        return true;
    }
    /// <summary>
    /// 失去道具
    /// </summary>
    /// <param name="info">道具信息（包括数量）</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>是否成功</returns>
    public bool LoseItem(ItemInfo info, params ItemInfo[] simulGetItems)
    {
        return LoseItem(info.item, info.Amount, simulGetItems);
    }

    /// <summary>
    /// 丢弃道具
    /// </summary>
    /// <param name="info">道具信息</param>
    public void DiscardItem(ItemInfo info)
    {
        if (Backpack == null || info == null || !info.item) return;
        if (!Backpack.Items.Contains(info))
        {
            MessageManager.Instance.New($"该物品已不在{GameManager.BackpackName}中");
            return;
        }
        if (!info.item.DiscardAble)
        {
            MessageManager.Instance.New("该物品不可丢弃");
            return;
        }
        if (info.Amount < 2 && info.Amount > 0)
        {
            ConfirmManager.Instance.New(string.Format("确定丢弃1个 [{0}] 吗？",
                ZetanUtility.ColorText(info.ItemName, GameManager.QualityToColor(info.item.Quality))), delegate
                {
                    if (LoseItem(info, 1))
                        MessageManager.Instance.New($"丢掉了1个 [{info.ItemName}]");
                });
        }
        else AmountManager.Instance.New(delegate
        {
            ConfirmManager.Instance.New(string.Format("确定丢弃{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount,
                ZetanUtility.ColorText(info.ItemName, GameManager.QualityToColor(info.item.Quality))), delegate
                {
                    if (LoseItem(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.New(string.Format("丢掉了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.ItemName));
                });
        }, info.Amount);
    }
    /// <summary>
    /// 批量丢弃道具
    /// </summary>
    /// <param name="items">道具列表</param>
    public void DiscardItems(IEnumerable<ItemInfo> items)
    {
        if (items == null) return;
        foreach (var item in items)
            LoseItem(item);
    }
    #endregion

    #region 道具使用相关
    public void UseItem(ItemInfo MItemInfo)
    {
        if (!MItemInfo.item.Usable)
        {
            MessageManager.Instance.New("该物品不可使用");
            return;
        }
        if (MItemInfo.item.IsBox) UseBox(MItemInfo);
        else if (MItemInfo.item.IsEquipment) UseEuipment(MItemInfo);
        else if (MItemInfo.item.IsBook) UseBook(MItemInfo);
        else if (MItemInfo.item.IsBag) UseBag(MItemInfo);
        else if (MItemInfo.item.IsForQuest) UseQuest(MItemInfo);
        if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseWindow();
    }

    void UseBox(ItemInfo MItemInfo)
    {
        BoxItem box = MItemInfo.item as BoxItem;
        LoseItem(MItemInfo, 1, box.GetItems());
    }

    void UseEuipment(ItemInfo MItemInfo)
    {
        Equip(MItemInfo);
    }

    void UseBook(ItemInfo MItemInfo)
    {
        BookItem book = MItemInfo.item as BookItem;
        switch (book.BookType)
        {
            case BookType.Building:
                if (TryLoseItem_Boolean(MItemInfo, 1) && BuildingManager.Instance.Learn(book.BuildingToLearn))
                {
                    LoseItem(MItemInfo, 1);
                    BuildingManager.Instance.Init();
                }
                break;
            case BookType.Making:
                if (TryLoseItem_Boolean(MItemInfo, 1) && MakingManager.Instance.Learn(book.ItemToLearn))
                {
                    LoseItem(MItemInfo, 1);
                    MakingManager.Instance.Init();
                }
                break;
            case BookType.Skill:
            default: break;
        }
    }

    void UseBag(ItemInfo MItemInfo)
    {
        BagItem bag = MItemInfo.item as BagItem;
        if (TryLoseItem_Boolean(MItemInfo, 1))
        {
            if (ExpandSize(bag.ExpandSize))
                LoseItem(MItemInfo, 1);
        }
    }

    void UseQuest(ItemInfo MItemInfo)
    {
        if (!TryLoseItem_Boolean(MItemInfo)) return;
        QuestItem item = MItemInfo.item as QuestItem;
        TriggerManager.Instance.SetTrigger(item.TriggerName, item.StateToSet);
        LoseItem(MItemInfo);
    }
    #endregion

    #region 道具装备相关
    public void Equip(ItemInfo toEquip)
    {
        if (toEquip == null || !toEquip.item) return;
        ItemInfo equiped = null;
        switch (toEquip.item.ItemType)
        {
            case ItemType.Weapon:
                Backpack.size--;//模拟为将要替换出来的武器留出空间
                if (PlayerManager.Instance.PlayerInfo.HasPrimaryWeapon && (toEquip.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerManager.Instance.PlayerInfo.UnequipWeapon(true);
                }
                else if (PlayerManager.Instance.PlayerInfo.HasSecondaryWeapon && !(toEquip.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerManager.Instance.PlayerInfo.UnequipWeapon(false);
                }
                if ((equiped && !TryGetItem_Boolean(equiped, 1)) || !PlayerManager.Instance.PlayerInfo.EquipWeapon(toEquip))
                {
                    PlayerManager.Instance.PlayerInfo.EquipWeapon(equiped);
                    Backpack.size++;
                    return;
                }
                break;
            default: MessageManager.Instance.New("敬请期待"); return;
        }
        LoseItem(toEquip, 1);
        Backpack.weight.Current += toEquip.item.Weight;//装备并不是真正没有了，而是装备在身上，所以负重不变，在此处修正。
        MessageManager.Instance.New(string.Format("装备了 [{0}]", toEquip.ItemName));
        if (equiped)
        {
            GetItem(equiped, 1);
            Backpack.weight.Current -= equiped.item.Weight;//装备并不是真正重新获得，而是本来就装备在身上，所以负重不变，在此处修正。
        }
        UpdateUI();
    }

    public void Unequip(ItemInfo toUnequip)
    {
        if (toUnequip == null) return;
        ItemInfo equiped = toUnequip;
        switch (toUnequip.item.ItemType)
        {
            case ItemType.Weapon:
                Backpack.weight.Current -= equiped.item.Weight;
                Backpack.size--;
                if (PlayerManager.Instance.PlayerInfo.HasPrimaryWeapon && (equiped.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerManager.Instance.PlayerInfo.UnequipWeapon(true);
                }
                else if (PlayerManager.Instance.PlayerInfo.HasSecondaryWeapon && !(equiped.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerManager.Instance.PlayerInfo.UnequipWeapon(false);
                }
                break;
            case ItemType.Armor:
            default: return;
        }
        if (!TryGetItem_Boolean(equiped))
        {
            PlayerManager.Instance.PlayerInfo.EquipWeapon(equiped);
            Backpack.weight.Current += equiped.item.Weight;
            Backpack.size++;
            return;
        }
        else GetItem(equiped, 1);
    }
    #endregion

    #region UI相关
    public override void OpenWindow()
    {
        if (DialogueManager.Instance.IsTalking && !WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        GridMask.raycastTarget = true;
        EnableHandwork(true);
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        foreach (ItemAgent ia in itemAgents)
            ia.FinishDrag();
        AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseWindow();
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        if (ShopManager.Instance.IsUIOpen) ShopManager.Instance.CloseWindow();
        if (MakingManager.Instance.IsUIOpen && !MakingManager.Instance.IsMaking) MakingManager.Instance.CloseWindow();
        if (ItemSelectionManager.Instance.IsUIOpen) ItemSelectionManager.Instance.CloseWindow();
    }
    public override void PauseDisplay(bool pause)
    {
        base.PauseDisplay(pause);
        if (!IsPausing && pause) ItemWindowManager.Instance.CloseWindow();
    }
    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) OpenWindow();
        else CloseWindow();
    }
    public void EnableHandwork(bool value)
    {
        ZetanUtility.SetActive(UI.handworkButton.gameObject,
            value && !ShopManager.Instance.IsUIOpen && !WarehouseManager.Instance.IsUIOpen && !ItemSelectionManager.Instance.IsUIOpen && !MakingManager.Instance.IsUIOpen);
    }

    public void OpenDiscardWindow()
    {
        foreach (var ia in itemAgents)
        {
            if (!ia.IsEmpty && !ia.MItemInfo.item.DiscardAble) ia.Dark();
        }
        ItemSelectionManager.Instance.StartSelection(ItemSelectionType.Discard, "丢弃物品", "确定要丢掉这些道具吗？", DiscardItems, delegate
        {
            foreach (var ia in itemAgents)
            {
                if (!ia.IsEmpty) ia.Light();
            }
        });
    }

    public void DarkUnmakeable(bool dark)
    {
        if (dark)
            foreach (var ia in itemAgents.Where(x => !x.IsEmpty))
            {
                if (ia.MItemInfo.item.MaterialType == MaterialType.None) ia.Dark();
            }
        else foreach (var ia in itemAgents.Where(x => !x.IsEmpty))
            {
                ia.Light();
            }
    }

    public void Arrange()
    {
        if (!UI || !UI.gameObject) return;
        Backpack.Arrange();
        Init();
        for (int i = 0; i < Backpack.Items.Count; i++)
            itemAgents[i].SetItem(Backpack.Items[i]);
        ItemWindowManager.Instance.CloseWindow();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (!UI || !UI.gameObject) return;
        UI.money.text = Backpack.Money.ToString() + GameManager.CoinName;
        Color color = UI.weight.color;
        float mul = Backpack.weight.Current / Backpack.WeightOver;
        if (mul > 1 && mul <= 1.5f) color = overColor;
        else if (mul > 1.5f) color = maxColor;
        UI.weight.text = ZetanUtility.ColorText(Backpack.weight.Current.ToString("F2") + "/" + Backpack.WeightOver.ToString("F2") + "WL", color);
        color = UI.size.color;
        if (Backpack.size.Rest < 5 && Backpack.size.Rest > 0) color = overColor;
        else if (Backpack.size.Rest < 1) color = maxColor;
        UI.size.text = ZetanUtility.ColorText(Backpack.size.ToString(), color);
        SetPage(currentPage);
        QuestManager.Instance.UpdateUI();
        BuildingManager.Instance.UpdateUI();
        MakingManager.Instance.UpdateUI();
    }

    public override void SetUI(BackpackUI UI)
    {
        itemAgents.RemoveAll(x => !x || !x.gameObject);
        foreach (var ia in itemAgents)
            ia.Empty();
        IsPausing = false;
        CloseWindow();
        base.SetUI(UI);
    }

    #region 道具页相关
    private int currentPage;
    public void SetPage(int index)
    {
        if (!UI || !UI.gameObject) return;
        currentPage = index;
        switch (index)
        {
            case 1: ShowEquipments(); break;
            case 2: ShowConsumables(); break;
            case 3: ShowMaterials(); break;
            default: ShowAll(); break;
        }
    }

    private void ShowAll()
    {
        if (!UI || !UI.gameObject) return;
        foreach (ItemAgent ia in itemAgents)
        {
            ZetanUtility.SetActive(ia.gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        if (!UI || !UI.gameObject) return;
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && ia.MItemInfo.item.IsEquipment)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        if (!UI || !UI.gameObject) return;
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && ia.MItemInfo.item.IsConsumable)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        if (!UI || !UI.gameObject) return;
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && ia.MItemInfo.item.IsMaterial)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }
    #endregion
    #endregion

    #region 其它
    #region 材料相关
    public bool IsMaterialsEnough(IEnumerable<MaterialInfo> materials)
    {
        if (materials == null) return false;
        if (materials.Count() < 1) return true;
        var materialEnum = materials.GetEnumerator();
        while (materialEnum.MoveNext())
        {
            if (materialEnum.Current.MakingType == MakingType.SingleItem)
            {
                if (Backpack.GetItemAmount(materialEnum.Current.Item) < materialEnum.Current.Amount) return false;
            }
            else
            {
                int amount = Backpack.Items.FindAll(x => x.item.MaterialType == materialEnum.Current.MaterialType).Select(x => x.Amount).Sum();
                if (amount < materialEnum.Current.Amount) return false;
            }
        }
        return true;
    }
    public bool IsMaterialsEnough(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> materials)
    {
        if (targetMaterials == null || targetMaterials.Count() < 1 || materials == null || materials.Count() < 1 || targetMaterials.Count() != materials.Count()) return false;
        foreach (var material in targetMaterials)
        {
            if (material.MakingType == MakingType.SingleItem)
            {
                ItemInfo find = materials.FirstOrDefault(x => x.ItemID == material.ItemID);
                if (!find) return false;//所提供的材料中没有这种材料
                if (find.Amount != material.Amount) return false;//若材料数量不符合，则无法制作
                else if (GetItemAmount(find.ItemID) < material.Amount) return false;//背包中材料数量不足
            }
            else
            {
                var finds = materials.Where(x => x.item.MaterialType == material.MaterialType);//找到种类相同的道具
                if (finds.Count() > 0)
                {
                    if (finds.Select(x => x.Amount).Sum() != material.Amount) return false;//若材料总数不符合，则无法制作
                    foreach (var find in finds)
                    {
                        if (GetItemAmount(find.item) < find.Amount || QuestManager.Instance.HasQuestRequiredItem(find.item, GetItemAmount(find.item) - find.Amount))
                        {
                            return false;
                        }//若任意一个相应数量的材料无法失去（包括数量不足），则会导致总数量不符合，所以无法制作
                    }
                }
                else return false;//材料不足
            }
        }
        return true;
    }

    public List<ItemInfo> GetMaterialsFromBackpack(IEnumerable<MaterialInfo> targetMaterials)
    {
        if (targetMaterials == null) return null;

        List<ItemInfo> items = new List<ItemInfo>();
        if (targetMaterials.Count() < 1) return items;

        var materialEnum = targetMaterials.GetEnumerator();
        while (materialEnum.MoveNext())
        {
            if (materialEnum.Current.MakingType == MakingType.SingleItem)
            {
                int have = GetItemAmount(materialEnum.Current.Item);
                have = have > materialEnum.Current.Amount ? materialEnum.Current.Amount : have;
                items.Add(new ItemInfo(materialEnum.Current.Item, have));
            }
            else
            {
                ItemInfo item = Backpack.Items.Find(x => x.item.MaterialType == materialEnum.Current.MaterialType && x.Amount >= materialEnum.Current.Amount);
                if (item) items.Add(new ItemInfo(item.item, materialEnum.Current.Amount));
            }
        }
        return items;
    }

    public IEnumerable<string> GetMaterialsInfoString(IEnumerable<MaterialInfo> materials)
    {
        List<string> info = new List<string>();
        using (var materialEnum = materials.GetEnumerator())
            while (materialEnum.MoveNext())
                if (materialEnum.Current.MakingType == MakingType.SingleItem)
                    info.Add(string.Format("{0}\t[{1}/{2}]", materialEnum.Current.ItemName, Backpack.GetItemAmount(materialEnum.Current.Item), materialEnum.Current.Amount));
                else
                {
                    var finds = Backpack.Items.FindAll(x => x.item.MaterialType == materialEnum.Current.MaterialType);
                    int amount = 0;
                    foreach (var item in finds)
                        amount += item.Amount;
                    info.Add(string.Format("{0}\t[{1}/{2}]", MaterialItem.GetMaterialTypeString(materialEnum.Current.MaterialType), amount, materialEnum.Current.Amount));
                }
        return info.AsEnumerable();
    }

    public int GetAmountCanMake(IEnumerable<MaterialInfo> materials)
    {
        if (Backpack == null) return 0;
        if (materials.Count() < 1) return 1;
        List<int> amounts = new List<int>();
        using (var materialEnum = materials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                if (materialEnum.Current.MakingType == MakingType.SingleItem)
                    amounts.Add(Backpack.GetItemAmount(materialEnum.Current.Item) / materialEnum.Current.Amount);
                else
                {
                    var finds = Backpack.Items.FindAll(x => x.item.MaterialType == materialEnum.Current.MaterialType);
                    int amount = 0;
                    foreach (var item in finds)
                        amount += item.Amount;
                    amounts.Add(amount / materialEnum.Current.Amount);
                }
            }
        return amounts.Min();
    }
    public int GetAmountCanMake(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<ItemInfo> materials)
    {
        if (materials == null || materials.Count() < 1 || itemMaterials == null || itemMaterials.Count() < 1 || itemMaterials.Count() != materials.Count()) return 0;
        List<int> amounts = new List<int>();
        foreach (var material in itemMaterials)
        {
            if (material.MakingType == MakingType.SingleItem)
            {
                ItemInfo find = materials.FirstOrDefault(x => x.ItemID == material.ItemID);
                if (!find) return 0;//所提供的材料中没有这种材料
                if (find.Amount != material.Amount) return 0;//若材料数量不符合，则无法制作
                amounts.Add(GetItemAmount(find.ItemID) / material.Amount);
            }
            else
            {
                var finds = materials.Where(x => x.item.MaterialType == material.MaterialType);//找到种类相同的道具
                if (finds.Count() > 0)
                {
                    if (finds.Select(x => x.Amount).Sum() != material.Amount) return 0;//若材料总数不符合，则无法制作
                    foreach (var find in finds)
                    {
                        int amount = GetItemAmount(find.ItemID);
                        if (QuestManager.Instance.HasQuestRequiredItem(find.item, GetItemAmount(find.item) - find.Amount))
                            return 0;//若任意一个相应数量的材料无法失去，则会导致总数量不符合，所以无法制作
                        amounts.Add(amount / find.Amount);
                    }
                }
                else return 0;//材料不足
            }
        }

        return amounts.Min();
    }
    #endregion

    public void GetMoney(long value)
    {
        if (value < 0) return;
        Backpack.GetMoneySimple(value);
        UpdateUI();
    }
    public bool TryLoseMoney(long value)
    {
        if (value < 0) return false;
        if (Backpack.Money < value)
        {
            MessageManager.Instance.New("钱币不足");
            return false;
        }
        return true;
    }
    public void LoseMoney(long value)
    {
        if (!TryLoseMoney(value)) return;
        Backpack.LoseMoneySimple(value);
        UpdateUI();
    }

    public int GetItemAmount(string id)
    {
        if (Backpack == null) return 0;
        return Backpack.GetItemAmount(id);
    }
    public int GetItemAmount(ItemBase item)
    {
        if (Backpack == null) return 0;
        return Backpack.GetItemAmount(item);
    }

    public ItemInfo GetItemInfo(string id)
    {
        return Backpack.Find(id);
    }
    public ItemInfo GetItemInfo(ItemBase item)
    {
        return Backpack.Find(item);
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmount(id) > 0;
    }
    public bool HasItem(ItemBase item)
    {
        return GetItemAmount(item) > 0;
    }

    public IEnumerable<ItemAgent> GetItemAgentsByItem(ItemBase item)
    {
        return itemAgents.FindAll(x => !x.IsEmpty && x.MItemInfo.item == item).AsEnumerable();
    }

    /// <summary>
    /// 扩展容量
    /// </summary>
    /// <param name="size">扩展数量</param>
    /// <returns>是否成功扩展</returns>
    public bool ExpandSize(int size)
    {
        if (size < 1) return false;
        if (Backpack.size.Max >= 192)
        {
            MessageManager.Instance.New(GameManager.BackpackName + "已经达到最大容量了");
            return false;
        }
        int finallyExpand = Backpack.size.Max + size > 192 ? 192 - Backpack.size.Max : size;
        Backpack.size.Max += finallyExpand;
        for (int i = 0; i < finallyExpand; i++)
        {
            ItemAgent ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
            itemAgents.Add(ia);
            ia.Init(ItemAgentType.Backpack, itemAgents.Count - 1, UI.gridScrollRect);
        }
        MessageManager.Instance.New(GameManager.BackpackName + "空间增加了");
        return true;
    }
    /// <summary>
    /// 扩展负重
    /// </summary>
    /// <param name="weightLoad">扩展数量</param>
    /// <returns>是否成功扩展</returns>
    public bool ExpandWeightLoad(float weightLoad)
    {
        if (weightLoad < 0.01f) return false;
        if (Backpack.weight.Max >= 1500.0f * 1.5f)
        {
            MessageManager.Instance.New(GameManager.BackpackName + "已经达到最大扩展载重了");
            return false;
        }
        Backpack.weight.Max += Backpack.weight.Max + weightLoad > 1500.0f * 1.5f ? 1500.0f * 1.5f - Backpack.weight.Max : weightLoad;
        MessageManager.Instance.New(GameManager.BackpackName + "载重增加了");
        return true;
    }

    /// <summary>
    /// 判断该任务是否需要某个道具，用于丢弃某个道具时，判断能不能丢
    /// </summary>
    /// <param name="item">所需判定的道具</param>
    /// <param name="leftAmount">所需判定的数量</param>
    /// <returns>是否需要</returns>
    public bool IsQuestRequireItem(QuestData quest, ItemBase item, int leftAmount)
    {
        if (quest.Info.CmpltObjctvInOrder)
        {
            foreach (ObjectiveData o in quest.ObjectiveInstances)
            {
                //当目标是收集类目标且在提交任务同时会失去相应道具时，才进行判断
                if (o is CollectObjectiveData co && item == co.Info.Item && co.Info.LoseItemAtSbmt)
                {
                    if (o.IsComplete && o.Info.InOrder)
                    {
                        //如果剩余的道具数量不足以维持该目标完成状态
                        if (o.Info.Amount > leftAmount)
                        {
                            ObjectiveData tempObj = o.nextObjective;
                            while (tempObj != null)
                            {
                                //则判断是否有后置目标在进行，以保证在打破该目标的完成状态时，后置目标不受影响
                                if (tempObj.CurrentAmount > 0 && tempObj.Info.OrderIndex > o.Info.OrderIndex)
                                {
                                    //Debug.Log("Required");
                                    return true;
                                }
                                tempObj = tempObj.nextObjective;
                            }
                        }
                        //Debug.Log("NotRequired3");
                        return false;
                    }
                    //Debug.Log("NotRequired2");
                    return false;
                }
            }
        }
        //Debug.Log("NotRequired1");
        return false;
    }

    public void Search()
    {
        if (!UI || !UI.gameObject || !UI.searchInput || !UI.searchButton) return;
        if (string.IsNullOrEmpty(UI.searchInput.text))
        {
            SetPage(currentPage);
            return;
        }
        foreach (var ia in itemAgents)
        {
            if (!ia.IsEmpty && ia.MItemInfo.ItemName.Contains(UI.searchInput.text)) continue;
            else ia.Hide();
        }
        UI.searchInput.text = string.Empty;
    }

    public void SaveData(SaveData data)
    {
        if (Backpack != null)
        {
            data.backpackData.currentSize = (int)Backpack.size;
            data.backpackData.maxSize = Backpack.size.Max;
            data.backpackData.currentWeight = (float)Backpack.weight;
            data.backpackData.maxWeightLoad = Backpack.weight.Max;
            data.backpackData.money = Backpack.Money;
            foreach (ItemInfo info in Backpack.Items)
            {
                data.backpackData.itemDatas.Add(new ItemSaveData(info));
            }
        }
    }
    public void LoadData(BackpackSaveData backpackData)
    {
        if (Backpack == null) return;
        foreach (ItemSaveData id in backpackData.itemDatas)
            if (!GameManager.GetItemByID(id.itemID)) return;
        Backpack.LoseMoneySimple(Backpack.Money);
        Backpack.GetMoneySimple(backpackData.money);
        Backpack.size = new ScopeInt(backpackData.maxSize) { Current = backpackData.currentSize };
        Backpack.weight = new ScopeFloat(backpackData.maxWeightLoad) { Current = backpackData.currentSize };
        Backpack.Items.Clear();
        Init();
        foreach (ItemSaveData id in backpackData.itemDatas)
        {
            ItemInfo newInfo = new ItemInfo(GameManager.GetItemByID(id.itemID), id.amount);
            //TODO 把newInfo的耐久度等信息处理
            Backpack.Items.Add(newInfo);
            if (id.indexInGrid > -1 && id.indexInGrid < itemAgents.Count)
                itemAgents[id.indexInGrid].SetItem(newInfo);
            else foreach (ItemAgent ia in itemAgents)
                {
                    if (ia.IsEmpty) { ia.SetItem(newInfo); break; }
                }
        }
        UpdateUI();
    }
    #endregion
}