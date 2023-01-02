using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.ConditionSystem
{
    [Serializable]
    public class ConditionGroup
    {
        [SerializeField, HideIf("conditions.Length", 0)]
        [Tooltip("1、操作数为条件的序号\n2、运算符可使用 \"(\"、\")\"、\"|\"(或)、\"&\"(且)、\"!\"(非)" +
                    "\n3、未对非法输入进行处理，需规范填写\n4、例：(0 | 1) * !2 表示满足条件0或1且不满足条件2\n5、为空时默认进行相互的“且”运算")]
        private string relational;
        public string Relational => relational;

        [SerializeReference]
        private Condition[] conditions = { };
        public ReadOnlyCollection<Condition> Conditions => new ReadOnlyCollection<Condition>(conditions);

        public bool IsValid => conditions.All(x => x != null && x.IsValid);

        public bool IsMeet()
        {
            if (string.IsNullOrEmpty(Relational)) return conditions.All(x => x.IsMeet());
            if (conditions.Length > 0)
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
                            getRPNItem(item);
                        }
                        if (c == '(' || c == ')' || c == '|' || c == '&' || c == '!')
                        {
                            item = c + "";
                            getRPNItem(item);
                        }
                        else break;//既不是数字也不是运算符，直接放弃计算
                    }
                    else
                    {
                        indexStr += c;//拼接数字
                        if (i + 1 >= cr.Length)
                        {
                            item = indexStr;
                            indexStr = string.Empty;
                            getRPNItem(item);
                        }
                    }
                }
                while (optStack.Count > 0)
                    RPN.Add(optStack.Pop() + "");
                Stack<bool> values = new Stack<bool>();
                foreach (var item in RPN)
                {
                    if (int.TryParse(item, out int index))
                    {
                        if (index >= 0 && index < conditions.Length)
                        {
                            values.Push(conditions[index].IsMeet());
                        }
                        else  return true;
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
                if (values.Count == 1) return values.Pop();

                void getRPNItem(string item)
                {
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
            foreach (Condition con in conditions)
                if (!con.IsMeet()) return false;
            return true;
        }

        public static implicit operator bool(ConditionGroup obj) => obj != null;
    }
}