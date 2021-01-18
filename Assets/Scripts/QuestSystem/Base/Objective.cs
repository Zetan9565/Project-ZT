using UnityEngine;

public delegate void ObjectiveStateListner(ObjectiveData objective, bool cmpltStateBef);
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
            return display;
        }
    }

    [SerializeField]
    private bool canNavigate = true;
    public bool CanNavigate => this is CollectObjective || this is CustomObjective ? false : canNavigate;

    [SerializeField]
    private bool showMapIcon = true;
    public bool ShowMapIcon => this is CollectObjective || this is CustomObjective ? false : showMapIcon || canNavigate;//可以导航，则一定能显示地图图标

    [SerializeField]
    private int amount = 1;
    public int Amount => amount;

    [SerializeField]
    private bool inOrder;
    public bool InOrder => inOrder;

    [SerializeField]
    private int orderIndex = 1;
    public int OrderIndex => orderIndex;

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
    public ItemBase Item => item;

    [SerializeField]
    private bool checkBagAtStart = true;
    /// <summary>
    /// 是否在目标开始执行时检查背包道具看是否满足目标，否则目标重头开始计数
    /// </summary>
    public bool CheckBagAtStart => checkBagAtStart;

    [SerializeField]
    private bool loseItemAtSbmt = true;
    /// <summary>
    /// 是否在提交任务时失去相应道具
    /// </summary>
    public bool LoseItemAtSbmt => loseItemAtSbmt;
}
/// <summary>
/// 打怪类目标
/// </summary>
[System.Serializable]
public class KillObjective : Objective
{
    [SerializeField]
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
}
public enum KillObjectiveType
{
    /// <summary>
    /// 特定敌人
    /// </summary>
    [InspectorName("特定敌人")]
    Specific,

    /// <summary>
    /// 特定种族
    /// </summary>
    [InspectorName("特定种族")]
    Race,

    /// <summary>
    /// 任意
    /// </summary>
    [InspectorName("任意敌人")]
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
    private TalkerType talkerType;
    public TalkerType TalkerType
    {
        get
        {
            return talkerType;
        }
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
}