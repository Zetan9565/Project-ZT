using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio
{
    public static class Expression
    {
        private readonly static string[] digitOpt = new string[] { "(", ")", "+", "-", "*", "/", "^", "#", "log", "," };
        public static string[] ValidBoolOperators => Array.ConvertAll(boolOpt, o => o);
        public static int ToInt(string expression, params float[] args)
        {
            return (int)CalculateDigit(true, expression, args);
        }
        public static int RoundToInt(string expression, params float[] args)
        {
            return Mathf.RoundToInt(CalculateDigit(true, expression, args));
        }
        public static float ToFloat(string expression, params float[] args)
        {
            return CalculateDigit(false, expression, args);
        }
        private static float CalculateDigit(bool integer, string expression, params float[] args)
        {
            var cr = expression.Replace(" ", "").ToCharArray();//删除所有空格才开始计算
            List<string> RPN = new List<string>();//逆波兰表达式
            string indexStr = string.Empty;//下标串
            string numStr = string.Empty;//数字串
            string undifined = string.Empty;
            Stack<string> optStack = new Stack<string>();//运算符栈
            Dictionary<string, int> priority = new Dictionary<string, int>()
            {
                {"log", 0},
                {"~", 1},
                {"#", 1},
                {"^", 2},
                {"*", 3},
                {"/", 3},
                {"+", 4},
                {"-", 4},
            };
            bool logStart = false;
            for (int i = 0; i < cr.Length; i++)
            {
                char c = cr[i];
                bool opt = isOp(c);
                bool log = isLog(i);
                if (opt || log)
                {
                    checkUndifined();
                    if (!string.IsNullOrEmpty(indexStr))
                        throw new InvalidCastException($"无法识别的数字表达式 {expression}, 错误位置: " + i);
                    if (!string.IsNullOrEmpty(numStr))
                    {
                        GetRPNItem(numStr);
                        numStr = string.Empty;
                    }
                    if ((i > 0 && isOp(cr[i - 1]) || i > 3 && isLog(i - 4) || i == 0) && c == '-') GetRPNItem("~");//负数用'~'代替'-'号
                    else if (opt) GetRPNItem(c.ToString());
                    else if (log)
                    {
                        GetRPNItem("log");
                        logStart = true;
                        i += 2;
                    }
                }
                else if ('0' <= c && c <= '9' || !integer && c == '.')
                {
                    checkUndifined();
                    if (string.IsNullOrEmpty(indexStr))
                    {
                        numStr += c;
                        if (i + 1 >= cr.Length)
                        {
                            GetRPNItem(numStr);
                            numStr = string.Empty;
                        }
                    }
                    else
                    {
                        if (c != '.') indexStr += c;//拼接下标
                        else throw new InvalidCastException($"无法识别的数字表达式 {expression}, 错误位置: " + i);
                    }
                }
                else if (c == '{')
                {
                    checkUndifined();
                    indexStr += c;
                }
                else if (c == '}')
                {
                    checkUndifined();
                    GetRPNItem(indexStr + c);
                    indexStr = string.Empty;
                }
                else if (logStart && c == ',')
                {
                    checkUndifined();
                    if (string.IsNullOrEmpty(indexStr))
                    {
                        GetRPNItem(numStr);
                        numStr = string.Empty;
                    }
                    else throw new InvalidCastException($"无法识别的数字表达式 {expression}, 错误位置: " + i);
                }
                else undifined += c;

                void checkUndifined()
                {
                    if (!string.IsNullOrEmpty(undifined))
                        throw new InvalidCastException("无法识别的符号: " + undifined);
                }
            }
            while (optStack.Count > 0)
                RPN.Add(optStack.Pop());
            Stack<float> values = new Stack<float>();
            for (int i = 0; i < RPN.Count; i++)
            {
                var item = RPN[i];
                if (!integer && float.TryParse(item, out var fv)) values.Push(fv);
                else if (integer && int.TryParse(item, out var iv)) values.Push(iv);
                else if (item.StartsWith('{') && int.TryParse(item.Split('{', '}')[^2], out var index) && index >= 0 && index < args.Length)
                    values.Push(args[index]);
                else if (values.Count > 0)
                {
                    if (item == "log" && values.Count < 2) values.Push(Mathf.Log(values.Pop()));
                    else if (item == "~") values.Push(-values.Pop());
                    else if (item == "#") values.Push(Mathf.Sqrt(values.Pop()));
                    else if (values.Count > 1)
                    {
                        float right = values.Pop();
                        float left = values.Pop();
                        if (item == "log") values.Push(Mathf.Log(left, right));
                        else if (item == "^") values.Push(Mathf.Pow(left, right));
                        else if (item == "*") values.Push(left * right);
                        else if (item == "/") values.Push(left / right);
                        else if (item == "+") values.Push(left + right);
                        else if (item == "-") values.Push(left - right);
                    }
                }
            }
            if (values.Count == 1)
                return values.Pop();
            throw new InvalidCastException($"表达式 {expression} 计算失败");

            void GetRPNItem(string item)
            {
                //Debug.Log(item);
                if (isOp(item))//遇到运算符
                {
                    string opt = item;
                    if (optStack.Count < 1) optStack.Push(opt);//栈空则直接入栈
                    else while (optStack.Count > 0)//栈不空则出栈所有优先级大于或等于opt的运算符后才入栈opt
                        {
                            string top = optStack.Peek();
                            if (isOp(top) && (top == item || priority[top] <= priority[opt]))
                            {
                                RPN.Add(optStack.Pop());
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
                else if (item == "(") optStack.Push(item);
                else if (item == ")")
                {
                    while (optStack.Count > 0)
                    {
                        string opt = optStack.Pop();
                        if (opt == "(") break;
                        else RPN.Add(opt);
                    }
                }
                else if (item.StartsWith('{') || !integer && float.TryParse(item, out _) || integer && int.TryParse(item, out _)) RPN.Add(item);

                static bool isOp(string item)
                {
                    return item == "+" || item == "-" || item == "*" || item == "/" || item == "^" || item == "#" || item == "~" || item == "log";
                }
            }
            bool isLog(int i)
            {
                return i + 4 < cr.Length && char.ToLower(cr[i]) == 'l' && char.ToLower(cr[i + 1]) == 'o' && char.ToLower(cr[i + 2]) == 'g' && cr[i + 3] == '(';
            }
            static bool isOp(char c)
            {
                return c == '(' || c == ')' || c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '#';
            }
        }

        public static string[] ValidDigitOperators => Array.ConvertAll(digitOpt, o => o);

        private readonly static string[] boolOpt = new string[] { "(", ")", "|", "&", "!" };
        public static bool ToBool(string expression, params bool[] args)
        {
            var cr = expression.Replace(" ", "").ToCharArray();//删除所有空格才开始计算
            List<string> RPN = new List<string>();//逆波兰表达式
            string indexStr = string.Empty;//数字串
            Stack<char> optStack = new Stack<char>();//运算符栈

            for (int i = 0; i < cr.Length; i++)
            {
                char c = cr[i];
                string item;
                if (isOp(c))
                {
                    if (!string.IsNullOrEmpty(indexStr))
                        throw new InvalidCastException($"无法识别的布尔表达式 {expression}, 错误位置: " + i);
                    item = c + "";
                    GetRPNItem(item);
                }
                else if (c == '{' || '0' <= c && c <= '9')
                {
                    indexStr += c;
                }
                else if (c == '}')
                {
                    GetRPNItem(indexStr + c);
                    indexStr = string.Empty;
                }
                else if (isTrue(i))
                {
                    GetRPNItem("true");
                    i += 3;
                }
                else if (isFalse(i))
                {
                    GetRPNItem("false");
                    i += 4;
                }
                else throw new InvalidCastException($"无法识别的布尔表达式 {expression}, 错误位置: " + i);
            }
            while (optStack.Count > 0)
                RPN.Add(optStack.Pop() + "");
            //Debug.Log(ZetanUtility.SerializeObject(RPN, false));
            Stack<bool> values = new Stack<bool>();
            foreach (var item in RPN)
            {
                //Debug.Log(item);
                if (bool.TryParse(item, out var value)) values.Push(value);
                else if (item.StartsWith('{') && int.TryParse(item.Split('{', '}')[^2], out var index) && index >= 0 && index < args.Length)
                    values.Push(args[index]);
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
            throw new InvalidCastException($"表达式 {expression} 计算失败");

            bool isTrue(int i)
            {
                return i + 3 < cr.Length && char.ToLower(cr[i]) == 't' && char.ToLower(cr[i + 1]) == 'r' && char.ToLower(cr[i + 2]) == 'u' && cr[i + 3] == 'e';
            }
            bool isFalse(int i)
            {
                return i + 4 < cr.Length && char.ToLower(cr[i]) == 'f' && char.ToLower(cr[i + 1]) == 'a' && char.ToLower(cr[i + 2]) == 'l' && cr[i + 3] == 's' && cr[i + 4] == 'e';
            }
            static bool isOp(char c)
            {
                return c == '(' || c == ')' || c == '|' || c == '&' || c == '!';
            }
            void GetRPNItem(string item)
            {
                //Debug.Log(item);
                if (item == "|" || item == "&" || item == "!")//遇到运算符
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
                else if (item.StartsWith('{') || bool.TryParse(item, out _)) RPN.Add(item);//遇到数字
            }
        }
    }
}
