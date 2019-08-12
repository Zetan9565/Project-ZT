using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialogue", menuName = "ZetanStudio/剧情/对话")]
public class Dialogue : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    private List<DialogueWords> words = new List<DialogueWords>();
    public List<DialogueWords> Words
    {
        get
        {
            return words;
        }
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
                if (TalkerInfo)
                    return TalkerInfo.Name;
                else return string.Empty;
            else return "玩家角色";
        }
    }

    [SerializeField]
    private TalkerType talkerType;
    public TalkerType TalkerType
    {
        get
        {
            return talkerType;
        }
    }

    [SerializeField]
    private TalkerInformation talkerInfo;
    public TalkerInformation TalkerInfo
    {
        get
        {
            return talkerInfo;
        }
    }

    [SerializeField, TextArea(3, 10)]
    private string words;
    public string Words
    {
        get
        {
            return words;
        }
    }

    [SerializeField]
    private int indexOfCorrectOption;
    public int IndexOfCorrectOption
    {
        get
        {
            return indexOfCorrectOption;
        }
    }

    public bool NeedToChusCorrectOption//仅当所有选项都是选择型且多于1个时才有效
    {
        get
        {
            return branches != null && indexOfCorrectOption > -1 && branches.Count > 1 && branches.TrueForAll(x => x.OptionType == WordsOptionType.Choice);
        }
    }

    [SerializeField]
    private string wordsWhenChusWB;
    /// <summary>
    /// ChuseWB = Choose Wrong Branch
    /// </summary>
    public string WordsWhenChusWB
    {
        get
        {
            return wordsWhenChusWB;
        }
    }

    [SerializeField]
    private List<WordsOption> branches = new List<WordsOption>();
    public List<WordsOption> Options
    {
        get
        {
            return branches;
        }
    }

    public bool IsValid
    {
        get
        {
            return !(talkerType == TalkerType.NPC && !talkerInfo || string.IsNullOrEmpty(words));
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

    public bool IsCorrectOption(WordsOption branch)
    {
        return NeedToChusCorrectOption && Options.Contains(branch) && Options.IndexOf(branch) == IndexOfCorrectOption;
    }

    public override string ToString()
    {
        if (TalkerType == TalkerType.NPC && talkerInfo)
            return "[" + talkerInfo.Name + "]说：" + words;
        else if (TalkerType == TalkerType.Player)
            return "[玩家]说：" + words;
        else return "[Unnamed]说：" + words;
    }

    public int IndexOf(WordsOption option)
    {
        return Options.IndexOf(option);
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
    public WordsOptionType OptionType
    {
        get
        {
            return optionType;
        }
    }

    [SerializeField]
    private bool hasWordsToSay;
    public bool HasWordsToSay
    {
        get
        {
            return hasWordsToSay;
        }
    }

    [SerializeField]
    public string TalkerName
    {
        get
        {
            if (TalkerType == TalkerType.NPC)
                if (TalkerInfo)
                    return TalkerInfo.Name;
                else return string.Empty;
            else return "玩家角色";
        }
    }

    [SerializeField]
    private TalkerType talkerType;
    public TalkerType TalkerType
    {
        get
        {
            return talkerType;
        }
    }

    [SerializeField]
    private TalkerInformation talkerInfo;
    public TalkerInformation TalkerInfo
    {
        get
        {
            return talkerInfo;
        }
    }

    [SerializeField]
    private string words;
    public string Words
    {
        get
        {
            return words;
        }
    }

    [SerializeField]
    private Dialogue dialogue;
    public Dialogue Dialogue
    {
        get
        {
            return dialogue;
        }
    }

    [SerializeField]
    private int specifyIndex = -1;//-1和0是等价的
    /// <summary>
    /// 指定分支句子序号
    /// </summary>
    public int SpecifyIndex
    {
        get
        {
            return specifyIndex;
        }
    }

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
    /// 指定对话返回序号
    /// </summary>
    public int IndexToGoBack
    {
        get
        {
            return indexToGoBack;
        }
    }

    [SerializeField]
    private ItemInfo itemToSubmit;
    public ItemInfo ItemToSubmit
    {
        get
        {
            return itemToSubmit;
        }
    }

    [SerializeField]
    private ItemInfo itemCanGet;
    public ItemInfo ItemCanGet
    {
        get
        {
            return itemCanGet;
        }
    }
    [SerializeField]
    private bool showOnlyWhenNotHave;
    public bool ShowOnlyWhenNotHave
    {
        get
        {
            return showOnlyWhenNotHave;
        }
    }
    [SerializeField]
    private bool onlyForQuest;
    public bool OnlyForQuest
    {
        get
        {
            return onlyForQuest;
        }
    }
    [SerializeField]
    private Quest bindedQuest;
    public Quest BindedQuest
    {
        get
        {
            return bindedQuest;
        }
    }

    [SerializeField]
    private bool deleteWhenCmplt = true;
    public bool DeleteWhenCmplt
    {
        get
        {
            return deleteWhenCmplt;
        }
    }

    public bool IsValid
    {
        get
        {
            return !(optionType == WordsOptionType.BranchDialogue && !dialogue
                || optionType == WordsOptionType.BranchWords && (TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words))
                || HasWordsToSay && (TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.SubmitAndGet && (!ItemToSubmit || !ItemToSubmit.item || TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.OnlyGet && (!ItemCanGet || !ItemCanGet.item || TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words)));
        }
    }

    [HideInInspector]
    public Dialogue runtimeParent;

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

public enum TalkerType
{
    NPC,
    Player
}

[Serializable]
public class AffectiveDialogue
{
    [SerializeField]
    private Dialogue level_1;
    public Dialogue Level_1
    {
        get
        {
            return level_1;
        }
    }

    [SerializeField]
    private Dialogue level_2;
    public Dialogue Level_2
    {
        get
        {
            return level_2;
        }
    }

    [SerializeField]
    private Dialogue level_3;
    public Dialogue Level_3
    {
        get
        {
            return level_3;
        }
    }

    [SerializeField]
    private Dialogue level_4;
    public Dialogue Level_4
    {
        get
        {
            return level_1;
        }
    }
}