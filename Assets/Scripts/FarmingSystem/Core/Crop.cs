using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/农牧/农作物")]
public class Crop : Gathering
{
    public string EntityID => Data ? Data.entityID : string.Empty;

    public CropData Data;// { get; private set; }

    public CropAgent UI;

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

    public override bool IsInteractive
    {
        get
        {
            if (Data) return base.IsInteractive && Data.HarvestAble;
            else return false;
        }
    }

    [HideInInspector]
    public UnityEngine.Events.UnityEvent onHarvestFinish = new UnityEngine.Events.UnityEvent();

    public void Init(CropData data, Field parent, Vector3 position)
    {
        Clear();
        data.entity = this;
        Data = data;
        Data.OnStageChange += OnStageChange;
        Parent = parent;
        transform.SetParent(parent.transform);
        transform.position = position;
    }

    public override void GatherSuccess()
    {
        base.GatherSuccess();
        Data.OnHarvest();
    }

    private void Clear()
    {
        if (Data && Data.Info)
        {
            GameManager.Crops.TryGetValue(Data.Info, out var crops);
            if (crops != null)
            {
                crops.Remove(this);
                if (crops.Count < 1) GameManager.Crops.Remove(Data.Info);
            }
            Data = null;
        }
        if (Parent)
        {
            Parent.Crops.Remove(this);
            Parent = null;
        }
        gatheringInfo = null;
        UI = null;
    }

    private void OnStageChange(CropStage stage)
    {
        if (!stage) return;
        if (stage.HarvestAble)
            gatheringInfo = Data.currentStage.GatherInfo;
        else gatheringInfo = null;
    }

    public void Recycle()
    {
        Clear();
        ObjectPool.Put(gameObject);
    }

    //农作物的刷新不依赖采集物
    protected override IEnumerator UpdateTime()
    {
        yield break;
    }

    private void Awake()
    {
        hideOnGathered = false;
    }
}