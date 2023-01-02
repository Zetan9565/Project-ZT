using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.FarmingSystem.UI
{
    using ItemSystem;
    using ItemSystem.Module;
    using ItemSystem.UI;
    using ZetanStudio;
    using ZetanStudio.FarmingSystem;
    using ZetanStudio.InventorySystem;
    using ZetanStudio.PlayerSystem;

    public class PlantWindow : Window, IHideable
    {
        [SerializeField]
        private Dropdown pageSelector;

        [SerializeField]
        private SeedList seedList;

        [SerializeField]
        private CanvasGroup descriptionWindow;
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private ItemSlot icon;
        [SerializeField]
        private Text description;

        [SerializeField]
        private InputField searchInput;
        [SerializeField]
        private Button searchButton;

        [SerializeField]
        private GameObject cancelArea;

        public Field CurrentField { get; private set; }

        private CropInformation currentInfo;

        public bool IsTyping => searchInput.isFocused;

        private CropPreview preview;

        public GameObject CancelArea { get { return cancelArea; } }

        public bool IsPreviewing { get; private set; }

        public bool PlantAble
        {
            get
            {
                if (preview && preview.ColliderCount < 1) return true;
                return false;
            }
        }

        public bool IsHidden { get; private set; }

        protected override void OnAwake()
        {
            pageSelector.onValueChanged.AddListener(SetPage);
            seedList.SetItemModifier(x => x.SetWindow(this));
            seedList.SetSelectCallback(OnSeeSelected);
        }

        private void OnSeeSelected(SeedAgent element)
        {
            if (element) ShowDescription(element.Data);
            else HideDescription();
        }

        public void CreatPreview(CropInformation info)
        {
            if (!PlayerManager.Instance.CheckIsNormalWithAlert())
                return;
            if (info == null) return;
            HideDescription();
            HideBuiltList();
            currentInfo = info;
            preview = Instantiate(currentInfo.PreviewPrefab);
            WindowsManager.HideAll(true);
            IsPreviewing = true;
#if UNITY_ANDROID
            Utility.SetActive(CancelArea, true);
#endif
            ShowAndMovePreview();
        }

        private Vector2 GetMovePosition()
        {
            Vector3 position;
            if (Utility.IsMouseInsideScreen())
                position = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
            else
                position = preview.transform.position;

            Bounds fieldB = CurrentField.Range.bounds;
            Bounds cropB = preview.collider2D.bounds;
            position = new Vector2(Mathf.Clamp(position.x, fieldB.center.x - fieldB.extents.x + cropB.extents.x, fieldB.center.x + fieldB.extents.x - cropB.extents.x),
                Mathf.Clamp(position.y, fieldB.center.y - fieldB.extents.y + cropB.extents.y, fieldB.center.y + fieldB.extents.y - cropB.extents.y));

            return position;
        }

        public void ShowAndMovePreview()
        {
            if (!preview) return;
            preview.transform.position = GetMovePosition();// ZetanUtility.PositionToGrid(GetMovePosition(), gridSize, preview.CenterOffset);
            if (preview.ColliderCount > 0)
            {
                if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.red;
            }
            else
            {
                if (preview.SpriteRenderer) preview.SpriteRenderer.color = Color.white;
            }
#if UNITY_STANDALONE
        if (Utility.IsMouseInsideScreen())
        {
            if (InputManager.GetMouseButtonDown(0))
            {
                Plant();
            }
            if (InputManager.GetMouseButtonDown(1))
            {
                FinishPreview();
            }
        }
#endif
        }

        public void FinishPreview()
        {
            if (preview) Destroy(preview.gameObject);
            preview = null;
            currentInfo = null;
            WindowsManager.HideAll(false);
            IsPreviewing = false;
#if UNITY_ANDROID
            Utility.SetActive(CancelArea, false);
#endif
        }

        private void HideBuiltList()
        {

        }

        public void Plant()
        {
            if (PlantAble)
            {
                CurrentField.PlantCrop(currentInfo, preview.Position);
            }
            else
                MessageManager.Instance.New("存在障碍物");
            FinishPreview();
        }

        public void Search()
        {
            var name = searchInput.text;
            if (string.IsNullOrEmpty(name)) ShowAll();
            else seedList.ForEach(x =>
            {
                if (!ItemFactory.GetColorName(x.Data).Contains(name))
                    Utility.SetActive(x, false);
            });
        }
        private int currentPage;
        public void SetPage(int index)
        {
            if (index < 0) return;
            currentPage = index;
            switch (index)
            {
                case 1: ShowVegetables(); break;
                default: ShowAll(); break;
            }
            HideDescription();
        }

        private void ShowAll()
        {
            seedList.ForEach(x => Utility.SetActive(x, true));
        }

        private void ShowVegetables()
        {

        }

        private void ShowFruit()
        {

        }

        private void ShowTree()
        {

        }

        protected override bool OnOpen(params object[] args)
        {
            if (args.Length < 1) return false;
            CurrentField = args[0] as Field;
            if (!CurrentField) return false;
            BackpackManager.Instance.Inventory.TryGetDatas(x => x.Model.GetModule<SeedModule>(), out var seeds);
            seedList.Refresh(seeds.Select(x => x.source.Model));
            HideDescription();
            pageSelector.SetValueWithoutNotify(0);
            SetPage(0);
            return true;
        }

        protected override bool OnClose(params object[] args)
        {
            if (IsPreviewing) FinishPreview();
            searchInput.text = string.Empty;
            HideDescription();
            return true;
        }

        public void ShowDescription(Item seed)
        {
            if (!seed)
            {
                HideDescription();
                return;
            }
            var Crop = seed.GetModule<SeedModule>().Crop;
            descriptionWindow.alpha = 1;
            nameText.text = Crop.Name;
            int amount = BackpackManager.Instance.GetAmount(seed);
            icon.SetItem(seed, amount > 0 ? amount.ToString() : null);
            StringBuilder str = new StringBuilder("占用田地空间：");
            str.Append(Crop.Size);
            str.Append("\n");
            str.Append(CropInformation.CropSeasonString(Crop.PlantSeason));
            str.Append("\n");
            str.Append("生长阶段：");
            str.Append("\n");
            for (int i = 0; i < Crop.Stages.Count; i++)
            {
                CropStage stage = Crop.Stages[i];
                str.Append(Utility.ColorText(CropStage.CropStageName(stage.Stage), Color.yellow));
                str.Append("持续");
                str.Append(Utility.ColorText(stage.LastingDays.ToString(), Color.green));
                str.Append("天");
                if (stage.HarvestAble)
                {
                    if (stage.RepeatTimes > 0)
                    {
                        str.Append("，可收割");
                        str.Append(Utility.ColorText(stage.RepeatTimes.ToString(), Color.green));
                        str.Append("次");
                    }
                    else if (stage.RepeatTimes < 0)
                    {
                        str.Append("，可无限收割");
                    }
                }
                if (i != Crop.Stages.Count - 1) str.Append("\n");
            }
            description.text = str.ToString();
        }

        public void HideDescription()
        {
            icon.Vacate();
            descriptionWindow.alpha = 0;
            nameText.text = string.Empty;
            description.text = string.Empty;
        }

        public void Hide(bool hide, params object[] args)
        {
            if (!IsOpen) return;
            IHideable.HideHelper(content, hide);
            IsHidden = hide;
        }
    }
}