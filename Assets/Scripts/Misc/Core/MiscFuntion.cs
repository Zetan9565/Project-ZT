using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class MiscFuntion
{
    public static string HandlingKeyWords(string input, bool color = false, params Array[] configs)
    {
        StringBuilder output = new StringBuilder();
        StringBuilder keyWordsGetter = new StringBuilder();
        bool startGetting = false;
        for (int i = 0; i < input.Length; i++)
        {
            if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{')
            {
                startGetting = true;
                i++;
            }
            else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
            {
                startGetting = false;
                output.Append(HandlingName(keyWordsGetter.ToString()));
                keyWordsGetter.Clear();
            }
            else if (!startGetting) output.Append(input[i]);
            if (startGetting) keyWordsGetter.Append(input[i]);
        }

        return output.ToString();

        string HandlingName(string keyWords)
        {
            if (keyWords.StartsWith("[NPC]"))
            {
                keyWords = keyWords.Replace("[NPC]", string.Empty);
                TalkerInformation[] talkers = null;
                if (configs != null)
                {
                    foreach (var array in configs)
                    {
                        if (array.GetType().IsArray)
                            if (array.GetType().GetElementType() == typeof(TalkerInformation))
                                talkers = array as TalkerInformation[];
                    }
                }
                if (talkers == null) talkers = Resources.LoadAll<TalkerInformation>("Configuration");
                var talker = Array.Find(talkers, x => x.ID == keyWords);
                if (talker) keyWords = talker.name;
                return color ? ZetanUtility.ColorText(keyWords, Color.green) : keyWords;
            }
            else if (keyWords.StartsWith("[ITEM]"))
            {
                keyWords = keyWords.Replace("[ITEM]", string.Empty);
                ItemBase[] items = null;
                if (configs != null)
                {
                    foreach (var array in configs)
                    {
                        if (array.GetType().IsArray)
                            if (array.GetType().GetElementType() == typeof(ItemBase))
                                items = array as ItemBase[];
                    }
                }
                if (items == null) items = Resources.LoadAll<ItemBase>("Configuration");
                var item = Array.Find(items, x => x.ID == keyWords);
                if (item) keyWords = item.name;
                return color ? ZetanUtility.ColorText(keyWords, Color.yellow) : keyWords;
            }
            else if (keyWords.StartsWith("[ENMY]"))
            {
                keyWords = keyWords.Replace("[ENMY]", string.Empty);
                EnemyInformation[] enemies = null;
                if (configs != null)
                {
                    foreach (var array in configs)
                    {
                        if (array.GetType().IsArray)
                            if (array.GetType().GetElementType() == typeof(EnemyInformation))
                                enemies = array as EnemyInformation[];
                    }
                }
                if (enemies == null) enemies = Resources.LoadAll<EnemyInformation>("Configuration");
                var enemy = Array.Find(enemies, x => x.ID == keyWords);
                if (enemy) keyWords = enemy.name;
                return color ? ZetanUtility.ColorText(keyWords, Color.red) : keyWords;
            }
            return keyWords;
        }
    }

    public static string GetFirstWords(Dialogue dialogue)
    {
        if (dialogue&& dialogue.Words.Count > 0)
        {
            return HandlingKeyWords(dialogue.Words[0].ToString());
        }
        return string.Empty;
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
                    if (index >= 0 && index < group.Conditions.Count)
                        values.Push(CheckCondition(group.Conditions[index]));
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
                QuestData quest = QuestManager.Instance.FindQuest(condition.RelatedQuest.ID);
                return quest && TimeManager.Instance.Days - quest.latestHandleDays >= condition.IntValue && quest.IsFinished;
            case ConditionType.AcceptQuest:
                quest = QuestManager.Instance.FindQuest(condition.RelatedQuest.ID);
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
                var state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist && (state == TriggerState.On);
            case ConditionType.TriggerReset:
                state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist && (state == TriggerState.Off);
            default: return true;
        }
    }
}