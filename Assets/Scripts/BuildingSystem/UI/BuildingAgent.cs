using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingAgent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Text buildingPosition;

    [SerializeField]
    private Text buildingStates;

    [SerializeField]
    private Button destoryButton;

    public Building MBuilding { get; private set; }

    private void Awake()
    {
        destoryButton.onClick.AddListener(Destroy);
    }

    public void Init(Building building)
    {
        MBuilding = building;
        MBuilding.buildingAgent = this;
        destoryButton.interactable = MBuilding.IsBuilt;
        buildingPosition.text = "位置" + ((Vector2)MBuilding.transform.position).ToString();
        buildingStates.text = MBuilding.IsBuilt ? "已建成" : "建设中[" + MBuilding.leftBuildTime.ToString("F2") + "s]";
    }

    public void Clear(bool recycle = false)
    {
        if (MBuilding) MBuilding.buildingAgent = null;
        MBuilding = null;
        buildingPosition.text = string.Empty;
        if (recycle) ObjectPool.Instance.Put(gameObject);
    }

    public void UpdateUI()
    {
        destoryButton.interactable = MBuilding.IsBuilt;
        //buildingPosition.text = "位置" + ((Vector2)MBuilding.transform.position).ToString();
        buildingStates.text = MBuilding.IsBuilt ? "已建成" : "建设中[" + MBuilding.leftBuildTime.ToString("F2") + "s]";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //TODO 移动视野至相应建筑
    }

    public void Show()
    {
        ZetanUtilities.SetActive(gameObject, true);
    }

    public void Hide()
    {
        ZetanUtilities.SetActive(gameObject, false);
    }

    public void Destroy()
    {
        BuildingManager.Instance.RequestDestroy(MBuilding);
        BuildingManager.Instance.DestroyToDestroyBuilding();
    }
}
