using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[DisallowMultipleComponent]
public class ItemSlot : ItemSlotBase, IDragAble,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite DragAbleIcon => icon.overrideSprite;

    [HideInInspector]
    public int indexInGrid;

    private ScrollRect parentScrollRect;

    private Action<ItemSlot> onRightClick;
    private Func<ItemSlot, ButtonWithTextData[]> getButtonsCallback;
    private bool dragable;
    private Action<GameObject, ItemSlot> onEndDrag;

    #region 操作相关
    public void Init(int index, ScrollRect parentScrollRect, Func<ItemSlot, ButtonWithTextData[]> getButtons, Action<ItemSlot> rightClick = null, Action<GameObject, ItemSlot> onEndDrag = null)
    {
        Empty();
        indexInGrid = index;
        this.parentScrollRect = parentScrollRect;
        getButtonsCallback = getButtons;
        onRightClick = rightClick;
        this.onEndDrag = onEndDrag;
        dragable = onEndDrag != null;
    }
    public void Init(Func<ItemSlot, ButtonWithTextData[]> getButtons, Action<ItemSlot> rightClick = null, Action<GameObject, ItemSlot> onEndDrag = null)
    {
        Init(-1, null, getButtons, rightClick, onEndDrag);
    }

    public override void SetItem(ItemInfo info)
    {
        if (info == null) return;
        base.SetItem(info);
        info.indexInGrid = indexInGrid;
    }

    public void OnRightClick()
    {
        if (!DragableManager.Instance.IsDraging)
        {
            onRightClick?.Invoke(this);
            ItemWindowManager.Instance.CloseWindow();
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
    public void SwapInfoTo(ItemSlot target)
    {
        if (target != this)
        {
            if (target.IsEmpty)
            {
                target.SetItem(MItemInfo);
                target.Mark(mark.activeSelf);
                if (IsDark) target.Dark();
                else target.Light();
                Empty();
            }
            else
            {
                bool targetMark = target.mark.activeSelf;
                bool targetDark = target.IsDark;

                ItemInfo targetInfo = target.MItemInfo;
                target.SetItem(MItemInfo);

                target.Mark(mark.activeSelf);

                SetItem(targetInfo);
                Mark(targetMark);

                if (IsDark) target.Dark();
                else target.Light();

                if (targetDark) Dark();
                else Light();
            }
            //else if (target.agentType == ItemAgentType.Backpack && agentType == ItemAgentType.Selling)
            //    ShopManager.Instance.SellItem(ShopManager.Instance.GetMerchandiseAgentByItem(MItemInfo).merchandiseInfo);
        }
        FinishDrag();
    }

    private void BeginDrag()
    {
        if (ItemSelectionManager.Instance.IsUIOpen && IsDark || !dragable)
            return;
        DragableManager.Instance.StartDrag(this, FinishDrag, icon.rectTransform.rect.width, icon.rectTransform.rect.height);
        ItemWindowManager.Instance.CloseWindow();
        Select();
    }
    public void FinishDrag()
    {
        if (!DragableManager.Instance.IsDraging) return;
        DeSelect();
        DragableManager.Instance.ResetIcon();
        ItemWindowManager.Instance.CloseWindow();
    }
    #endregion

    #region 事件相关
    public override void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if (eventData.button == PointerEventData.InputButton.Left && !IsEmpty)
            BeginDrag();
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
                    ButtonWithTextData[] buttonDatas = getButtonsCallback?.Invoke(this);
                    foreach (var data in buttonDatas)
                    {
                        data.callback += ItemWindowManager.Instance.CloseWindow;
                    }
                    ItemWindowManager.Instance.ShowItemInfo(this, buttonDatas);
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
        if (DragableManager.Instance.IsDraging && dragable && (DragableManager.Instance.Current as ItemSlot) == this)
            OnEndDrag(eventData);
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)//用于PC悬停
    {
#if UNITY_STANDALONE
        NewItemWindowManager.Instance.ShowItemInfo(this);
        if (!DragableManager.Instance.IsDraging)
            Select();
#endif
    }

    public void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC悬停
    {
#if UNITY_STANDALONE
        if (!NewItemWindowManager.Instance.IsHeld) NewItemWindowManager.Instance.CloseWindow();
        if (!DragableManager.Instance.IsDraging)
        {
            DeSelect();
        }
#elif UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect) parentScrollRect.OnBeginDrag(eventData);//修复ScrollRect冲突
#if UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
        isClick = false;
#endif
    }

    public void OnDrag(PointerEventData eventData)//用于安卓拖拽
    {
        if (!DragableManager.Instance.IsDraging && parentScrollRect) parentScrollRect.OnDrag(eventData);//修复ScrollRect冲突
#if UNITY_ANDROID
        if (!IsEmpty && dragable && eventData.button == PointerEventData.InputButton.Left && (DragableManager.Instance.Current as ItemSlot) == this)
        {
            DragableManager.Instance.MoveIcon();
        }
#endif
    }

    public void OnEndDrag(PointerEventData eventData)//用于安卓拖拽
    {
        if (parentScrollRect) parentScrollRect.OnEndDrag(eventData);//修复ScrollRect冲突
#if UNITY_ANDROID
        if (!IsEmpty && dragable && eventData.button == PointerEventData.InputButton.Left && DragableManager.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
        {
            onEndDrag?.Invoke(eventData.pointerCurrentRaycast.gameObject, this);
        }
#endif
        FinishDrag();
    }
    #endregion
}