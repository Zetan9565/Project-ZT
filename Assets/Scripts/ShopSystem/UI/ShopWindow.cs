using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindow : Window
{
    [SerializeField]
    private Text shopName;

    [SerializeField]
    private GoodsList goodsList;

    [SerializeField]
    private TabBar tabBar;

    public ShopData MShop { get; private set; }

    private bool bagOpenBef;
    private bool bagHiddenBef;

    protected override void OnAwake()
    {
        tabBar.Refresh(OnSwitchPage);
        goodsList.SetItemModifier(x => x.SetWindow(this));
    }

    private void OnSwitchPage(int page)
    {
        if (!MShop) return;
        switch (page)
        {
            case 1:
                goodsList.Refresh(MShop.Commodities);
                break;
            case 2:
                goodsList.Refresh(MShop.Acquisitions);
                break;
            default: break;
        }
        NewWindowsManager.CloseWindow<ItemWindow>();
    }

    #region 货物处理相关
    /// <summary>
    /// 向玩家卖出道具
    /// </summary>
    /// <param name="goods">商品信息</param>
    public void SellItem(GoodsData goods)
    {
        if (MShop == null || goods == null || !goods.Info.IsValid) return;
        if (!MShop.Commodities.Contains(goods)) return;
        long maxAmount = goods.Info.EmptyAble ? goods.LeftAmount : (goods.Info.SellPrice > 0 ? BackpackManager.Instance.Inventory.Money / goods.Info.SellPrice : 999);
        if (goods.LeftAmount == 1 && goods.Info.EmptyAble)
        {
            ConfirmWindow.StartConfirm(string.Format("确定购买1个 [{0}] 吗？", goods.Item.Name), delegate
            {
                if (OnSell(goods))
                    MessageManager.Instance.New(string.Format("购买了1个 [{0}]", goods.Item.Name));
            });
        }
        else if (goods.IsEmpty)
        {
            ConfirmWindow.StartConfirm("该商品暂时缺货");
        }
        else
        {
            AmountWindow.StartInput(delegate (long amount)
            {
                ConfirmWindow.StartConfirm(string.Format("确定购买{0}个 [{1}] 吗？", (int)amount, goods.Item.Name), delegate
                {
                    if (OnSell(goods, (int)amount))
                        MessageManager.Instance.New(string.Format("购买了{0}个 [{1}]", (int)amount, goods.Item.Name));
                });
            }, maxAmount, "购买数量", ZetanUtility.ScreenCenter, Vector2.zero);
        }
    }

    bool OnSell(GoodsData data, int amount = 1)
    {
        if (MShop == null || data == null || !data.IsValid || amount < 1) return false;
        if (!MShop.Commodities.Contains(data)) return false;
        if (!BackpackManager.Instance.CanGet(data.Item, amount)) return false;
        if (data.Info.EmptyAble && amount > data.LeftAmount)
        {
            if (!data.IsEmpty) MessageManager.Instance.New("该商品数量不足");
            else MessageManager.Instance.New("该商品暂时缺货");
            return false;
        }
        if (!BackpackManager.Instance.CanLoseMoney(amount * data.Info.SellPrice))
            return false;
        BackpackManager.Instance.LoseMoney(amount * data.Info.SellPrice);
        BackpackManager.Instance.GetItem(data.Item, amount);
        if (data.Info.EmptyAble) data.LeftAmount -= amount;
        goodsList.RefreshItemIf(x => x.Data == data);
        return true;
    }

    /// <summary>
    /// 从玩家那里购入道具
    /// </summary>
    /// <param name="data">商品信息</param>
    public void PurchaseItem(GoodsData data)
    {
        if (MShop == null || data == null || !data.IsValid) return;
        if (!MShop.Acquisitions.Contains(data)) return;
        int backpackAmount = BackpackManager.Instance.GetAmount(data.Item);
        int maxAmount = data.Info.EmptyAble ? (data.LeftAmount > backpackAmount ? backpackAmount : data.LeftAmount) : backpackAmount;
        if (data.LeftAmount == 1 && data.Info.EmptyAble)
        {
            ConfirmWindow.StartConfirm(string.Format("确定出售1个 [{0}] 吗？", data.Item.Name), delegate
            {
                if (OnPurchase(data, 1))
                    MessageManager.Instance.New(string.Format("出售了1个 [{1}]", 1, data.Item.Name));
            });
        }
        else if (data.IsEmpty)
        {
            ConfirmWindow.StartConfirm("这种物品暂无特价收购需求，确定按原价出售吗？", delegate
            {
                if (BackpackManager.Instance.GetItemData(data.Item, out var item, out var amount))
                    PurchaseItem(item, amount, true);
                else MessageManager.Instance.New($"{BackpackManager.Instance.Name}中没有{data.Item.Name}");
            });
        }
        else
        {
            AmountWindow.StartInput(delegate (long amount)
            {
                ConfirmWindow.StartConfirm(string.Format("确定出售{0}个 [{1}] 吗？", (int)amount, data.Item.Name), delegate
                {
                    if (OnPurchase(data, (int)amount))
                        MessageManager.Instance.New(string.Format("出售了{0}个 [{1}]", (int)amount, data.Item.Name));
                });
            }, maxAmount, "出售数量", ZetanUtility.ScreenCenter, Vector2.zero);
        }
    }
    bool OnPurchase(GoodsData data, int amount = 1)
    {
        if (MShop == null || data == null || !data.IsValid || amount < 1) return false;
        if (!MShop.Acquisitions.Contains(data)) return false;
        if (BackpackManager.Instance.Inventory.TryGetDatas(data.Item, out var items))
        {
            MessageManager.Instance.New($"{BackpackManager.Instance.Name}中没有{data.Item.Name}");
            return false;
        }
        if (items.Sum(x => x.amount) < amount)
        {
            MessageManager.Instance.New($"{BackpackManager.Instance.Name}中没有这么多的{data.Item.Name}");
            return false;
        }
        if (data.Info.EmptyAble && amount > data.LeftAmount)
        {
            if (!data.IsEmpty) MessageManager.Instance.New($"不收够这么多的{data.Item.Name}");
            else MessageManager.Instance.New($"无{data.Item.Name}收购需求");
            return false;
        }
        ItemBase item = items[0].source.Model;
        if (!BackpackManager.Instance.CanLose(items[0].source, amount)) return false;
        BackpackManager.Instance.GetMoney(amount * data.Info.PurchasePrice);
        BackpackManager.Instance.LoseItem(items[0].source, amount);
        if (data.Info.EmptyAble)
        {
            data.LeftAmount -= amount;
            goodsList.RefreshItemIf(x => x.Data == data);
        }
        return true;
    }

    /// <summary>
    /// 从玩家那里购入道具
    /// </summary>
    /// <param name="item">道具信息</param>
    /// <param name="force">强制购入</param>
    public void PurchaseItem(ItemData item, int have, bool force = false)
    {
        if (MShop == null || item == null || !item.Model)
        {
            Debug.Log(item);
            return;
        }
        if (!item.Model.SellAble)
        {
            MessageManager.Instance.New("这种物品不可出售");
            return;
        }
        if (item is EquipmentData eqm)
            if (eqm.gems.Count > 0)
            {
                MessageManager.Instance.New("镶嵌宝石的物品不可出售");
                return;
            }
        GoodsData find = MShop.Acquisitions.Find(x => x.Item == item.Model);
        if (find != null && !force)//采购品列表里有该道具，说明对该道具有特殊购价
        {
            PurchaseItem(find);
            return;
        }
        if (have == 1)
        {
            ConfirmWindow.StartConfirm(string.Format("确定出售1个 [{0}] 吗？", item.Model.Name), delegate
            {
                if (OnPurchase(item, have))
                    MessageManager.Instance.New(string.Format("出售了1个 [{0}]", item.Model.Name));
            });
        }
        else
        {
            AmountWindow.StartInput(delegate (long amount)
            {
                ConfirmWindow.StartConfirm(string.Format("确定出售{0}个 [{1}] 吗？", (int)amount, item.Model.Name), delegate
                {
                    if (OnPurchase(item, have, (int)amount))
                        MessageManager.Instance.New(string.Format("出售了{0}个 [{1}]", (int)amount, item.Model.Name));
                });
            }, have, "出售数量", ZetanUtility.ScreenCenter, Vector2.zero);
        }
    }
    bool OnPurchase(ItemData item, int have, int amount = 1)
    {
        if (MShop == null || item == null || !item.Model || amount < 1)
            return false;
        if (!item.Model.SellAble)
        {
            MessageManager.Instance.New("这种物品不可出售");
            return false;
        }
        if (have < 1)
        {
            MessageManager.Instance.New($"{BackpackManager.Instance.Name}中没有{item.Model.Name}");
            return false;
        }
        if (amount > have)
        {
            MessageManager.Instance.New($"{BackpackManager.Instance.Name}中没有这么多的{item.Model.Name}");
            return false;
        }
        if (!BackpackManager.Instance.CanLose(item, amount)) return false;
        BackpackManager.Instance.GetMoney(amount * item.Model.SellPrice);
        BackpackManager.Instance.LoseItem(item, amount);
        return true;
    }
    #endregion

    #region UI相关
    protected override bool OnOpen(params object[] args)
    {
        if (args.Length > 0 && args[0] is ShopData shop)
        {
            MShop = shop;
            UIManager.Instance.EnableJoyStick(false);
            bagHiddenBef = NewWindowsManager.IsWindowHidden<BackpackWindow>();
            bagOpenBef = NewWindowsManager.IsWindowOpen<BackpackWindow>();
            var backpack = NewWindowsManager.UnhideOrOpenWindow<BackpackWindow>();
            if (!backpack) return false;
            backpack.onClose += () => CloseBy(backpack);
            return true;
        }
        else return false;
    }

    protected override bool OnClose(params object[] args)
    {
        MShop = null;
        NewWindowsManager.CloseWindow<ItemWindow>();
        NewWindowsManager.HideWindow<DialogueWindow>(false);
        NewWindowsManager.CloseWindow<AmountWindow>();
        UIManager.Instance.EnableJoyStick(true);
        if (closeBy is not BackpackWindow)
        {
            if (bagHiddenBef) NewWindowsManager.HideWindow<BackpackWindow>(true);
            else if (!bagOpenBef) NewWindowsManager.CloseWindow<BackpackWindow>();
        }
        bagHiddenBef = false;
        bagOpenBef = false;
        return true;
    }
    #endregion

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(ShopManager.VendorGoodsRefresh, OnGoodsRefresh, this);
    }
    private void OnGoodsRefresh(object[] msg)
    {
        if (msg.Length > 0 && msg[0] is GoodsData goods && goods.Shop == MShop)
            goodsList.RefreshItemIf(x => x.Data == goods);
    }
}