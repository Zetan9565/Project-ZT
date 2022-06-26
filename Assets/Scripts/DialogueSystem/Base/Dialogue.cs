using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialogue", menuName = "Zetan Studio/剧情/对话")]
public class Dialogue : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private bool storyDialogue;
    public bool StoryDialogue => storyDialogue;

    [SerializeField]
    private bool useUnifiedNPC;
    public bool UseUnifiedNPC => useUnifiedNPC;

    [SerializeField]
    private bool useCurrentTalkerInfo;
    public bool UseCurrentTalkerInfo => useCurrentTalkerInfo && !storyDialogue;

    [SerializeField]
    private TalkerInformation unifiedNPC;
    public TalkerInformation UnifiedNPC => unifiedNPC;

    [SerializeField, NonReorderable]
    private List<DialogueWords> words = new List<DialogueWords>();
    public List<DialogueWords> Words => words;

    public string Preview()
    {
        return PreviewDialogue(this);
    }

    public static string PreviewDialogue(Dialogue dialogue, params IEnumerable<ScriptableObject>[] caches)
    {
        if (!dialogue) return null;
        string dialoguePreview = string.Empty;
        for (int i = 0; i < dialogue.Words.Count; i++)
        {
            var words = dialogue.Words[i];
            dialoguePreview += "[" + words.TalkerName + "]说：\n-" + MiscFuntion.HandlingKeyWords(words.Content, false, caches);
            for (int j = 0; j < words.Options.Count; j++)
            {
                dialoguePreview += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
            }
            dialoguePreview += i == dialogue.Words.Count - 1 ? string.Empty : "\n";
        }
        return dialoguePreview;
    }

    public static string GetFirstWords(Dialogue dialogue)
    {
        if (dialogue && dialogue.Words.Count > 0)
        {
            return MiscFuntion.HandlingKeyWords(dialogue.Words[0].ToString(), true);
        }
        return string.Empty;
    }

#if UNITY_EDITOR
    public static class Editor
    {
        public static string GetAutoID(Dialogue[] all, int length = 6)
        {
            string newID = string.Empty;
            var len = Mathf.Pow(10, length);
            for (int i = 1; i < len; i++)
            {
                newID = "DIALG" + i.ToString().PadLeft(length, '0');
                if (!Array.Exists(all, x => x.ID == newID))
                    break;
            }
            return newID;
        }

        public static bool IsIDDuplicate(Dialogue dialogue, Dialogue[] all)
        {
            Dialogue find = Array.Find(all, x => x.ID == dialogue.ID);
            if (!find) return false;//若没有找到，则ID可用
                                    //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
            return find != dialogue || (find == dialogue && Array.FindAll(all, x => x.ID == dialogue.ID).Length > 1);
        }
    }
#endif
}
[Serializable]
public class DialogueWords
{
    public string TalkerName
    {
        get
        {
            if (TalkerType == TalkerType.NPC)
                if (TalkerInfo) return TalkerInfo.Name;
                else return "未知NPC";
            else if (TalkerType == TalkerType.UnifiedNPC)
                return "NPC";
            else return "玩家角色";
        }
    }

    [SerializeField]
    private TalkerType talkerType = TalkerType.Player;
    public TalkerType TalkerType => talkerType;

    [SerializeField]
    private TalkerInformation talkerInfo;
    public TalkerInformation TalkerInfo => talkerInfo;

    [SerializeField, TextArea(3, 10)]
    private string content;
    public string Content => content;

    [SerializeField]
    private int indexOfCorrectOption;
    public int IndexOfCorrectOption => indexOfCorrectOption;

    public bool NeedToChusCorrectOption//仅当选择型选项多于1个时才需要选取正确选项
    {
        get
        {
            return indexOfCorrectOption > -1 && options.FindAll(x => x.OptionType == WordsOptionType.Choice).Count > 1;
        }
    }

    [SerializeField]
    private string wrongChoiceWords;
    public string WrongChoiceWords => wrongChoiceWords;

    [SerializeField, NonReorderable]
    private List<WordsOption> options = new List<WordsOption>();
    public List<WordsOption> Options => options;

    [SerializeField, NonReorderable]
    private List<WordsEvent> events = new List<WordsEvent>();
    public List<WordsEvent> Events => events;

    public bool IsValid
    {
        get
        {
            return !(TalkerType == TalkerType.NPC && !talkerInfo || string.IsNullOrEmpty(content) ||
            options.Exists(b => b && !b.IsValid) || events.Exists(e => e && !e.IsValid) || NeedToChusCorrectOption && string.IsNullOrEmpty(wrongChoiceWords));
        }
    }

    public DialogueWords()
    {

    }

    public DialogueWords(TalkerInformation talkerInfo, string words, TalkerType talkerType = 0)
    {
        this.talkerInfo = talkerInfo;
        this.content = words;
        this.talkerType = talkerType;
    }

    public bool IsCorrectOption(WordsOption option)
    {
        return NeedToChusCorrectOption && Options.Contains(option) && Options.IndexOf(option) == IndexOfCorrectOption;
    }

    public override string ToString()
    {
        if (TalkerType == TalkerType.NPC && talkerInfo)
            return "[" + talkerInfo.Name + "]说：" + content;
        else if (TalkerType == TalkerType.Player)
            return "[玩家]说：" + content;
        else return "[Unnamed]说：" + content;
    }

    public static implicit operator bool(DialogueWords self)
    {
        return self != null;
    }
}
[Serializable]
public class WordsOption
{
    [SerializeField]
    private string title;
    public string Title
    {
        get
        {
            if (string.IsNullOrEmpty(title)) return "……";
            return title;
        }
    }

    [SerializeField]
    private WordsOptionType optionType;
    public WordsOptionType OptionType => optionType;

    [SerializeField]
    private bool hasWordsToSay;
    public bool HasWordsToSay
    {
        get
        {
            return hasWordsToSay && optionType == WordsOptionType.Choice || optionType != WordsOptionType.Choice && optionType != WordsOptionType.BranchDialogue;
        }
    }

    [SerializeField]
    private TalkerType talkerType;
    public TalkerType TalkerType => talkerType;

    [SerializeField]
    private string words;
    public string Words => words;

    [SerializeField]
    private Dialogue dialogue;
    public Dialogue Dialogue => dialogue;

    [SerializeField]
    private int specifyIndex = 0;
    /// <summary>
    /// 指定分支句子序号，在进入该分支时从第几句开始
    /// </summary>
    public int SpecifyIndex => specifyIndex;

    [SerializeField]
    private bool goBack;
    public bool GoBack
    {
        get
        {
            return goBack || optionType == WordsOptionType.Choice || optionType == WordsOptionType.SubmitAndGet || optionType == WordsOptionType.OnlyGet;
        }
    }

    [SerializeField]
    private int indexToGoBack = -1;//-1表示返回分支开始时的句子
    /// <summary>
    /// 指定对话返回序号，在返回原对话时从第几句开始
    /// </summary>
    public int IndexToGoBack => indexToGoBack;

    [SerializeField]
    private ItemInfo itemToSubmit;
    public ItemInfo ItemToSubmit => itemToSubmit;

    [SerializeField]
    private ItemInfo itemCanGet;
    public ItemInfo ItemCanGet => itemCanGet;
    [SerializeField]
    private bool showOnlyWhenNotHave;
    public bool ShowOnlyWhenNotHave => showOnlyWhenNotHave;
    [SerializeField]
    private bool onlyForQuest;
    public bool OnlyForQuest
    {
        get
        {
            return onlyForQuest && optionType == WordsOptionType.OnlyGet ? showOnlyWhenNotHave : true;
        }
    }
    [SerializeField]
    private Quest bindedQuest;
    public Quest BindedQuest => bindedQuest;

    [SerializeField]
    private bool deleteWhenCmplt = true;
    public bool DeleteWhenCmplt => deleteWhenCmplt && optionType == WordsOptionType.Choice;

    public bool IsValid
    {
        get
        {
            return !(optionType == WordsOptionType.BranchDialogue && (!dialogue || dialogue.Words.Count < 1)
                || optionType == WordsOptionType.BranchWords && string.IsNullOrEmpty(words)
                || optionType == WordsOptionType.Choice && hasWordsToSay && string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.SubmitAndGet && (!ItemToSubmit || !ItemToSubmit.Item || string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.OnlyGet && (!ItemCanGet || !ItemCanGet.Item || string.IsNullOrEmpty(words));
        }
    }

    public WordsOption() { }

    public WordsOption(WordsOptionType optionType)
    {
        this.optionType = optionType;
    }

    public static implicit operator bool(WordsOption self)
    {
        return self != null;
    }
}
public enum WordsOptionType
{
    [InspectorName("类型：一句分支")]
    BranchWords,

    [InspectorName("类型：一段分支")]
    BranchDialogue,

    [InspectorName("类型：选择项")]
    Choice,

    [InspectorName("提交、交换道具")]
    SubmitAndGet,

    [InspectorName("取得道具")]
    OnlyGet
}

[Serializable]
public class WordsEvent
{
    [SerializeField]
    private WordsEventType eventType;
    public WordsEventType EventType => eventType;

    [SerializeField]
    private string wordsTrigrName;
    public string WordsTrigrName => wordsTrigrName;

    [SerializeField]
    private bool doOnlyOnce;
    public bool DoOnlyOnce => doOnlyOnce;

    [SerializeField]
    private TriggerActionType triggerActType;
    public TriggerActionType TriggerActType => triggerActType;

    [SerializeField]
    private TalkerInformation toWhom;
    public TalkerInformation ToWhom => toWhom;

    [SerializeField]
    private int favorabilityValue;
    public int AmityValue => favorabilityValue;

    public bool IsValid => eventType == WordsEventType.Trigger && !string.IsNullOrEmpty(wordsTrigrName)
        || eventType != WordsEventType.Trigger && toWhom && favorabilityValue >= 0;

    public static implicit operator bool(WordsEvent self)
    {
        return self != null;
    }
}
public enum WordsEventType
{
    [InspectorName("触发器")]
    Trigger,

    [InspectorName("增加好感")]
    GetAmity,

    [InspectorName("减少好感")]
    LoseAmity
}

public enum TalkerType
{
    [InspectorName("NPC")]
    NPC,

    [InspectorName("玩家角色")]
    Player,

    [InspectorName("统一的NPC")]
    UnifiedNPC
}

[Serializable]
public class AffectiveDialogue
{
    [SerializeField]
    private int lowerBound = 10;
    public int LowerBound => lowerBound;

    [SerializeField]
    private int upperBound = 20;
    public int UpperBound => upperBound;

    [SerializeField]
    private Dialogue dialogue;
    public Dialogue Dialogue => dialogue;
}

[Serializable]
public class ConditionDialogue
{
    [SerializeField]
    private ConditionGroup condition;
    public ConditionGroup Condition => condition;

    [SerializeField]
    private Dialogue dialogue;
    public Dialogue Dialogue => dialogue;

    public bool IsValid => dialogue && condition.IsValid;
}