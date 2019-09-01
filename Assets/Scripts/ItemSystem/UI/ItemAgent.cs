using System.Collections;
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

    private ScrollRect parentScrollRect;

    public bool IsEmpty { get { return MItemInfo == null || !MItemInfo.item; } }

    #region 道具使用相关
    public void OnUse()
    {
        if (!IsEmpty) UseItem();
    }

    public void UseItem()
    {
        if (!MItemInfo.item) return;
        if (!MItemInfo.item.Useable)
        {
            MessageManager.Instance.NewMessage("该物品不可使用");
            return;
        }
        if (MItemInfo.item.IsBox) UseBox();
        else if (MItemInfo.item.IsEquipment) UseEuipment();
        else if (MItemInfo.item.IsBook) UseBook();
        else if (MItemInfo.item.IsBag) UseBag();
        if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
    }

    void UseBox()
    {
        BoxItem box = MItemInfo.item as BoxItem;
        if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo, 1))
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
        PlayerManager.Instance.Equip(MItemInfo);
    }

    void UseBook()
    {
        BookItem book = MItemInfo.item as BookItem;
        switch (book.BookType)
        {
            case BookType.Building:
                if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo, 1) && BuildingManager.Instance.Learn(book.BuildingToLearn))
                {
                    BackpackManager.Instance.LoseItem(MItemInfo);
                    BuildingManager.Instance.Init();
                }
                break;
            case BookType.Making:
                if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo, 1) && MakingManager.Instance.Learn(book.ItemToLearn))
                {
                    BackpackManager.Instance.LoseItem(MItemInfo);
                    MakingManager.Instance.Init();
                }
                break;
            case BookType.Skill:
            default: break;
        }
    }

    void UseBag()
    {
        BagItem bag = MItemInfo.item as BagItem;
        if (BackpackManager.Instance.TryLoseItem_Boolean(MItemInfo, 1))
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
        if (GameManager.QualityColors.Count >= 5)
        {
            qualityEdge.color = GameManager.QualityColors[(int)info.item.Quality];
        }
        UpdateInfo();
    }

    public void Show()
    {
        ZetanUtilities.SetActive(gameObject, true);
    }

    public void Hide()
    {
        ZetanUtilities.SetActive(gameObject, false);
    }

    public void UpdateInfo()
    {
        if (MItemInfo == null || !MItemInfo.item) return;
        if (MItemInfo.item.Icon) icon.overrideSprite = MItemInfo.item.Icon;
        if (agentType != ItemAgentType.Selling && agentType != ItemAgentType.Purchasing)
            amount.text = MItemInfo.Amount > 1 || (agentType == ItemAgentType.Making && MItemInfo.Amount > 0) ? MItemInfo.Amount.ToString() : string.Empty;
        else amount.text = string.Empty;
        if (MItemInfo.Amount < 1 && (agentType == ItemAgentType.Backpack || agentType == ItemAgentType.Warehouse || agentType == ItemAgentType.Loot)) Empty();
    }

    public void OnRightClick()
    {
        if (!DragableManager.Instance.IsDraging)
            if (agentType == ItemAgentType.Backpack && !WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen) OnUse();
            else if (WarehouseManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Warehouse)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
                    WarehouseManager.Instance.TakeOutItem(MItemInfo, true);
                }
                else if (agentType == ItemAgentType.Backpack)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
                    WarehouseManager.Instance.StoreItem(MItemInfo, true);
                }
            }
            else if (ShopManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Selling && ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo))
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
                    ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
                }
                else if (agentType == ItemAgentType.Backpack)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
                    ShopManager.Instance.PurchaseItem(MItemInfo);
                }
            }
            else if (LootManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Loot)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseItemWindow();
                    LootManager.Instance.TakeItem(MItemInfo, true);
                }
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
    private float clickTime;
    private int clickCount;
    private bool isClick;

    private void FixedUpdate()
    {
        if (isClick)
        {
            clickTime += Time.fixedDeltaTime;
            if (clickTime > 0.2f)
            {
                isClick = false;
                clickCount = 0;
                clickTime = 0;
            }
        }
    }

    WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    Coroutine pressCoroutine;
    IEnumerator Press()
    {
        float touchTime = 0;
        bool isPress = true;
        while (isPress)
        {
            touchTime += Time.fixedDeltaTime;
            if (touchTime >= 0.5f)
            {
                OnLongPress();
                yield break;
            }
            yield return WaitForFixedUpdate;
        }
    }

    /// <summary>
    /// 安卓长按时所作所为
    /// </summary>
    public void OnLongPress()
    {
        if (agentType == ItemAgentType.None || agentType == ItemAgentType.Purchasing) return;
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
            else if (target.agentType == ItemAgentType.Backpack && agentType == ItemAgentType.Selling)
                ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
        }
        FinishDrag();
    }

    private void BeginDrag()
    {
        icon.color = Color.grey;
        DragableManager.Instance.GetDragable(this, FinishDrag, icon.rectTransform.rect.width, icon.rectTransform.rect.height);
        ItemWindowManager.Instance.PauseShowing(true);
    }
    public void FinishDrag()
    {
        icon.color = Color.white;
        DragableManager.Instance.ResetIcon();
        ItemWindowManager.Instance.PauseShowing(false);
    }
    #endregion

    #region 事件相关
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && agentType == ItemAgentType.Making)
        {
            ItemWindowManager.Instance.OpenItemWindow(this);
            return;
        }
#if UNITY_STANDALONE
        if (eventData.button == PointerEventData.InputButton.Left)
            if (DragableManager.Instance.IsDraging)
            {
                ItemAgent source = DragableManager.Instance.Current as ItemAgent;
                if (source)
                    source.SwapInfoTo(this);
            }
            else
            {
                if (!IsEmpty && agentType != ItemAgentType.None && agentType != ItemAgentType.Loot && agentType != ItemAgentType.Purchasing)
                    BeginDrag();
            }
        else if (eventData.button == PointerEventData.InputButton.Right && !IsEmpty) OnRightClick();
#elif UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (clickCount < 1)
                isClick = true;
            if (clickTime <= 0.2f)
                clickCount++;
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
                    ItemWindowManager.Instance.OpenItemWindow(this);
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
            if (pressCoroutine != null) StopCoroutine(pressCoroutine);
            pressCoroutine = StartCoroutine(Press());
        }
#endif
    }

    public void OnPointerUp(PointerEventData eventData)//用于安卓拖拽
    {
#if UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
        if (DragableManager.Instance.IsDraging && (DragableManager.Instance.Current as ItemAgent) == this)
            OnEndDrag(eventData);
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)//用于PC
    {
#if UNITY_STANDALONE
        ItemWindowManager.Instance.OpenItemWindow(this);
        if (!DragableManager.Instance.IsDraging)
        {
            if ((agentType == ItemAgentType.None && !IsEmpty) || agentType != ItemAgentType.None)
                icon.color = Color.yellow;
        }
#endif
    }

    public void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC
    {
#if UNITY_STANDALONE
        if (!ItemWindowManager.Instance.IsHeld) ItemWindowManager.Instance.CloseItemWindow();
        if (!DragableManager.Instance.IsDraging)
        {
            icon.color = Color.white;
        }
#elif UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (parentScrollRect) parentScrollRect.OnBeginDrag(eventData);//修复ScrollRect冲突
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
        isClick = false;
#endif
    }

    public void OnDrag(PointerEventData eventData)//用于安卓拖拽
    {
#if UNITY_STANDALONE
        if (agentType == ItemAgentType.None) return;
#elif UNITY_ANDROID
        if (!DragableManager.Instance.IsDraging && parentScrollRect) parentScrollRect.OnDrag(eventData);//修复ScrollRect冲突
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && agentType != ItemAgentType.None)
        {
            DragableManager.Instance.MoveIcon();
        }
#endif
    }

    public void OnEndDrag(PointerEventData eventData)//用于安卓拖拽
    {
        if (agentType == ItemAgentType.None) return;
#if UNITY_ANDROID
        if (parentScrollRect) parentScrollRect.OnEndDrag(eventData);//修复ScrollRect冲突
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && DragableManager.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
        {
            ItemAgent target = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ItemAgent>();
            if (target) SwapInfoTo(target);
            else if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<DiscardArea>() == BackpackManager.Instance.DiscardArea && agentType == ItemAgentType.Backpack)
            {
                BackpackManager.Instance.DiscardItem(MItemInfo);
                AmountManager.Instance.SetPosition(eventData.position);
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
    Making,//带制作按钮
    Selling,//带购买按钮
    Purchasing,//带出售按钮
    Loot//带拾取按钮
}