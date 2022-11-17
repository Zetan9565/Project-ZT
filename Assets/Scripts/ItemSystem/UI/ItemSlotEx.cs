using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    using ZetanStudio;
    using ZetanStudio.UI;

    [DisallowMultipleComponent]
    public class ItemSlotEx : ItemSlot, IDraggable, IPointerDownHandler, IPointerUpHandler
    {
        Sprite IDraggable.DraggableIcon => icon.overrideSprite;

        [HideInInspector]
        public int indexInGrid;

        private ScrollRect parentScrollRect;

        private Action<ItemSlotEx> rightClickAction;
        private Func<ItemSlotEx, IEnumerable<ButtonWithTextData>> buttonGetter;
        private bool dragable;
        private Action<GameObject, ItemSlotEx> dragPutAction;

        #region 操作相关
        public void SetCallbacks(Func<ItemSlotEx, IEnumerable<ButtonWithTextData>> buttonGetter, Action<ItemSlotEx> rightClickAction = null, Action<GameObject, ItemSlotEx> dragPutAction = null)
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
                WindowsManager.CloseWindow<ItemWindow>();
            }
        }

#if UNITY_ANDROID
        private int clickCount;
        private Timer clickTimer;
#endif

        private Timer pressTimer;

        /// <summary>
        /// 安卓长按时所作所为
        /// </summary>
        public void OnLongPress()
        {
            if (!IsEmpty)
            {
                BeginDrag();
#if UNITY_ANDROID
                clickTimer?.Stop();
                clickCount = 0;
#endif
            }
        }

        /// <summary>
        /// 交换单元格内容
        /// </summary>
        /// <param name="target"></param>
        public void Swap(ItemSlotEx target)
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
            if (WindowsManager.IsWindowOpen<ItemSelectionWindow>() && IsDark || !dragable)
                return;
            DragableManager.Instance.BeginDrag(this, OnEndDrag, icon.rectTransform.rect.width, icon.rectTransform.rect.height);
            WindowsManager.CloseWindow<ItemWindow>();
            if (parentScrollRect) parentScrollRect.enabled = false;
            Highlight(true);
        }
        private void OnEndDrag(PointerEventData eventData)//用于拖拽
        {
            if (parentScrollRect) parentScrollRect.enabled = true;//修复ScrollRect冲突
            if (!IsEmpty && dragable && eventData.button == PointerEventData.InputButton.Left && DragableManager.Instance.IsDraging && eventData.pointerCurrentRaycast.gameObject)
                dragPutAction?.Invoke(eventData.pointerCurrentRaycast.gameObject, this);
            FinishDrag();
        }
        public void FinishDrag()
        {
            if (!DragableManager.Instance.IsDraging) return;
            Highlight(false);
            WindowsManager.CloseWindow<ItemWindow>();
        }
        #endregion

        #region 事件相关
        public override void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_STANDALONE
        if (eventData.button == PointerEventData.InputButton.Left && !IsEmpty)
        {
            var datas = buttonGetter?.Invoke(this);
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    data.callback += () => WindowsManager.CloseWindow<FloatButtonPanel>();
                }
                WindowsManager.OpenWindow<FloatButtonPanel>(datas, eventData.position);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right && !IsEmpty) OnRightClick();
#elif UNITY_ANDROID
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (clickCount < 1)
                {
                    if (clickTimer == null)
                        clickTimer = Timer.Create(() =>
                        {
                            clickCount = 0;
                        }, 0.2f, true);
                    else clickTimer.Restart();
                }
                if (clickTimer != null && !clickTimer.IsStop && clickTimer.Time <= 0.2f) clickCount++;
                if (!IsEmpty)
                {
                    if (clickCount > 1)
                    {
                        OnRightClick();
                        clickCount = 0;
                        clickTimer?.Stop();
                    }
                    else if (clickCount == 1 && (pressTimer == null || pressTimer.Time < 0.5f))
                    {
                        var buttonDatas = buttonGetter?.Invoke(this);
                        if (buttonDatas != null)
                            foreach (var data in buttonDatas)
                            {
                                data.callback += () => WindowsManager.CloseWindow<ItemWindow>();
                            }
                        WindowsManager.OpenWindow<ItemWindow>(this, buttonDatas);
                    }
                }
            }
#endif
        }

        public void OnPointerDown(PointerEventData eventData)//用于安卓拖拽
        {
            if (!IsEmpty && eventData.button == PointerEventData.InputButton.Left)
            {
                if (pressTimer == null) pressTimer = Timer.Create(OnLongPress, 0.5f, true);
                else pressTimer.Restart();
            }
        }

        public void OnPointerUp(PointerEventData eventData)//用于安卓拖拽
        {
            pressTimer?.Stop();
        }

        public override void OnPointerExit(PointerEventData eventData)//用于安卓拖拽、PC悬停
        {
            base.OnPointerExit(eventData);
            pressTimer?.Stop();
        }
        #endregion
    }
}