using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MerchandiseAgent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("名称")]
#endif
    private Text nameText;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("价格")]
#endif
    private Text priceText;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("数量")]
#endif
    private Text amountText;

    [SerializeField]
    private ItemAgent itemAgentSon;

    public bool IsEmpty { get { return merchandiseInfo == null || !merchandiseInfo.Item; } }

    [HideInInspector]
    public MerchandiseInfo merchandiseInfo { get; private set; }

    [HideInInspector]
    public MerchandiseType merchandiseType;

    public void Init(MerchandiseInfo info, MerchandiseType type)
    {
        if (info == null || !info.Item) return;
        merchandiseInfo = info;
        merchandiseType = type;
        if (type == MerchandiseType.SellToPlayer) itemAgentSon.Init(ItemAgentType.ShopSelling);
        else itemAgentSon.Init(ItemAgentType.ShopBuying);
        itemAgentSon.InitItem(new ItemInfo(info.Item));
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        if (merchandiseInfo == null || !merchandiseInfo.Item) return;
        nameText.text = merchandiseInfo.Item.Name;
        if (merchandiseType == MerchandiseType.SellToPlayer) priceText.text = (merchandiseInfo.Item.BuyPrice * merchandiseInfo.PriceMultiple).ToString("F0") + "文";
        else priceText.text = (merchandiseInfo.Item.SellPrice * merchandiseInfo.PriceMultiple).ToString("F0") + "文";
        if (merchandiseInfo.SOorENAble)
        {
            if (merchandiseType == MerchandiseType.BuyFromPlayer) amountText.text = merchandiseInfo.IsEnough ? "暂无需求" : merchandiseInfo.LeftAmount + "/" + merchandiseInfo.MaxAmount;
            else amountText.text = merchandiseInfo.IsSoldOut ? "售罄" : merchandiseInfo.LeftAmount + "/" + merchandiseInfo.MaxAmount;
        }
        else
        {
            if (merchandiseType == MerchandiseType.BuyFromPlayer) amountText.text = "无限收购";
            else amountText.text = "现货充足";
        }
        itemAgentSon.MItemInfo.Amount = merchandiseInfo.LeftAmount;
        itemAgentSon.UpdateInfo();
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        priceText.text = string.Empty;
        amountText.text = string.Empty;
        merchandiseInfo = null;
        if (recycle)
        {
            itemAgentSon.Empty();
            ObjectPool.Instance.Put(gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        itemAgentSon.OnPointerClick(eventData);
    }

    public bool IsMacth(ItemInfo info)
    {
        return itemAgentSon.MItemInfo == info;
    }

    public void OnSellOrPurchase()
    {
        if (merchandiseType == MerchandiseType.SellToPlayer) ShopManager.Instance.SellItem(merchandiseInfo);
        else ShopManager.Instance.PurchaseItem(merchandiseInfo);
    }
}

public enum MerchandiseType
{
    SellToPlayer,
    BuyFromPlayer
}