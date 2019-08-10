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
    private int indexOfRrightBranch;
    public int IndexOfRightBranch
    {
        get
        {
            return indexOfRrightBranch;
        }
    }

    public bool NeedToChusRightBranch//仅当所有选项都是选择型时才有效
    {
        get
        {
            return branches != null && indexOfRrightBranch > -1 && branches.TrueForAll(x => x.OptionType == WordsOptionType.Choice);
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
    private List<BranchDialogue> branches = new List<BranchDialogue>();
    public List<BranchDialogue> Branches
    {
        get
        {
            return branches;
        }
    }

    public bool IsInvalid
    {
        get
        {
            return talkerType == TalkerType.NPC && !talkerInfo || string.IsNullOrEmpty(words);
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

    public bool IsRightBranch(BranchDialogue branch)
    {
        return NeedToChusRightBranch && Branches.Contains(branch) && Branches.IndexOf(branch) == IndexOfRightBranch;
    }

    public override string ToString()
    {
        if (TalkerType == TalkerType.NPC && talkerInfo)
            return "[" + talkerInfo.Name + "]说：" + words;
        else if (TalkerType == TalkerType.Player)
            return "[玩家]说：" + words;
        else return "[Unnamed]说：" + words;
    }

    public int IndexOf(BranchDialogue branch)
    {
        return Branches.IndexOf(branch);
    }
}


[Serializable]
public class BranchDialogue
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
    [EnumMemberNames("一句普通分支", "一段普通分支", "选择型分支", "提交和取得道具", "取得道具")]
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
    private bool hasWords;
    public bool HasWords
    {
        get
        {
            return hasWords && optionType == WordsOptionType.Choice;
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
            return goBack;
        }
    }

    [SerializeField]
    private int indexToGo = -1;//-1表示不自定义
    public int IndexToGo
    {
        get
        {
            return indexToGo;
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
                || HasWords && (TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.SubmitAndGet && (!ItemToSubmit || !ItemToSubmit.item || TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words))
                || optionType == WordsOptionType.OnlyGet && (!ItemCanGet || !ItemCanGet.item || TalkerType == TalkerType.NPC && !TalkerInfo || string.IsNullOrEmpty(words)));
        }
    }

    [HideInInspector]
    public Dialogue runtimeParent;

    [HideInInspector]
    public int runtimeIndexToGoBack;

    public BranchDialogue Cloned => MemberwiseClone() as BranchDialogue;

    public static implicit operator bool(BranchDialogue self)
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