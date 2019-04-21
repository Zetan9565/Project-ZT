using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public string sceneName;

    public DateTime saveDate;

    public BackpackData backpackData;

    public List<QuestData> ongoingQuestDatas;
    public List<QuestData> completeQuestDatas;

    public List<DialogueData> dialogueDatas;

    public SaveData()
    {
        sceneName = SceneManager.GetActiveScene().name;
        saveDate = DateTime.Now;
        backpackData = new BackpackData();
        ongoingQuestDatas = new List<QuestData>();
        completeQuestDatas = new List<QuestData>();
        dialogueDatas = new List<DialogueData>();
    }
}

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
public class ItemData
{
    public string itemID;

    public int amount;

    public int indexInBP;

    public ItemData(ItemInfo itemInfo, int index)
    {
        itemID = itemInfo.ItemID;
        amount = itemInfo.Amount;
        //this.itemInfo = itemInfo;
        indexInBP = index;
    }
}

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

    public List<DialogueWordsData> wordsInfos;

    public DialogueData()
    {
        wordsInfos = new List<DialogueWordsData>();
    }

    public DialogueData(Dialogue dialogue)
    {
        dialogID = dialogue.ID;
        wordsInfos = new List<DialogueWordsData>();
        foreach (DialogueWords words in dialogue.Words)
            wordsInfos.Add(new DialogueWordsData() { wordsIndex = dialogue.Words.IndexOf(words) });
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