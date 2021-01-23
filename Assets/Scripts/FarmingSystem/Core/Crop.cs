using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/农牧/农作物")]
public class Crop : MonoBehaviour
{
    public string RuntimeID { get; private set; }

    public CropInformation Info { get; private set; }

    /// <summary>
    /// 当前收割次数
    /// </summary>
    private int currentHarvestTimes;
    public int totalGrowthDays;

    public CropStage currentStage;
    public CropStage nextStage;
    public int stageGrowthDays;

    public bool Dry { get; private set; }
    public bool Pest { get; private set; }

    public Field Parent { get; private set; }

    public bool HarvestAble { get; private set; }
    [HideInInspector]
    public UnityEngine.Events.UnityEvent onHarvestFinish = new UnityEngine.Events.UnityEvent();

    public void Plant(CropInformation info, Field parent = null)
    {
        Clear();
        Info = info;
        HarvestAble = false;
        Parent = parent;
        totalGrowthDays = 1;
        GameManager.Crops.TryGetValue(Info, out var crops);
        if (crops != null)
        {
            RuntimeID = Info.ID + "I" + crops.Count.ToString().PadLeft(4);
            crops.Add(this);
        }
        else
        {
            RuntimeID = Info.ID + "I0000";
            GameManager.Crops.Add(Info, new List<Crop>() { this });
        }
        NotifyCenter.Instance.RemoveListener(this);
        NotifyCenter.Instance.AddListener(NotifyCenter.CommonKeys.DayChange, UpdateDay);
    }

    public void UpdateDay(params object[] msg)
    {
        if (msg.Length > 1)
        {
            int days = (int)msg[1] - (int)msg[0];
            totalGrowthDays += days;
            stageGrowthDays += days;
            HandlingStage();
        }
    }

    private void HandlingStage()
    {
        if (nextStage && stageGrowthDays >= nextStage.LastingDays)
        {
            NextStage();
        }
        if (currentStage.Stage == CropStages.Decay && stageGrowthDays >= currentStage.LastingDays)//腐败后土壤肥沃
        {
            if (Parent) Parent.fertility++;
            //Parent.Empty();
            Recycle();
        }
    }

    private void NextStage()
    {
        currentStage = nextStage;
        stageGrowthDays = 0;
        if (currentStage.HarvestAble)
            HarvestAble = true;
        int currentIndex = Info.Stages.IndexOf(currentStage);
        int nextIndex = -1;
        if (currentIndex < Info.Stages.Count - 1)
        {
            if (currentStage.RepeatAble && currentIndex == currentStage.IndexToReturn && (currentHarvestTimes < currentStage.RepeatTimes || currentStage.RepeatTimes < 0))
            {
                nextIndex = currentStage.IndexToReturn;
            }
            else nextIndex = currentIndex + 1;
        }
        if (nextIndex > -1 && nextIndex < Info.Stages.Count) nextStage = Info.Stages[nextIndex];
        else nextStage = null;
    }

    private void HarvestDone()
    {
        currentHarvestTimes++;
        HarvestAble = false;
        NextStage();
    }

    public void OnHarvestSuccess()
    {
        onHarvestFinish?.Invoke();
        HarvestAble = false;
        if (currentStage.ProductItems.Count > 0)
        {
            List<ItemInfo> lootItems = new List<ItemInfo>();
            foreach (DropItemInfo di in currentStage.ProductItems)
                if (ZetanUtility.Probability(di.DropRate))
                    if (!di.OnlyDropForQuest || (di.OnlyDropForQuest && QuestManager.Instance.HasOngoingQuestWithID(di.BindedQuest.ID)))
                        lootItems.Add(new ItemInfo(di.Item, Random.Range(1, di.Amount + 1)));
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Get(currentStage.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
        HarvestDone();
    }

    private void Clear()
    {
        GameManager.Crops.TryGetValue(Info, out var crops);
        if (crops != null)
        {
            crops.Remove(this);
            if (crops.Count < 1) GameManager.Crops.Remove(Info);
        }
        HarvestAble = false;
        RuntimeID = string.Empty;
        Info = null;
        Parent.Crops.Remove(this);
        Parent = null;
        currentStage = null;
        nextStage = null;
        currentHarvestTimes = -1;
        totalGrowthDays = -1;
        NotifyCenter.Instance.RemoveListener(this);
    }

    public void Recycle()
    {
        Clear();
        ObjectPool.Put(gameObject);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (HarvestAble)
        { }
    }

    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (HarvestAble)
        { }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (HarvestAble)
        { }
    }
}