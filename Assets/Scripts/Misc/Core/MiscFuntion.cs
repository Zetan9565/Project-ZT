using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZetanStudio.ItemSystem;
using ZetanStudio;

public static class MiscFuntion
{
    public static string HandlingKeyWords(string input, bool color = false)
    {
        StringBuilder output = new StringBuilder();
        StringBuilder keyWordsGetter = new StringBuilder();
        bool startGetting = false;
        for (int i = 0; i < input.Length; i++)
        {
            if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
            else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
            {
                startGetting = false;
                keyWordsGetter.Append(input[i]);
                output.Append(Keywords.Translate(keyWordsGetter.ToString(), color));
                keyWordsGetter.Clear();
            }
            else if (!startGetting) output.Append(input[i]);
            if (startGetting) keyWordsGetter.Append(input[i]);
        }

        return output.ToString();
    }
    public static bool CheckCondition(ConditionGroup group)
    {
        if (!group) return true;
        bool calFailed = false;
        if (string.IsNullOrEmpty(group.Relational)) return group.Conditions.TrueForAll(x => CheckCondition(x));
        if (group.Conditions.Count < 1) calFailed = true;
        else
        {
            var cr = group.Relational.Replace(" ", "").ToCharArray();//删除所有空格才开始计算
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
                    if (c == '(' || c == ')' || c == '|' || c == '&' || c == '!')
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
                    if (index >= 0 && index < group.Conditions.Count)
                        values.Push(CheckCondition(group.Conditions[index]));
                    else
                    {
                        //Debug.Log("return 1");
                        return true;
                    }
                }
                else if (values.Count > 0)
                {
                    if (item == "!") values.Push(!values.Pop());
                    else if (values.Count > 1)
                    {
                        bool right = values.Pop();
                        bool left = values.Pop();
                        if (item == "|") values.Push(left | right);
                        else if (item == "&") values.Push(left & right);
                    }
                }
            }
            if (values.Count == 1)
            {
                //Debug.Log("return 2");
                return values.Pop();
            }

            void GetRPNItem(string item)
            {
                //Debug.Log(item);
                if (item == "!" || item == "&" || item == "|")//遇到运算符
                {
                    char opt = item[0];
                    if (optStack.Count < 1) optStack.Push(opt);//栈空则直接入栈
                    else while (optStack.Count > 0)//栈不空则出栈所有优先级大于或等于opt的运算符后才入栈opt
                        {
                            char top = optStack.Peek();
                            if (top + "" == item || top == '!' || top == '&' && opt == '|')
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
            foreach (Condition con in group.Conditions)
                if (!CheckCondition(con))
                {
                    //Debug.Log("return 4");
                    return false;
                }
            //Debug.Log("return 5");
            return true;
        }
    }
    private static bool CheckCondition(Condition condition)
    {
        switch (condition.Type)
        {
            case ConditionType.CompleteQuest:
                QuestData quest = QuestManager.FindQuest(condition.RelatedQuest.ID);
                return quest && TimeManager.Instance.Days - quest.latestHandleDays >= condition.IntValue && quest.IsSubmitted;
            case ConditionType.AcceptQuest:
                quest = QuestManager.FindQuest(condition.RelatedQuest.ID);
                return quest && TimeManager.Instance.Days - quest.latestHandleDays >= condition.IntValue && quest.InProgress;
            case ConditionType.HasItem:
                return BackpackManager.Instance.HasItemWithID(condition.RelatedItem.ID);
            case ConditionType.Level:
                switch (condition.CompareType)
                {
                    case ValueCompareType.Equals:
                        return PlayerManager.Instance.PlayerInfo.level == condition.IntValue;
                    case ValueCompareType.LargeThen:
                        return PlayerManager.Instance.PlayerInfo.level > condition.IntValue;
                    case ValueCompareType.LessThen:
                        return PlayerManager.Instance.PlayerInfo.level < condition.IntValue;
                    default:
                        return true;
                }
            case ConditionType.TriggerSet:
                var state = TriggerManager.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist && (state == TriggerState.On);
            case ConditionType.TriggerReset:
                state = TriggerManager.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist && (state == TriggerState.Off);
            default: return true;
        }
    }

    public static string GetColorAmountString(int current, int target, Color? enough = null, Color? lack = null)
    {
        return $"{ZetanUtility.ColorText(current, current >= target ? enough ?? Color.green : lack ?? Color.red)}/{target}";
    }

    public static string ToChineseSortNum(long value)
    {
        decimal temp = value / 100000000m;
        if (temp >= 1)
        {
            return $"{temp:#0.#}亿";
        }
        temp = value / 10000m;
        if (temp >= 1)
        {
            return $"{temp:#0.#}万";
        }
        return value.ToString();
    }

    public static string SecondsToSortTime(float seconds, string dayStr = "天", string hourStr = "时", string minuStr = "分", string secStr = "秒")
    {
        if (!string.IsNullOrEmpty(dayStr))
        {
            const float day = 86400f;
            if (seconds >= day) return Mathf.CeilToInt(seconds / day) + dayStr;
        }
        if (!string.IsNullOrEmpty(hourStr))
        {
            const float hour = 3600f;
            if (seconds >= hour) return Mathf.CeilToInt(seconds / hour) + hourStr;
        }
        if (!string.IsNullOrEmpty(minuStr))
        {
            const float minute = 60f;
            if (seconds >= minute) return Mathf.CeilToInt(seconds / minute) + minuStr;
        }
        return Mathf.CeilToInt(seconds) + secStr;
    }
}