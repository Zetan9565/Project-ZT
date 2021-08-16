using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "quest", menuName = "Zetan Studio/任务/任务", order = 1)]
public class Quest : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField, TextArea(2, 3)]
    private string title = string.Empty;
    public string Title => title;

    public new string name => title;

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
    private ConditionGroup acceptCondition = new ConditionGroup();
    public ConditionGroup AcceptCondition => acceptCondition;

    [SerializeField]
    private QuestType questType;
    public QuestType QuestType => questType;

    [SerializeField]
    private int repeatFrequancy = 1;
    public int RepeatFrequancy => repeatFrequancy;

    [SerializeField]
    private TimeUnit timeUnit = TimeUnit.Day;
    public TimeUnit TimeUnit => timeUnit;

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
    private List<ItemInfoBase> rewardItems = new List<ItemInfoBase>();
    public List<ItemInfoBase> RewardItems => rewardItems;

    [SerializeField]
    private TalkerInformation _NPCToSubmit;
    public TalkerInformation NPCToSubmit => _NPCToSubmit;

    [SerializeField]
    private bool cmpltObjctvInOrder = false;
    public bool CmpltObjctvInOrder => cmpltObjctvInOrder;

    [SerializeField, NonReorderable]
    private List<CollectObjective> collectObjectives = new List<CollectObjective>();
    public List<CollectObjective> CollectObjectives => collectObjectives;

    [SerializeField, NonReorderable]
    private List<KillObjective> killObjectives = new List<KillObjective>();
    public List<KillObjective> KillObjectives => killObjectives;

    [SerializeField, NonReorderable]
    private List<TalkObjective> talkObjectives = new List<TalkObjective>();
    public List<TalkObjective> TalkObjectives => talkObjectives;

    [SerializeField, NonReorderable]
    private List<MoveObjective> moveObjectives = new List<MoveObjective>();
    public List<MoveObjective> MoveObjectives => moveObjectives;

    [SerializeField, NonReorderable]
    private List<SubmitObjective> submitObjectives = new List<SubmitObjective>();
    public List<SubmitObjective> SubmitObjectives => submitObjectives;

    [SerializeField, NonReorderable]
    private List<CustomObjective> customObjectives = new List<CustomObjective>();
    public List<CustomObjective> CustomObjectives => customObjectives;

    public static string GetAutoID(int length = 3)
    {
        string newID = string.Empty;
        var len = Mathf.Pow(10, length);
        Quest[] all = Resources.LoadAll<Quest>("Configuration");
        for (int i = 1; i < len; i++)
        {
            newID = "QEST" + i.ToString().PadLeft(length, '0');
            if (!Array.Exists(all, x => x.ID == newID))
                break;
        }
        return newID;
    }

    public static bool IsIDDuplicate(Quest quest, Quest[] all = null)
    {
        if (all == null) all = Resources.LoadAll<Quest>("Configuration");
        Quest find = Array.Find(all, x => x.ID == quest.ID);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != quest || (find == quest && Array.FindAll(all, x => x.ID == quest.ID).Length > 1);
    }
}

public enum QuestType
{
    [InspectorName("普通")]
    Normal,

    [InspectorName("主要")]
    Main,

    [InspectorName("反复")]
    Repeated,
}