using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : GatherAgent
{
    public CropInformation Info { get; private set; }

    public int currentHarvestTimes;
    public float currentGrowthDay;

    public CropStage currentStage;
    public CropStage nextStage;

    private int harvestTimes;

    public FieldGrid Parent { get; private set; }

    private bool harvestAble = false;
    public bool HarvestAble
    {
        get
        {
            return harvestAble && Info;
        }

        private set
        {
            harvestAble = value;
        }
    }

    public void Init(CropInformation info, FieldGrid parent = null)
    {
        if (!info || !info.IsValid) return;
        Info = Instantiate(info);
        gatheringInfo = info.GatheringInfo;
        GatherAble = false;
        Parent = parent;
        currentGrowthDay = 1;
        TimeManager.Instance.OnDayPassed += UpdateDay;
    }

    public void UpdateDay()
    {
        currentGrowthDay++;
        HandlingStage();
    }

    private void HandlingStage()
    {
        if (nextStage && currentGrowthDay >= nextStage.LifespanPer.Min && currentGrowthDay < nextStage.LifespanPer.Max)
        {
            NextStage();
        }
        if (currentStage.stage == CropStages.Decay && currentGrowthDay >= currentStage.LifespanPer.Max && Parent)//腐败后土壤肥沃
        {
            Parent.fertility++;
            Parent.Empty();
            TimeManager.Instance.OnDayPassed -= UpdateDay;
            DestroyImmediate(gameObject);
        }
    }

    private void NextStage()
    {
        currentStage = nextStage;
        if (currentStage.stage == CropStages.Maturity)
        {
            harvestTimes++;
            HarvestAble = true;
            GatherAble = true;
        }
        int currentIndex = Info.Stages.IndexOf(currentStage);
        int nextIndex = -1;
        if (currentIndex < Info.Stages.Count - 1)
        {
            if (Info.CanRepeat && currentIndex == Info.RepeatStage.Max && (harvestTimes < Info.RepeatTimes || Info.RepeatTimes < 0))
            {
                nextIndex = Info.RepeatStage.Min;
            }
            else nextIndex = currentIndex + 1;
        }
        if (nextIndex > -1 && nextIndex < Info.Stages.Count) nextStage = Info.Stages[nextIndex];
        else nextStage = null;
    }

    private void HarvestDone()
    {
        HarvestAble = false;
        NextStage();
        currentGrowthDay = currentStage.LifespanPer.Min * Info.GrowthTime;
    }

    public override void GatherSuccess()
    {
        onGatherFinish?.Invoke();
        GatherAble = false;
        if (GatheringInfo.ProductItems.Count > 0)
        {
            List<ItemInfo> lootItems = new List<ItemInfo>();
            foreach (DropItemInfo di in GatheringInfo.ProductItems)
                if (ZetanUtil.Probability(di.DropRate))
                    if (!di.OnlyDropForQuest || (di.OnlyDropForQuest && QuestManager.Instance.HasOngoingQuestWithID(di.BindedQuest.ID)))
                        lootItems.Add(new ItemInfo(di.Item, Random.Range(1, di.Amount + 1)));
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Instance.Get(GatheringInfo.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
        HarvestDone();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (HarvestAble)
            base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        if (HarvestAble)
            base.OnTriggerStay2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (HarvestAble)
            base.OnTriggerExit2D(collision);
    }
}