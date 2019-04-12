using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<DialogueManager>();
            return instance;
        }
    }

    public event DialogueListener OnBeginDialogueEvent;
    public event DialogueListener OnFinishDialogueEvent;

    [SerializeField]
    private DialogueUI UI;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("语句类型", true)]
#endif
    private DialogueType dialogueType = DialogueType.Normal;

    private Queue<DialogueWords> Words = new Queue<DialogueWords>();

    private int page = 1;
    public int Page
    {
        get
        {
            return page;
        }
        set
        {
            if (value > 1) page = value;
            else page = 1;
        }
    }

    private int MaxPage = 1;
    [Space]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public List<OptionAgent> OptionAgents;

    private Talker MTalker;
    private TalkObjective talkObjective;
    private Quest MQuest;

    public bool IsTalking { get; private set; }
    public bool TalkAble { get; private set; }

    #region 开始新对话
    public void BeginNewDialogue()
    {
        if (!MTalker || !TalkAble || IsTalking) return;
        else StartNormalDialogue(MTalker);
        OnBeginDialogueEvent?.Invoke();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (!UI) return;
        if (dialogue.Words.Count < 1 || !dialogue) return;
        IsTalking = true;
        Words.Clear();
        foreach (DialogueWords saying in dialogue.Words)
        {
            Words.Enqueue(saying);
        }
        SayNextWords();
        if (OptionAgents.Count < 1) MyTools.SetActive(UI.optionsParent.gameObject, false);
        MyTools.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        OpenDialogueWindow();
    }
    public void StartDialogue(DialogueWords words)
    {
        if (!UI) return;
        if (words == null) return;
        IsTalking = true;
        Words.Clear();
        Words.Enqueue(words);
        SayNextWords();
        if (OptionAgents.Count < 1) MyTools.SetActive(UI.optionsParent.gameObject, false);
        MyTools.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        OpenDialogueWindow();
    }

    public void StartNormalDialogue(Talker talker)
    {
        if (!UI) return;
        MyTools.SetActive(UI.talkButton.gameObject, false);
        MTalker = talker;
        dialogueType = DialogueType.Normal;
        if (talker is QuestGiver && (talker as QuestGiver).QuestInstances.Count > 0)
        {
            MyTools.SetActive(UI.questButton.gameObject, true);
        }
        else
        {
            MyTools.SetActive(UI.questButton.gameObject, false);
        }
        CloseQuestDescriptionWindow();
        StartDialogue(talker.DefaultDialogue);
        talker.OnTalkBegin();
    }

    public void StartQuestDialogue(Quest quest)
    {
        MQuest = quest;
        dialogueType = DialogueType.Quest;
        if (!MQuest.IsComplete && !MQuest.IsOngoing) StartDialogue(quest.BeginDialogue);
        else if (!MQuest.IsComplete && MQuest.IsOngoing) StartDialogue(quest.OngoingDialogue);
        else StartDialogue(quest.CompleteDialogue);
    }

    public void StartObjectiveDialogue(TalkObjective talkObjective)
    {
        this.talkObjective = talkObjective;
        dialogueType = DialogueType.Objective;
        StartDialogue(talkObjective.Dialogue);
    }
    #endregion

    #region 处理对话选项
    /// <summary>
    /// 生成继续按钮选项
    /// </summary>
    private void MakeContinueOption()
    {
        ClearOptionExceptContinue();
        OptionAgent oa = OptionAgents.Find(x => x.optionType == OptionType.Continue);
        if (oa) OpenOptionArea();
        if (Words.Count > 1)
        {
            //如果还有话没说完，弹出一个“继续”按钮
            if (!oa)
            {
                oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.optionType = OptionType.Continue;
                oa.TitleText.text = "继续";
                if (!OptionAgents.Contains(oa))
                {
                    OptionAgents.Add(oa);
                }
                OpenOptionArea();
            }
        }
        else if (oa)
        {
            //如果话说完了，把“继续”按钮去掉
            OptionAgents.Remove(oa);
            ObjectPool.Instance.Put(oa.gameObject);
        }
        //当“继续”选项出现时，总没有其他选项出现，因此不必像下面一样还要处理一下，除非自己作死把行数写满让“继续”按钮没法显示
    }
    /// <summary>
    /// 生成任务列表的选项
    /// </summary>
    private void MakeTalkerQuestOption()
    {
        if (!(MTalker is QuestGiver)) return;
        ClearOptions();
        foreach (Quest quest in (MTalker as QuestGiver).QuestInstances)
        {
            if (!QuestManager.Instance.HasCompleteQuest(quest) && quest.AcceptAble)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.optionType = OptionType.Quest;
                oa.MQuest = quest;
                oa.TitleText.text = quest.Title + (quest.IsComplete ? "(完成)" : quest.IsOngoing ? "(进行中)" : string.Empty);
                if (quest.IsComplete)
                {
                    //保证完成的任务优先显示，方便玩家提交完成的任务
                    oa.transform.SetAsFirstSibling();
                    OptionAgents.Insert(0, oa);
                }
                else OptionAgents.Add(oa);
            }
        }
        //保证进行中的任务最后显示，方便玩家接取未接取的任务
        for (int i = OptionAgents.Count - 1; i >= 0; i--)
        {
            OptionAgent oa = OptionAgents[i];
            //若当前选项关联的任务在进行中
            if(oa.MQuest.IsOngoing && !oa.MQuest.IsComplete)
            {
                //则从后向前找一个新位置以放置该选项
                for (int j = OptionAgents.Count - 1; j > i; j--)
                {
                    //若找到了合适的位置
                    if(!OptionAgents[j].MQuest.IsOngoing && !OptionAgents[j].MQuest.IsComplete)
                    {
                        //则从该位置开始到选项的原位置，逐个前移一位，填补(覆盖)选项的原位置并空出新位置
                        for (int k = i; k < j; k++)
                        {
                            //在k指向目标位置之前，逐个前移
                            OptionAgents[k] = OptionAgents[k + 1];
                        }
                        //把选项放入新位置，此时选项的原位置即OptionAgents[i]已被填补(覆盖)
                        OptionAgents[j] = oa;
                        oa.transform.SetSiblingIndex(j);
                        break;
                    }
                }
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < OptionAgents.Count; i++)
        {
            MyTools.SetActive(OptionAgents[i].gameObject, false);
        }
        CheckPages();
    }
    /// <summary>
    /// 生成对话目标列表的选项
    /// </summary>
    private void MakeTalkerObjectiveOption()
    {
        int index = 1;
        if (MTalker.talkToThisObjectives != null && MTalker.talkToThisObjectives.Count > 0)
        {
            ClearOptions();
            foreach (TalkObjective to in MTalker.talkToThisObjectives)
            {
                if (to.AllPrevObjCmplt && !to.HasNextObjOngoing)
                {
                    OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                    oa.optionType = OptionType.Objective;
                    oa.TitleText.text = to.runtimeParent.Title;
                    oa.talkObjective = to;
                    OptionAgents.Add(oa);
                    if (index > UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) MyTools.SetActive(oa.gameObject, false);//第一页以外隐藏
                    index++;
                }
            }
        }
        CheckPages();
    }

    public void OptionPageUp()
    {
        int leftLineCount = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight);
        if (page > 0)
        {
            Page--;
            for (int i = 0; i < leftLineCount; i++)
            {
                if ((page - 1) * leftLineCount + i < OptionAgents.Count && (page - 1) * leftLineCount + i >= 0)
                    MyTools.SetActive(OptionAgents[(page - 1) * leftLineCount + i].gameObject, true);
                if (page * leftLineCount + i >= 0 && page * leftLineCount + i < OptionAgents.Count)
                    MyTools.SetActive(OptionAgents[page * leftLineCount + i].gameObject, false);
            }
        }
        if (Page == 1 && MaxPage > 1) SetPageArea(false, true, true);
        else SetPageArea(true, true, true);
        UI.pageText.text = Page.ToString() + "/" + MaxPage.ToString();
    }

    public void OptionPageDown()
    {
        int leftLineCount = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight);
        if (page < Mathf.CeilToInt(OptionAgents.Count * 1.0f / (leftLineCount * 1.0f)))
        {
            for (int i = 0; i < leftLineCount; i++)
            {
                if ((page - 1) * leftLineCount + i < OptionAgents.Count && (page - 1) * leftLineCount + i >= 0)
                    MyTools.SetActive(OptionAgents[(page - 1) * leftLineCount + i].gameObject, false);
                if (page * leftLineCount + i >= 0 && page * leftLineCount + i < OptionAgents.Count)
                    MyTools.SetActive(OptionAgents[page * leftLineCount + i].gameObject, true);
            }
            Page++;
        }
        if (Page == MaxPage && MaxPage > 1) SetPageArea(true, false, true);
        else SetPageArea(true, true, true);
        UI.pageText.text = Page.ToString() + "/" + MaxPage.ToString();
    }
    #endregion

    #region 处理每句话
    /// <summary>
    /// 转到下一句话
    /// </summary>
    public void SayNextWords()
    {
        MakeContinueOption();
        if (Words.Count == 1)
        {
            HandlingLastWords();
            //因为Dequeue之后，话就没了，Words.Count就不是1了，而是0，所以要在此之前做这一步
        }
        if (Words.Count > 0)
        {
            UI.nameText.text = Words.Peek().TalkerName;
            UI.wordsText.text = Words.Dequeue().Words;
        }
    }

    /// <summary>
    /// 处理最后一句对话
    /// </summary>
    private void HandlingLastWords()
    {
        if (dialogueType == DialogueType.Normal && MTalker)
        {
            MTalker.OnTalkFinished();
            MakeTalkerObjectiveOption();
            QuestManager.Instance.UpdateObjectivesUI();
        }
        else if (dialogueType == DialogueType.Objective && talkObjective != null)
        {
            HandlingLastObjectiveWords();
        }
        else if (dialogueType == DialogueType.Quest && MQuest)
        {
            HandlingLastQuestWords();
        }
        OnFinishDialogueEvent?.Invoke();
    }
    /// <summary>
    /// 处理最后一句对话型目标的对话
    /// </summary>
    private void HandlingLastObjectiveWords()
    {
        talkObjective.UpdateTalkStatus();
        if (talkObjective.IsComplete)
        {
            OptionAgent oa = OptionAgents.Find(x => x.talkObjective == talkObjective);
            if (oa && oa.gameObject)
            {
                //去掉该对话目标自身的对话型目标选项
                OptionAgents.Remove(oa);
                RecycleOption(oa);
            }
            //目标已经完成，不再需要保留在对话人的目标列表里，从对话人的对话型目标里删掉相应信息
            MTalker.talkToThisObjectives.RemoveAll(x => x == talkObjective);
        }
        talkObjective = null;//重置管理器的对话目标以防出错
        QuestManager.Instance.UpdateObjectivesUI();
    }
    /// <summary>
    /// 处理最后一句任务的对话
    /// </summary>
    private void HandlingLastQuestWords()
    {
        if (!MQuest.IsOngoing || MQuest.IsComplete)
        {
            ClearOptions();
            //若是任务对话的最后一句，则根据任务情况弹出确认按钮
            OptionAgent yes = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
            OptionAgents.Add(yes);
            yes.optionType = OptionType.Confirm;
            yes.MQuest = MQuest;
            yes.TitleText.text = MQuest.IsComplete ? "完成" : "接受";
            if (!MQuest.IsComplete)
            {
                OptionAgent no = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
                OptionAgents.Add(no);
                no.optionType = OptionType.Back;
                no.TitleText.text = "拒绝";
            }
            OpenQuestDescriptionWindow(MQuest);
        }
        MQuest = null;
    }
    #endregion

    #region UI相关
    public void OpenDialogueWindow()
    {
        if (!TalkAble) return;
        UI.dialogueWindow.alpha = 1;
        UI.dialogueWindow.blocksRaycasts = true;
    }
    public void CloseDialogueWindow()
    {
        UI.dialogueWindow.alpha = 0;
        UI.dialogueWindow.blocksRaycasts = false;
        dialogueType = DialogueType.Normal;
        MTalker = null;
        MQuest = null;
        ClearOptions();
        CloseQuestDescriptionWindow();
        IsTalking = false;
    }

    public void OpenOptionArea()
    {
        if (OptionAgents.Count < 1) return;
        MyTools.SetActive(UI.optionsParent.gameObject, true);
    }
    public void CloseOptionArea()
    {
        MyTools.SetActive(UI.optionsParent.gameObject, false);
        CloseQuestDescriptionWindow();
    }

    public void OpenQuestDescriptionWindow(Quest quest)
    {
        InitDescription(quest);
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
    }
    public void CloseQuestDescriptionWindow()
    {
        MQuest = null;
        UI.descriptionWindow.alpha = 0;
        UI.descriptionWindow.blocksRaycasts = false;
    }
    private void InitDescription(Quest quest)
    {
        if (quest == null) return;
        MQuest = quest;
        UI.descriptionText.text = string.Format("<size=16><b>{0}</b></size>\n[委托人: {1}]\n{2}", 
            MQuest.Title, 
            MQuest.OriginalQuestGiver.Info.Name, 
            MQuest.Description);
        UI.money_EXPText.text = string.Format("[奖励]\n<size=14>经验:\n{0}\n金币:\n{1}</size>",
            MQuest.RewardEXP > 0 ? MQuest.RewardEXP.ToString() : "无",
            MQuest.RewardMoney > 0 ? MQuest.RewardMoney.ToString() : "无");
        foreach (ItemAgent rwc in UI.rewardCells)
        {
            rwc.Item = null;
            rwc.Icon.overrideSprite = null;
        }
        foreach (ItemInfo info in quest.RewardItems)
            foreach (ItemAgent rw in UI.rewardCells)
            {
                if (rw.Item == null)
                {
                    rw.Item = info.Item;
                    rw.Icon.overrideSprite = info.Item.Icon;
                    break;
                }
            }
    }

    public void CanTalk(TalkTrigger2D talkTrigger)
    {
        if (IsTalking || !talkTrigger.talker) return;
        MTalker = talkTrigger.talker;
        UI.talkButton.onClick.RemoveAllListeners();
        UI.talkButton.onClick.AddListener(BeginNewDialogue);
        MyTools.SetActive(UI.talkButton.gameObject, true);
        TalkAble = true;
    }
    public void CannotTalk()
    {
        TalkAble = false;
        UI.talkButton.onClick.RemoveAllListeners();
        MyTools.SetActive(UI.talkButton.gameObject, false);
        CloseDialogueWindow();
    }
    #endregion

    #region 其它
    public void LoadTalkerQuest()
    {
        if (MTalker == null) return;
        MyTools.SetActive(UI.questButton.gameObject, false);
        Skip();
        MakeTalkerQuestOption();
        OpenOptionArea();
    }

    public void Skip()
    {
        while (Words.Count > 0)
            SayNextWords();
    }

    public void GotoDefault()
    {
        ClearOptions();
        CloseQuestDescriptionWindow();
        StartNormalDialogue(MTalker);
    }

    private void RecycleOption(OptionAgent oa)
    {
        oa.TitleText.text = string.Empty;
        oa.talkObjective = null;
        oa.MQuest = null;
        oa.optionType = OptionType.None;
        ObjectPool.Instance.Put(oa.gameObject);
    }

    private void ClearOptions()
    {
        for (int i = 0; i < OptionAgents.Count; i++)
        {
            if (OptionAgents[i])
            {
                RecycleOption(OptionAgents[i]);
            }
        }
        OptionAgents.Clear();
        Page = 1;
        SetPageArea(false, false, false);
    }

    private void ClearOptionExceptContinue()
    {
        for (int i = 0; i < OptionAgents.Count; i++)
        {
            if (OptionAgents[i] && OptionAgents[i].optionType != OptionType.Continue)
            {
                RecycleOption(OptionAgents[i]);
            }
        }
        OptionAgents.RemoveAll(x => !x.gameObject.activeSelf);
        Page = 1;
        SetPageArea(false, false, false);
    }

    private void CheckPages()
    {
        MaxPage = Mathf.CeilToInt(OptionAgents.Count * 1.0f / ((UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) * 1.0f));
        if (MaxPage > 1)
        {
            SetPageArea(false, true, true);
            UI.pageText.text = Page.ToString() + "/" + MaxPage.ToString();
        }
        else
        {
            SetPageArea(false, false, false);
        }
    }

    private void SetPageArea(bool activeUp, bool activeDown, bool activeText)
    {
        MyTools.SetActive(UI.pageUpButton.gameObject, activeUp);
        MyTools.SetActive(UI.pageDownButton.gameObject, activeDown);
        MyTools.SetActive(UI.pageText.gameObject, activeText);
    }

    public void SetUI(DialogueUI UI)
    {
        if (!UI) return;
        this.UI = UI;
    }
    #endregion

    private enum DialogueType
    {
        Normal,
        Quest,
        Objective
    }
}