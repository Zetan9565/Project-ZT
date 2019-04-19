using System.Collections.Generic;
using System;

[Serializable]
public class SaveData
{
    public string sceneName;

    public DateTime saveDate;

    public List<ItemData> itemDatas;

    public List<QuestData> ongoingQuestDatas;
    public List<QuestData> completeQuestDatas;

    public SaveData(string sceneName)
    {
        this.sceneName = sceneName;
        saveDate = DateTime.Now;
        itemDatas = new List<ItemData>();
        ongoingQuestDatas = new List<QuestData>();
        completeQuestDatas = new List<QuestData>();
    }
}

[Serializable]
public class ItemData
{
    public string itemID;

    public int itemAmount;

    public int indexInBP;

    public ItemData(string ID, int amount, int index)
    {
        itemID = ID;
        itemAmount = amount;
        indexInBP = index;
    }
}

#region 任务相关
[System.Serializable]
public class QuestData
{
    public string questID;

    public string originalGiverID;

    public List<ObjectiveData> objectiveDatas = new List<ObjectiveData>();

    public QuestData(Quest quest)
    {
        questID = quest.ID;
        originalGiverID = quest.OriginalQuestGiver.Info.ID;
        foreach(Objective o in quest.Objectives)
        {
            objectiveDatas.Add(new ObjectiveData(o));
        }
    }
}
[System.Serializable]
public class ObjectiveData
{
    public string runtimeID;

    public int currentAmount;

    public ObjectiveData(Objective objective)
    {
        runtimeID = objective.runtimeID;
        currentAmount = objective.CurrentAmount;
    }
}
#endregion