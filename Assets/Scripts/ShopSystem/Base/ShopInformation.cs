using UnityEngine;
using System.Collections.Generic;
using ZetanStudio.Item;
using ZetanStudio.Item.Module;

[CreateAssetMenu(fileName = "shop info", menuName = "Zetan Studio/商店信息")]
public class ShopInformation : ScriptableObject
{
    [SerializeField]
    private string shopName;
    public string ShopName => shopName;

    [SerializeField, NonReorderable]
    private List<GoodsInfo> commodities = new List<GoodsInfo>();
    /// <summary>
    /// 在出售的东西
    /// </summary>
    public List<GoodsInfo> Commodities => commodities;

    [SerializeField, NonReorderable]
    private List<GoodsInfo> acquisitions = new List<GoodsInfo>();
    /// <summary>
    /// 在收购的东西
    /// </summary>
    public List<GoodsInfo> Acquisitions => acquisitions;
}

[System.Serializable]
public class GoodsInfo
{
    [SerializeField, ItemFilter(typeof(SellableModule))]
    private Item item;
    public Item Item => item;

    [SerializeField]
    private int maxAmount = 1;
    public int MaxAmount => maxAmount;

    [SerializeField]
    private float priceMultiple = 1;
    public float PriceMultiple => priceMultiple;

    [SerializeField]
    private bool emptyAble;
    public bool EmptyAble => emptyAble;//是否会购满或者售罄

    [SerializeField]
    private float refreshTime = 300.0f;//小于0表示永久限购或限售
    public float RefreshTime => refreshTime;

    [SerializeField]
    private int minRefreshAmount = 1;
    public int MinRefreshAmount => minRefreshAmount;

    [SerializeField]
    private int maxRefreshAmount = 1;//每次刷新最大补充量
    public int MaxRefreshAmount => maxRefreshAmount;

    public int Price => (int)priceMultiple * item.GetModule<SellableModule>().Price;

    public bool IsValid => Item;

    public static implicit operator bool(GoodsInfo self)
    {
        return self != null;
    }
}