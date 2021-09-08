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
    private ItemSlot itemIcon;

    public bool IsEmpty { get { return Data == null || !Data.Info || !Data.Info.IsValid; } }

    [HideInInspector]
    public MerchandiseData Data { get; private set; }

    [HideInInspector]
    public MerchandiseType merchandiseType;

    public void Init(MerchandiseData data, MerchandiseType type)
    {
        if (data == null || !data.Info) return;
        Data = data;
        merchandiseType = type;
        itemIcon.Init(GetHandleButtons);
        itemIcon.SetItem(new ItemInfo(data.Info.Item));
        UpdateInfo();
    }

    private ButtonWithTextData[] GetHandleButtons(ItemSlot slot)
    {
        if (!slot || slot.IsEmpty) return null;

        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
        if (merchandiseType == MerchandiseType.SellToPlayer)
            buttons.Add(new ButtonWithTextData("购买", delegate
            {
                ShopManager.Instance.SellItem(Data);
            }));
        else
            buttons.Add(new ButtonWithTextData("出售", delegate
            {
                ShopManager.Instance.PurchaseItem(Data);
            }));
        return buttons.ToArray();
    }

    public void UpdateInfo()
    {
        if (Data == null || !Data.Item) return;
        nameText.text = Data.Item.name;
        if (merchandiseType == MerchandiseType.SellToPlayer) priceText.text = Data.Info.SellPrice.ToString("F0") + "文";
        else priceText.text = Data.Info.PurchasePrice.ToString("F0") + "文";
        if (Data.Info.EmptyAble)
        {
            if (merchandiseType == MerchandiseType.BuyFromPlayer) amountText.text = Data.IsEmpty ? "暂无需求" : Data.LeftAmount + "/" + Data.Info.MaxAmount;
            else amountText.text = Data.IsEmpty ? "售罄" : Data.LeftAmount + "/" + Data.Info.MaxAmount;
        }
        else
        {
            if (merchandiseType == MerchandiseType.BuyFromPlayer) amountText.text = "无限收购";
            else amountText.text = "现货充足";
        }
        itemIcon.UpdateInfo();
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        priceText.text = string.Empty;
        amountText.text = string.Empty;
        itemIcon.Empty();
        Data = null;
        if (recycle)
        {
            ObjectPool.Put(gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        itemIcon.OnPointerClick(eventData);
    }

    public void OnSellOrPurchase()
    {
        if (merchandiseType == MerchandiseType.SellToPlayer) ShopManager.Instance.SellItem(Data);
        else ShopManager.Instance.PurchaseItem(Data);
    }
}

public enum MerchandiseType
{
    SellToPlayer,
    BuyFromPlayer
}