using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "shop info", menuName = "ZetanStudio/商店信息")]
public class ShopInformation : ScriptableObject
{
    [SerializeField]
    private string shopName;
    public string ShopName => shopName;

    [SerializeField, NonReorderable]
    private List<MerchandiseInfo> commodities = new List<MerchandiseInfo>();
    /// <summary>
    /// 在出售的东西
    /// </summary>
    public List<MerchandiseInfo> Commodities => commodities;

    [SerializeField, NonReorderable]
    private List<MerchandiseInfo> acquisitions = new List<MerchandiseInfo>();
    /// <summary>
    /// 在收购的东西
    /// </summary>
    public List<MerchandiseInfo> Acquisitions => acquisitions;

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
                if (commodity.EmptyAble && commodity.RefreshTime > 0)
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
                if (acquisiton.EmptyAble && acquisiton.RefreshTime > 0)
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
    public ItemBase Item => item;

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
    public int MaxAmount => maxAmount;

    [SerializeField]
    private float priceMultiple = 1;
    public float PriceMultiple => priceMultiple;

    [SerializeField]
    private bool emptyAble;
    public bool EmptyAble => emptyAble;//Able to sold out Or purchase Enough?

    [HideInInspector]
    public float leftRefreshTime;

    [SerializeField]
    private float refreshTime = 300.0f;//小于0表示永久限购或限售
    public float RefreshTime => refreshTime;

    [SerializeField]
    private int minRefreshAmount = 1;
    public int MinRefreshAmount => minRefreshAmount;

    [SerializeField]
    private int maxRefreshAmount = 1;//每次刷新最大补充量
    public int MaxRefreshAmount => maxRefreshAmount;

    public int SellPrice => (int)priceMultiple * item.BuyPrice;

    public int PurchasePrice => (int)priceMultiple * item.SellPrice;

    public bool IsValid => Item;

    public bool IsEmpty => EmptyAble && leftAmount <= 0;

    public static implicit operator bool(MerchandiseInfo self)
    {
        return self != null;
    }
}