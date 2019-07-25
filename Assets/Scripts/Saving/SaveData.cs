using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public string sceneName;

    public DateTime saveDate;

    public BackpackData backpackData;

    public BuildingSystemData buildingSystemData;

    public List<WarehouseData> warehouseDatas;

    public List<QuestData> ongoingQuestDatas;
    public List<QuestData> completeQuestDatas;

    public List<DialogueData> dialogueDatas;

    public List<TriggerData> triggerDatas;

    public SaveData()
    {
        sceneName = SceneManager.GetActiveScene().name;
        saveDate = DateTime.Now;
        backpackData = new BackpackData();
        buildingSystemData = new BuildingSystemData();
        warehouseDatas = new List<WarehouseData>();
        ongoingQuestDatas = new List<QuestData>();
        completeQuestDatas = new List<QuestData>();
        dialogueDatas = new List<DialogueData>();
        triggerDatas = new List<TriggerData>();
    }
}

#region 道具相关
[Serializable]
public class BackpackData
{
    public long money;

    public int currentSize;
    public int maxSize;

    public float currentWeight;
    public float maxWeightLoad;

    public List<ItemData> itemDatas = new List<ItemData>();
}

[Serializable]
public class WarehouseData
{
    public string handlerID;

    public long money;

    public int currentSize;
    public int maxSize;

    public List<ItemData> itemDatas = new List<ItemData>();

    public WarehouseData(string id, Warehouse warehouse)
    {
        handlerID = id;
        money = warehouse.Money;
        currentSize = (int)warehouse.warehouseSize;
        maxSize = warehouse.warehouseSize.Max;
        foreach (ItemInfo info in warehouse.Items)
        {
            itemDatas.Add(new ItemData(info));
        }
    }
}

[Serializable]
public class ItemData
{
    public string itemID;

    public int amount;

    public int indexInGrid;

    public ItemData(ItemInfo itemInfo, int index = -1)
    {
        itemID = itemInfo.ItemID;
        amount = itemInfo.Amount;
        indexInGrid = index;
    }
}
#endregion

#region 建筑相关
[Serializable]
public class BuildingSystemData
{
    public string[] learneds;

    public List<BuildingData> buildingDatas;

    public BuildingSystemData()
    {
        buildingDatas = new List<BuildingData>();
    }
}

[Serializable]
public class BuildingData
{
    public string IDStarter;

    public string IDTail;

    public float posX;
    public float posY;
    public float posZ;

    public float leftBuildTime;

    public BuildingData(Building building)
    {
        IDStarter = building.IDStarter;
        IDTail = building.IDTail;
        posX = building.transform.position.x;
        posY = building.transform.position.y;
        posZ = building.transform.position.z;
        leftBuildTime = building.leftBuildTime;
    }
}
#endregion

#region 任务相关
[Serializable]
public class QuestData
{
    public string questID;

    public string originalGiverID;

    public List<ObjectiveData> objectiveDatas = new List<ObjectiveData>();

    public QuestData(Quest quest)
    {
        questID = quest.ID;
        originalGiverID = quest.OriginalQuestGiver.TalkerID;
        foreach (Objective o in quest.Objectives)
        {
            objectiveDatas.Add(new ObjectiveData(o));
        }
    }
}
[Serializable]
public class ObjectiveData
{
    public string objectiveID;

    public int currentAmount;

    public ObjectiveData(Objective objective)
    {
        objectiveID = objective.runtimeID;
        currentAmount = objective.CurrentAmount;
    }
}
#endregion

#region 对话相关
[Serializable]
public class DialogueData
{
    public string dialogID { get; private set; }

    public List<DialogueWordsData> wordsDatas;

    public DialogueData()
    {
        wordsDatas = new List<DialogueWordsData>();
    }

    public DialogueData(Dialogue dialogue)
    {
        dialogID = dialogue.ID;
        wordsDatas = new List<DialogueWordsData>();
        foreach (DialogueWords words in dialogue.Words)
            wordsDatas.Add(new DialogueWordsData() { wordsIndex = dialogue.Words.IndexOf(words) });
    }
}

[Serializable]
public class DialogueWordsData
{
    public int wordsIndex;//该语句在对话中的位置

    public List<int> cmpltBranchIndexes;//已完成的分支的序号集

    public DialogueWordsData()
    {
        wordsIndex = -1;
        cmpltBranchIndexes = new List<int>();
    }
    public DialogueWordsData(int wordsIndex)
    {
        this.wordsIndex = wordsIndex;
        cmpltBranchIndexes = new List<int>();
    }

    public bool IsCmpltBranchWithIndex(int index)
    {
        return cmpltBranchIndexes.Contains(index);
    }
}
#endregion

[Serializable]
public class TriggerData
{
    public string triggerName;

    public bool triggerState;

    public TriggerData(string triggerName, bool triggerState)
    {
        this.triggerName = triggerName;
        this.triggerState = triggerState;
    }
}