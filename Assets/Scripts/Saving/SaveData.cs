using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public string sceneName;

    public DateTime saveDate;
    public float totalTime;

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    public BackpackData backpackData = new BackpackData();

    public List<string> makingDatas = new List<string>();

    public BuildingSystemData buildingSystemData = new BuildingSystemData();

    public List<WarehouseData> warehouseDatas = new List<WarehouseData>();

    public List<QuestData> ongoingQuestDatas = new List<QuestData>();
    public List<QuestData> completeQuestDatas = new List<QuestData>();

    public List<DialogueData> dialogueDatas = new List<DialogueData>();

    public List<MapMarkData> markDatas = new List<MapMarkData>();

    public List<ActionData> actionDatas = new List<ActionData>();

    public TriggerData triggerData = new TriggerData();

    public SaveData()
    {
        sceneName = SceneManager.GetActiveScene().name;
        saveDate = DateTime.Now;
        var playerPos = PlayerManager.Instance.PlayerTransform.position;
        playerPosX = playerPos.x;
        playerPosY = playerPos.y;
        playerPosZ = playerPos.z;
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
        currentSize = (int)warehouse.size;
        maxSize = warehouse.size.Max;
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

    public ItemData(ItemInfo itemInfo)
    {
        itemID = itemInfo.ItemID;
        amount = itemInfo.Amount;
        indexInGrid = itemInfo.indexInGrid;
    }
}
#endregion

#region 建筑相关
[Serializable]
public class BuildingSystemData
{
    public string[] learneds;

    public List<BuildingData> buildingDatas = new List<BuildingData>();
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
        originalGiverID = quest.originalQuestHolder.TalkerID;
        foreach (Objective o in quest.ObjectiveInstances)
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
    public string dialogID;

    public List<DialogueWordsData> wordsDatas = new List<DialogueWordsData>();

    public DialogueData(Dialogue dialogue)
    {
        dialogID = dialogue.ID;
        for (int i = 0; i < dialogue.Words.Count; i++)
            wordsDatas.Add(new DialogueWordsData() { wordsIndex = i });
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

    public bool IsCmpltOptionWithIndex(int index)
    {
        return cmpltBranchIndexes.Contains(index);
    }
}
#endregion

#region 其它
[Serializable]
public class TriggerData
{
    public List<TriggerStateData> stateDatas = new List<TriggerStateData>();
    public List<TriggerHolderData> holderDatas = new List<TriggerHolderData>();
}
[Serializable]
public class TriggerHolderData
{
    public string ID;
    public bool isSetAtFirst;

    public TriggerHolderData(TriggerHolder holder)
    {
        ID = holder.ID;
        isSetAtFirst = holder.isSetAtFirst;
    }
}
[Serializable]
public class TriggerStateData
{
    public string triggerName;

    public int triggerState;

    public TriggerStateData(string triggerName, TriggerState triggerState)
    {
        this.triggerName = triggerName;
        this.triggerState = (int)triggerState;
    }
}

[Serializable]
public class ActionData
{
    public string ID;

    public bool isExecuting;
    public float executionTime;

    public bool isDone;

    public int actionType;

    public ActionData(ActionStackData stackElement)
    {
        ID = stackElement. executor.ID;
        isExecuting = stackElement.executor.IsExecuting;
        executionTime = stackElement. executor.ExecutionTime;
        isDone = stackElement. executor.IsDone;
        actionType = (int)stackElement.actionType;
    }
}

[Serializable]
public class MapMarkData
{
    public float worldPosX;
    public float worldPosY;
    public float worldPosZ;
    public bool keepOnMap;
    public bool removeAble;
    public string textToDisplay;

    public MapMarkData(MapManager.MapIconWithoutHolder iconWoH)
    {
        worldPosX = iconWoH.worldPosition.x;
        worldPosY = iconWoH.worldPosition.y;
        worldPosZ = iconWoH.worldPosition.z;
        keepOnMap = iconWoH.keepOnMap;
        removeAble = iconWoH.removeAble;
        textToDisplay = iconWoH.textToDisplay;
    }
}
#endregion