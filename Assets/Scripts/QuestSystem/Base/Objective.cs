using UnityEngine;
using System;
using ZetanStudio.ItemSystem;

/// <summary>
/// 任务目标
/// </summary>
[Serializable]
public abstract class Objective
{
    [SerializeField]
    protected string displayName = string.Empty;
    public string DisplayName => displayName;

    [SerializeField]
    protected bool display = true;
    public bool Display
    {
        get
        {
            return display;
        }
    }

    [SerializeField]
    protected bool canNavigate = true;
    public bool CanNavigate => this is CollectObjective || this is TriggerObjective ? false : canNavigate;

    [SerializeField]
    protected bool showMapIcon = true;
    public bool ShowMapIcon => (this is CollectObjective || this is TriggerObjective) ? false : showMapIcon;

    [SerializeField]
    protected DestinationInformation auxiliaryPos;
    /// <summary>
    /// 辅助位置，用于地图图标、导航等
    /// </summary>
    public DestinationInformation AuxiliaryPos
    {
        get
        {
            return auxiliaryPos;
        }
    }

    [SerializeField]
    protected int amount = 1;
    public int Amount => amount;

    [SerializeField]
    protected bool showAmount = true;
    public bool ShowAmount => showAmount;

    [SerializeField]
    protected bool inOrder;
    public bool InOrder => inOrder;

    [SerializeField]
    protected int priority = 1;
    public int Priority => priority;

    public abstract ObjectiveData CreateData();

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

    public sealed class NameAttribute : Attribute
    {
        public readonly string name;

        public NameAttribute(string name)
        {
            this.name = name;
        }
    }
}
/// <summary>
/// 收集类目标
/// </summary>
[Serializable, Name("收集目标")]
public class CollectObjective : Objective
{
    [SerializeField]
    private Item itemToCollect;
    public Item ItemToCollect => itemToCollect;

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

    public override ObjectiveData CreateData()
    {
        return new CollectObjectiveData(this);
    }

    public override bool IsValid
    {
        get
        {
            return base.IsValid && itemToCollect && (!showMapIcon || showMapIcon && auxiliaryPos);
        }
    }
}
/// <summary>
/// 打怪类目标
/// </summary>
[Serializable, Name("杀敌目标")]
public class KillObjective : Objective
{
    [SerializeField]
    private KillObjectiveType killType;
    public KillObjectiveType KillType
    {
        get
        {
            return killType;
        }
    }

    [SerializeField, ObjectSelector(memberAsGroup: "race")]
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

    public override ObjectiveData CreateData()
    {
        return new KillObjectiveData(this);
    }

    public override bool IsValid
    {
        get
        {
            if (killType == KillObjectiveType.Specific && !enemy)
                return false;
            else if (killType == KillObjectiveType.Race && !race)
                return false;
            else if (killType == KillObjectiveType.Group && !group)
                return false;
            else return base.IsValid && (!showMapIcon || showMapIcon && auxiliaryPos);
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
[Serializable, Name("谈话目标")]
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

    public TalkObjective()
    {
        showAmount = false;
    }

    public override ObjectiveData CreateData()
    {
        return new TalkObjectiveData(this);
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
[Serializable, Name("检查点目标")]
public class MoveObjective : Objective
{
    [SerializeField]
    public new CheckPointInformation AuxiliaryPos => auxiliaryPos as CheckPointInformation;

    [SerializeField]
    private Item itemToUseHere;
    public Item ItemToUseHere => itemToUseHere;

    public MoveObjective()
    {
        showAmount = false;
    }

    public override ObjectiveData CreateData()
    {
        return new MoveObjectiveData(this);
    }

    public override bool IsValid
    {
        get
        {
            return base.IsValid && AuxiliaryPos;
        }
    }
}
/// <summary>
/// 提交类目标
/// </summary>
[Serializable, Name("道具提交目标")]
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
    private Item itemToSubmit;
    public Item ItemToSubmit
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

    public override ObjectiveData CreateData()
    {
        return new SubmitObjectiveData(this);
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
/// 触发器目标
/// </summary>
[Serializable, Name("触发器目标")]
public class TriggerObjective : Objective
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
    private bool stateToCheck;
    public bool StateToCheck => stateToCheck;

    [SerializeField]
    private bool checkStateAtAcpt = true;//用于标识是否在接取任务时检触发器状态看是否满足目标，否则目标重头开始等待触发
    public bool CheckStateAtAcpt
    {
        get
        {
            return checkStateAtAcpt;
        }
    }

    public override ObjectiveData CreateData()
    {
        return new TriggerObjectiveData(this);
    }

    public override bool IsValid
    {
        get
        {
            return base.IsValid && !string.IsNullOrEmpty(triggerName);
        }
    }
}