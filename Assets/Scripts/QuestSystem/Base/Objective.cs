using UnityEngine;

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
    public bool ShowMapIcon => (this is CollectObjective || this is CustomObjective) ? false : showMapIcon;

    [SerializeField]
    private int amount = 1;
    public int Amount => amount;

    [SerializeField]
    private bool inOrder;
    public bool InOrder => inOrder;

    [SerializeField]
    private int orderIndex = 1;
    public int OrderIndex => orderIndex;

    public virtual bool IsValid
    {
        get
        {
            return amount > 0;
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

    public override bool IsValid
    {
        get
        {
            return base.IsValid && item;
        }
    }
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

    [SerializeField]
    private EnemyGroup group;
    public EnemyGroup Group => group;

    public override bool IsValid
    {
        get
        {
            if (objectiveType == KillObjectiveType.Specific && !enemy)
                return false;
            else if (objectiveType == KillObjectiveType.Race && !race)
                return false;
            else if (objectiveType == KillObjectiveType.Group && !group)
                return false;
            else return base.IsValid;
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
    /// 特定组合
    /// </summary>
    [InspectorName("特定组合")]
    Group,

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

    public override bool IsValid
    {
        get
        {
            return base.IsValid && _NPCToTalk && dialogue;
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
    private CheckPointInformation checkPoint;
    public CheckPointInformation CheckPoint
    {
        get
        {
            return checkPoint;
        }
    }

    public override bool IsValid
    {
        get
        {
            return base.IsValid && CheckPoint;
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

    public override bool IsValid
    {
        get
        {
            return base.IsValid && _NPCToSubmit && itemToSubmit && !string.IsNullOrEmpty(wordsWhenSubmit);
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

    public override bool IsValid
    {
        get
        {
            return base.IsValid && !string.IsNullOrEmpty(triggerName);
        }
    }
}