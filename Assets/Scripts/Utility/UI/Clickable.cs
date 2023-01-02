using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZetanStudio.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class Clickable : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IPointerExitHandler
    {
        public bool isEnabled = true;

        [Min(0.2f)]
        public float doubleClickInterval = 0.2f;
        [Min(0.3f)]
        public float longPressTime = 0.5f;
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onRightClick = new UnityEvent();
        public UnityEvent onDoubleClick = new UnityEvent();
        public UnityEvent onLongPress = new UnityEvent();

        private Timer pressTimer;

#if UNITY_ANDROID
        private int clickCount;
        private Timer clickTimer;
#endif

        private void Awake()
        {
            var graphic = GetComponent<Graphic>();
            if (!graphic) graphic = gameObject.AddComponent<EmptyGraphic>();
            graphic.raycastTarget = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isEnabled && isActiveAndEnabled)
                if (eventData.button == PointerEventData.InputButton.Left)
                {
#if UNITY_STANDALONE
                if (eventData.clickCount > 1) onDoubleClick?.Invoke();
                else onClick?.Invoke();
#elif UNITY_ANDROID
                    if (clickCount < 1)
                    {
                        if (clickTimer == null)
                            clickTimer = Timer.Create(() =>
                            {
                                clickCount = 0;
                            }, doubleClickInterval, true);
                        else clickTimer.Restart();
                    }
                    if (clickTimer != null && !clickTimer.IsStop && clickTimer.Time <= 0.2f) clickCount++;
                    if (clickCount > 1)
                    {
                        onDoubleClick?.Invoke();
                        clickCount = 0;
                        clickTimer?.Stop();
                    }
                    else if (clickCount == 1 && (pressTimer == null || pressTimer.Time < longPressTime)) onClick?.Invoke();
#endif
                }
                else if (eventData.button == PointerEventData.InputButton.Right) onRightClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isEnabled && isActiveAndEnabled && eventData.button == PointerEventData.InputButton.Left)
            {
                if (pressTimer == null)
                    pressTimer = Timer.Create(() =>
                    {
                        onLongPress?.Invoke();
                    }, longPressTime, true);
                else pressTimer.Restart();
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            pressTimer?.Stop();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            pressTimer?.Stop();
        }
    }
}