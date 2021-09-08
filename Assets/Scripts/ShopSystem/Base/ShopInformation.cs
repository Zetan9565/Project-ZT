using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "shop info", menuName = "Zetan Studio/商店信息")]
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
}

[System.Serializable]
public class MerchandiseInfo
{
    [SerializeField]
    private ItemBase item;
    public ItemBase Item => item;

    [SerializeField]
    private int maxAmount = 1;
    public int MaxAmount => maxAmount;

    [SerializeField]
    private float priceMultiple = 1;
    public float PriceMultiple => priceMultiple;

    [SerializeField]
    private bool emptyAble;
    public bool EmptyAble => emptyAble;//Able to sold out Or purchase Enough?

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

    public static implicit operator bool(MerchandiseInfo self)
    {
        return self != null;
    }
}