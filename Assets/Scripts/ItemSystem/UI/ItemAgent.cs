using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ItemAgent : MonoBehaviour, IDragAble,
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

    public Sprite DragAbleIcon => icon.overrideSprite;

    [HideInInspector]
    public ItemInfo MItemInfo { get; private set; }

    [HideInInspector]
    public ItemAgentType agentType;

    [HideInInspector]
    public int indexInGrid;

    private ScrollRect parentScrollRect;

    public bool IsEmpty { get { return MItemInfo == null || !MItemInfo.item; } }

    #region 操作相关
    public void Init(ItemAgentType agentType = ItemAgentType.None, int index = -1, ScrollRect parent = null)
    {
        Clear();
        this.agentType = agentType;
        indexInGrid = index;
        parentScrollRect = parent;
    }

    public void SetItem(ItemInfo info)
    {
        if (info == null) return;
        MItemInfo = info;
        if (agentType == ItemAgentType.Warehouse || agentType == ItemAgentType.Backpack) MItemInfo.indexInGrid = indexInGrid;
        if (GameManager.QualityColors.Count >= 5)
        {
            qualityEdge.color = GameManager.QualityColors[(int)info.item.Quality];
        }
        UpdateInfo();
    }

    public void Show()
    {
        ZetanUtility.SetActive(gameObject, true);
    }

    public void Hide()
    {
        ZetanUtility.SetActive(gameObject, false);
    }

    public void UpdateInfo()
    {
        if (MItemInfo == null || !MItemInfo.item) return;
        if (MItemInfo.item.Icon) icon.overrideSprite = MItemInfo.item.Icon;
        if (agentType != ItemAgentType.Selling && agentType != ItemAgentType.Purchasing)
            amount.text = MItemInfo.Amount > 0 && MItemInfo.item.StackAble || agentType == ItemAgentType.Making && MItemInfo.Amount > 0 ? MItemInfo.Amount.ToString() : string.Empty;
        else amount.text = string.Empty;
        if (MItemInfo.Amount < 1 && (agentType == ItemAgentType.Backpack || agentType == ItemAgentType.Warehouse || agentType == ItemAgentType.Loot)) Empty();
    }

    public void OnRightClick()
    {
        if (!DragableManager.Instance.IsDraging)
        {
            if (agentType == ItemAgentType.Backpack && !WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen && !ItemSelectionManager.Instance.IsUIOpen)
                BackpackManager.Instance.UseItem(MItemInfo);
            else if (WarehouseManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Warehouse)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseWindow();
                    WarehouseManager.Instance.TakeOutItem(MItemInfo, true);
                }
                else if (agentType == ItemAgentType.Backpack)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseWindow();
                    WarehouseManager.Instance.StoreItem(MItemInfo, true);
                }
            }
            else if (ShopManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Selling && ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo))
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseWindow();
                    ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
                }
                else if (agentType == ItemAgentType.Backpack)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseWindow();
                    ShopManager.Instance.PurchaseItem(MItemInfo);
                }
            }
            else if (LootManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Loot)
                {
                    if (ItemWindowManager.Instance.MItemInfo == MItemInfo) ItemWindowManager.Instance.CloseWindow();
                    LootManager.Instance.TakeItem(MItemInfo, true);
                }
            }
            else if (ItemSelectionManager.Instance.IsUIOpen)
            {
                if (agentType == ItemAgentType.Backpack)
                    ItemSelectionManager.Instance.Place(MItemInfo);
                else if (agentType == ItemAgentType.Selection)
                    ItemSelectionManager.Instance.TakeOut(MItemInfo);
            }
            ItemWindowManager.Instance.CloseWindow();
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
            ObjectPool.Put(gameObject);
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

    readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    Coroutine pressCoroutine;
    float touchTime = 0;
    IEnumerator Press()
    {
        touchTime = 0;
        while (true)
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
                    target.SetItem(MItemInfo);
                    Empty(); ;
                }
                else
                {
                    ItemInfo targetInfo = target.MItemInfo;
                    target.SetItem(MItemInfo);
                    SetItem(targetInfo);
                }
            else if (target.agentType == ItemAgentType.Warehouse && agentType == ItemAgentType.Backpack)
                WarehouseManager.Instance.StoreItem(MItemInfo);
            else if (target.agentType == ItemAgentType.Backpack && agentType == ItemAgentType.Warehouse)
                WarehouseManager.Instance.TakeOutItem(MItemInfo);
            else if (target.agentType == ItemAgentType.Backpack && agentType == ItemAgentType.Selling)
                ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
            else if (target.agentType == ItemAgentType.Selection && agentType == ItemAgentType.Backpack)
                ItemSelectionManager.Instance.Place(MItemInfo);
        }
        FinishDrag();
    }

    private void BeginDrag()
    {
        if (agentType == ItemAgentType.Selection) return;
        if (ItemSelectionManager.Instance.IsUIOpen)
            if (ItemSelectionManager.Instance.SelectionType == ItemSelectionType.Discard && !MItemInfo.item.DiscardAble) return;
            else if (ItemSelectionManager.Instance.SelectionType == ItemSelectionType.Making && MItemInfo.item.MaterialType == MaterialType.None) return;
        DragableManager.Instance.GetDragable(this, FinishDrag, icon.rectTransform.rect.width, icon.rectTransform.rect.height);
        ItemWindowManager.Instance.CloseWindow();
        Dark();
    }
    public void FinishDrag()
    {
        if (!DragableManager.Instance.IsDraging) return;
        Light();
        DragableManager.Instance.ResetIcon();
        ItemWindowManager.Instance.CloseWindow();
    }

    public void Select()
    {
        if (isDark)
        {
            icon.color = (Color.yellow + Color.grey) / 2;
        }
        else
        {
            icon.color = Color.yellow;
        }
    }
    public void DeSelect()
    {
        if (isDark)
        {
            Dark();
        }
        else
        {
            Light();
        }
    }

    private bool isDark;
    public void Dark()
    {
        icon.color = Color.grey;
        isDark = true;
    }
    public void Light()
    {
        icon.color = Color.white;
        isDark = false;
    }
    #endregion

    #region 事件相关
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && agentType == ItemAgentType.Making)
        {
            ItemWindowManager.Instance.SetItemAndOpenWindow(this);
            return;
        }
#if UNITY_STANDALONE
        if (eventData.button == PointerEventData.InputButton.Left)
            if (DragableManager.Instance.IsDraging && (agentType == ItemAgentType.Backpack || agentType == ItemAgentType.Warehouse))
            {
                ItemAgent source = DragableManager.Instance.Current as ItemAgent;
                if (source) source.SwapInfoTo(this);
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
                else if (clickCount == 1 && touchTime < 0.5f)
                {
                    ItemWindowManager.Instance.SetItemAndOpenWindow(this);
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

    public void OnPointerEnter(PointerEventData eventData)//用于PC悬停
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

    public void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC悬停
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
        if (agentType == ItemAgentType.None) return;
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
#if UNITY_ANDROID
        if (parentScrollRect) parentScrollRect.OnEndDrag(eventData);//修复ScrollRect冲突
        if (agentType == ItemAgentType.None) return;
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && DragableManager.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
        {
            ItemAgent target = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ItemAgent>();
            if (target) SwapInfoTo(target);
            else if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<DiscardButton>() == BackpackManager.Instance.DiscardButton && agentType == ItemAgentType.Backpack)
            {
                BackpackManager.Instance.DiscardItem(MItemInfo);
                AmountManager.Instance.SetPosition(eventData.position);
            }
            else if (eventData.pointerCurrentRaycast.gameObject == ItemSelectionManager.Instance.PlacementArea && agentType == ItemAgentType.Backpack)
                ItemSelectionManager.Instance.Place(MItemInfo);
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
    Loot,//带拾取按钮
    Selection//带取出按钮
}