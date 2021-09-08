using System.Collections.Generic;
using UnityEngine;

public class ShopData
{

    public ShopInformation Info { get; private set; }

    public List<MerchandiseData> Commodities { get; } = new List<MerchandiseData>();
    public List<MerchandiseData> Acquisitions { get; } = new List<MerchandiseData>();

    public ShopData(ShopInformation info)
    {
        Info = info;
        foreach (MerchandiseInfo mi in Info.Commodities)
        {
            Commodities.Add(new MerchandiseData(mi));
        }
        foreach (MerchandiseInfo mi in Info.Acquisitions)
        {
            Acquisitions.Add(new MerchandiseData(mi));
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

public class MerchandiseData
{
    public MerchandiseInfo Info { get; private set; }

    public ItemBase Item => Info.Item;

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
            }
        }
    }

    public MerchandiseData(MerchandiseInfo info)
    {
        Info = info;
        leftRefreshTime = info.RefreshTime;
        leftAmount = info.MaxAmount;
    }
}