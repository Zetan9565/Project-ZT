using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Shop
{
    [SerializeField]
    private string shopName;
    public string ShopName
    {
        get
        {
            return shopName;
        }
    }

    [SerializeField]
    /// <summary>
    /// 在出售的东西
    /// </summary>
    private List<MerchandiseInfo> commodities = new List<MerchandiseInfo>();
    public List<MerchandiseInfo> Commodities
    {
        get
        {
            return commodities;
        }
    }

    [SerializeField]
    /// <summary>
    /// 在收购的东西
    /// </summary>
    private List<MerchandiseInfo> acquisitions = new List<MerchandiseInfo>();
    public List<MerchandiseInfo> Acquisitions
    {
        get
        {
            return acquisitions;
        }
    }

    public void Init()
    {
        foreach (MerchandiseInfo mi in Commodities)
        {
            mi.LeftAmount = mi.MaxAmount;
            mi.leftRefreshTime = mi.RefreshTime;
        }
        foreach (MerchandiseInfo mi in Acquisitions)
        {
            mi.LeftAmount = mi.MaxAmount;
            mi.leftRefreshTime = mi.RefreshTime;
        }
    }

    public void Refresh(float time)
    {
        using (var commodityEnum = Commodities.GetEnumerator())
            while (commodityEnum.MoveNext())
            {
                MerchandiseInfo commodity = commodityEnum.Current;
                if (commodity.SOorENAble && commodity.RefreshTime > 0)
                {
                    commodity.leftRefreshTime -= time;
                    if (commodity.leftRefreshTime <= 0)
                    {
                        commodity.leftRefreshTime = commodity.RefreshTime;
                        commodity.LeftAmount += Random.Range(commodity.MinRefreshAmount, commodity.MaxRefreshAmount);
                    }
                }
            }

        using (var acquisitionEnum = Acquisitions.GetEnumerator())
            while (acquisitionEnum.MoveNext())
            {
                MerchandiseInfo acquisiton = acquisitionEnum.Current;
                if (acquisiton.SOorENAble && acquisiton.RefreshTime > 0)
                {
                    acquisiton.leftRefreshTime -= time;
                    if (acquisiton.leftRefreshTime <= 0)
                    {
                        acquisiton.leftRefreshTime = acquisiton.RefreshTime;
                        acquisiton.LeftAmount += Random.Range(acquisiton.MinRefreshAmount, acquisiton.MaxRefreshAmount);
                    }
                }
            }
    }
}

[System.Serializable]
public class MerchandiseInfo
{
    [SerializeField]
    private ItemBase item;
    public ItemBase Item
    {
        get
        {
            return item;
        }
    }

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
            if (value > maxAmount) leftAmount = maxAmount;
            else if (value < 0) leftAmount = 0;
            else leftAmount = value;
        }
    }

    [SerializeField]
    private int maxAmount = 1;
    public int MaxAmount
    {
        get
        {
            return maxAmount;
        }
    }

    [SerializeField]
    private float priceMultiple = 1;
    public float PriceMultiple
    {
        get
        {
            return priceMultiple;
        }
    }

    [SerializeField]
    private bool emptyAble;
    public bool SOorENAble
    {
        get
        {
            return emptyAble;
        }
    }//Able to sold out Or purchase Enough?

    [HideInInspector]
    public float leftRefreshTime;

    [SerializeField]
    private float refreshTime = 300.0f;//小于0表示永久限购
    public float RefreshTime
    {
        get
        {
            return refreshTime;
        }
    }

    [SerializeField]
    private int minRefreshAmount = 1;
    public int MinRefreshAmount
    {
        get
        {
            return minRefreshAmount;
        }
    }

    [SerializeField]
    private int maxRefreshAmount = 1;//每次刷新最大补充量
    public int MaxRefreshAmount
    {
        get
        {
            return maxRefreshAmount;
        }
    }

    public int SellPrice
    {
        get
        {
            return (int)priceMultiple * item.BuyPrice;
        }
    }

    public int PurchasePrice
    {
        get
        {
            return (int)priceMultiple * item.SellPrice;
        }
    }

    public bool IsInvalid
    {
        get
        {
            return !Item;
        }
    }

    public bool IsSoldOut
    {
        get
        {
            return SOorENAble && leftAmount <= 0;
        }
    }

    public bool IsEnough
    {
        get
        {
            return SOorENAble && leftAmount <= 0;
        }
    }
}