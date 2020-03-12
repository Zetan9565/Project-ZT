using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialogue", menuName = "ZetanStudio/剧情/对话")]
public class Dialogue : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private bool useUnifiedNPC;
    public bool UseUnifiedNPC => useUnifiedNPC;

    [SerializeField]
    private bool useCurrentTalkerInfo;
    public bool UseCurrentTalkerInfo => useCurrentTalkerInfo;

    [SerializeField]
    private TalkerInformation unifiedNPC;
    public TalkerInformation UnifiedNPC => unifiedNPC;

    [SerializeField]
    private List<DialogueWords> words = new List<DialogueWords>();
    public List<DialogueWords> Words => words;

    public int IndexOfWords(DialogueWords words)
    {
        return Words.IndexOf(words);
    }
}
[Serializable]
public class DialogueWords
{
    public string TalkerName
    {
        get
        {
            if (TalkerType == TalkerType.NPC)
                if (TalkerInfo) return TalkerInfo.name;
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
    private string words;
    public string Words => words;

    [SerializeField]
    private int indexOfCorrectOption;
    public int IndexOfCorrectOption => indexOfCorrectOption;

    public bool NeedToChusCorrectOption//仅当选择型选项多于1个时才需要选取正确选项
    {
        get
        {
            return branches != null && indexOfCorrectOption > -1 && branches.FindAll(x => x.OptionType == WordsOptionType.Choice).Count > 1;
        }
    }

    [SerializeField]
    private string wordsWhenChusWB;
    /// <summary>
    /// ChuseWB = Choose Wrong Branch
    /// </summary>
    public string WordsWhenChusWB => wordsWhenChusWB;

    [SerializeField]
    private List<WordsOption> branches = new List<WordsOption>();
    public List<WordsOption> Options => branches;

    [SerializeField]
    private List<WordsEvent> events = new List<WordsEvent>();
    public List<WordsEvent> Events => events;

    public bool IsValid
    {
        get
        {
            return !(TalkerType == TalkerType.NPC && !talkerInfo || string.IsNullOrEmpty(words) ||
            branches.Exists(b => b && !b.IsValid) || events.Exists(e => e && !e.IsValid) || NeedToChusCorrectOption && string.IsNullOrEmpty(wordsWhenChusWB));
        }
    }

    public DialogueWords()
    {

    }

    public DialogueWords(TalkerInformation talkerInfo, string words, TalkerType talkerType = 0)
    {
        this.talkerInfo = talkerInfo;
        this.words = words;
        this.talkerType = talkerType;
    }

    public bool IsCorrectOption(WordsOption option)
    {
        return NeedToChusCorrectOption && Options.Contains(option) && Options.IndexOf(option) == IndexOfCorrectOption;
    }

    public override string ToString()
    {
        if (TalkerType == TalkerType.NPC && talkerInfo)
            return "[" + talkerInfo.name + "]说：" + words;
        else if (TalkerType == TalkerType.Player)
            return "[玩家]说：" + words;
        else return "[Unnamed]说：" + words;
    }

    public int IndexOfOption(WordsOption option)
    {
        return Options.IndexOf(option);
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
#if UNITY_EDITOR
    [EnumMemberNames("类型：一句分支", "类型：一段分支", "类型：选择项", "类型：提交、交换道具", "类型：取得道具")]
#endif
    private WordsOptionType optionType;
    public WordsOptionType OptionType => optionType;

    [SerializeField]
    private bool hasWordsToSay;
    public bool HasWordsToSay
    {
        get
        {
            return hasWordsToSay && optionType == WordsOptionType.Choice || optionType != WordsOptionType.Choice;
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
            return goBack || optionType == WordsOptionType.SubmitAndGet || optionType == WordsOptionType.OnlyGet;
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
    public bool DeleteWhenCmplt => deleteWhenCmplt;

    public bool IsValid
    {
        get
        {
            return !(optionType == WordsOptionType.BranchDialogue && (!dialogue || dialogue.Words.Count < 1)
                || optionType == WordsOptionType.BranchWords && string.IsNullOrEmpty(words)
                || optionType == WordsOptionType.Choice && hasWordsToSay && string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.SubmitAndGet && (!ItemToSubmit || !ItemToSubmit.item || string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.OnlyGet && (!ItemCanGet || !ItemCanGet.item || string.IsNullOrEmpty(words));
        }
    }

    [HideInInspector]
    public Dialogue runtimeDialogParent;
    [HideInInspector]
    public int runtimeWordsParentIndex;

    [HideInInspector]
    public int runtimeIndexToGoBack;

    public WordsOption()
    {

    }

    public WordsOption(WordsOptionType optionType)
    {
        this.optionType = optionType;
    }

    public WordsOption Cloned => MemberwiseClone() as WordsOption;

    public static implicit operator bool(WordsOption self)
    {
        return self != null;
    }
}
public enum WordsOptionType
{
    BranchWords,
    BranchDialogue,
    Choice,
    SubmitAndGet,
    OnlyGet
}

[Serializable]
public class WordsEvent
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("触发器", "增加好感", "减少好感")]
#endif
    private WordsEventType eventType;
    public WordsEventType EventType => eventType;

    [SerializeField]
    private string wordsTrigrName;
    public string WordsTrigrName => wordsTrigrName;

    [SerializeField]
    private bool doOnlyOnce;
    public bool DoOnlyOnce => doOnlyOnce;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("无", "置位", "复位")]
#endif
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
    Trigger,
    GetAmity,
    LoseAmity
}

public enum TalkerType
{
    NPC,
    Player,
    UnifiedNPC
}

[Serializable]
public class AffectiveDialogue
{
    [SerializeField]
    private Dialogue level_1;
    public Dialogue Level_1 => level_1;

    [SerializeField]
    private Dialogue level_2;
    public Dialogue Level_2 => level_2;

    [SerializeField]
    private Dialogue level_3;
    public Dialogue Level_3 => level_3;

    [SerializeField]
    private Dialogue level_4;
    public Dialogue Level_4 => level_4;
}