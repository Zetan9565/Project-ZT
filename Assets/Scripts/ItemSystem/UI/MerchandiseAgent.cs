using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    private ItemSlot itemAgentSon;

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
        itemAgentSon.Init(GetHandleButtons);
        itemAgentSon.SetItem(new ItemInfo(info.Item));
        UpdateInfo();
    }

    private ButtonWithTextData[] GetHandleButtons(ItemSlot slot)
    {
        if (!slot || slot.IsEmpty) return null;

        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
        if (merchandiseType == MerchandiseType.SellToPlayer)
            buttons.Add(new ButtonWithTextData("购买", delegate
            {
                ShopManager.Instance.SellItem(merchandiseInfo);
            }));
        else
            buttons.Add(new ButtonWithTextData("出售", delegate
            {
                ShopManager.Instance.PurchaseItem(merchandiseInfo);
            }));
        return buttons.ToArray();
    }

    public void UpdateInfo()
    {
        if (merchandiseInfo == null || !merchandiseInfo.Item) return;
        nameText.text = merchandiseInfo.Item.name;
        if (merchandiseType == MerchandiseType.SellToPlayer) priceText.text = (merchandiseInfo.Item.BuyPrice * merchandiseInfo.PriceMultiple).ToString("F0") + "文";
        else priceText.text = (merchandiseInfo.Item.SellPrice * merchandiseInfo.PriceMultiple).ToString("F0") + "文";
        if (merchandiseInfo.EmptyAble)
        {
            if (merchandiseType == MerchandiseType.BuyFromPlayer) amountText.text = merchandiseInfo.IsEmpty ? "暂无需求" : merchandiseInfo.LeftAmount + "/" + merchandiseInfo.MaxAmount;
            else amountText.text = merchandiseInfo.IsEmpty ? "售罄" : merchandiseInfo.LeftAmount + "/" + merchandiseInfo.MaxAmount;
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
            ObjectPool.Put(gameObject);
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