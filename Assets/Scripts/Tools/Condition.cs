using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[System.Serializable]
public class Condition
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("等级等于", "等级大于", "等级小于", "完成任务", "接取任务", "拥有道具", "触发器开启", "触发器关闭")]
#endif
    private ConditionType type = ConditionType.CompleteQuest;
    public ConditionType Type => type;

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

public class ConditionGroupDrawer
{
    private readonly SerializedObject owner;

    private readonly SerializedProperty property;

    private readonly float lineHeightSpace;

    public ReorderableList ConditionList { get; }

    public ConditionGroupDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "条件列表")
    {
        this.owner = owner;
        this.property = property;
        this.lineHeightSpace = lineHeightSpace;
        SerializedProperty conditions = property.FindPropertyRelative("conditions");
        ConditionList = new ReorderableList(property.serializedObject, conditions, true, true, true, true);
        ConditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            owner.Update();
            SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
            SerializedProperty type = condition.FindPropertyRelative("type");
            ConditionType conditionType = (ConditionType)type.enumValueIndex;
            SerializedProperty level;
            SerializedProperty completeQuest;
            SerializedProperty ownedItem;
            if (condition != null)
            {
                switch (conditionType)
                {
                    case ConditionType.CompleteQuest:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "完成任务");
                        break;
                    case ConditionType.AcceptQuest:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "接取任务");
                        break;
                    case ConditionType.HasItem:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "拥有道具");
                        break;
                    case ConditionType.LevelEquals:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级等于");
                        break;
                    case ConditionType.LevelLargeThen:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级大于");
                        break;
                    case ConditionType.LevelLessThen:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级小于");
                        break;
                    case ConditionType.TriggerSet:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "触发器开启");
                        break;
                    case ConditionType.TriggerReset:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "触发器关闭");
                        break;
                    default:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "未定义条件");
                        break;
                }
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                type, new GUIContent(string.Empty), true);

            switch (conditionType)
            {
                case ConditionType.CompleteQuest:
                case ConditionType.AcceptQuest:
                    completeQuest = condition.FindPropertyRelative("completeQuest");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), completeQuest, new GUIContent("需完成的任务"));
                    if (completeQuest.objectReferenceValue == owner.targetObject as Quest) completeQuest.objectReferenceValue = null;
                    if (completeQuest.objectReferenceValue)
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "任务标题", completeQuest.FindPropertyRelative("title").stringValue);
                    }
                    break;
                case ConditionType.HasItem:
                    ownedItem = condition.FindPropertyRelative("ownedItem");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), ownedItem, new GUIContent("需拥有的道具"));
                    if (ownedItem.objectReferenceValue)
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "道具名称", ownedItem.FindPropertyRelative("_Name").stringValue);
                    }
                    break;
                case ConditionType.LevelEquals:
                case ConditionType.LevelLargeThen:
                case ConditionType.LevelLessThen:
                    level = condition.FindPropertyRelative("level");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), level, new GUIContent("限制的等级"));
                    if (level.intValue < 1) level.intValue = 1;
                    break;
                case ConditionType.TriggerSet:
                case ConditionType.TriggerReset:
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                        condition.FindPropertyRelative("triggerName"), new GUIContent("触发器名称"));
                    break;
                default: break;
            }

            if (EditorGUI.EndChangeCheck())
                owner.ApplyModifiedProperties();
        };

        ConditionList.elementHeightCallback = (int index) =>
        {
            SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
            SerializedProperty type = condition.FindPropertyRelative("type");
            ConditionType conditionType = (ConditionType)type.enumValueIndex;
            switch (conditionType)
            {
                case ConditionType.CompleteQuest:
                case ConditionType.AcceptQuest:
                    if (condition.FindPropertyRelative("completeQuest").objectReferenceValue)
                        return 3 * lineHeightSpace;
                    else return 2 * lineHeightSpace;
                case ConditionType.HasItem:
                    if (condition.FindPropertyRelative("ownedItem").objectReferenceValue)
                        return 3 * lineHeightSpace;
                    else return 2 * lineHeightSpace;
                case ConditionType.LevelEquals:
                case ConditionType.LevelLargeThen:
                case ConditionType.LevelLessThen:
                case ConditionType.TriggerSet:
                case ConditionType.TriggerReset:
                    return 2 * lineHeightSpace;
                default: return lineHeightSpace;
            }
        };

        ConditionList.onRemoveCallback = (list) =>
        {
            owner.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个条件吗？", "确定", "取消"))
            {
                conditions.DeleteArrayElementAtIndex(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                owner.ApplyModifiedProperties();
        };

        ConditionList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = 0;
            for (int i = 0; i < conditions.arraySize; i++)
            {
                SerializedProperty condition = conditions.GetArrayElementAtIndex(i);
                SerializedProperty type = condition.FindPropertyRelative("type");
                ConditionType conditionType = (ConditionType)type.enumValueIndex;
                SerializedProperty level = condition.FindPropertyRelative("level");
                SerializedProperty completeQuest = condition.FindPropertyRelative("completeQuest");
                SerializedProperty ownedItem = condition.FindPropertyRelative("ownedItem");
                switch (conditionType)
                {
                    case ConditionType.CompleteQuest:
                    case ConditionType.AcceptQuest:
                        if (completeQuest.objectReferenceValue == null) notCmpltCount++;
                        break;
                    case ConditionType.HasItem:
                        if (ownedItem.objectReferenceValue == null) notCmpltCount++;
                        break;
                    case ConditionType.LevelEquals:
                    case ConditionType.LevelLargeThen:
                    case ConditionType.LevelLessThen:
                        if (level.intValue < 1) notCmpltCount++;
                        break;
                    case ConditionType.TriggerSet:
                    case ConditionType.TriggerReset:
                        if (!string.IsNullOrEmpty(condition.FindPropertyRelative("triggerName").stringValue)) notCmpltCount++;
                        break;
                    default: break;
                }
            }
            EditorGUI.LabelField(rect, listTitle, "数量：" + conditions.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        ConditionList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    public void DoLayoutDraw()
    {
        owner?.Update();
        ConditionList?.DoLayoutList();
        owner?.ApplyModifiedProperties();
        if (ConditionList != null && ConditionList.count > 0)
        {
            owner?.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("relational"), new GUIContent("(?)条件关系表达式"));
            if (EditorGUI.EndChangeCheck())
                owner?.ApplyModifiedProperties();
        }
    }

    public void DoDraw(Rect rect)
    {
        owner?.Update();
        ConditionList?.DoList(rect);
        owner?.ApplyModifiedProperties();
        if (ConditionList != null && ConditionList.count > 0)
        {
            owner?.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + ConditionList.GetHeight(), rect.width, lineHeightSpace), property.FindPropertyRelative("relational"), new GUIContent("(?)条件关系表达式"));
            if (EditorGUI.EndChangeCheck())
                owner?.ApplyModifiedProperties();
        }
    }

    public float GetDrawHeight()
    {
        if (ConditionList == null) return 0;
        float height = ConditionList.GetHeight();
        if (ConditionList.count > 0)
            height += lineHeightSpace;
        return height;
    }
}