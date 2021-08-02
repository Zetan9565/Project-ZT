using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Condition
{
    [SerializeField]
    private ConditionType type = ConditionType.CompleteQuest;
    public ConditionType Type => type;

    [SerializeField]
    private int level = 1;
    public int Level => level;

    [SerializeField]
    private Quest relatedQuest;
    public Quest RelatedQuest => relatedQuest;

    [SerializeField]
    private ItemBase relatedItem;
    public ItemBase RelatedItem => relatedItem;

    [SerializeField]
    private string triggerName;
    public string TriggerName => triggerName;

    public static implicit operator bool(Condition self)
    {
        return self != null;
    }
}

public enum ConditionType
{
    [InspectorName("等级等于")]
    LevelEquals,

    [InspectorName("等级大于")]
    LevelLargeThen,

    [InspectorName("等级小于")]
    LevelLessThen,

    [InspectorName("完成任务")]
    CompleteQuest,

    [InspectorName("接取任务")]
    AcceptQuest,

    [InspectorName("拥有道具")]
    HasItem,

    [InspectorName("触发器置位")]
    TriggerSet,

    [InspectorName("触发器复位")]
    TriggerReset
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

    public bool IsMeet()
    {
        bool calFailed = false;
        if (string.IsNullOrEmpty(Relational)) return Conditions.TrueForAll(x => IsConditionMeet(x));
        if (Conditions.Count < 1) calFailed = true;
        else
        {
            var cr = Relational.Replace(" ", "").ToCharArray();//删除所有空格才开始计算
            List<string> RPN = new List<string>();//逆波兰表达式
            string indexStr = string.Empty;//数字串
            Stack<char> optStack = new Stack<char>();//运算符栈
            for (int i = 0; i < cr.Length; i++)
            {
                char c = cr[i];
                string item;
                if (c < '0' || c > '9')
                {
                    if (!string.IsNullOrEmpty(indexStr))
                    {
                        item = indexStr;
                        indexStr = string.Empty;
                        GetRPNItem(item);
                    }
                    if (c == '(' || c == ')' || c == '+' || c == '*' || c == '~')
                    {
                        item = c + "";
                        GetRPNItem(item);
                    }
                    else
                    {
                        calFailed = true;
                        break;
                    }//既不是数字也不是运算符，直接放弃计算
                }
                else
                {
                    indexStr += c;//拼接数字
                    if (i + 1 >= cr.Length)
                    {
                        item = indexStr;
                        indexStr = string.Empty;
                        GetRPNItem(item);
                    }
                }
            }
            while (optStack.Count > 0)
                RPN.Add(optStack.Pop() + "");
            Stack<bool> values = new Stack<bool>();
            foreach (var item in RPN)
            {
                //Debug.Log(item);
                if (int.TryParse(item, out int index))
                {
                    if (index >= 0 && index < Conditions.Count)
                        values.Push(IsConditionMeet(Conditions[index]));
                    else
                    {
                        //Debug.Log("return 1");
                        return true;
                    }
                }
                else if (values.Count > 1)
                {
                    if (item == "+") values.Push(values.Pop() | values.Pop());
                    else if (item == "~") values.Push(!values.Pop());
                    else if (item == "*") values.Push(values.Pop() & values.Pop());
                }
                else if (item == "~") values.Push(!values.Pop());
            }
            if (values.Count == 1)
            {
                //Debug.Log("return 2");
                return values.Pop();
            }

            void GetRPNItem(string item)
            {
                //Debug.Log(item);
                if (item == "+" || item == "*" || item == "~")//遇到运算符
                {
                    char opt = item[0];
                    if (optStack.Count < 1) optStack.Push(opt);//栈空则直接入栈
                    else while (optStack.Count > 0)//栈不空则出栈所有优先级大于或等于opt的运算符后才入栈opt
                        {
                            char top = optStack.Peek();
                            if (top + "" == item || top == '~' || top == '*' && opt == '+')
                            {
                                RPN.Add(optStack.Pop() + "");
                                if (optStack.Count < 1)
                                {
                                    optStack.Push(opt);
                                    break;
                                }
                            }
                            else
                            {
                                optStack.Push(opt);
                                break;
                            }
                        }
                }
                else if (item == "(") optStack.Push('(');
                else if (item == ")")
                {
                    while (optStack.Count > 0)
                    {
                        char opt = optStack.Pop();
                        if (opt == '(') break;
                        else RPN.Add(opt + "");
                    }
                }
                else if (int.TryParse(item, out _)) RPN.Add(item);//遇到数字
            }
        }
        if (!calFailed)
        {
            //Debug.Log("return 3");
            return true;
        }
        else
        {
            foreach (Condition con in Conditions)
                if (!IsConditionMeet(con))
                {
                    //Debug.Log("return 4");
                    return false;
                }
            //Debug.Log("return 5");
            return true;
        }
    }
    /// <summary>
    /// 条件是否符合
    /// </summary>
    private bool IsConditionMeet(Condition condition)
    {
        switch (condition.Type)
        {
            case ConditionType.CompleteQuest: return QuestManager.Instance.HasCompleteQuestWithID(condition.RelatedQuest.ID);
            case ConditionType.AcceptQuest: return QuestManager.Instance.HasOngoingQuestWithID(condition.RelatedQuest.ID);
            case ConditionType.HasItem: return BackpackManager.Instance.HasItemWithID(condition.RelatedItem.ID);
            case ConditionType.LevelEquals: return PlayerManager.Instance.PlayerInfo.level == condition.Level;
            case ConditionType.LevelLargeThen: return PlayerManager.Instance.PlayerInfo.level > condition.Level;
            case ConditionType.LevelLessThen: return PlayerManager.Instance.PlayerInfo.level < condition.Level;
            case ConditionType.TriggerSet:
                var state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist && (state == TriggerState.On);
            case ConditionType.TriggerReset:
                state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist && (state == TriggerState.Off);
            default: return true;
        }
    }
}