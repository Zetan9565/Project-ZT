using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingAgent : ListItem<BuildingAgent, BuildingData>, IPointerClickHandler
{
    [SerializeField]
    private Text buildingPosition;

    [SerializeField]
    private Text buildingStates;

    [SerializeField]
    private Button destoryButton;

    private BuildingWindow window;

    private void Awake()
    {
        destoryButton.onClick.AddListener(AskDestroy);
    }

    public void SetWindow(BuildingWindow window)
    {
        this.window = window;
    }

    public override void OnClear()
    {
        base.OnClear();
        if (Data) Data.buildingAgent = null;
        Data = null;
        window = null;
        buildingPosition.text = string.Empty;
    }

    private void Update()
    {
        if (Data && Data.IsBuilding)
        {
            buildingStates.text = Data.IsBuilt ? "已建成" : !Data.IsBuilding ? $"等待中[剩余{Data.leftBuildTime:F2}s]" : $"建造中[剩余{Data.leftBuildTime:F2}s]";
        }
    }

    public void UpdateUI()
    {
        if (Data)
        {
            destoryButton.interactable = Data.IsBuilt;
            buildingStates.text = Data.IsBuilt ? "已建成" : !Data.IsBuilding ? $"等待中[剩余{Data.leftBuildTime:F2}s]" : $"建造中[剩余{Data.leftBuildTime:F2}s]";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //TODO 移动视野至相应建筑
        window.LocateBuilding(Data);
    }

    public void AskDestroy()
    {
        if (Data) BuildingManager.Instance.DestroyBuilding(Data);
    }

    public override void Refresh()
    {
        Data.buildingAgent = this;
        destoryButton.interactable = Data.IsBuilt;
        buildingPosition.text = "位置" + ((Vector2)Data.position).ToString();
        buildingStates.text = Data.IsBuilt ? "已建成" : !Data.IsBuilding ? $"等待中[剩余{Data.leftBuildTime:F2}s]" : $"建造中[剩余{Data.leftBuildTime:F2}s]";
    }

    protected override void OnInit()
    {

    }
}
