using UnityEngine.UI;
using ZetanStudio.UI;

namespace ZetanStudio.FarmingSystem.UI
{
    public class CropAgent : ListItem<CropAgent, Crop>
    {
        public Text nameText;

        public Text totalDays;
        public Image dryIcon;
        public Image pestIcon;
        public Image matureIcon;
        public Button destroyButton;

        private CropStage stageBef;

        private FieldWindow window;

        public void SetWindow(FieldWindow window)
        {
            this.window = window;
        }

        public void OnStageChanged(CropStage stage)
        {
            if (stage != stageBef)
            {
                stageBef = stage;
                Refresh();
            }
        }

        public void Clear(bool recycle = false)
        {
            Data = null;
            nameText.text = string.Empty;
            totalDays.text = string.Empty;
            Utility.SetActive(dryIcon.gameObject, false);
            Utility.SetActive(pestIcon.gameObject, false);
            Utility.SetActive(matureIcon.gameObject, false);
            if (recycle) ObjectPool.Put(gameObject);
        }

        public void DestroyCrop()
        {
            ConfirmWindow.StartConfirm("销毁作物不会有任何产物，确定销毁吗？", delegate
             {
                 window.Remove(Data);
             });
        }

        public override void Refresh(Crop data)
        {
            base.Refresh(data);
            Data.UI = this;
            Data.Data.OnStageChanged -= OnStageChanged;
            Data.Data.OnStageChanged += OnStageChanged;
        }

        public override void Clear()
        {
            if (Data)
            {
                Data.UI = null;
                Data.Data.OnStageChanged -= OnStageChanged;
            }
            base.Clear();
        }

        public override void Refresh()
        {
            totalDays.text = Data.Data.growthDays.ToString();
            nameText.text = Data.Data.Info.Name;
            Utility.SetActive(dryIcon.gameObject, Data.Dry);
            Utility.SetActive(pestIcon.gameObject, Data.Pest);
            Utility.SetActive(matureIcon.gameObject, Data.Data.HarvestAble);
        }
    }
}