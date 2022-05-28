using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GoodsAgent : GridItem<GoodsAgent, GoodsData>, IPointerClickHandler
{
    [SerializeField, Label("名称")]
    private Text nameText;

    [SerializeField, Label("价格")]
    private Text priceText;

    [SerializeField, Label("数量")]
    private Text amountText;

    [SerializeField]
    private ItemSlot itemIcon;

    private ShopWindow window;

    public bool IsEmpty { get { return Data == null || !Data.Info || !Data.Info.IsValid; } }

    protected override void OnAwake()
    {
        itemIcon.SetCallbacks(GetHandleButtons);
    }

    public void SetWindow(ShopWindow window)
    {
        this.window = window;
    }

    private ButtonWithTextData[] GetHandleButtons(ItemSlot slot)
    {
        if (!slot || slot.IsEmpty) return null;

        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
        if (Data.Type == GoodsType.SellToPlayer)
            buttons.Add(new ButtonWithTextData("购买", delegate
            {
                window.SellItem(Data);
            }));
        else
            buttons.Add(new ButtonWithTextData("出售", delegate
            {
                window.PurchaseItem(Data);
            }));
        return buttons.ToArray();
    }

    public override void OnClear()
    {
        Data = null;
        nameText.text = string.Empty;
        priceText.text = string.Empty;
        amountText.text = string.Empty;
        itemIcon.Vacate();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        itemIcon.OnPointerClick(eventData);
    }

    public void OnSellOrPurchase()
    {
        if (Data.Type == GoodsType.SellToPlayer) window.SellItem(Data);
        else window.PurchaseItem(Data);
    }

    public override void Refresh()
    {
        if (Data == null || !Data.Item) return;
        nameText.text = Data.Item.Name;
        if (Data.Type == GoodsType.SellToPlayer) priceText.text = Data.Info.Price.ToString("F0") + "文";
        else priceText.text = Data.Info.Price.ToString("F0") + "文";
        if (Data.Info.EmptyAble)
        {
            if (Data.Type == GoodsType.BuyFromPlayer) amountText.text = Data.IsEmpty ? "暂无需求" : Data.LeftAmount + "/" + Data.Info.MaxAmount;
            else amountText.text = Data.IsEmpty ? "售罄" : Data.LeftAmount + "/" + Data.Info.MaxAmount;
        }
        else
        {
            if (Data.Type == GoodsType.BuyFromPlayer) amountText.text = "无限收购";
            else amountText.text = "现货充足";
        }
        itemIcon.SetItem(Data.Item);
    }
}