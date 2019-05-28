using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingAgent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Text buildingPosition;

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
        buildingPosition.text = (MBuilding.IsBuilt ? string.Empty : "[建设中]") + "位置" + ((Vector2)MBuilding.transform.position).ToString();
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
        buildingPosition.text = (MBuilding.IsBuilt ? string.Empty : "[建设中]") + "位置" + ((Vector2)MBuilding.transform.position).ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //TODO 移动视野至相应建筑
    }

    public void Show()
    {
        MyTools.SetActive(gameObject, true);
    }

    public void Hide()
    {
        MyTools.SetActive(gameObject, false);
    }

    public void Destroy()
    {
        BuildingManager.Instance.RequestDestroy(MBuilding);
        BuildingManager.Instance.DestroyToDestroyBuilding();
    }
}
