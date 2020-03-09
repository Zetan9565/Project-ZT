using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/商铺管理器")]
public class ShopManager : WindowHandler<ShopUI, ShopManager>
{
    public static List<TalkerData> Vendors { get; } = new List<TalkerData>();

    private readonly List<MerchandiseAgent> merchandiseAgents = new List<MerchandiseAgent>();

    public ShopInformation MShop { get; private set; }

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
                vendorEnum.Current.shop.Refresh(time);
        if (IsUIOpen)
            using (var agentEnum = merchandiseAgents.GetEnumerator())
                while (agentEnum.MoveNext())
                    agentEnum.Current.UpdateInfo();
        /*stopwatch.Stop();
        Debug.Log(stopwatch.Elapsed.TotalMilliseconds);*/
    }

    public void Init(ShopInformation shop)
    {
        if (shop == null) return;
        MShop = shop;
        UI.shopName.text = MShop.ShopName;
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
    /// <param name="info">商品信息</param>
    public void SellItem(MerchandiseInfo info)
    {
        if (MShop == null || info == null || !info.IsValid) return;
        if (!MShop.Commodities.Contains(info)) return;
        long maxAmount = info.EmptyAble ? info.LeftAmount : info.SellPrice > 0 ? BackpackManager.Instance.Money / info.SellPrice : 999;
        if (info.LeftAmount == 1 && info.EmptyAble)
        {
            ConfirmManager.Instance.New(string.Format("确定购买1个 [{0}] 吗？", info.Item.name), delegate
            {
                if (OnSell(info))
                    MessageManager.Instance.New(string.Format("购买了1个 [{0}]", info.Item.name));
            });
        }
        else if (info.IsEmpty)
        {
            ConfirmManager.Instance.New("该商品暂时缺货");
        }
        else
        {
            AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
            AmountManager.Instance.New(delegate
            {
                ConfirmManager.Instance.New(string.Format("确定购买{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount, info.Item.name), delegate
                {
                    if (OnSell(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.New(string.Format("购买了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.Item.name));
                });
            }, maxAmount);
        }
    }

    bool OnSell(MerchandiseInfo info, int amount = 1)
    {
        if (MShop == null || info == null || !info.IsValid || amount < 1) return false;
        if (!MShop.Commodities.Contains(info)) return false;
        if (!BackpackManager.Instance.TryGetItem_Boolean(info.Item, amount)) return false;
        if (info.EmptyAble && amount > info.LeftAmount)
        {
            if (!info.IsEmpty) MessageManager.Instance.New("该商品数量不足");
            else MessageManager.Instance.New("该商品暂时缺货");
            return false;
        }
        if (!BackpackManager.Instance.TryLoseMoney(amount * info.SellPrice))
            return false;
        BackpackManager.Instance.LoseMoney(amount * info.SellPrice);
        BackpackManager.Instance.GetItem(info.Item, amount);
        if (info.EmptyAble) info.LeftAmount -= amount;
        MerchandiseAgent ma = merchandiseAgents.Find(x => x.merchandiseInfo == info);
        if (ma) ma.UpdateInfo();
        return true;
    }

    /// <summary>
    /// 从玩家那里购入道具
    /// </summary>
    /// <param name="info">商品信息</param>
    public void PurchaseItem(MerchandiseInfo info)
    {
        if (MShop == null || info == null || !info.IsValid) return;
        if (!MShop.Acquisitions.Contains(info)) return;
        int backpackAmount = BackpackManager.Instance.GetItemAmount(info.Item);
        int maxAmount = info.EmptyAble ? (info.LeftAmount > backpackAmount ? backpackAmount : info.LeftAmount) : backpackAmount;
        if (info.LeftAmount == 1 && info.EmptyAble)
        {
            ConfirmManager.Instance.New(string.Format("确定出售1个 [{0}] 吗？", info.Item.name), delegate
            {
                if (OnPurchase(info, 1))
                    MessageManager.Instance.New(string.Format("出售了1个 [{1}]", 1, info.Item.name));
            });
        }
        else if (info.IsEmpty)
        {
            ConfirmManager.Instance.New("这种物品暂无特价收购需求，确定按原价出售吗？", delegate
            {
                PurchaseItem(BackpackManager.Instance.GetItemInfo(info.Item), true);
            });
        }
        else
        {
            AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
            AmountManager.Instance.New(delegate
            {
                ConfirmManager.Instance.New(string.Format("确定出售{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount, info.Item.name), delegate
                {
                    if (OnPurchase(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.New(string.Format("出售了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.Item.name));
                });
            }, maxAmount);
        }
    }
    bool OnPurchase(MerchandiseInfo info, int amount = 1)
    {
        if (MShop == null || info == null || !info.IsValid || amount < 1) return false;
        if (!MShop.Acquisitions.Contains(info)) return false;
        var itemAgents = BackpackManager.Instance.GetItemAgentsByItem(info.Item).ToArray();
        if (itemAgents.Length < 1)
        {
            MessageManager.Instance.New(GameManager.BackpackName + "中没有这种物品");
            return false;
        }
        if (info.EmptyAble && amount > info.LeftAmount)
        {
            if (!info.IsEmpty) MessageManager.Instance.New("不收够这么多的这种物品");
            else MessageManager.Instance.New("这种物品暂无收购需求");
            return false;
        }
        ItemBase item = itemAgents[0].MItemInfo.item;
        if (!BackpackManager.Instance.TryLoseItem_Boolean(item, amount)) return false;
        BackpackManager.Instance.GetMoney(amount * info.PurchasePrice);
        BackpackManager.Instance.LoseItem(item, amount);
        if (info.EmptyAble) info.LeftAmount -= amount;
        MerchandiseAgent ma = merchandiseAgents.Find(x => x.merchandiseInfo == info);
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
        if (info.gemstone1 != null || info.gemstone2 != null)
        {
            MessageManager.Instance.New("镶嵌宝石的物品不可出售");
            return;
        }
        MerchandiseInfo find = MShop.Acquisitions.Find(x => x.Item == info.item);
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
                    MessageManager.Instance.New(string.Format("出售了1个 [{1}]", (int)AmountManager.Instance.Amount, info.item.name));
            });
        }
        else
        {
            AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
            AmountManager.Instance.New(delegate
            {
                ConfirmManager.Instance.New(string.Format("确定出售{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount, info.item.name), delegate
                {
                    if (OnPurchase(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.New(string.Format("出售了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.item.name));
                });
            }, BackpackManager.Instance.GetItemAmount(info.item));
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
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        MShop = null;
        if (BackpackManager.Instance.IsUIOpen) BackpackManager.Instance.CloseWindow();
        ItemWindowManager.Instance.CloseWindow();
        if (DialogueManager.Instance.IsUIOpen) DialogueManager.Instance.PauseDisplay(false);
        AmountManager.Instance.Cancel();
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
                foreach (MerchandiseInfo mi in MShop.Commodities)
                    foreach (MerchandiseAgent ma in merchandiseAgents)
                        if (ma.IsEmpty && mi.IsValid)
                        {
                            ma.Init(mi, MerchandiseType.SellToPlayer);
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
                foreach (MerchandiseInfo mi in MShop.Acquisitions)
                    foreach (MerchandiseAgent ma in merchandiseAgents)
                        if (ma.IsEmpty && mi.IsValid && mi.Item.SellAble)
                        {
                            ma.Init(mi, MerchandiseType.BuyFromPlayer);
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

    public MerchandiseAgent GetMerchandiseAgentByItem(ItemInfo info)
    {
        return merchandiseAgents.Find(x => x.IsMacth(info));
    }
}