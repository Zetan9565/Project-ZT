using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZetanStudio.Item;
using ZetanStudio.Item.Module;

[DisallowMultipleComponent]
public class ItemSlotBase : GridItem<ItemSlotBase, ItemSlotData>, IPointerClickHandler
{
    [SerializeField, Label("图标")]
    protected Image icon;

    [SerializeField, Label("数量")]
    protected Text amount;

    [SerializeField, Label("品质识别框")]
    protected Image qualityEdge;

    [SerializeField, Label("复选框")]
    protected GameObject mark;

    [SerializeField]
    protected ItemCoolDown coolDown;

    public bool IsDark { get; protected set; }
    public bool IsMarked { get; protected set; }

    public ItemData Item => Data ? Data.item : null;

    public bool IsEmpty { get { return Data == null || Data.IsEmpty; } }

    public void Vacate()
    {
        Mark(false);
        Dark(false);
        icon.overrideSprite = null;
        amount.text = string.Empty;
        qualityEdge.color = Color.white;
        ZetanUtility.SetActive(coolDown, false);
    }
    public override void OnClear()
    {
        Vacate();
        Data = null;
        darkCondition = null;
        markCondition = null;
        base.OnClear();
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
    public void ShowOrHide(bool show)
    {
        ZetanUtility.SetActive(gameObject, show);
    }

    protected override void OnInit()
    {
        Vacate();
    }

    public void Mark(bool mark = true)
    {
        if (this.mark) ZetanUtility.SetActive(this.mark, !IsEmpty && mark);
    }

    /// <summary>
    /// 用于单独展示
    /// </summary>
    /// <param name="item"></param>
    public virtual void SetItem(Item item, string amountText = null)
    {
        if (!Data) Data = new ItemSlotData(new ItemData(item, false));
        else Data.item = new ItemData(item, false);
        icon.overrideSprite = item.Icon;
        qualityEdge.color = Data.Model.Quality.Color;
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
        qualityEdge.color = Data.Model.Quality.Color;
        if (darkCondition != null) Dark(darkCondition(this));
        if (markCondition != null) Mark(markCondition(this));
        if (coolDown)
            if (Item.GetModuleData<CoolDownData>() is CoolDownData cd && !cd.Available)
                coolDown.Init(Item);
            else
                coolDown.Init(null);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) OnClick();
    }

    protected virtual void OnClick()
    {
        if (!IsEmpty) WindowsManager.OpenWindow<ItemWindow>(this);
    }
}
public interface ISlotContainer
{
    void MarkIf(Predicate<ItemSlotBase> slot);
    void DarkIf(Predicate<ItemSlotBase> slot);
}