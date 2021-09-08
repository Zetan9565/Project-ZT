using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/商铺管理器")]
public class ShopManager : WindowHandler<ShopUI, ShopManager>
{
    public static List<TalkerData> Vendors { get; } = new List<TalkerData>();

    private readonly List<MerchandiseAgent> merchandiseAgents = new List<MerchandiseAgent>();

    private bool bagOpenBef;
    private bool bagPauseBef;

    public ShopData MShop { get; private set; }

    private void Update()
    {
        RefreshAll(Time.deltaTime);
    }

    public void RefreshAll(float time)
    {
        /*System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();*/
        using (var vendorEnum = Vendors.GetEnumerator())
            while (vendorEnum.MoveNext())
                vendorEnum.Current.shop.TimePass(time);
        if (IsUIOpen)
            using (var agentEnum = merchandiseAgents.GetEnumerator())
                while (agentEnum.MoveNext())
                    agentEnum.Current.UpdateInfo();
        /*stopwatch.Stop();
        Debug.Log(stopwatch.Elapsed.TotalMilliseconds);*/
    }

    public void Init(ShopData shop)
    {
        if (shop == null || !shop.Info) return;
        MShop = shop;
        UI.shopName.text = MShop.Info.ShopName;
        foreach (MerchandiseAgent ma in merchandiseAgents)
            ma.Clear(true);
        merchandiseAgents.RemoveAll(x => !x.gameObject.activeSelf);
        UI.commodityTab.isOn = true;
        SetPage(0);
    }

    #region 货物处理相关
    /// <summary>
    /// 向玩家卖出道具
    /// </summary>
    /// <param name="goods">商品信息</param>
    public void SellItem(MerchandiseData goods)
    {
        if (MShop == null || goods == null || !goods.Info.IsValid) return;
        if (!MShop.Commodities.Contains(goods)) return;
        long maxAmount = goods.Info.EmptyAble ? goods.LeftAmount : (goods.Info.SellPrice > 0 ? BackpackManager.Instance.Money / goods.Info.SellPrice : 999);
        if (goods.LeftAmount == 1 && goods.Info.EmptyAble)
        {
            ConfirmManager.Instance.New(string.Format("确定购买1个 [{0}] 吗？", goods.Item.name), delegate
            {
                if (OnSell(goods))
                    MessageManager.Instance.New(string.Format("购买了1个 [{0}]", goods.Item.name));
            });
        }
        else if (goods.IsEmpty)
        {
            ConfirmManager.Instance.New("该商品暂时缺货");
        }
        else
        {
            AmountManager.Instance.New(delegate (long amount)
            {
                ConfirmManager.Instance.New(string.Format("确定购买{0}个 [{1}] 吗？", (int)amount, goods.Item.name), delegate
                {
                    if (OnSell(goods, (int)amount))
                        MessageManager.Instance.New(string.Format("购买了{0}个 [{1}]", (int)amount, goods.Item.name));
                });
            }, maxAmount, "购买数量", ZetanUtility.ScreenCenter, Vector2.zero);
        }
    }

    bool OnSell(MerchandiseData data, int amount = 1)
    {
        if (MShop == null || data == null || !data.IsValid || amount < 1) return false;
        if (!MShop.Commodities.Contains(data)) return false;
        if (!BackpackManager.Instance.TryGetItem_Boolean(data.Item, amount)) return false;
        if (data.Info.EmptyAble && amount > data.LeftAmount)
        {
            if (!data.IsEmpty) MessageManager.Instance.New("该商品数量不足");
            else MessageManager.Instance.New("该商品暂时缺货");
            return false;
        }
        if (!BackpackManager.Instance.TryLoseMoney(amount * data.Info.SellPrice))
            return false;
        BackpackManager.Instance.LoseMoney(amount * data.Info.SellPrice);
        BackpackManager.Instance.GetItem(data.Item, amount);
        if (data.Info.EmptyAble) data.LeftAmount -= amount;
        MerchandiseAgent ma = merchandiseAgents.Find(x => x.Data == data);
        if (ma) ma.UpdateInfo();
        return true;
    }

    /// <summary>
    /// 从玩家那里购入道具
    /// </summary>
    /// <param name="data">商品信息</param>
    public void PurchaseItem(MerchandiseData data)
    {
        if (MShop == null || data == null || !data.IsValid) return;
        if (!MShop.Acquisitions.Contains(data)) return;
        int backpackAmount = BackpackManager.Instance.GetItemAmount(data.Item);
        int maxAmount = data.Info.EmptyAble ? (data.LeftAmount > backpackAmount ? backpackAmount : data.LeftAmount) : backpackAmount;
        if (data.LeftAmount == 1 && data.Info.EmptyAble)
        {
            ConfirmManager.Instance.New(string.Format("确定出售1个 [{0}] 吗？", data.Item.name), delegate
            {
                if (OnPurchase(data, 1))
                    MessageManager.Instance.New(string.Format("出售了1个 [{1}]", 1, data.Item.name));
            });
        }
        else if (data.IsEmpty)
        {
            ConfirmManager.Instance.New("这种物品暂无特价收购需求，确定按原价出售吗？", delegate
            {
                PurchaseItem(BackpackManager.Instance.GetItemInfo(data.Item), true);
            });
        }
        else
        {
            AmountManager.Instance.New(delegate (long amount)
            {
                ConfirmManager.Instance.New(string.Format("确定出售{0}个 [{1}] 吗？", (int)amount, data.Item.name), delegate
                {
                    if (OnPurchase(data, (int)amount))
                        MessageManager.Instance.New(string.Format("出售了{0}个 [{1}]", (int)amount, data.Item.name));
                });
            }, maxAmount, "出售数量", ZetanUtility.ScreenCenter, Vector2.zero);
        }
    }
    bool OnPurchase(MerchandiseData data, int amount = 1)
    {
        if (MShop == null || data == null || !data.IsValid || amount < 1) return false;
        if (!MShop.Acquisitions.Contains(data)) return false;
        var itemAgents = BackpackManager.Instance.GetItemAgentsByItem(data.Item).ToArray();
        if (itemAgents.Length < 1)
        {
            MessageManager.Instance.New(GameManager.BackpackName + "中没有这种物品");
            return false;
        }
        if (data.Info.EmptyAble && amount > data.LeftAmount)
        {
            if (!data.IsEmpty) MessageManager.Instance.New("不收够这么多的这种物品");
            else MessageManager.Instance.New("这种物品暂无收购需求");
            return false;
        }
        ItemBase item = itemAgents[0].MItemInfo.item;
        if (!BackpackManager.Instance.TryLoseItem_Boolean(item, amount)) return false;
        BackpackManager.Instance.GetMoney(amount * data.Info.PurchasePrice);
        BackpackManager.Instance.LoseItem(item, amount);
        if (data.Info.EmptyAble) data.LeftAmount -= amount;
        MerchandiseAgent ma = merchandiseAgents.Find(x => x.Data == data);
        if (ma) ma.UpdateInfo();
        return true;
    }

    /// <summary>
    /// 从玩家那里购入道具
    /// </summary>
    /// <param name="info">道具信息</param>
    /// <param name="force">强制购入</param>
    public void PurchaseItem(ItemInfo info, bool force = false)
    {
        if (MShop == null || info == null || !info.item)
        {
            Debug.Log(info);
            return;
        }
        if (!info.item.SellAble)
        {
            MessageManager.Instance.New("这种物品不可出售");
            return;
        }
        if (info is EquipmentInfo eqm)
            if (eqm.gemstone1 != null || eqm.gemstone2 != null)
            {
                MessageManager.Instance.New("镶嵌宝石的物品不可出售");
                return;
            }
        MerchandiseData find = MShop.Acquisitions.Find(x => x.Item == info.item);
        if (find != null && !force)//采购品列表里有该道具，说明对该道具有特殊购价
        {
            PurchaseItem(find);
            return;
        }
        if (info.Amount == 1)
        {
            ConfirmManager.Instance.New(string.Format("确定出售1个 [{0}] 吗？", info.item.name), delegate
            {
                if (OnPurchase(info))
                    MessageManager.Instance.New(string.Format("出售了1个 [{0}]", info.item.name));
            });
        }
        else
        {
            AmountManager.Instance.New(delegate (long amount)
            {
                ConfirmManager.Instance.New(string.Format("确定出售{0}个 [{1}] 吗？", (int)amount, info.item.name), delegate
                {
                    if (OnPurchase(info, (int)amount))
                        MessageManager.Instance.New(string.Format("出售了{0}个 [{1}]", (int)amount, info.item.name));
                });
            }, BackpackManager.Instance.GetItemAmount(info.item), "出售数量", ZetanUtility.ScreenCenter, Vector2.zero);
        }
    }
    bool OnPurchase(ItemInfo info, int amount = 1)
    {
        if (MShop == null || info == null || !info.item || amount < 1)
            return false;
        if (!info.item.SellAble)
        {
            MessageManager.Instance.New("这种物品不可出售");
            return false;
        }
        if (BackpackManager.Instance.GetItemAmount(info.item) < 1)
        {
            MessageManager.Instance.New(GameManager.BackpackName + "中没有这种物品");
            return false;
        }
        if (amount > info.Amount)
        {
            MessageManager.Instance.New(GameManager.BackpackName + "中没有这么多的这种物品");
            return false;
        }
        if (!BackpackManager.Instance.TryLoseItem_Boolean(info, amount)) return false;
        BackpackManager.Instance.GetMoney(amount * info.item.SellPrice);
        BackpackManager.Instance.LoseItem(info, amount);
        return true;
    }
    #endregion

    #region UI相关
    public override void OpenWindow()
    {
        if (!MShop) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        UIManager.Instance.EnableJoyStick(false);
        bagPauseBef = BackpackManager.Instance.IsPausing;
        bagOpenBef = BackpackManager.Instance.IsUIOpen;
        if (bagPauseBef)
            BackpackManager.Instance.PauseDisplay(false);
        else if (!bagOpenBef)
            BackpackManager.Instance.OpenWindow();
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        MShop = null;
        ItemWindowManager.Instance.CloseWindow();
        if (DialogueManager.Instance.IsUIOpen) DialogueManager.Instance.PauseDisplay(false);
        AmountManager.Instance.Cancel();
        UIManager.Instance.EnableJoyStick(true);
        if (bagPauseBef)
            BackpackManager.Instance.PauseDisplay(true);
        else if (!bagOpenBef)
            BackpackManager.Instance.CloseWindow();
    }

    public void SetPage(int page)
    {
        if (!UI || !UI.gameObject || !MShop) return;
        switch (page)
        {
            case 0:
                int originalSize = merchandiseAgents.Count;
                if (MShop.Commodities.Count >= originalSize)
                    for (int i = 0; i < MShop.Commodities.Count - originalSize; i++)
                    {
                        MerchandiseAgent ma = ObjectPool.Get(UI.merchandiseCellPrefab, UI.merchandiseCellsParent).GetComponent<MerchandiseAgent>();
                        ma.Clear();
                        merchandiseAgents.Add(ma);
                    }
                else
                {
                    for (int i = 0; i < originalSize - MShop.Commodities.Count; i++)
                        merchandiseAgents[i].Clear(true);
                    merchandiseAgents.RemoveAll(x => !x.gameObject.activeSelf);
                }
                foreach (MerchandiseAgent ma in merchandiseAgents)
                    ma.Clear();
                foreach (MerchandiseData md in MShop.Commodities)
                    foreach (MerchandiseAgent ma in merchandiseAgents)
                        if (ma.IsEmpty && md.IsValid)
                        {
                            ma.Init(md, MerchandiseType.SellToPlayer);
                            break;
                        }
                break;
            case 1:
                originalSize = merchandiseAgents.Count;
                int acqCount = MShop.Acquisitions.FindAll(x => x.IsValid && x.Item.SellAble).Count;
                if (acqCount >= originalSize)
                    for (int i = 0; i < acqCount - originalSize; i++)
                    {
                        MerchandiseAgent ma = ObjectPool.Get(UI.merchandiseCellPrefab, UI.merchandiseCellsParent).GetComponent<MerchandiseAgent>();
                        ma.Clear();
                        merchandiseAgents.Add(ma);
                    }
                else
                {
                    for (int i = 0; i < originalSize - acqCount; i++)
                        merchandiseAgents[i].Clear(true);
                    merchandiseAgents.RemoveAll(x => !x.gameObject.activeSelf);
                }
                foreach (MerchandiseAgent ma in merchandiseAgents)
                    ma.Clear();
                foreach (MerchandiseData md in MShop.Acquisitions)
                    foreach (MerchandiseAgent ma in merchandiseAgents)
                        if (ma.IsEmpty && md.IsValid && md.Item.SellAble)
                        {
                            ma.Init(md, MerchandiseType.BuyFromPlayer);
                            break;
                        }
                break;
            default: break;
        }
        ItemWindowManager.Instance.CloseWindow();
    }

    public override void SetUI(ShopUI UI)
    {
        foreach (var ma in merchandiseAgents.Where(x => x && x.gameObject))
            ma.Clear(true);
        merchandiseAgents.Clear();
        IsPausing = false;
        CloseWindow();
        base.SetUI(UI);
    }
    #endregion
}