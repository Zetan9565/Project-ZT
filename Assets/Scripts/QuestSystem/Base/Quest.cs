using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "quest", menuName = "ZetanStudio/任务/任务", order = 1)]
public class Quest : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField, TextArea(2, 3)]
    private string title = string.Empty;
    public string Title => title;

    [SerializeField, TextArea(5, 5)]
    private string description;
    public string Description => description;

    [SerializeField]
    private bool abandonable = true;
    public bool Abandonable => abandonable;

    [SerializeField]
    private QuestGroup group;
    public QuestGroup Group => group;

    [SerializeField]
    private List<QuestAcceptCondition> acceptConditions = new List<QuestAcceptCondition>();
    public List<QuestAcceptCondition> AcceptConditions => acceptConditions;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("普通", "主线", "反复")]
#endif
    private QuestType questType;
    public QuestType QuestType => questType;

    [SerializeField]
    private int repeatFrequancy = 1;
    public int RepeatFrequancy => repeatFrequancy;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("分", "时", "天", "周", "月", "年")]
#endif
    private TimeUnit timeUnit = TimeUnit.Day;
    public TimeUnit TimeUnit => timeUnit;

    [SerializeField]
    private string conditionRelational;
    public string ConditionRelational => conditionRelational;

    [SerializeField]
    private Dialogue beginDialogue;
    public Dialogue BeginDialogue => beginDialogue;
    [SerializeField]
    private Dialogue ongoingDialogue;
    public Dialogue OngoingDialogue => ongoingDialogue;
    [SerializeField]
    private Dialogue completeDialogue;
    public Dialogue CompleteDialogue => completeDialogue;

    [SerializeField]
    private int rewardMoney;
    public int RewardMoney => rewardMoney;

    [SerializeField]
    private int rewardEXP;
    public int RewardEXP => rewardEXP;

    [SerializeField]
    private List<ItemInfo> rewardItems = new List<ItemInfo>();
    public List<ItemInfo> RewardItems => rewardItems;

    [SerializeField]
    private TalkerInformation _NPCToSubmit;
    public TalkerInformation NPCToSubmit => _NPCToSubmit;

    [SerializeField]
    private bool cmpltObjctvInOrder = false;
    public bool CmpltObjctvInOrder => cmpltObjctvInOrder;

    public List<Objective> ObjectiveInstances { get; } = new List<Objective>();

    [SerializeField]
    private List<CollectObjective> collectObjectives = new List<CollectObjective>();
    public List<CollectObjective> CollectObjectives => collectObjectives;

    [SerializeField]
    private List<KillObjective> killObjectives = new List<KillObjective>();
    public List<KillObjective> KillObjectives => killObjectives;

    [SerializeField]
    private List<TalkObjective> talkObjectives = new List<TalkObjective>();
    public List<TalkObjective> TalkObjectives => talkObjectives;

    [SerializeField]
    private List<MoveObjective> moveObjectives = new List<MoveObjective>();
    public List<MoveObjective> MoveObjectives => moveObjectives;

    [SerializeField]
    private List<SubmitObjective> submitObjectives = new List<SubmitObjective>();
    public List<SubmitObjective> SubmitObjectives => submitObjectives;

    [SerializeField]
    private List<CustomObjective> customObjectives = new List<CustomObjective>();
    public List<CustomObjective> CustomObjectives => customObjectives;

    [HideInInspector]
    public TalkerData originalQuestHolder;

    [HideInInspector]
    public TalkerData currentQuestHolder;

    public bool IsOngoing { get; set; }//任务是否正在执行，在运行时用到

    public bool IsComplete
    {
        get
        {
            if (ObjectiveInstances.Exists(x => !x.IsComplete))
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
}

public enum QuestType
{
    Normal,
    Main,
    Repeated,
}

#region 任务条件
/// <summary>
/// 任务接取条件
/// </summary>
[System.Serializable]
public class QuestAcceptCondition
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("等级等于", "等级大于", "等级小于", "完成任务", "拥有道具", "触发器开启", "触发器关闭")]
#endif
    private QuestCondition acceptCondition = QuestCondition.CompleteQuest;
    public QuestCondition AcceptCondition => acceptCondition;

    [SerializeField]
    private int level = 1;
    public int Level => level;

    [SerializeField]
    private Quest completeQuest;
    public Quest CompleteQuest => completeQuest;

    [SerializeField]
    private ItemBase ownedItem;
    public ItemBase OwnedItem => ownedItem;

    [SerializeField]
    private string triggerName;
    public string TriggerName => triggerName;
}

public enum QuestCondition
{
    LevelEquals,
    LevelLargeThen,
    LevelLessThen,
    CompleteQuest,
    HasItem,
    TriggerSet,
    TriggerReset
}
#endregion

#region 任务目标
public delegate void ObjectiveStateListner(Objective objective, bool cmpltStateBef);
/// <summary>
/// 任务目标
/// </summary>
public abstract class Objective
{
    [SerializeField]
    private string displayName = string.Empty;
    public string DisplayName => displayName;

    [SerializeField]
    private bool display = true;
    public bool Display
    {
        get
        {
            if (runtimeParent && !runtimeParent.CmpltObjctvInOrder)
                return true;
            return display;
        }
    }

    [SerializeField]
    private bool showMapIcon = true;
    public bool ShowMapIcon => this is CollectObjective || this is CustomObjective ? false : showMapIcon;

    [SerializeField]
    private int amount = 1;
    public int Amount => amount;

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
            OnStateChangeEvent?.Invoke(this, befCmplt);
        }
    }

    [SerializeField]
    private bool inOrder;
    public bool InOrder => inOrder;

    [SerializeField]
    private int orderIndex = 1;
    public int OrderIndex => orderIndex;

    public bool IsComplete
    {
        get
        {
            if (currentAmount >= amount)
                return true;
            return false;
        }
    }

    public bool IsValid
    {
        get
        {
            if (Amount < 0) return false;
            if (this is CollectObjective && !(this as CollectObjective).Item)
                return false;
            if (this is KillObjective)
            {
                var ko = this as KillObjective;
                if (ko.ObjectiveType == KillObjectiveType.Specific && !ko.Enemy)
                    return false;
                else if (ko.ObjectiveType == KillObjectiveType.Race && !ko.Race)
                    return false;
            }
            if (this is TalkObjective && (!(this as TalkObjective).NPCToTalk || !(this as TalkObjective).Dialogue))
                return false;
            if (this is MoveObjective && string.IsNullOrEmpty((this as MoveObjective).PointID))
                return false;
            if (this is SubmitObjective)
            {
                var so = this as SubmitObjective;
                if (!so.NPCToSubmit || !so.ItemToSubmit || string.IsNullOrEmpty(so.WordsWhenSubmit))
                    return false;
            }
            if (this is CustomObjective && string.IsNullOrEmpty((this as CustomObjective).TriggerName))
                return false;
            return true;
        }
    }

    public Objective PrevObjective;
    public Objective NextObjective;

    [HideInInspector]
    public string runtimeID;

    [HideInInspector]
    public Quest runtimeParent;

    [HideInInspector]
    public ObjectiveStateListner OnStateChangeEvent;

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
    /// 可并行？
    /// </summary>
    public bool Parallel
    {
        get
        {
            if (!InOrder) return true;//不按顺序，说明可以并行执行
            if (PrevObjective && PrevObjective.OrderIndex == OrderIndex) return true;//有前置目标，而且顺序码与前置目标相同，说明可以并行执行
            if (NextObjective && NextObjective.OrderIndex == OrderIndex) return true;//有后置目标，而且顺序码与后置目标相同，说明可以并行执行
            return false;
        }
    }

    public static implicit operator bool(Objective self)
    {
        return self != null;
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
    private bool checkBagAtStart = true;
    /// <summary>
    /// 是否在目标开始执行时检查背包道具看是否满足目标，否则目标重头开始计数
    /// </summary>
    public bool CheckBagAtStart
    {
        get
        {
            return checkBagAtStart;
        }
    }

    [SerializeField]
    private bool loseItemAtSbmt = true;
    /// <summary>
    /// 是否在提交任务时失去相应道具
    /// </summary>
    public bool LoseItemAtSbmt
    {
        get
        {
            return loseItemAtSbmt;
        }
    }

    public void UpdateCollectAmount(ItemBase item, int leftAmount)//得道具时用到
    {
        if (item == Item)
        {
            if (IsComplete) return;
            if (!InOrder) CurrentAmount = leftAmount;
            else if (AllPrevObjCmplt) CurrentAmount = leftAmount;
            if (CurrentAmount > 0)
            {
                string message = DisplayName + (IsComplete ? "(完成)" : "[" + CurrentAmount + "/" + Amount + "]");
                MessageManager.Instance.NewMessage(message);
            }
            if (runtimeParent.IsComplete)
                MessageManager.Instance.NewMessage("[任务]" + runtimeParent.Title + "(已完成)");
        }
    }

    public void UpdateCollectAmountDown(ItemBase item, int leftAmount)//丢道具时用到
    {
        if (item == Item && AllPrevObjCmplt && !HasNextObjOngoing && LoseItemAtSbmt)
            //前置目标都完成且没有后置目标在进行时，才允许更新；在提交任务时不需要提交相应道具，也不会更新减少值。
            CurrentAmount = leftAmount;
    }
}
/// <summary>
/// 打怪类目标
/// </summary>
[System.Serializable]
public class KillObjective : Objective
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("特定敌人", "特定种群", "任意敌人")]
#endif
    private KillObjectiveType objectiveType;
    public KillObjectiveType ObjectiveType
    {
        get
        {
            return objectiveType;
        }
    }

    [SerializeField]
    private EnemyInformation enemy;
    public EnemyInformation Enemy
    {
        get
        {
            return enemy;
        }
    }

    [SerializeField]
    private EnemyRace race;
    public EnemyRace Race
    {
        get
        {
            return race;
        }
    }

    public void UpdateKillAmount()
    {
        UpdateAmountUp();
    }
}
public enum KillObjectiveType
{
    /// <summary>
    /// 特定敌人
    /// </summary>
    Specific,

    /// <summary>
    /// 特定种族
    /// </summary>
    Race,

    /// <summary>
    /// 任意
    /// </summary>
    Any
}
/// <summary>
/// 谈话类目标
/// </summary>
[System.Serializable]
public class TalkObjective : Objective
{
    [SerializeField]
    private TalkerInformation _NPCToTalk;
    public TalkerInformation NPCToTalk
    {
        get
        {
            return _NPCToTalk;
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

    public void UpdateTalkState()
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

    public void UpdateMoveState(QuestPoint point)
    {
        if (point.ID == PointID) UpdateAmountUp();
    }
}
/// <summary>
/// 提交类目标
/// </summary>
[System.Serializable]
public class SubmitObjective : Objective
{
    [SerializeField]
    private TalkerInformation _NPCToSubmit;
    public TalkerInformation NPCToSubmit
    {
        get
        {
            return _NPCToSubmit;
        }
    }

    [SerializeField]
    private ItemBase itemToSubmit;
    public ItemBase ItemToSubmit
    {
        get
        {
            return itemToSubmit;
        }
    }

    [SerializeField]
    private string wordsWhenSubmit;
    public string WordsWhenSubmit
    {
        get
        {
            return wordsWhenSubmit;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("提交处的NPC", "玩家")]
#endif
    private TalkerType talkerType;
    public TalkerType TalkerType
    {
        get
        {
            return talkerType;
        }
    }

    public void UpdateSubmitState(int amount = 1)
    {
        UpdateAmountUp(amount);
    }
}
/// <summary>
/// 自定义目标
/// </summary>
[System.Serializable]
public class CustomObjective : Objective
{
    [SerializeField]
    private string triggerName;
    public string TriggerName
    {
        get
        {
            return triggerName;
        }
    }

    [SerializeField]
    private bool checkStateAtAcpt = true;//用于标识是否在接取任务时检触发器状态看是否满足目标，否则目标重头开始等待触发
    public bool CheckStateAtAcpt
    {
        get
        {
            return checkStateAtAcpt;
        }
    }

    public void UpdateTriggerState(string name, bool state)
    {
        if (name != TriggerName) return;
        if (state) UpdateAmountUp();
        else if (AllPrevObjCmplt && !HasNextObjOngoing) CurrentAmount--;
    }
}
#endregion