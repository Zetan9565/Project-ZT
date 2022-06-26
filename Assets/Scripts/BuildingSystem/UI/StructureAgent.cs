using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZetanStudio.StructureSystem;

public class StructureAgent : ListItem<StructureAgent, StructureData>, IPointerClickHandler
{
    [SerializeField]
    private Text structurePosition;

    [SerializeField]
    private Text structureStates;

    [SerializeField]
    private Button destoryButton;

    private StructureWindow window;

    private void Awake()
    {
        destoryButton.onClick.AddListener(AskDestroy);
    }

    public void SetWindow(StructureWindow window)
    {
        this.window = window;
    }

    public override void Clear()
    {
        base.Clear();
        if (Data) Data.structureAgent = null;
        Data = null;
        window = null;
        structurePosition.text = string.Empty;
    }

    private void Update()
    {
        if (Data && Data.IsBuilding)
        {
            structureStates.text = Data.IsBuilt ? "已建成" : !Data.IsBuilding ? $"等待中[剩余{Data.leftBuildTime:F2}s]" : $"建造中[剩余{Data.leftBuildTime:F2}s]";
        }
    }

    public void UpdateUI()
    {
        if (Data)
        {
            destoryButton.interactable = Data.IsBuilt;
            structureStates.text = Data.IsBuilt ? "已建成" : !Data.IsBuilding ? $"等待中[剩余{Data.leftBuildTime:F2}s]" : $"建造中[剩余{Data.leftBuildTime:F2}s]";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //TODO 移动视野至相应建筑
        window.LocateStructure(Data);
    }

    public void AskDestroy()
    {
        if (Data) StructureManager.DestroyStructure(Data);
    }

    public override void Refresh()
    {
        Data.structureAgent = this;
        destoryButton.interactable = Data.IsBuilt;
        structurePosition.text = "位置" + ((Vector2)Data.position).ToString();
        structureStates.text = Data.IsBuilt ? "已建成" : !Data.IsBuilding ? $"等待中[剩余{Data.leftBuildTime:F2}s]" : $"建造中[剩余{Data.leftBuildTime:F2}s]";
    }

    protected override void OnInit()
    {

    }
}
