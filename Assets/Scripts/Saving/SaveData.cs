using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ZetanStudio.Item;

[Serializable]
public class SaveData
{
    public string version;
    public string sceneName;

    public DateTime saveDate;
    public decimal totalTime;

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    public BackpackSaveData backpackData = new BackpackSaveData();

    public List<string> craftDatas = new List<string>();

    public StructureSystemSaveData structureSystemData = new StructureSystemSaveData();

    public List<WarehouseSaveData> warehouseDatas = new List<WarehouseSaveData>();

    public List<QuestSaveData> inProgressQuestDatas = new List<QuestSaveData>();
    public List<QuestSaveData> finishedQuestDatas = new List<QuestSaveData>();

    public List<DialogueSaveData> dialogueDatas = new List<DialogueSaveData>();

    public List<MapMarkSaveData> markDatas = new List<MapMarkSaveData>();

    public List<ActionSaveData> actionDatas = new List<ActionSaveData>();

    public TriggerSaveData triggerData = new TriggerSaveData();

    public SaveData(string version)
    {
        this.version = version;
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
    public List<string> items;
}

[Serializable]
public class WarehouseSaveData
{
    public string handlerID;
    public string scene;
    public float posX;
    public float posY;
    public float posZ;

    public long money;

    public int currentSize;
    public int maxSize;

    public List<ItemSaveData> itemDatas = new List<ItemSaveData>();

    public WarehouseSaveData(string id, WarehouseData warehouse)
    {
        handlerID = id;
        scene = warehouse.scene;
        posX = warehouse.position.x;
        posY = warehouse.position.y;
        posZ = warehouse.position.z;
        money = warehouse.Inventory.Money;
        currentSize = warehouse.Inventory.SpaceCost;
        maxSize = warehouse.Inventory.SpaceLimit;
        //foreach (ItemInfo info in warehouse.Items)
        //{
        //    itemDatas.Add(new ItemSaveData(info));
        //}
    }
}
public class InventorySaveData
{
    public List<InventoryItemSaveData> items;
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
        //indexInGrid = itemInfo.indexInGrid;
    }
}
[Serializable]
public class NewItemSaveData//因为存在同一个ItemData可能分别存储在不同Inventory中的情况，所以用一个总的列表存储它们，最后读档时再根据ID从中依次读取到相应的Inventory。
{
    public string modelID;
    public string ID;

    public NewItemSaveData(ItemData item)
    {
        modelID = item.ModelID;
        ID = item.ID;
    }
}
[Serializable]
public class InventoryItemSaveData
{
    public string ID;
    public int amount;
    public bool isLocked;
    public List<SlotSaveData> slots = new List<SlotSaveData>();

    public string warehouseID;
    public string structureID;

    public InventoryItemSaveData(ItemData data, int amount, List<ItemSlotData> slots, Warehouse warehouse = null, Structure2D structure = null)
    {
        ID = data.ID;
        this.amount = amount;
        isLocked = data.IsLocked;
        this.slots.AddRange(slots.ConvertAll(x => new SlotSaveData(x)));
        warehouseID = warehouse != null ? warehouse.EntityID : string.Empty;
        structureID = structure != null ? structure.EntityID : string.Empty;
    }
}
[Serializable]
public class SlotSaveData
{
    public int index;
    public int amount;

    public SlotSaveData(ItemSlotData slot)
    {
        index = slot.index;
        amount = slot.amount;
    }
}
#endregion

#region 建筑相关
[Serializable]
public class StructureSystemSaveData
{
    public string[] learneds;

    public List<StructureSaveData> structureDatas = new List<StructureSaveData>();
}

[Serializable]
public class StructureSaveData
{
    public string modelID;
    public string ID;

    public string scene;
    public float posX;
    public float posY;
    public float posZ;

    public float leftBuildTime;
    public int stageIndex;

    public StructureSaveData(StructureData structure)
    {
        modelID = structure.Info.ID;
        ID = structure.ID;
        scene = structure.scene;
        posX = structure.position.x;
        posY = structure.position.y;
        posZ = structure.position.z;
        leftBuildTime = structure.leftBuildTime;
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
        questID = quest.Model.ID;
        originalGiverID = quest.originalQuestHolder.TalkerID;
        foreach (ObjectiveData o in quest.Objectives)
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
        objectiveID = objective.ID;
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
        dialogID = dialogue.model.ID;
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
        ID = stackElement.executor.ID;
        isExecuting = stackElement.executor.IsExecuting;
        executionTime = stackElement.executor.ExecutionTime;
        endDelayTime = stackElement.executor.EndDelayTime;
        isDone = stackElement.executor.IsDone;
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

    public MapMarkSaveData(MapIcon iconWoH)
    {
        worldPosX = iconWoH.Position.x;
        worldPosY = iconWoH.Position.y;
        worldPosZ = iconWoH.Position.z;
        keepOnMap = iconWoH.KeepOnMap;
        removeAble = iconWoH.RemoveAble;
        textToDisplay = iconWoH.TextToDisplay;
    }
}
#endregion

public interface ISaveLoad
{
    void SaveData(SaveData data);
    void LoadData(SaveData data);
}