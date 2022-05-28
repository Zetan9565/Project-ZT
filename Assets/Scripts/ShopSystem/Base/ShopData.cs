using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.Item;

public class ShopData
{
    public TalkerData Vendor { get; private set; }

    public ShopInformation Info { get; private set; }

    /// <summary>
    /// 出售的物品
    /// </summary>
    public List<GoodsData> Commodities { get; } = new List<GoodsData>();
    /// <summary>
    /// 收购的物品
    /// </summary>
    public List<GoodsData> Acquisitions { get; } = new List<GoodsData>();

    public ShopData(TalkerData vendor, ShopInformation info)
    {
        Vendor = vendor;
        Info = info;
        foreach (GoodsInfo mi in Info.Commodities)
        {
            Commodities.Add(new GoodsData(this, mi, GoodsType.SellToPlayer));
        }
        foreach (GoodsInfo mi in Info.Acquisitions)
        {
            Acquisitions.Add(new GoodsData(this, mi, GoodsType.BuyFromPlayer));
        }
    }

    public void TimePass(float time)
    {
        using (var commodityEnum = Commodities.GetEnumerator())
            while (commodityEnum.MoveNext())
            {
                commodityEnum.Current.TimePass(time);
            }

        using (var acquisitionEnum = Acquisitions.GetEnumerator())
            while (acquisitionEnum.MoveNext())
            {
                acquisitionEnum.Current.TimePass(time);
            }
    }

    public static implicit operator bool(ShopData self)
    {
        return self != null;
    }
}

public class GoodsData
{
    public ShopData Shop { get; private set; }

    public GoodsInfo Info { get; private set; }

    public GoodsType Type { get; private set; }

    public Item Item => Info.Item;

    public float leftRefreshTime;

    [HideInInspector]
    private int leftAmount;
    public int LeftAmount
    {
        get
        {
            return leftAmount;
        }
        set
        {
            if (value > Info.MaxAmount) leftAmount = Info.MaxAmount;
            else if (value < 0) leftAmount = 0;
            else leftAmount = value;
        }
    }

    public bool IsEmpty => Info.EmptyAble && leftAmount <= 0;

    public bool IsValid => Info && Info.Item && Info.MaxAmount > 0;

    public void TimePass(float time)
    {
        if (Info.EmptyAble && Info.RefreshTime > 0)
        {
            leftRefreshTime -= time;
            if (leftRefreshTime <= 0)
            {
                leftRefreshTime = Info.RefreshTime;
                LeftAmount += Random.Range(Info.MinRefreshAmount, Info.MaxRefreshAmount);
                NotifyCenter.PostNotify(ShopManager.VendorGoodsRefresh, this);
            }
        }
    }

    public GoodsData(ShopData shop, GoodsInfo info, GoodsType type)
    {
        Shop = shop;
        Info = info;
        Type = type;
        leftRefreshTime = info.RefreshTime;
        leftAmount = info.MaxAmount;
    }
}
public enum GoodsType
{
    SellToPlayer,
    BuyFromPlayer
}