using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ItemSlotBase : GridItem<ItemSlotBase, ItemSlotData>, IPointerClickHandler
{
    [SerializeField, DisplayName("图标")]
    protected Image icon;

    [SerializeField, DisplayName("数量")]
    protected Text amount;

    [SerializeField, DisplayName("品质识别框")]
    protected Image qualityEdge;

    [SerializeField, DisplayName("复选框")]
    protected GameObject mark;

    public bool IsDark { get; protected set; }
    public bool IsMarked { get; protected set; }

    public ItemInfo Info { get; protected set; }

    public ItemData Item => Data ? Data.item : null;

    public bool IsEmpty { get { return Data == null || Data.IsEmpty; } }

    public void Init()
    {
        Clear();
    }
    public void Vacate()
    {
        Mark(false);
        Dark(false);
        icon.overrideSprite = null;
        amount.text = string.Empty;
        qualityEdge.color = Color.white;
    }
    public void Clear()
    {
        Vacate();
        Info = null;
        Data = null;
        View = null;
        darkCondition = null;
        markCondition = null;
    }
    public void Recycle()
    {
        Clear();
        ObjectPool.Put(gameObject);
    }

    private Predicate<ItemSlotBase> darkCondition;
    public void SetDarkCondition(Predicate<ItemSlotBase> darkCondition, bool immediate = false)
    {
        this.darkCondition = darkCondition;
        if (immediate)
            if (darkCondition != null) Dark(darkCondition(this));
            else Dark(false);
    }
    private Predicate<ItemSlotBase> markCondition;
    public void SetMarkCondition(Predicate<ItemSlotBase> markCondition, bool immediate = false)
    {
        this.markCondition = markCondition;
        if (immediate)
            if (markCondition != null) Mark(markCondition(this));
            else Mark(false);
    }

    public void Dark(bool dark = true)
    {
        icon.color = !IsEmpty && dark ? Color.grey : Color.white;
        IsDark = dark;
    }

    public void Highlight(bool highlight = true)
    {
        if (highlight)
            if (IsDark)
            {
                icon.color = (Color.yellow + Color.grey) / 2;
            }
            else
            {
                icon.color = Color.yellow;
            }
        else Dark(IsDark);
    }

    public void Show()
    {
        ZetanUtility.SetActive(gameObject, true);
    }
    public void Hide()
    {
        ZetanUtility.SetActive(gameObject, false);
    }

    public void Mark(bool mark = true)
    {
        if (this.mark) ZetanUtility.SetActive(this.mark, !IsEmpty && mark);
    }

    /// <summary>
    /// 用于单独展示
    /// </summary>
    /// <param name="item"></param>
    public virtual void SetItem(ItemBase item, string amountText = null)
    {
        if (!Data) Data = new ItemSlotData(new ItemData(item, false));
        else Data.item = new ItemData(item, false);
        icon.overrideSprite = item.Icon;
        qualityEdge.color = ItemUtility.QualityToColor(Data.Model.Quality);
        amount.text = amountText ?? string.Empty;
        Dark(false);
        Mark(false);
    }

    public override void Refresh()
    {
        if (Data == null || Data.IsEmpty)
        {
            Vacate();
            return;
        }
        if (Data.Model.Icon) icon.overrideSprite = Data.Model.Icon;
        amount.text = Data.amount > 0 && Data.Model.StackAble ? Data.amount.ToString() : string.Empty;
        qualityEdge.color = ItemUtility.QualityToColor(Data.Model.Quality);
        if (darkCondition != null) Dark(darkCondition(this));
        if (markCondition != null) Mark(markCondition(this));
    }

    public virtual void UpdateInfo()
    {
        if (Info == null || !Info.item || Info.Amount < 1)
        {
            Vacate();
            return;
        }
        if (Info.item.Icon) icon.overrideSprite = Info.item.Icon;
        amount.text = Info.Amount > 0 && Info.item.StackAble ? Info.Amount.ToString() : string.Empty;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!IsEmpty) NewWindowsManager.OpenWindow<ItemWindow>(this);
            return;
        }
    }
}
public interface ISlotContainer
{
    void MarkIf(Predicate<ItemSlotBase> slot);
    void DarkIf(Predicate<ItemSlotBase> slot);
}