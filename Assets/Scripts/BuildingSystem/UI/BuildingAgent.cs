﻿using UnityEngine;
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

    public BuildingData MBuilding { get; private set; }

    private void Awake()
    {
        destoryButton.onClick.AddListener(AskDestroy);
    }

    public void Init(BuildingData building)
    {
        if (!building) return;
        MBuilding = building;
        MBuilding.entity.buildingAgent = this;
        destoryButton.interactable = MBuilding.IsBuilt;
        buildingPosition.text = "位置" + ((Vector2)MBuilding.entity.transform.position).ToString();
        buildingStates.text = MBuilding.IsBuilt ? "已建成" : "建设中[" + MBuilding.leftBuildTime.ToString("F2") + "s]";
    }

    public void Clear(bool recycle = false)
    {
        if (MBuilding) MBuilding.entity.buildingAgent = null;
        MBuilding = null;
        buildingPosition.text = string.Empty;
        if (recycle) ObjectPool.Put(gameObject);
    }

    public void UpdateUI()
    {
        if (MBuilding)
        {
            destoryButton.interactable = MBuilding.IsBuilt;
            buildingStates.text = MBuilding.IsBuilt ? "已建成" : "建设中[" + MBuilding.leftBuildTime.ToString("F2") + "s]";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //TODO 移动视野至相应建筑
        BuildingManager.Instance.LocateBuilding(MBuilding.entity);
    }

    public void Show()
    {
        ZetanUtility.SetActive(gameObject, true);
    }

    public void Hide()
    {
        ZetanUtility.SetActive(gameObject, false);
    }

    public void AskDestroy()
    {
        if (MBuilding) BuildingManager.Instance.DestroyBuilding(MBuilding);
    }
}
