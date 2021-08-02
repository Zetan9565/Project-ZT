using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public string sceneName;

    public DateTime saveDate;
    public decimal totalTime;

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    public BackpackSaveData backpackData = new BackpackSaveData();

    public List<string> makingDatas = new List<string>();

    public BuildingSystemSaveData buildingSystemData = new BuildingSystemSaveData();

    public List<WarehouseSaveData> warehouseDatas = new List<WarehouseSaveData>();

    public List<QuestSaveData> inProgressQuestDatas = new List<QuestSaveData>();
    public List<QuestSaveData> completeQuestDatas = new List<QuestSaveData>();

    public List<DialogueSaveData> dialogueDatas = new List<DialogueSaveData>();

    public List<MapMarkSaveData> markDatas = new List<MapMarkSaveData>();

    public List<ActionSaveData> actionDatas = new List<ActionSaveData>();

    public TriggerSaveData triggerData = new TriggerSaveData();

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
public class BackpackSaveData
{
    public long money;

    public int currentSize;
    public int maxSize;

    public float currentWeight;
    public float maxWeightLoad;

    public List<ItemSaveData> itemDatas = new List<ItemSaveData>();
}

[Serializable]
public class WarehouseSaveData
{
    public string handlerID;

    public long money;

    public int currentSize;
    public int maxSize;

    public List<ItemSaveData> itemDatas = new List<ItemSaveData>();

    public WarehouseSaveData(string id, WarehouseData warehouse)
    {
        handlerID = id;
        money = warehouse.Money;
        currentSize = (int)warehouse.size;
        maxSize = warehouse.size.Max;
        foreach (ItemInfo info in warehouse.Items)
        {
            itemDatas.Add(new ItemSaveData(info));
        }
    }
}

[Serializable]
public class ItemSaveData
{
    public string itemID;

    public int amount;

    public int indexInGrid;

    public ItemSaveData(ItemInfo itemInfo)
    {
        itemID = itemInfo.ItemID;
        amount = itemInfo.Amount;
        indexInGrid = itemInfo.indexInGrid;
    }
}
#endregion

#region 建筑相关
[Serializable]
public class BuildingSystemSaveData
{
    public string[] learneds;

    public List<BuildingSaveData> buildingDatas = new List<BuildingSaveData>();
}

[Serializable]
public class BuildingSaveData
{
    public string IDPrefix;

    public string IDTail;

    public string scene;
    public float posX;
    public float posY;
    public float posZ;

    public float leftBuildTime;

    public BuildingSaveData(BuildingData building)
    {
        IDPrefix = building.IDPrefix;
        IDTail = building.IDTail;
        scene = building.scene;
        posX = building.position.x;
        posY = building.position.y;
        posZ = building.position.z;
        leftBuildTime = building.leftBuildTime;
    }
}
#endregion

#region 任务相关
[Serializable]
public class QuestSaveData
{
    public string questID;

    public string originalGiverID;

    public List<ObjectiveSaveData> objectiveDatas = new List<ObjectiveSaveData>();

    public QuestSaveData(QuestData quest)
    {
        questID = quest.Info.ID;
        originalGiverID = quest.originalQuestHolder.TalkerID;
        foreach (ObjectiveData o in quest.ObjectiveInstances)
        {
            objectiveDatas.Add(new ObjectiveSaveData(o));
        }
    }
}
[Serializable]
public class ObjectiveSaveData
{
    public string objectiveID;

    public int currentAmount;

    public ObjectiveSaveData(ObjectiveData objective)
    {
        objectiveID = objective.entityID;
        currentAmount = objective.CurrentAmount;
    }
}
#endregion

#region 对话相关
[Serializable]
public class DialogueSaveData
{
    public string dialogID;

    public List<DialogueWordsSaveData> wordsDatas = new List<DialogueWordsSaveData>();

    public DialogueSaveData(DialogueData dialogue)
    {
        dialogID = dialogue.origin.ID;
        foreach (DialogueWordsData words in dialogue.wordsDatas)
        {
            wordsDatas.Add(new DialogueWordsSaveData(words));
        }
    }
}

[Serializable]
public class DialogueWordsSaveData
{
    public HashSet<int> cmpltOptionIndexes = new HashSet<int>();//已完成的选项的序号集

    public DialogueWordsSaveData(DialogueWordsData words)
    {
        for (int i = 0; i < words.optionDatas.Count; i++)
        {
            if (words.optionDatas[i].isDone)
                cmpltOptionIndexes.Add(i);
        }
    }

    public bool IsOptionCmplt(int index)
    {
        return cmpltOptionIndexes.Contains(index);
    }
}
#endregion

#region 其它
[Serializable]
public class TriggerSaveData
{
    public List<TriggerStateSaveData> stateDatas = new List<TriggerStateSaveData>();
    public List<TriggerHolderSaveData> holderDatas = new List<TriggerHolderSaveData>();
}
[Serializable]
public class TriggerHolderSaveData
{
    public string ID;
    public bool isSetAtFirst;

    public TriggerHolderSaveData(TriggerHolder holder)
    {
        ID = holder.ID;
        isSetAtFirst = holder.isSetAtFirst;
    }
}
[Serializable]
public class TriggerStateSaveData
{
    public string triggerName;

    public int triggerState;

    public TriggerStateSaveData(string triggerName, TriggerState triggerState)
    {
        this.triggerName = triggerName;
        this.triggerState = (int)triggerState;
    }
}

[Serializable]
public class ActionSaveData
{
    public string ID;

    public bool isExecuting;
    public float executionTime;
    public float endDelayTime;

    public bool isDone;

    public int actionType;

    public ActionSaveData(ActionStackData stackElement)
    {
        ID = stackElement. executor.ID;
        isExecuting = stackElement.executor.IsExecuting;
        executionTime = stackElement. executor.ExecutionTime;
        endDelayTime = stackElement. executor.EndDelayTime;
        isDone = stackElement. executor.IsDone;
        actionType = (int)stackElement.actionType;
    }
}

[Serializable]
public class MapMarkSaveData
{
    public float worldPosX;
    public float worldPosY;
    public float worldPosZ;
    public bool keepOnMap;
    public bool removeAble;
    public string textToDisplay;

    public MapMarkSaveData(MapManager.MapIconWithoutHolder iconWoH)
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