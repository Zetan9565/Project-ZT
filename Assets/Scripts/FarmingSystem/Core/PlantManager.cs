using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[AddComponentMenu("ZetanStudio/管理器/种植管理器")]
public class PlantManager : WindowHandler<PlantUI, PlantManager>
{
    public List<SeedAgent> SeedAgents { get; private set; } = new List<SeedAgent>();

    public Field CurrentField { get; private set; }

    public CropInformation currentInfo;

    public bool IsInputFocused => UI ? IsUIOpen && UI.searchInput.isFocused : false;

    [HideInInspector]
    public CropPreview preview;

    public GameObject CancelArea { get { return UI.cancelArea; } }

    public bool IsPreviewing { get; private set; }

    public bool PlantAble
    {
        get
        {
            if (preview && preview.ColliderCount < 1) return true;
            return false;
        }
    }

    public void CreatPreview(CropInformation info)
    {
        if (info == null) return;
        HideDescription();
        HideBuiltList();
        currentInfo = info;
        preview = Instantiate(currentInfo.PreviewPrefab);
        WindowsManager.Instance.PauseAll(true);
        IsPreviewing = true;
#if UNITY_ANDROID
        ZetanUtility.SetActive(CancelArea, true);
        UIManager.Instance.EnableJoyStick(false);
#endif
        ShowAndMovePreview();
    }


    private Vector2 GetMovePosition()
    {
        Vector3 position;
        if (ZetanUtility.IsMouseInsideScreen)
            position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
        if (ZetanUtility.IsMouseInsideScreen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Plant();
            }
            if (Input.GetMouseButtonDown(1))
            {
                FinishPreview();
            }
        }
    }

    public void FinishPreview()
    {
        if (preview) Destroy(preview.gameObject);
        preview = null;
        currentInfo = null;
        WindowsManager.Instance.PauseAll(false);
        IsPreviewing = false;
#if UNITY_ANDROID
        ZetanUtility.SetActive(CancelArea, false);
#endif
        UIManager.Instance.EnableJoyStick(true);
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

    public void Init(Field field)
    {
        if (!UI || !UI.gameObject) return;
        CurrentField = field;
        var seeds = BackpackManager.Instance.Seeds.Select(x => x.item).ToList();
        while (seeds.Count > SeedAgents.Count)
        {
            SeedAgent sa = ObjectPool.Get(UI.seedCellPrefab, UI.seedCellsParent).GetComponent<SeedAgent>();
            SeedAgents.Add(sa);
        }
        while (seeds.Count < SeedAgents.Count)
        {
            SeedAgents[SeedAgents.Count - 1].Clear(true);
            SeedAgents.RemoveAt(SeedAgents.Count - 1);
        }
        for (int i = 0; i < seeds.Count; i++)
            SeedAgents[i].Init(seeds[i] as SeedItem);
        HideDescription();
    }

    public void Search()
    {
        if (!UI || !UI.gameObject) return;
        var name = UI.searchInput.text;
        if (string.IsNullOrEmpty(name)) ShowAll();
        else foreach (var sa in SeedAgents)
                if (!sa.name.Contains(name))
                    ZetanUtility.SetActive(sa.gameObject, false);
    }

    private int currentPage;
    public void SetPage(int index)
    {
        if (!UI || !UI.gameObject || index < 0) return;
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
        foreach (var sa in SeedAgents)
            ZetanUtility.SetActive(sa.gameObject, true);
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

    public override void OpenWindow()
    {
        if (!CurrentField) return;
        base.OpenWindow();
        if (IsUIOpen) return;
        UIManager.Instance.EnableJoyStick(false);
        UI.pageSelector.SetValueWithoutNotify(0);
        SetPage(0);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (!IsUIOpen) return;
        UI.searchInput.text = string.Empty;
        HideDescription();
    }

    public void ShowDescription(SeedItem seed)
    {
        if (!UI || !UI.gameObject || !UI.descriptionWindow) return;
        if (!seed)
        {
            HideDescription();
            return;
        }

        UI.descriptionWindow.alpha = 1;
        UI.nameText.text = seed.Crop.name;
        UI.amount.text = BackpackManager.Instance.GetItemAmount(seed).ToString();
        StringBuilder str = new StringBuilder("占用田地空间：");
        str.Append(seed.Crop.Size);
        str.Append("\n");
        str.Append(CropInformation.CropSeasonString(seed.Crop.PlantSeason));
        str.Append("\n");
        str.Append("生长阶段：");
        str.Append("\n");
        for (int i = 0; i < seed.Crop.Stages.Count; i++)
        {
            CropStage stage = seed.Crop.Stages[i];
            str.Append(ZetanUtility.ColorText(CropStage.CropStageName(stage.Stage), Color.yellow));
            str.Append("持续");
            str.Append(ZetanUtility.ColorText(stage.LastingDays.ToString(), Color.green));
            str.Append("天");
            if (stage.HarvestAble)
            {
                if (stage.RepeatTimes > 0)
                {
                    str.Append("，可收割");
                    str.Append(ZetanUtility.ColorText(stage.RepeatTimes.ToString(), Color.green));
                    str.Append("次");
                }
                else if (stage.RepeatTimes < 0)
                {
                    str.Append("，可无限收割");
                }
            }
            if (i != seed.Crop.Stages.Count - 1) str.Append("\n");
        }
        UI.description.text = str.ToString();
    }

    public void HideDescription()
    {
        if (!UI || !UI.gameObject || !UI.descriptionWindow) return;

        UI.descriptionWindow.alpha = 0;
        UI.nameText.text = string.Empty;
        UI.amount.text = string.Empty;
        UI.description.text = string.Empty;
    }
}
