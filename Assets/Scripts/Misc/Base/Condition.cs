using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.Item;

[System.Serializable]
public class Condition
{
    [SerializeField]
    private ConditionType type = ConditionType.CompleteQuest;
    public ConditionType Type => type;

    [SerializeField]
    private int intValue = 0;
    public int IntValue => intValue;

    [SerializeField]
    private ValueCompareType compareType;
    public ValueCompareType CompareType => compareType;

    [SerializeField, ObjectSelector("title")]
    private Quest relatedQuest;
    public Quest RelatedQuest => relatedQuest;

    [SerializeField]
    private Item relatedItem;
    public Item RelatedItem => relatedItem;

    [SerializeField]
    private CharacterInformation relatedCharInfo;
    public CharacterInformation RelateCharInfo => relatedCharInfo;

    [SerializeField]
    private string triggerName;
    public string TriggerName => triggerName;

    public bool IsValid
    {
        get
        {
            switch (type)
            {
                case ConditionType.Level:
                    return intValue > 0;
                case ConditionType.CompleteQuest:
                case ConditionType.AcceptQuest:
                    return relatedQuest;
                case ConditionType.HasItem:
                    return relatedItem;
                case ConditionType.TriggerSet:
                case ConditionType.TriggerReset:
                    return !string.IsNullOrEmpty(triggerName);
                case ConditionType.NPCIntimacy:
                    return relatedCharInfo;
                default:
                    return true;
            }
        }
    }

    public static implicit operator bool(Condition self)
    {
        return self != null;
    }
}

public enum ValueCompareType
{
    [InspectorName("等于")]
    Equals,
    [InspectorName("大于")]
    LargeThen,
    [InspectorName("小于")]
    LessThen,
}

public enum ConditionType
{
    [InspectorName("等级")]
    Level,

    [InspectorName("完成任务")]
    CompleteQuest,

    [InspectorName("接取任务")]
    AcceptQuest,

    [InspectorName("拥有道具")]
    HasItem,

    [InspectorName("触发器置位")]
    TriggerSet,

    [InspectorName("触发器复位")]
    TriggerReset,

    [InspectorName("亲密度")]
    NPCIntimacy,
}

[System.Serializable]
public class ConditionGroup
{
    [SerializeField]
    private List<Condition> conditions = new List<Condition>();
    public List<Condition> Conditions => conditions;

    [SerializeField]
    [Tooltip("1、操作数为条件的序号\n2、运算符可使用 \"(\"、\")\"、\"+\"(或)、\"*\"(且)、\"~\"(非)" +
                        "\n3、未对非法输入进行处理，需规范填写\n4、例：(0 + 1) * ~2 表示满足条件0或1且不满足条件2\n5、为空时默认进行相互的“且”运算")]
    private string relational;
    public string Relational => relational;

    public bool IsValid => conditions.TrueForAll(x => x.IsValid);

    public static implicit operator bool(ConditionGroup self)
    {
        return self != null;
    }
}