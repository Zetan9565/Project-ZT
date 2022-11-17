using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZetanStudio.InventorySystem;
using ZetanStudio.UI;

namespace ZetanStudio.StructureSystem.UI
{
    [DisallowMultipleComponent]
    public class StructureInfoAgent : ListItem<StructureInfoAgent, StructureInformation>,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private Text nameText;

        private ScrollRect parentRect;

        private StructureWindow window;

        public void Init(StructureInformation structureInfo, ScrollRect parentRect = null)
        {
            Data = structureInfo;
            nameText.text = structureInfo.Name;
            this.parentRect = parentRect;
        }

        public void SetWindow(StructureWindow window)
        {
            this.window = window;
        }

        public override void Refresh()
        {
            if (!Data) return;
            nameText.text = Data.Name;
        }

        protected override void OnInit()
        {
            parentRect = (View as StructureInfoList).ScrollRect;
        }

        public void Clear(bool recycle = false)
        {
            nameText.text = string.Empty;
            Data = null;
            window = null;
            if (recycle) ObjectPool.Put(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_STANDALONE
        if(eventData.button == PointerEventData.InputButton.Right)
            TryBuild();
        else if(eventData.button == PointerEventData.InputButton.Left)
            window.ShowBuiltList(Data);
#elif UNITY_ANDROID
            if (touchTime < 0.5f && window)
            {
                window.ShowDescription(Data);
                window.ShowBuiltList(Data);
            }
#endif
        }

        private float touchTime;
        private bool isPress;

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

        void TryBuild()
        {
            if (!BackpackManager.Instance.IsMaterialsEnough(Data.Materials))
            {
                MessageManager.Instance.New("耗材不足");
                return;
            }
            else if (window) window.CreatPreview(Data);
        }

        public void OnLongPress()
        {
#if UNITY_ANDROID
            TryBuild();
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
#if UNITY_ANDROID
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                touchTime = 0;
                isPress = true;
            }
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if UNITY_ANDROID
            isPress = false;
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
#if UNITY_STANDALONE
        window.ShowDescription(Data);
#endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if UNITY_STANDALONE
        window.HideDescription();
#endif
#if UNITY_ANDROID
            isPress = false;
#endif
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
#if UNITY_ANDROID
            if (parentRect) parentRect.OnBeginDrag(eventData);
#endif
        }

        public void OnDrag(PointerEventData eventData)
        {
#if UNITY_ANDROID
            if (window.IsPreviewing && eventData.button == PointerEventData.InputButton.Left)
                window.ShowAndMovePreview();
            else if (parentRect) parentRect.OnDrag(eventData);
#endif
        }

        public void OnEndDrag(PointerEventData eventData)
        {
#if UNITY_ANDROID
            if (parentRect) parentRect.OnEndDrag(eventData);
            if (window.IsPreviewing && eventData.button == PointerEventData.InputButton.Left)
                window.DoPlace();
#endif
        }
    }
}