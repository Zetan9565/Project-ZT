using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ItemAgent : MonoBehaviour, IDragable,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("图标")]
#endif
    private Image icon;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("数量")]
#endif
    private Text amount;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("品质识别框")]
#endif
    private Image qualityEdge;

    public Color currentQualityColor { get { return qualityEdge.color; } }

    public Sprite DragableIcon
    {
        get
        {
            return icon.overrideSprite;
        }
    }

    [HideInInspector]
    public ItemInfo MItemInfo { get; private set; }

    [HideInInspector]
    public ItemAgentType agentType;

    [HideInInspector]
    public int indexInGrid;

    [HideInInspector]
    private ScrollRect parentScrollRect;

    public bool IsEmpty { get { return MItemInfo == null || !MItemInfo.Item; } }

    #region 道具使用相关
    public void OnUse()
    {
        if (!IsEmpty) UseItem();
    }


    public void UseItem()
    {
        if (!MItemInfo.Item) return;
        if (!MItemInfo.Item.Useable)
        {
            MessageManager.Instance.NewMessage("该物品不可使用");
            return;
        }
        if (MItemInfo.Item.IsBox) UseBox();
        else if (MItemInfo.Item.IsEquipment) UseEuipment();
        else if (MItemInfo.Item.IsBook) UseBook();
        else if (MItemInfo.Item.IsBag) UseBag();
    }

    void UseBox()
    {
        BoxItem box = MItemInfo.Item as BoxItem;
        if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo))
        {
            BackpackManager.Instance.MBackpack.weightLoad -= box.Weight;
            BackpackManager.Instance.MBackpack.backpackSize--;
            foreach (ItemInfo info in box.ItemsInBox)
            {
                if (!BackpackManager.Instance.TryGetItem_Boolean(info))
                {
                    BackpackManager.Instance.MBackpack.weightLoad += box.Weight;
                    BackpackManager.Instance.MBackpack.backpackSize++;
                    return;
                }
            }
            BackpackManager.Instance.MBackpack.weightLoad += box.Weight;
            BackpackManager.Instance.MBackpack.backpackSize++;
            BackpackManager.Instance.LoseItem(MItemInfo);
            foreach (ItemInfo info in box.ItemsInBox)
            {
                BackpackManager.Instance.GetItem(info);
            }
        }
    }

    void UseEuipment()
    {
        PlayerInfoManager.Instance.Equip(MItemInfo);
    }

    void UseBook()
    {
        BookItem book = MItemInfo.Item as BookItem;
        switch (book.BookType)
        {
            case BookType.Building:
                if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo) && BuildingManager.Instance.Learn(book.BuildingInfo))
                {
                    BackpackManager.Instance.LoseItem(MItemInfo);
                    BuildingManager.Instance.Init();
                }
                break;
            case BookType.Skill:
            default: break;
        }
    }

    void UseBag()
    {
        BagItem bag = MItemInfo.Item as BagItem;
        if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo))
        {
            if (BackpackManager.Instance.Expand(bag.ExpandSize))
                BackpackManager.Instance.LoseItem(MItemInfo);
        }
    }
    #endregion

    #region 操作相关
    public void Init(ItemAgentType agentType = ItemAgentType.None, int index = -1, ScrollRect parent = null)
    {
        Clear();
        this.agentType = agentType;
        indexInGrid = index;
        parentScrollRect = parent;
    }

    public void InitItem(ItemInfo info)
    {
        if (info == null) return;
        MItemInfo = info;
        MItemInfo.indexInGrid = indexInGrid;
        if (GameManager.Instance.QualityColors.Count >= 5)
        {
            qualityEdge.color = GameManager.Instance.QualityColors[(int)info.Item.Quality];
        }
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        if (MItemInfo == null || !MItemInfo.Item) return;
        if (MItemInfo.Item.Icon) icon.overrideSprite = MItemInfo.Item.Icon;
        if (agentType != ItemAgentType.ShopSelling && agentType != ItemAgentType.ShopBuying) amount.text = MItemInfo.Amount > 1 ? MItemInfo.Amount.ToString() : string.Empty;
        else amount.text = string.Empty;
        if (MItemInfo.Amount <= 0 && (agentType == ItemAgentType.Backpack || agentType == ItemAgentType.Warehouse)) Empty();
    }

    public void OnRightClick()
    {
        if (!DragableHandler.Instance.IsDraging)
            if (agentType == ItemAgentType.Backpack && !WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen)
                OnUse();
            else if (WarehouseManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Warehouse)
                    WarehouseManager.Instance.TakeOutItem(MItemInfo, true);
                else if (agentType == ItemAgentType.Backpack)
                    WarehouseManager.Instance.StoreItem(MItemInfo, true);
            }
            else if (ShopManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.ShopSelling && ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo))
                    ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
                if (agentType == ItemAgentType.Backpack)
                    ShopManager.Instance.PurchaseItem(MItemInfo);
            }
    }

    public void Empty()
    {
        icon.color = Color.white;
        icon.overrideSprite = null;
        amount.text = string.Empty;
        MItemInfo = null;
        qualityEdge.color = Color.white;
    }

    public void Clear(bool recycle = false)
    {
        icon.color = Color.white;
        icon.overrideSprite = null;
        amount.text = string.Empty;
        MItemInfo = null;
        indexInGrid = -1;
        qualityEdge.color = Color.white;
        if (recycle)
        {
            agentType = ItemAgentType.None;
            ObjectPool.Instance.Put(gameObject);
        }
    }

#if UNITY_ANDROID
    private float touchTime;
    private bool isPress;

    private float clickTime;
    private int clickCount;
    private bool isClick;

    private void FixedUpdate()
    {
        if (isPress)
        {
            touchTime += Time.deltaTime;
            if (touchTime >= 0.5f)
            {
                isPress = false;
                OnLongPress();
            }
        }
        if (isClick)
        {
            clickTime += Time.deltaTime;
            if (clickTime > 0.2f)
            {
                isClick = false;
                clickCount = 0;
                clickTime = 0;
            }
        }
    }

    /// <summary>
    /// 安卓长按时所作所为
    /// </summary>
    public void OnLongPress()
    {
        if (agentType == ItemAgentType.None || agentType == ItemAgentType.ShopBuying) return;
        if (!IsEmpty)
        {
            BeginDrag();
            isClick = false;
            clickCount = 0;
            clickTime = 0;
        }
    }
#endif


    /// <summary>
    /// 交换单元格内容
    /// </summary>
    /// <param name="target"></param>
    public void SwapInfoTo(ItemAgent target)
    {
        if (target != this && agentType != ItemAgentType.None)
        {
            if (target.agentType == agentType && (agentType == ItemAgentType.Backpack || agentType == ItemAgentType.Warehouse))
                if (target.IsEmpty)
                {
                    target.InitItem(MItemInfo);
                    Empty(); ;
                }
                else
                {
                    ItemInfo targetInfo = target.MItemInfo;
                    target.InitItem(MItemInfo);
                    InitItem(targetInfo);
                }
            else if (target.agentType == ItemAgentType.Warehouse && agentType == ItemAgentType.Backpack)
                WarehouseManager.Instance.StoreItem(MItemInfo);
            else if (target.agentType == ItemAgentType.Backpack && agentType == ItemAgentType.Warehouse)
                WarehouseManager.Instance.TakeOutItem(MItemInfo);
            else if (target.agentType == ItemAgentType.Backpack && agentType == ItemAgentType.ShopSelling)
                ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
        }
        FinishDrag();
    }

    private void BeginDrag()
    {
        icon.color = Color.grey;
        DragableHandler.Instance.GetDragable(this, FinishDrag, icon.rectTransform.rect.width, icon.rectTransform.rect.height);
        ItemWindowHandler.Instance.PauseShowing(true);
    }
    public void FinishDrag()
    {
        icon.color = Color.white;
        DragableHandler.Instance.ResetIcon();
        ItemWindowHandler.Instance.PauseShowing(false);
    }
    #endregion

    #region 事件相关
    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (eventData.button == PointerEventData.InputButton.Left)
            if (DragableHandler.Instance.IsDraging)
            {
                ItemAgent source = DragableHandler.Instance.Current as ItemAgent;
                if (source)
                    source.SwapInfoTo(this);
            }
            else
            {
                if (!IsEmpty && agentType != ItemAgentType.None && agentType != ItemAgentType.ShopBuying)
                {
                    StartDrag();
                }
            }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
#elif UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (clickCount < 1) isClick = true;
            if (clickTime <= 0.2f) clickCount++;
            if (!IsEmpty)
            {
                if (clickCount > 1)
                {
                    OnRightClick();
                    isClick = false;
                    clickCount = 0;
                    clickTime = 0;
                }
                else if (clickCount == 1)
                {
                    ItemWindowHandler.Instance.OpenItemWindow(this);
                }
            }
        }
#endif
    }

    public void OnPointerDown(PointerEventData eventData)//用于安卓拖拽
    {
#if UNITY_ANDROID
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left)
        {
            touchTime = 0;
            isPress = true;
        }
#endif
    }

    public void OnPointerUp(PointerEventData eventData)//用于安卓拖拽
    {
#if UNITY_ANDROID
        isPress = false;
        if (DragableHandler.Instance.IsDraging && (DragableHandler.Instance.Current as ItemAgent) == this)
            OnEndDrag(eventData);
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)//用于PC
    {
#if UNITY_STANDALONE
        ItemWindowHandler.Instance.OpenItemWindow(this);
        if (!DragableHandler.Instance.IsDraging)
        {
            if ((agentType == ItemAgentType.None && !IsEmpty) || agentType != ItemAgentType.None)
                icon.color = Color.yellow;
        }
#endif
    }

    public void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC
    {
#if UNITY_STANDALONE
        if (!ItemWindowHandler.Instance.IsHeld) ItemWindowHandler.Instance.CloseItemWindow();
        if (!DragableHandler.Instance.IsDraging)
        {
            icon.color = Color.white;
        }
#elif UNITY_ANDROID
        isPress = false;
#endif
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (parentScrollRect) parentScrollRect.OnBeginDrag(eventData);//修复ScrollRect冲突
        isPress = false;
        isClick = false;
#endif
    }

    public void OnDrag(PointerEventData eventData)//用于安卓拖拽
    {
#if UNITY_STANDALONE
        if (agentType == ItemAgentType.None) return;
#elif UNITY_ANDROID
        if (!DragableHandler.Instance.IsDraging && parentScrollRect) parentScrollRect.OnDrag(eventData);//修复ScrollRect冲突
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && agentType != ItemAgentType.None)
        {
            DragableHandler.Instance.MoveIcon();
        }
#endif
    }

    public void OnEndDrag(PointerEventData eventData)//用于安卓拖拽
    {
        if (agentType == ItemAgentType.None) return;
#if UNITY_ANDROID
        if (parentScrollRect) parentScrollRect.OnEndDrag(eventData);//修复ScrollRect冲突
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && DragableHandler.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
        {
            ItemAgent target = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ItemAgent>();
            if (target) SwapInfoTo(target);
            else if (eventData.pointerCurrentRaycast.gameObject == BackpackManager.Instance.DiscardArea && agentType == ItemAgentType.Backpack)
            {
                BackpackManager.Instance.DiscardItem(MItemInfo);
                AmountHandler.Instance.SetPosition(eventData.position);
            }
        }
#endif
        FinishDrag();
    }
    #endregion
}

public enum ItemAgentType
{
    None,//只带关闭按钮
    Backpack,//带使用按钮、丢弃按钮、存储按钮
    Warehouse,//带取出按钮
    Process,//带制作按钮
    ShopSelling,//带购买按钮
    ShopBuying//带出售按钮
}