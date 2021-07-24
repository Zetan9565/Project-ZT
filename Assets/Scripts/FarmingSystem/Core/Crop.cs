using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/农牧/农作物")]
public class Crop : Gathering
{
    public string EntityID { get; private set; }

    public CropInformation Info { get; private set; }

    public CropData Data { get; private set; }

    public bool Dry
    {
        get
        {
            if (Data) return Data.dry;
            else return false;
        }
    }
    public bool Pest
    {
        get
        {
            if (Data) return Data.pest;
            else return false;
        }
    }

    public Field Parent { get; private set; }

    public override bool Interactive
    {
        get
        {
            if (Data) return Data.harvestAble;
            else return false;
        }
    }

    [HideInInspector]
    public UnityEngine.Events.UnityEvent onHarvestFinish = new UnityEngine.Events.UnityEvent();

    public void Plant(CropData data, Field parent, Vector3 position)
    {
        Clear();
        Data = data;
        Data.OnStageChange += OnStageChange;
        Parent = parent;
        transform.SetParent(parent.transform);
        transform.position = position;
    }


    private void HarvestDone()
    {

    }

    public void OnHarvestSuccess()
    {
        onHarvestFinish?.Invoke();
        List<ItemInfo> lootItems = Data.OnHarvestSuccess();
        if (lootItems.Count > 0)
        {
            LootAgent la = ObjectPool.Get(Data.currentStage.GatherInfo.LootPrefab).GetComponent<LootAgent>();
            la.Init(lootItems, transform.position);
        }
        HarvestDone();
    }

    private void Clear()
    {
        if (Info)
        {
            GameManager.Crops.TryGetValue(Info, out var crops);
            if (crops != null)
            {
                crops.Remove(this);
                if (crops.Count < 1) GameManager.Crops.Remove(Info);
            }
            Info = null;
        }
        if (Parent)
        {
            Parent.Crops.Remove(this);
            Parent = null;
        }
        EntityID = string.Empty;
        gatheringInfo = null;
        NotifyCenter.Instance.RemoveListener(this);
    }

    private void OnStageChange(CropStage stage)
    {
        if (stage.HarvestAble)
            gatheringInfo = Data.currentStage.GatherInfo;
    }

    public void Recycle()
    {
        Clear();
        ObjectPool.Put(gameObject);
    }

    //农作物的刷新不依赖采集物
    protected override IEnumerator UpdateTime()
    {
        yield return null;
    }

    private void Awake()
    {
        hideOnGathered = false;
    }
}