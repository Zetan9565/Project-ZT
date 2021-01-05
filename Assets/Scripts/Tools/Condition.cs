using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Condition
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("等级等于", "等级大于", "等级小于", "完成任务", "接取任务", "拥有道具", "触发器开启", "触发器关闭")]
#endif
    private ConditionType acceptCondition = ConditionType.CompleteQuest;
    public ConditionType AcceptCondition => acceptCondition;

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

public enum ConditionType
{
    LevelEquals,
    LevelLargeThen,
    LevelLessThen,
    CompleteQuest,
    AcceptQuest,
    HasItem,
    TriggerSet,
    TriggerReset
}

[System.Serializable]
public class ConditionGroup
{
    [SerializeField]
    private List<Condition> conditions = new List<Condition>();
    public List<Condition> Conditions => conditions;

    [SerializeField]
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
        switch (condition.AcceptCondition)
        {
            case ConditionType.CompleteQuest: return QuestManager.Instance.HasCompleteQuestWithID(condition.CompleteQuest.ID);
            case ConditionType.AcceptQuest: return QuestManager.Instance.HasOngoingQuestWithID(condition.CompleteQuest.ID);
            case ConditionType.HasItem: return BackpackManager.Instance.HasItemWithID(condition.OwnedItem.ID);
            case ConditionType.LevelEquals: return PlayerManager.Instance.PlayerInfo.level == condition.Level;
            case ConditionType.LevelLargeThen: return PlayerManager.Instance.PlayerInfo.level > condition.Level;
            case ConditionType.LevelLessThen: return PlayerManager.Instance.PlayerInfo.level < condition.Level;
            case ConditionType.TriggerSet:
                var state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist ? (state == TriggerState.On ? true : false) : false;
            case ConditionType.TriggerReset:
                state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist ? (state == TriggerState.Off ? true : false) : false;
            default: return true;
        }
    }
}