using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ItemSlot : ItemSlotBase, IDragAble,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public Sprite DragAbleIcon => icon.overrideSprite;

    [HideInInspector]
    public int indexInGrid;

    private ScrollRect parentScrollRect;

    private Action<ItemSlot> rightClickAction;
    private Func<ItemSlot, ButtonWithTextData[]> buttonGetter;
    private bool dragable;
    private Action<GameObject, ItemSlot> dragPutAction;

    #region 操作相关
    public void SetCallbacks(Func<ItemSlot, ButtonWithTextData[]> buttonGetter, Action<ItemSlot> rightClickAction = null, Action<GameObject, ItemSlot> dragPutAction = null)
    {
        this.buttonGetter = buttonGetter;
        this.rightClickAction = rightClickAction;
        this.dragPutAction = dragPutAction;
        dragable = dragPutAction != null;
    }
    public void SetScrollRect(ScrollRect scrollRect)
    {
        parentScrollRect = scrollRect;
    }

    public void OnRightClick()
    {
        if (!DragableManager.Instance.IsDraging)
        {
            rightClickAction?.Invoke(this);
            NewWindowsManager.CloseWindow<ItemWindow>();
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
    public void Swap(ItemSlot target)
    {
        if (target != this)
        {
            if (target.IsEmpty)
            {
                Data.Swap(target.Data);
                target.Refresh();
                target.Mark(mark.activeSelf);
            }
            else
            {
                bool targetMark = target.mark.activeSelf;

                Data.Swap(target.Data);
                target.Refresh();
                target.Mark(mark.activeSelf);

                Refresh();
                Mark(targetMark);
            }
        }
        FinishDrag();
    }

    private void BeginDrag()
    {
        if (NewWindowsManager.IsWindowOpen<ItemSelectionWindow>() && IsDark || !dragable)
            return;
        DragableManager.Instance.BeginDrag(this, OnEndDrag, icon.rectTransform.rect.width, icon.rectTransform.rect.height);
        NewWindowsManager.CloseWindow<ItemWindow>();
        if (parentScrollRect) parentScrollRect.enabled = false;
        Highlight(true);
    }
    public void FinishDrag()
    {
        if (!DragableManager.Instance.IsDraging) return;
        Highlight(false);
        NewWindowsManager.CloseWindow<ItemWindow>();
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
                    ButtonWithTextData[] buttonDatas = buttonGetter?.Invoke(this);
                    if (buttonDatas != null)
                        foreach (var data in buttonDatas)
                        {
                            data.callback += () => NewWindowsManager.CloseWindow<ItemWindow>();
                        }
                    NewWindowsManager.OpenWindow<ItemWindow>(this, buttonDatas);
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
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)//用于PC悬停
    {
#if UNITY_STANDALONE
        NewWindowsManager.OpenWindow<ItemWindow>(this);
        if (!DragableManager.Instance.IsDraging)
            Select();
#endif
    }

    public void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC悬停
    {
#if UNITY_STANDALONE
        NewWindowsManager.CloseWindow<ItemWindow>();
        if (!DragableManager.Instance.IsDraging)
        {
            DeSelect();
        }
#elif UNITY_ANDROID
        if (pressCoroutine != null) StopCoroutine(pressCoroutine);
#endif
    }

    public void OnEndDrag(PointerEventData eventData)//用于安卓拖拽
    {
        if (parentScrollRect) parentScrollRect.enabled = true;//修复ScrollRect冲突
#if UNITY_ANDROID
        if (!IsEmpty && dragable && eventData.button == PointerEventData.InputButton.Left && DragableManager.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
        {
            dragPutAction?.Invoke(eventData.pointerCurrentRaycast.gameObject, this);
        }
#endif
        FinishDrag();
    }
    #endregion
}