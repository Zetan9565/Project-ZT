using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        bool hide = ShouldHide(hideAttr, property);
        if (!hide || hideAttr.readOnly)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(hide && hideAttr.readOnly);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideIfAttribute hideAttr = (HideIfAttribute)attribute;
        bool hide = ShouldHide(hideAttr, property);
        if (!hide || hideAttr.readOnly)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool ShouldHide(HideIfAttribute hideAttr, SerializedProperty property)
    {
        if (hideAttr.paths.Length < 1)
            return false;
        if (hideAttr.type != null) return ShouldHide0();
        else if (string.IsNullOrEmpty(hideAttr.relational)) return ShouldHide1();
        else return ShouldHide2();

        bool ShouldHide(string p, object v)
        {
            if (p == "typeof(this)") return (v as System.Type).IsAssignableFrom(fieldInfo.ReflectedType);
            else if (ZetanUtility.Editor.TryGetValue(property, out _))
            {
                bool type = p.StartsWith("typeof(");
                if (type) p = p.Replace("typeof(", "").Replace(")", "");
                if (ZetanUtility.TryGetMemberValue(p, property.serializedObject.targetObject ?? property.managedReferenceValue, out var value, out _))
                {
                    if (type) return value.GetType().IsAssignableFrom(v as System.Type);
                    else if (Equals(value, v)) return true;
                    else return false;
                }
                else
                {
                    Debug.LogWarning($"找不到路径：{(property.serializedObject.targetObject ?? property.managedReferenceValue).GetType().Name}.{p}");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("无法访问：" + property.propertyPath);
                return false;
            }
        }

        bool ShouldHide0()
        {
            return property.serializedObject.GetType().Equals(hideAttr.type);
        }

        bool ShouldHide1()
        {
            bool hide = ShouldHide(hideAttr.paths[0], hideAttr.values[0]);
            for (int i = 1; i < hideAttr.paths.Length; i++)
            {
                if (hideAttr.and) hide &= ShouldHide(hideAttr.paths[i], hideAttr.values[i]);
                else hide |= ShouldHide(hideAttr.paths[i], hideAttr.values[i]);
            }
            return hide;
        }

        bool ShouldHide2()
        {
            bool calFailed = false;
            if (hideAttr.paths.Length < 1) calFailed = true;
            else
            {
                var cr = hideAttr.relational.Replace(" ", "").ToCharArray();//删除所有空格才开始计算
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
                //Debug.Log(ZetanUtility.SerializeObject(RPN, false));
                Stack<bool> values = new Stack<bool>();
                foreach (var item in RPN)
                {
                    //Debug.Log(item);
                    if (int.TryParse(item, out int index))
                    {
                        if (index >= 0 && index < hideAttr.paths.Length)
                            values.Push(ShouldHide(hideAttr.paths[index], hideAttr.values[index]));
                        else
                        {
                            Debug.Log("return 1");
                            return true;
                        }
                    }
                    else if (values.Count > 1)
                    {
                        if (item == "|") values.Push(values.Pop() | values.Pop());
                        else if (item == "!") values.Push(!values.Pop());
                        else if (item == "&") values.Push(values.Pop() & values.Pop());
                    }
                    else if (item == "!") values.Push(!values.Pop());
                }
                if (values.Count == 1)
                {
                    //Debug.Log("return 2");
                    return values.Pop();
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
                //Debug.Log("return 4");
                return ShouldHide1();
            }
        }
    }
}