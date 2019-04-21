using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ItemAgent : MonoBehaviour, IDragable,
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
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

    [SerializeField]
    private List<Color> qualityColors = new List<Color>();

    public Color currentQualityColor { get { return qualityEdge.color; } }

    public Sprite DragableIcon
    {
        get
        {
            return icon.overrideSprite;
        }
    }

    [HideInInspector]
    public ItemInfo itemInfo;

    [HideInInspector]
    public ItemAgentType agentType;

    [HideInInspector]
    public int index;

    public bool IsEmpty { get { return itemInfo == null || !itemInfo.Item; } }

    public void Init(ItemInfo info, ItemAgentType agentType = ItemAgentType.None)
    {
        if (info == null) return;
        itemInfo = info;
        this.agentType = agentType;
        if (qualityColors.Count >= 5)
            qualityEdge.color = qualityColors[(int)info.Item.Quality];
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        if (itemInfo == null || !itemInfo.Item) return;
        if (itemInfo.Item.Icon) icon.overrideSprite = itemInfo.Item.Icon;
        amount.text = itemInfo.Amount > 1 ? itemInfo.Amount.ToString() : string.Empty;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (eventData.button == PointerEventData.InputButton.Left)
            if (DragableManager.Instance.IsDraging)
            {
                ItemAgent source = DragableManager.Instance.Current as ItemAgent;
                if (source)
                {
                    source.SwapInfoTo(this);
                }
            }
            else
            {
                if (!IsEmpty && agentType != ItemAgentType.None)
                {
                    icon.color = Color.grey;
                    DragableManager.Instance.GetDragable(this, FinishDrag);
                    ItemWindowManager.Instance.PauseShowing(true);
                }
            }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
#elif UNITY_ANDROID
        if (!IsEmpty)
        {
            ItemWindowManager.Instance.OpenItemWindow(this);
        }
#endif
    }

    public void OnRightClick()
    {
        if (!DragableManager.Instance.IsDraging && agentType != ItemAgentType.None)
            OnUse();
    }

    public void Clear(bool keepType = false, bool recycle = false)
    {
        icon.overrideSprite = null;
        amount.text = string.Empty;
        itemInfo = null;
        index = -1;
        qualityEdge.color = Color.white;
        if (!keepType || recycle) agentType = ItemAgentType.None;
        if (recycle) ObjectPool.Instance.Put(gameObject);
    }

    public void OnUse()
    {
        if (!IsEmpty) itemInfo.UseItem();
    }

    public void SwapInfoTo(ItemAgent target)
    {
        if (target != this && agentType != ItemAgentType.None)
        {
            if (target.agentType == agentType)
                if (target.IsEmpty)
                {
                    target.Init(itemInfo, agentType);
                    Clear(true);
                }
                else
                {
                    ItemInfo targetInfo = target.itemInfo;
                    target.Init(itemInfo, agentType);
                    Init(targetInfo, target.agentType);
                }
        }
        FinishDrag();
    }

    public void OnDiscard()
    {
        BackpackManager.Instance.DiscardItem(itemInfo);
    }

    private float touchTime;
    bool isPress;

    private void Update()
    {
#if UNITY_ANDROID
        if (isPress)
        {
            touchTime += Time.deltaTime;
            if (touchTime >= 0.5f)
            {
                isPress = false;
                OnLongPress();
            }
        }
#endif
    }

    #region 拖拽相关
    public void OnLongPress()//安卓长按
    {
        if (agentType == ItemAgentType.None) return;
        if (!IsEmpty)
        {
            icon.color = Color.grey;
            DragableManager.Instance.GetDragable(this, FinishDrag);
            ItemWindowManager.Instance.PauseShowing(true);
        }
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
        isPress = false;
#endif
    }

    public void OnBeginDrag(PointerEventData eventData)//修复ScrollRect冲突
    {
        BackpackManager.Instance.GridRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)//用于安卓拖拽
    {
#if UNITY_STANDALONE
        BackpackManager.Instance.GridRect.OnDrag(eventData);
        if (agentType == ItemAgentType.None) return;
#elif UNITY_ANDROID
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && agentType != ItemAgentType.None)
        {
            DragableManager.Instance.MoveIcon();
        }
        if (!DragableManager.Instance.IsDraging) BackpackManager.Instance.GridRect.OnDrag(eventData);
#endif
    }

    public void OnEndDrag(PointerEventData eventData)//用于安卓拖拽
    {
        BackpackManager.Instance.GridRect.OnEndDrag(eventData);
        if (agentType == ItemAgentType.None) return;
#if UNITY_ANDROID
        if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left && DragableManager.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
        {
            ItemAgent target = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ItemAgent>();
            if (target) SwapInfoTo(target);
            else if (eventData.pointerCurrentRaycast.gameObject == BackpackManager.Instance.DiscardArea)
            {
                BackpackManager.Instance.LoseItem(itemInfo);
            }
        }
#endif
        FinishDrag();
    }

    public void FinishDrag()
    {
        icon.color = Color.white;
        DragableManager.Instance.ResetIcon();
        ItemWindowManager.Instance.PauseShowing(false);
    }
    #endregion
}

public enum ItemAgentType
{
    None,//只带关闭按钮
    Backpack,//带使用按钮、丢弃按钮、存储按钮
    Warehouse,//带取出按钮
    Process,//带制作按钮
}