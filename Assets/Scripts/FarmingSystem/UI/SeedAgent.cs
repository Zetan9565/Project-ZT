using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.UI;

namespace ZetanStudio.FarmingSystem.UI
{
    public class SeedAgent : ListItem<SeedAgent, Item>,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Text nameText;

        private ScrollRect parentRect;

        private PlantWindow window;

        public void SetWindow(PlantWindow window)
        {
            this.window = window;
        }

        protected override void OnInit()
        {
            parentRect = (View as SeedList).ScrollRect;
        }

        public override void Clear()
        {
            base.Clear();
            nameText.text = string.Empty;
        }

        public void TryBuild()
        {
            window.CreatPreview(Data.GetModule<SeedModule>().Crop);
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
            {
                if (eventData.pointerCurrentRaycast.gameObject && eventData.pointerCurrentRaycast.gameObject == window.CancelArea)
                    window.FinishPreview();
                else
                    window.Plant();
            }
#endif
        }

        public override void Refresh()
        {
            nameText.text = ItemFactory.GetColorName(Data);
        }
    }
}