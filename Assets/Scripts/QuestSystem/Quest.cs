using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "quest", menuName = "ZetanStudio/任务/任务")]
public class Quest : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField, TextArea(1, 1)]
    private string title = string.Empty;
    public string Title
    {
        get
        {
            return title;
        }
    }

    [SerializeField, TextArea(3, 5)]
    private string description;
    public string Description
    {
        get
        {
            return description;
        }
    }

    [SerializeField]
    private bool abandonable = true;
    public bool Abandonable
    {
        get
        {
            return abandonable;
        }
    }

    [SerializeField]
    private QuestGroup group;
    public QuestGroup Group
    {
        get
        {
            return group;
        }
    }

    [SerializeField]
    private List<QuestAcceptCondition> acceptConditions = new List<QuestAcceptCondition>();
    public List<QuestAcceptCondition> AcceptConditions
    {
        get
        {
            return acceptConditions;
        }
    }

    [SerializeField]
    private Dialogue beginDialogue;
    public Dialogue BeginDialogue
    {
        get
        {
            return beginDialogue;
        }
    }
    [SerializeField]
    private Dialogue ongoingDialogue;
    public Dialogue OngoingDialogue
    {
        get
        {
            return ongoingDialogue;
        }
    }
    [SerializeField]
    private Dialogue completeDialogue;
    public Dialogue CompleteDialogue
    {
        get
        {
            return completeDialogue;
        }
    }

    [SerializeField]
    private int rewardMoney;
    public int RewardMoney
    {
        get
        {
            return rewardMoney;
        }
    }

    [SerializeField]
    private int rewardEXP;
    public int RewardEXP
    {
        get
        {
            return rewardEXP;
        }
    }

    [SerializeField]
    private List<ItemInfo> rewardItems = new List<ItemInfo>();
    public List<ItemInfo> RewardItems
    {
        get
        {
            return rewardItems;
        }
    }

    [SerializeField]
    private bool sbmtOnOriginalNPC = true;
    public bool SbmtOnOriginalNPC
    {
        get
        {
            return sbmtOnOriginalNPC;
        }
    }
    [SerializeField]
    private TalkerInfomation _NPCToSubmit;
    public TalkerInfomation NPCToSubmit
    {
        get
        {
            return _NPCToSubmit;
        }
    }

    [SerializeField]
    private bool cmpltObjctvInOrder = false;
    public bool CmpltObjctvInOrder
    {
        get
        {
            return cmpltObjctvInOrder;
        }
    }

    [System.NonSerialized]
    private List<Objective> objectives = new List<Objective>();//存储所有目标，在运行时用到，初始化时自动填，不用人为干预，详见QuestGiver类
    public List<Objective> Objectives
    {
        get
        {
            return objectives;
        }
    }

    [SerializeField]
    private List<CollectObjective> collectObjectives = new List<CollectObjective>();
    public List<CollectObjective> CollectObjectives
    {
        get
        {
            return collectObjectives;
        }
    }

    [SerializeField]
    private List<KillObjective> killObjectives = new List<KillObjective>();
    public List<KillObjective> KillObjectives
    {
        get
        {
            return killObjectives;
        }
    }

    [SerializeField]
    private List<TalkObjective> talkObjectives = new List<TalkObjective>();
    public List<TalkObjective> TalkObjectives
    {
        get
        {
            return talkObjectives;
        }
    }

    [SerializeField]
    private List<MoveObjective> moveObjectives = new List<MoveObjective>();
    public List<MoveObjective> MoveObjectives
    {
        get
        {
            return moveObjectives;
        }
    }

    [HideInInspector]
    public QuestGiver OriginalQuestGiver;

    [HideInInspector]
    public QuestGiver CurrentQuestGiver;

    [HideInInspector]
    public bool IsOngoing;//任务是否正在执行，在运行时用到

    public bool IsComplete
    {
        get
        {
            if (Objectives.Exists(x => !x.IsComplete))
                return false;
            return true;
        }
    }

    public bool IsFinished
    {
        get
        {
            return IsComplete && !IsOngoing;
        }
    }

    public bool AcceptAble
    {
        get
        {
            foreach (QuestAcceptCondition qac in AcceptConditions)
            {
                if (!qac.IsEligible) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 判断该任务是否需要某个道具，用于丢弃某个道具时，判断能不能丢
    /// </summary>
    /// <param name="item">所需判定的道具</param>
    /// <param name="leftAmount">所需判定的数量</param>
    /// <returns>是否需要</returns>
    public bool RequiredItem(ItemBase item, int leftAmount)
    {
        if (CmpltObjctvInOrder)
        {
            foreach (Objective o in Objectives)
            {
                //当目标是收集类目标时才进行判断
                if (o is CollectObjective && item == (o as CollectObjective).Item)
                {
                    if (o.IsComplete && o.InOrder)
                    {
                        //如果剩余的道具数量不足以维持该目标完成状态
                        if (o.Amount > leftAmount)
                        {
                            Objective tempObj = o.NextObjective;
                            while (tempObj != null)
                            {
                                //则判断是否有后置目标在进行，以保证在打破该目标的完成状态时，后置目标不受影响
                                if (tempObj.CurrentAmount > 0 && tempObj.OrderIndex > o.OrderIndex)
                                {
                                    //Debug.Log("Required");
                                    return true;
                                }
                                tempObj = tempObj.NextObjective;
                            }
                        }
                        //Debug.Log("NotRequired3");
                        return false;
                    }
                    //Debug.Log("NotRequired2");
                    return false;
                }
            }
        }
        //Debug.Log("NotRequired1");
        return false;
    }

    /// <summary>
    /// 是否在收集某个道具
    /// </summary>
    /// <param name="itemID">许判断的道具</param>
    /// <returns>是否在收集</returns>
    public bool CollectingItem(string itemID)
    {
        return collectObjectives.Exists(x => x.Item.ID == itemID && !x.IsComplete);
    }
}

#region 任务条件
/// <summary>
/// 任务接收条件
/// </summary>
[System.Serializable]
public class QuestAcceptCondition
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("等级大于", "等级小于", "等级大于或等于", "等级小于或等于", "完成任务", "拥有道具")]
#endif
    private QuestCondition acceptCondition = QuestCondition.ComplexQuest;
    public QuestCondition AcceptCondition
    {
        get
        {
            return acceptCondition;
        }
    }

    [SerializeField]
    private int level;
    public int Level
    {
        get
        {
            return level;
        }
    }

    [SerializeField]
    private Quest completeQuest;
    public Quest CompleteQuest
    {
        get
        {
            return completeQuest;
        }
    }

    [SerializeField]
    private ItemBase ownedItem;
    public ItemBase OwnedItem
    {
        get
        {
            return ownedItem;
        }
    }
    /// <summary>
    /// 是否符合条件
    /// </summary>
    public bool IsEligible
    {
        get
        {
            switch (AcceptCondition)
            {
                case QuestCondition.ComplexQuest: return QuestManager.Instance.HasCmpltQuestWithID(CompleteQuest.ID);
                case QuestCondition.HasItem: return BackpackManager.Instance.HasItemWithID(OwnedItem.ID);
                default: return true;
            }
        }
    }
}

public enum QuestCondition
{
    LevelLargeThen,
    LevelLessThen,
    LevelLargeOrEqualsThen,
    LevelLessOrEqualsThen,
    ComplexQuest,
    HasItem
}
#endregion

#region 任务目标
/// <summary>
/// 任务目标
/// </summary>
public abstract class Objective
{
    [SerializeField]
    private string displayName = string.Empty;
    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    [SerializeField]
    private int amount = 1;
    public int Amount
    {
        get
        {
            return amount;
        }
    }

    private int currentAmount;
    public int CurrentAmount
    {
        get
        {
            return currentAmount;
        }
        set
        {
            bool befCmplt = IsComplete;
            if (value < amount && value >= 0)
                currentAmount = value;
            else if (value < 0)
            {
                currentAmount = 0;
            }
            else currentAmount = amount;
            if (!befCmplt && IsComplete)
                UpdateNextCollectObjectives();
        }
    }

    public bool IsComplete
    {
        get
        {
            if (currentAmount >= amount)
                return true;
            return false;
        }
    }

    [SerializeField]
    private bool inOrder;
    public bool InOrder
    {
        get
        {
            return inOrder;
        }
    }

    [SerializeField]
    private int orderIndex;
    public int OrderIndex
    {
        get
        {
            return orderIndex;
        }
    }

    [System.NonSerialized]
    public Objective PrevObjective;
    [System.NonSerialized]
    public Objective NextObjective;

    [HideInInspector]
    public string runtimeID;

    [HideInInspector]
    public Quest runtimeParent;

    protected virtual void UpdateAmountUp(int amount = 1)
    {
        if (IsComplete) return;
        if (!InOrder) CurrentAmount += amount;
        else if (AllPrevObjCmplt) CurrentAmount += amount;
        if (CurrentAmount > 0)
        {
            string message = DisplayName + (IsComplete ? "(完成)" : "[" + currentAmount + "/" + Amount + "]");
            MessageManager.Instance.NewMessage(message);
        }
        if (runtimeParent.IsComplete)
            MessageManager.Instance.NewMessage("[任务]" + runtimeParent.Title + "(已完成)");
    }

    public bool AllPrevObjCmplt//判定所有前置目标是否都完成
    {
        get
        {
            Objective tempObj = PrevObjective;
            while (tempObj != null)
            {
                if (!tempObj.IsComplete && tempObj.OrderIndex < OrderIndex)
                {
                    return false;
                }
                tempObj = tempObj.PrevObjective;
            }
            return true;
        }
    }
    public bool HasNextObjOngoing//判定是否有后置目标正在进行
    {
        get
        {
            Objective tempObj = NextObjective;
            while (tempObj != null)
            {
                if (tempObj.CurrentAmount > 0 && tempObj.OrderIndex > OrderIndex)
                {
                    return true;
                }
                tempObj = tempObj.NextObjective;
            }
            return false;
        }
    }

    /// <summary>
    /// 更新某个收集类任务目标，用于在其他前置目标完成时，更新后置收集类目标
    /// </summary>
    void UpdateNextCollectObjectives()
    {
        Objective tempObj = NextObjective;
        CollectObjective co;
        while (tempObj != null)
        {
            if (tempObj is CollectObjective)
            {
                co = tempObj as CollectObjective;
                co.CurrentAmount = BackpackManager.Instance.GetItemAmountByID(co.Item.ID);
            }
            tempObj = tempObj.NextObjective;
            co = null;
        }
    }
}
/// <summary>
/// 收集类目标
/// </summary>
[System.Serializable]
public class CollectObjective : Objective
{
    [SerializeField]
    private ItemBase item;
    public ItemBase Item
    {
        get
        {
            return item;
        }
    }

    [SerializeField]
    private bool checkBagAtAcpt = true;//用于标识是否在接取任务时检查背包道具看是否满足目标，否则目标重头开始计数
    public bool CheckBagAtAcpt
    {
        get
        {
            return checkBagAtAcpt;
        }
    }
    [SerializeField]
    private bool loseItemAtSbmt = true;//用于标识是否在提交任务时失去相应道具
    public bool LoseItemAtSbmt
    {
        get
        {
            return loseItemAtSbmt;
        }
    }

    public void UpdateCollectAmountUp(ItemBase item, int leftAmount)//得道具时用到
    {
        if (item == Item)
        {
            UpdateAmountUp(leftAmount);
        }
    }

    public void UpdateCollectAmountDown(ItemBase item, int leftAmount)//丢道具时用到
    {
        if (item == Item)
        {
            //前置目标都完成且没有后置目标在进行时，才允许更新
            if (AllPrevObjCmplt && !HasNextObjOngoing) CurrentAmount = leftAmount;
        }
    }
}
/// <summary>
/// 打怪类目标
/// </summary>
[System.Serializable]
public class KillObjective : Objective
{
    //[SerializeField]
    //private string enemyID;
    //public string EnemyID
    //{
    //    get
    //    {
    //        return enemyID;
    //    }
    //}

    [SerializeField]
    private EnemyInfomation enemy;
    public EnemyInfomation Enemy
    {
        get
        {
            return enemy;
        }
    }

    public void UpdateKillAmount()
    {
        UpdateAmountUp();
    }
}
/// <summary>
/// 谈话类目标
/// </summary>
[System.Serializable]
public class TalkObjective : Objective
{
    [SerializeField]
    private TalkerInfomation talker;
    public TalkerInfomation Talker
    {
        get
        {
            return talker;
        }
    }

    [SerializeField]
    private Dialogue dialogue;
    public Dialogue Dialogue
    {
        get
        {
            return dialogue;
        }
    }

    public void UpdateTalkStatus()
    {
        UpdateAmountUp();
    }
}
/// <summary>
/// 移动到点类目标
/// </summary>
[System.Serializable]
public class MoveObjective : Objective
{
    [SerializeField]
    private string pointID = string.Empty;
    public string PointID
    {
        get
        {
            return pointID;
        }
    }

    public void UpdateMoveStatus(QuestPoint point)
    {
        if (point.ID == PointID)
            UpdateAmountUp();
    }
}
#endregion