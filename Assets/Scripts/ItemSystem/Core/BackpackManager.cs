using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/背包管理器")]
public class BackpackManager : SingletonMonoBehaviour<BackpackManager>, IWindowHandler, IOpenCloseAbleWindow
{
    [SerializeField]
    private BackpackUI UI;

    [SerializeField]
    private Color overColor = Color.yellow;
    [SerializeField]
    private Color maxColor = Color.red;

    public bool IsPausing { get; private set; }
    public bool IsUIOpen { get; private set; }

#if UNITY_ANDROID
    public DiscardArea DiscardArea { get { return UI.discardArea; } }
#endif

    public Image GridMask { get { return UI.gridMask; } }

    public delegate void ItemAmountListener(ItemBase item, int leftAmount);
    public event ItemAmountListener OnGetItemEvent;
    public event ItemAmountListener OnLoseItemEvent;

    public List<ItemAgent> itemAgents = new List<ItemAgent>();

    private Backpack Backpack
    {
        get
        {
            return PlantManager.Instance ? PlayerManager.Instance.Backpack : null;
        }
    }

    public List<ItemInfo> Seeds => Backpack ? Backpack.Items.FindAll(x => x.item.IsSeed) : null;

    public long Money => Backpack ? Backpack.Money : 0;

    public void Init()
    {
        if (!UI || !UI.gameObject) return;
        if (Backpack != null)
        {
            foreach (ItemAgent ia in itemAgents)
                ia.Empty();
            int befCount = itemAgents.Count;
            while (Backpack.backpackSize.Max > itemAgents.Count)
            {
                ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                itemAgents.Add(ia);
                ia.Init(ItemAgentType.Backpack, itemAgents.IndexOf(ia), UI.gridRect);
            }
            while (Backpack.backpackSize.Max < itemAgents.Count)
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
        if (Backpack.IsFull)
        {
            return 0;
        }
        int finalGet = amount;
        if (!item.StackAble)
        {
            if (amount > Backpack.backpackSize.Rest)
                finalGet = Backpack.backpackSize.Rest;
        }
        if (Backpack.weightLoad.Rest + finalGet * item.Weight > Backpack.weightLoad.Max)
            for (int i = 0; i < Backpack.backpackSize.Rest; i++)
            {
                if (Backpack.weightLoad.Rest + i * item.Weight <= Backpack.weightLoad.Max &&
                    Backpack.weightLoad.Rest + (i + 1) * item.Weight > Backpack.weightLoad.Max)
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
            MessageManager.Instance.NewMessage("无效的道具");
            return false;
        }
        if (amount < 1)
        {
            if (amount < 0) MessageManager.Instance.NewMessage("无效的数量");
            return false;
        }
        if (!item.StackAble)//不可叠加，则看剩余空间是否足够放下
        {
            int vacateSize = 0;
            if (HasItemToLose())
                foreach (var info in simulLoseItems)
                {
                    if (!TryLoseItem_Boolean(info, info.Amount)) return false;//有一个要失去的道具不能失去，则直接不能获取该道具
                    if (info.item.StackAble && info.Amount - GetItemAmount(info.item) == 0)//只要该可叠加道具会全部失去，则能留出一个位置
                        vacateSize++;
                    else if (!info.item.StackAble)//若该道具不可叠加，则失去多少个就能空出多少位置
                        vacateSize += info.Amount;
                }
            if (amount > Backpack.backpackSize.Rest + vacateSize)//如果留出位置还不能放下
            {
                MessageManager.Instance.NewMessage(string.Format("请多留出至少{0}个{1}空间", Backpack.backpackSize.Rest + vacateSize - amount, GameManager.BackpackName));
                return false;
            }
        }
        float vacateWeightload = 0;
        if (HasItemToLose())
            foreach (var info in simulLoseItems)
            {
                if (!TryLoseItem_Boolean(info, info.Amount)) return false;
                vacateWeightload += info.item.Weight * info.Amount;
            }
        if (Backpack.weightLoad - vacateWeightload + amount * item.Weight > Backpack.LimitWeightload)//如果留出负重还不能放下
        {
            MessageManager.Instance.NewMessage("这些物品太重了");
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
                    MessageManager.Instance.NewMessage("发生内部错误！");
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
        OnGetItemEvent?.Invoke(item, GetItemAmount(item));
        UpdateUI();
        return true;
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
                    MessageManager.Instance.NewMessage("发生内部错误！");
                    Debug.Log("[Get Item Error] ID: " + info.item.ID + "[" + System.DateTime.Now.ToString() + "]");
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
        OnGetItemEvent?.Invoke(info.item, amount);
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
        return GetItem(info, info.Amount, simulLoseItems);
    }

    /// <summary>
    /// 制作道具，用于根据已知配方直接消耗材料来制作道具的玩法
    /// </summary>
    /// <param name="itemToMake"></param>
    /// <param name="amount"></param>
    /// <returns>是否成功制作</returns>
    public bool MakeItem(ItemBase itemToMake, int amount = 1)
    {
        if (!itemToMake || itemToMake.Materials.Count < 1 || amount < 1) return false;
        /*foreach (MaterialInfo mi in itemToMake.Materials)
            if (!TryLoseItem_Boolean(mi.Item, mi.Amount * amount))
                return false;
        foreach (MaterialInfo mi in itemToMake.Materials)//模拟空出位置来放制作的道具
        {
            MBackpack.weightLoad -= mi.Item.Weight * mi.Amount * amount;
            if (mi.Item.StackAble && GetItemAmount(mi.Item) - mi.Amount * amount == 0)//可叠加且消耗后用尽，则空出一格
                MBackpack.backpackSize--;
            else if (!mi.Item.StackAble)//不可叠加，则消耗多少个就能空出多少格
                MBackpack.backpackSize -= amount;
        }
        if (!TryGetItem_Boolean(itemToMake, amount))
        {
            foreach (MaterialInfo mi in itemToMake.Materials)//取消模拟
            {
                MBackpack.weightLoad += mi.Item.Weight * mi.Amount * amount;
                if (mi.Item.StackAble && GetItemAmount(mi.Item) - mi.Amount * amount == 0)
                    MBackpack.backpackSize++;
                else if (!mi.Item.StackAble)
                    MBackpack.backpackSize += amount;
            }
            return false;
        }
        foreach (MaterialInfo mi in itemToMake.Materials)//取消模拟并确认操作
        {
            MBackpack.weightLoad += mi.Item.Weight * mi.Amount * amount;
            if (mi.Item.StackAble && GetItemAmount(mi.Item) - mi.Amount * amount == 0)
                MBackpack.backpackSize++;
            else if (!mi.Item.StackAble)
                MBackpack.backpackSize += amount;
            LoseItem(mi.Item, mi.Amount * amount);
        }*/
        List<ItemInfo> materials = new List<ItemInfo>();
        foreach (var m in itemToMake.Materials)
        {
            materials.Add(new ItemInfo(m.Item, amount * m.Amount));
        }
        return GetItem(itemToMake, amount, materials.ToArray());
        /*UpdateUI();
        return true;*/
    }
    /// <summary>
    /// 制作道具，用于自己放入材料来组合新道具的玩法
    /// </summary>
    /// <param name="materials">材料列表</param>
    /// <returns>制作出来的道具</returns>
    public ItemInfo MakeItem(params ItemInfo[] materials)
    {
        return new ItemInfo();
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
                if (!TryGetItem_Boolean(si)) return false;
        if (GetItemAmount(item) < amount)
        {
            MessageManager.Instance.NewMessage(GameManager.BackpackName + "中没有这么多的 [" + item.name + "]");
            return false;
        }
        if (QuestManager.Instance.HasQuestRequiredItem(item, GetItemAmount(item) - amount))
        {
            MessageManager.Instance.NewMessage(string.Format("[{0}] 为任务所需", item.name));
            return false;
        }
        return true;
    }
    /// <summary>
    /// 尝试可否失去道具
    /// </summary>
    /// <param name="info">道具信息</param>
    /// <param name="amount">失去数量</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>可否失去</returns>
    public bool TryLoseItem_Boolean(ItemInfo info, int amount, params ItemInfo[] simulGetItems)
    {
        if (!info) return false;
        return TryLoseItem_Boolean(info.item, amount, simulGetItems);
    }
    /// <summary>
    /// 尝试可否失去道具
    /// </summary>
    /// <param name="info">道具信息（包括数量）</param>
    /// <param name="simulGetItems">会同时获得的道具</param>
    /// <returns>可否失去</returns>
    public bool TryLoseItem_Boolean(ItemInfo info, params ItemInfo[] simulGetItems)
    {
        return TryLoseItem_Boolean(info, info.Amount, simulGetItems);
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
                    if (!TryGetItem_Boolean(si)) return false;
            ItemInfo[] finds = Backpack.FindAll(item).ToArray();
            if (finds.Length < 1)
            {
                MessageManager.Instance.NewMessage("该物品不在" + GameManager.BackpackName + "中");
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
    /// 失去道具
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
                if (!TryGetItem_Boolean(si)) return false;
        Backpack.LoseItemSimple(info, amount);
        ItemAgent ia = itemAgents.Find(x => x.MItemInfo == info);
        if (ia) ia.UpdateInfo();
        OnLoseItemEvent?.Invoke(info.item, GetItemAmount(info.item));
        if (ItemWindowManager.Instance.MItemInfo == info && info.Amount < 1) ItemWindowManager.Instance.CloseItemWindow();
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
        return LoseItem(info, info.Amount, simulGetItems);
    }

    /// <summary>
    /// 丢弃道具
    /// </summary>
    /// <param name="info">道具信息</param>
    public void DiscardItem(ItemInfo info)
    {
        if (Backpack == null || info == null || !info.item) return;
        if (!info.item.DiscardAble)
        {
            MessageManager.Instance.NewMessage("该物品不可丢弃");
            return;
        }
        if (info.Amount < 2 && info.Amount > 0)
        {
            ConfirmManager.Instance.NewConfirm(string.Format("确定丢弃1个 [{0}] 吗？",
                ZetanUtility.ColorText(info.ItemName, GameManager.QualityToColor(info.item.Quality))), delegate
                {
                    if (LoseItem(info, 1))
                        MessageManager.Instance.NewMessage(string.Format("丢掉了1个 [{0}]", info.ItemName));
                });
        }
        else AmountManager.Instance.NewAmount(delegate
        {
            ConfirmManager.Instance.NewConfirm(string.Format("确定丢弃{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount,
                ZetanUtility.ColorText(info.ItemName, GameManager.QualityToColor(info.item.Quality))), delegate
                {
                    if (LoseItem(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.NewMessage(string.Format("丢掉了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.ItemName));
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
        ConfirmManager.Instance.NewConfirm("确定丢掉这些道具吗？", delegate
        {
            foreach (var item in items)
                LoseItem(item);
        });
    }
    #endregion

    #region 道具使用相关
    public void UseItem(ItemInfo MItemInfo)
    {
        if (!MItemInfo.item.Usable)
        {
            MessageManager.Instance.NewMessage("该物品不可使用");
            return;
        }
        if (MItemInfo.item.IsBox) UseBox(MItemInfo);
        else if (MItemInfo.item.IsEquipment) UseEuipment(MItemInfo);
        else if (MItemInfo.item.IsBook) UseBook(MItemInfo);
        else if (MItemInfo.item.IsBag) UseBag(MItemInfo);
        if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
    }

    void UseBox(ItemInfo MItemInfo)
    {
        BoxItem box = MItemInfo.item as BoxItem;
        LoseItem(box, 1, box.ItemsInBox.ToArray());
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
    #endregion

    #region 道具装备相关
    public void Equip(ItemInfo toEquip)
    {
        if (toEquip == null || !toEquip.item) return;
        ItemInfo equiped = null;
        switch (toEquip.item.ItemType)
        {
            case ItemType.Weapon:
                Backpack.backpackSize--;//模拟为将要替换出来的武器留出空间
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
                    Backpack.backpackSize++;
                    return;
                }
                break;
            case ItemType.Armor:
            default: return;
        }
        LoseItem(toEquip, 1);
        Backpack.weightLoad += toEquip.item.Weight;//装备并不是真正没有了，而是装备在身上，所以负重不变，在此处修正。
        MessageManager.Instance.NewMessage(string.Format("装备了 [{0}]", toEquip.ItemName));
        if (equiped)
        {
            GetItem(equiped, 1);
            Backpack.weightLoad -= equiped.item.Weight;//装备并不是真正重新获得，而是本来就装备在身上，所以负重不变，在此处修正。
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
                Backpack.weightLoad -= equiped.item.Weight;
                Backpack.backpackSize--;
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
            Backpack.weightLoad += equiped.item.Weight;
            Backpack.backpackSize++;
            return;
        }
        else GetItem(equiped, 1);
    }
    #endregion

    #region UI相关
    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsPausing) return;
        if (DialogueManager.Instance.IsTalking && !WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen) return;
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
        GridMask.raycastTarget = true;
        ZetanUtility.SetActive(UI.handworkButton.gameObject, !ShopManager.Instance.IsUIOpen && !WarehouseManager.Instance.IsUIOpen);
    }
    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsPausing) return;
        foreach (ItemAgent ia in itemAgents)
            ia.FinishDrag();
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        IsUIOpen = false;
        IsPausing = false;
        WindowsManager.Instance.Remove(this);
        AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseItemWindow();
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        if (ShopManager.Instance.IsUIOpen) ShopManager.Instance.CloseWindow();
    }
    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.window.alpha = 1;
            UI.window.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.window.alpha = 0;
            UI.window.blocksRaycasts = false;
            ItemWindowManager.Instance.CloseItemWindow();
        }
        IsPausing = pause;
    }
    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen)
            OpenWindow();
        else CloseWindow();
    }
    public Canvas CanvasToSort
    {
        get
        {
            if (!UI) return null;
            return UI.windowCanvas;
        }
    }

    public void Sort()
    {
        if (!UI || !UI.gameObject) return;
        Backpack.Sort();
        Init();
        for (int i = 0; i < Backpack.Items.Count; i++)
            itemAgents[i].SetItem(Backpack.Items[i]);
        ItemWindowManager.Instance.CloseItemWindow();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (!UI || !UI.gameObject) return;
        UI.money.text = Backpack.Money.ToString() + GameManager.CoinName;
        Color color = UI.weight.color;
        float mul = Backpack.weightLoad.Current / Backpack.NormalWeightload;
        if (mul > 1 && mul <= 1.5f) color = overColor;
        else if (mul > 1.5f) color = maxColor;
        UI.weight.text = ZetanUtility.ColorText(Backpack.weightLoad.Current.ToString("F2") + "/" + Backpack.NormalWeightload.ToString("F2") + "WL", color);
        color = UI.size.color;
        if (Backpack.backpackSize.Rest < 5 && Backpack.backpackSize.Rest > 0) color = overColor;
        else if (Backpack.backpackSize.Rest < 1) color = maxColor;
        UI.size.text = ZetanUtility.ColorText(Backpack.backpackSize.ToString(), color);
        SetPage(currentPage);
        QuestManager.Instance.UpdateUI();
        BuildingManager.Instance.UpdateUI();
        MakingManager.Instance.UpdateUI();
    }

    public void SetUI(BackpackUI UI)
    {
        itemAgents.RemoveAll(x => !x || !x.gameObject);
        foreach (var ia in itemAgents)
        {
            ia.Empty();
        }
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
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
    public bool CheckMaterialsEnough(IEnumerable<MaterialInfo> materials)
    {
        var materialEnum = materials.GetEnumerator();
        while (materialEnum.MoveNext())
        {
            if (Backpack.GetItemAmount(materialEnum.Current.Item) < materialEnum.Current.Amount)
                return false;
        }
        return true;
    }

    public IEnumerable<string> GetMaterialsInfo(IEnumerable<MaterialInfo> materials)
    {
        List<string> info = new List<string>();
        using (var makingInfo = materials.GetEnumerator())
            while (makingInfo.MoveNext())
                info.Add(string.Format("{0}\t[{1}/{2}]", makingInfo.Current.ItemName, Backpack.GetItemAmount(makingInfo.Current.Item), makingInfo.Current.Amount));
        return info.AsEnumerable();
    }
    public int GetAmountCanMake(IEnumerable<MaterialInfo> Materials)
    {
        if (Backpack == null) return 0;
        if (Materials.Count() < 1) return 0;
        List<int> amounts = new List<int>();
        using (var makingInfo = Materials.GetEnumerator())
            while (makingInfo.MoveNext())
                amounts.Add(Backpack.GetItemAmount(makingInfo.Current.Item) / makingInfo.Current.Amount);
        return amounts.Min();
    }

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
            MessageManager.Instance.NewMessage("钱币不足");
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
        if (Backpack.backpackSize.Max >= 192)
        {
            MessageManager.Instance.NewMessage(GameManager.BackpackName + "已经达到最大容量了");
            return false;
        }
        int finallyExpand = Backpack.backpackSize.Max + size > 192 ? 192 - Backpack.backpackSize.Max : size;
        Backpack.backpackSize.Max += finallyExpand;
        for (int i = 0; i < finallyExpand; i++)
        {
            ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
            itemAgents.Add(ia);
            ia.Init(ItemAgentType.Backpack, itemAgents.IndexOf(ia), UI.gridRect);
        }
        MessageManager.Instance.NewMessage(GameManager.BackpackName + "空间增加了");
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
        if (Backpack.weightLoad.Max >= 1500.0f * 1.5f)
        {
            MessageManager.Instance.NewMessage(GameManager.BackpackName + "已经达到最大扩展载重了");
            return false;
        }
        Backpack.weightLoad.Max += Backpack.weightLoad.Max + weightLoad > 1500.0f * 1.5f ? 1500.0f * 1.5f - Backpack.weightLoad.Max : weightLoad;
        MessageManager.Instance.NewMessage(GameManager.BackpackName + "载重增加了");
        return true;
    }

    /// <summary>
    /// 判断该任务是否需要某个道具，用于丢弃某个道具时，判断能不能丢
    /// </summary>
    /// <param name="item">所需判定的道具</param>
    /// <param name="leftAmount">所需判定的数量</param>
    /// <returns>是否需要</returns>
    public bool QuestRequireItem(Quest quest, ItemBase item, int leftAmount)
    {
        if (quest.CmpltObjctvInOrder)
        {
            foreach (Objective o in quest.ObjectiveInstances)
            {
                //当目标是收集类目标且在提交任务同时会失去相应道具时，才进行判断
                if (o is CollectObjective && item == (o as CollectObjective).Item && (o as CollectObjective).LoseItemAtSbmt)
                {
                    if (o.IsComplete && o.InOrder)
                    {
                        //如果剩余的道具数量不足以维持该目标完成状态
                        if (o.Amount > leftAmount)
                        {
                            Objective tempObj = o.NextObjective;
                            while (tempObj != null)
                            {
                                //则判断是否有后置目标在进行，以保证在打破该目标的完成状态时，后置目标不受影响
                                if (tempObj.CurrentAmount > 0 && tempObj.OrderIndex > o.OrderIndex)
                                {
                                    //Debug.Log("Required");
                                    return true;
                                }
                                tempObj = tempObj.NextObjective;
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

    public void SaveData(SaveData data)
    {
        if (Backpack != null)
        {
            data.backpackData.currentSize = (int)Backpack.backpackSize;
            data.backpackData.maxSize = Backpack.backpackSize.Max;
            data.backpackData.currentWeight = (float)Backpack.weightLoad;
            data.backpackData.maxWeightLoad = Backpack.weightLoad.Max;
            data.backpackData.money = Backpack.Money;
            foreach (ItemInfo info in Backpack.Items)
            {
                data.backpackData.itemDatas.Add(new ItemData(info));
            }
        }
    }
    public void LoadData(BackpackData backpackData)
    {
        if (Backpack == null) return;
        foreach (ItemData id in backpackData.itemDatas)
            if (!GameManager.GetItemByID(id.itemID)) return;
        Backpack.LoseMoneySimple(Backpack.Money);
        Backpack.GetMoneySimple(backpackData.money);
        Backpack.backpackSize = new ScopeInt(backpackData.maxSize) { Current = backpackData.currentSize };
        Backpack.weightLoad = new ScopeFloat(backpackData.maxWeightLoad) { Current = backpackData.currentSize };
        Backpack.Items.Clear();
        Init();
        foreach (ItemData id in backpackData.itemDatas)
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