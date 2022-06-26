using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.ItemSystem.UI;

[DisallowMultipleComponent]
public class ItemSlot : GridItem<ItemSlot, ItemSlotData>, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Label("图标")]
    protected Image icon;

    [SerializeField, Label("数量")]
    protected Text amount;

    [SerializeField, Label("强化等级")]
    protected Text enhLevel;

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
        enhLevel.text = string.Empty;
        qualityEdge.color = Color.white;
        if (coolDown) coolDown.Init(null);
    }
    public override void Clear()
    {
        Vacate();
        Data = null;
        darkCondition = null;
        markCondition = null;
        base.Clear();
    }

    private Predicate<ItemSlot> darkCondition;
    public void SetDarkCondition(Predicate<ItemSlot> darkCondition, bool immediate = false)
    {
        this.darkCondition = darkCondition;
        if (immediate)
            if (darkCondition != null) Dark(darkCondition(this));
            else Dark(false);
    }
    private Predicate<ItemSlot> markCondition;
    public void SetMarkCondition(Predicate<ItemSlot> markCondition, bool immediate = false)
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
    /// <param name="model"></param>
    public virtual void SetItem(Item model, string amountText = null)
    {
        if (!model)
        {
            Vacate();
            return;
        }
        if (!Data) Data = new ItemSlotData(ItemData.Empty(model));
        else Data.item = ItemData.Empty(model);
        icon.overrideSprite = model.Icon;
        qualityEdge.color = model.Quality.Color;
        amount.text = amountText ?? string.Empty;
        RefreshEnhLevel();
        Dark(false);
        Mark(false);
    }
    /// <summary>
    /// 用于单独展示
    /// </summary>
    /// <param name="item"></param>
    public virtual void SetItem(ItemData item, string amountText = null)
    {
        if (!item)
        {
            Vacate();
            return;
        }
        if (!Data) Data = new ItemSlotData(item);
        else Data.item = item;
        icon.overrideSprite = item.Icon;
        qualityEdge.color = item.Quality.Color;
        amount.text = amountText ?? string.Empty;
        RefreshEnhLevel();
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
        RefreshEnhLevel();
        qualityEdge.color = Data.Model.Quality.Color;
        if (darkCondition != null) Dark(darkCondition(this));
        if (markCondition != null) Mark(markCondition(this));
        if (coolDown)
            if (Item.GetModuleData<CoolDownData>())
                coolDown.Init(Item);
            else
                coolDown.Init(null);
    }

    private void RefreshEnhLevel()
    {
        enhLevel.text = Item.TryGetModuleData<EnhancementData>(out var enhancement) && enhancement.level > 0 ? (!enhancement.IsMax ? $"+{enhancement.level}" : "MAX") : string.Empty;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
            if (!IsEmpty) WindowsManager.OpenWindow<ItemWindow>(this);
#endif
    }

    public virtual void OnPointerEnter(PointerEventData eventData)//用于PC悬停
    {
#if UNITY_STANDALONE
        if (!IsEmpty) WindowsManager.OpenWindow<ItemWindow>(this);
        if (!DragableManager.Instance.IsDraging) Highlight(true);
#endif
    }

    public virtual void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC悬停
    {
#if UNITY_STANDALONE
        if (!IsEmpty) WindowsManager.CloseWindow<ItemWindow>();
        if (!DragableManager.Instance.IsDraging) Highlight(false);
#endif
    }

}
public interface ISlotContainer
{
    void MarkIf(Predicate<ItemSlot> slot);
    void DarkIf(Predicate<ItemSlot> slot);
    bool Contains(ItemSlot slot);
}