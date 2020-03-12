using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void DialogueListner();
[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/对话管理器")]
public class DialogueManager : WindowHandler<DialogueUI, DialogueManager>
{
    public event DialogueListner OnBeginDialogueEvent;
    public event DialogueListner OnFinishDialogueEvent;

    private int page = 1;
    public int Page
    {
        get
        {
            return page;
        }
        private set
        {
            if (value > 1) page = value;
            else page = 1;
        }
    }

    private int MaxPage = 1;

    #region 选项相关
    private readonly List<OptionAgent> optionAgents = new List<OptionAgent>();
    public int OptionsCount => optionAgents.Count;
    public OptionAgent FirstOption
    {
        get
        {
            if (optionAgents.Count < 1) return null;
            return optionAgents[0];
        }
    }
    #endregion

    public Talker CurrentTalker { get; private set; }

    #region 任务相关
    private TalkObjective currentTalkObj;
    private SubmitObjective currentSubmitObj;
    public Quest CurrentQuest { get; private set; }
    private readonly List<ItemAgent> rewardCells = new List<ItemAgent>();
    #endregion

    private Dialogue currentDialog;
    private DialogueWords currentWords;
    private WordsOption currentOption;
    private readonly Stack<WordsOption> wordsOptionInstances = new Stack<WordsOption>();

    public DialogueType CurrentType { get; private set; } = DialogueType.Normal;

    private readonly Queue<DialogueWords> wordsToSay = new Queue<DialogueWords>();

    private readonly Dictionary<string, DialogueData> dialogueDatas = new Dictionary<string, DialogueData>();

    public bool NPCHasNotAcptQuests
    {
        get
        {
            if (!CurrentTalker || !CurrentTalker.QuestInstances.Exists(x => !x.InProcessing) || CurrentTalker.QuestInstances.Exists(x => x.IsComplete)) return false;
            return true;
        }
    }

    public bool IsTalking { get; private set; }
    public bool TalkAble { get; private set; }

    private int indexToGoBack = -1;

    #region 开始新对话
    public void BeginNewDialogue()
    {
        if (!CurrentTalker || !TalkAble || IsTalking) return;
        StartNormalDialogue(CurrentTalker);
        OnBeginDialogueEvent?.Invoke();
    }

    public void StartDialogue(Dialogue dialogue, int startIndex = 0, bool sayImmediately = true)
    {
        if (!UI && !dialogue) return;
        StopAllCoroutines();
        if (dialogue.Words.Count < 1 || !dialogue) return;
        currentDialog = dialogue;
        if (!dialogueDatas.ContainsKey(dialogue.ID)) dialogueDatas.Add(dialogue.ID, new DialogueData(dialogue));
        IsTalking = true;
        wordsToSay.Clear();
        if (startIndex < 0) startIndex = 0;
        else if (startIndex > dialogue.Words.Count - 1) startIndex = dialogue.Words.Count - 1;
        for (int i = startIndex; i < dialogue.Words.Count; i++)
            wordsToSay.Enqueue(dialogue.Words[i]);
        if (sayImmediately) SayNextWords();
        else MakeContinueOption(true);
        ZetanUtility.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        if (!IsUIOpen) OpenWindow();
    }
    //public void StartDialogue(DialogueWords words, bool sayImmediately = true)
    //{
    //    if (!UI) return;
    //    if (words == null) return;
    //    IsTalking = true;
    //    Words.Clear();
    //    Words.Enqueue(words);
    //    if (sayImmediately) SayNextWords();
    //    else MakeContinueOption(true);
    //    MyTools.SetActive(UI.wordsText.gameObject, true);
    //    SetPageArea(false, false, false);
    //    OpenDialogueWindow();
    //}

    public void StartNormalDialogue(Talker talker)
    {
        if (!UI) return;
        CurrentTalker = talker;
        CurrentType = DialogueType.Normal;
        if (talker.QuestInstances.FindAll(q => QuestManager.Instance.IsQuestAcceptable(q)).Count > 0)
            ZetanUtility.SetActive(UI.questButton.gameObject, true);
        else ZetanUtility.SetActive(UI.questButton.gameObject, false);
        ZetanUtility.SetActive(UI.warehouseButton.gameObject, talker.Info.IsWarehouseAgent);
        ZetanUtility.SetActive(UI.shopButton.gameObject, talker.Info.IsVendor);
        HideQuestDescription();
        StartDialogue(talker.Info.DefaultDialogue);
        talker.OnTalkBegin();
    }

    public void StartQuestDialogue(Quest quest)
    {
        if (!quest) return;
        CurrentQuest = quest;
        CurrentType = DialogueType.Quest;
        ShowButtons(false, false, false);
        if (!CurrentQuest.IsComplete && !CurrentQuest.InProcessing) StartDialogue(quest.BeginDialogue);
        else if (!CurrentQuest.IsComplete && CurrentQuest.InProcessing) StartDialogue(quest.OngoingDialogue);
        else StartDialogue(quest.CompleteDialogue);
    }

    public void StartObjectiveDialogue(TalkObjective talkObj)
    {
        if (talkObj == null) return;
        currentTalkObj = talkObj;
        CurrentType = DialogueType.Objective;
        ShowButtons(false, false, false);
        StartDialogue(talkObj.Dialogue);
    }

    public void StartObjectiveDialogue(SubmitObjective submitObj)
    {
        if (submitObj == null) return;
        Quest parentQ = submitObj.runtimeParent;
        var amount = BackpackManager.Instance.GetItemAmount(submitObj.ItemToSubmit);
        bool submitAble = true;
        if (parentQ.CmpltObjctvInOrder)
            foreach (var o in parentQ.ObjectiveInstances)
                if (o is CollectObjective && (o as CollectObjective).LoseItemAtSbmt && o.InOrder && o.IsComplete)
                    if (amount - submitObj.Amount < o.Amount)
                    {
                        submitAble = false;
                        MessageManager.Instance.New($"该物品为目标[{o.DisplayName}]所需");
                        break;
                    }
        submitAble &= BackpackManager.Instance.TryLoseItem_Boolean(submitObj.ItemToSubmit, submitObj.Amount);
        if (!submitAble) return;
        currentSubmitObj = submitObj;
        CurrentType = DialogueType.Objective;
        ShowButtons(false, false, false);
        StartOneWords(new DialogueWords(submitObj.TalkerType == TalkerType.NPC ? CurrentTalker.Info : null, submitObj.WordsWhenSubmit, submitObj.TalkerType));
        SayNextWords();
    }

    public void StartOptionDialogue(WordsOption option)
    {
        if (option == null || !option.IsValid) return;
        var optionInstance = option.Cloned;
        optionInstance.runtimeWordsParentIndex = currentDialog.IndexOfWords(currentWords);
        if (currentWords.NeedToChusCorrectOption)
        {
            optionInstance.runtimeDialogParent = currentDialog;
            if (currentWords.IndexOfCorrectOption == currentWords.IndexOfOption(option))
                optionInstance.runtimeIndexToGoBack = optionInstance.runtimeWordsParentIndex + 1;
            else optionInstance.runtimeIndexToGoBack = optionInstance.runtimeWordsParentIndex;
        }
        else if (option.GoBack)
        {
            optionInstance.runtimeDialogParent = currentDialog;
            if (option.OptionType == WordsOptionType.SubmitAndGet || option.OptionType == WordsOptionType.OnlyGet || option.IndexToGoBack < 0)
                optionInstance.runtimeIndexToGoBack = optionInstance.runtimeWordsParentIndex;
            else optionInstance.runtimeIndexToGoBack = option.IndexToGoBack;
        }
        wordsOptionInstances.Push(optionInstance);
        currentOption = option;
        if (option.OptionType == WordsOptionType.SubmitAndGet && option.IsValid)
        {
            if (BackpackManager.Instance.TryLoseItem_Boolean(option.ItemToSubmit, option.ItemCanGet))
                BackpackManager.Instance.LoseItem(option.ItemToSubmit, option.ItemCanGet);
            else
            {
                wordsOptionInstances.Pop();
                currentOption = null;
                return;
            }
        }
        if (option.OptionType == WordsOptionType.OnlyGet && option.IsValid)
        {
            if (!BackpackManager.Instance.TryGetItem_Boolean(option.ItemCanGet))
            {
                wordsOptionInstances.Pop();
                currentOption = null;
                return;
            }
            else BackpackManager.Instance.GetItem(option.ItemCanGet);
        }
        if (option.OptionType == WordsOptionType.Choice && (!option.HasWordsToSay || option.HasWordsToSay && string.IsNullOrEmpty(option.Words)))
        {
            HandlingLastOptionWords();
            SayNextWords();
            return;
        }
        if (option.OptionType == WordsOptionType.BranchDialogue && option.Dialogue) StartDialogue(optionInstance.Dialogue, optionInstance.SpecifyIndex);
        else if (!string.IsNullOrEmpty(option.Words))
        {
            TalkerInformation talkerInfo = null;
            if (optionInstance.TalkerType == TalkerType.NPC)
                if (!currentDialog.UseUnifiedNPC && optionInstance.runtimeWordsParentIndex > -1 && optionInstance.runtimeWordsParentIndex < currentDialog.Words.Count)
                {
                    talkerInfo = currentDialog.Words[optionInstance.runtimeWordsParentIndex].TalkerInfo;
                    //Debug.Log(talkerInfo + "A");
                }
                else if (currentDialog.UseUnifiedNPC && currentDialog.UseCurrentTalkerInfo)
                {
                    talkerInfo = CurrentTalker.Info;
                    //Debug.Log(talkerInfo + "B");
                }
                else if (currentDialog.UseUnifiedNPC && !currentDialog.UseCurrentTalkerInfo)
                {
                    talkerInfo = currentDialog.UnifiedNPC;
                    //Debug.Log(talkerInfo + "C");
                }
            if (option.GoBack)
                StartOneWords(new DialogueWords(talkerInfo, optionInstance.Words, optionInstance.TalkerType), currentDialog, optionInstance.runtimeIndexToGoBack);
            else StartOneWords(new DialogueWords(talkerInfo, optionInstance.Words, optionInstance.TalkerType));
            SayNextWords();

        }
    }

    public void StartOneWords(DialogueWords words, Dialogue dialogToGoBack = null, int indexToGoBack = -1)
    {
        if (!UI || !words.IsValid)
        {
            Debug.Log(words.Words);
            Debug.Log(words.TalkerType);
            Debug.Log(words.TalkerInfo);
            return;
        }
        IsTalking = true;
        wordsToSay.Clear();
        wordsToSay.Enqueue(words);
        MakeContinueOption(true);
        ZetanUtility.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        if (dialogToGoBack && indexToGoBack > -1)
        {
            if (waitToGoBackRoutine != null) StopCoroutine(waitToGoBackRoutine);
            currentDialog = dialogToGoBack;
            this.indexToGoBack = indexToGoBack;
            waitToGoBackRoutine = StartCoroutine(WaitToGoBack());
        }
    }
    #endregion

    #region 处理对话选项
    /// <summary>
    /// 生成继续按钮选项
    /// </summary>
    private void MakeContinueOption(bool force = false)
    {
        ClearOptions(OptionType.Continue);
        OptionAgent oa = optionAgents.Find(x => x.OptionType == OptionType.Continue);
        if (wordsToSay.Count > 1 || force)
        {
            //如果还有话没说完，弹出一个“继续”按钮
            if (!oa)
            {
                oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.InitContinue("继续");
                if (!optionAgents.Contains(oa))
                {
                    optionAgents.Add(oa);
                }
            }
        }
        else if (oa)
        {
            //如果话说完了，这是最后一句，则把“继续”按钮去掉
            optionAgents.Remove(oa);
            ObjectPool.Put(oa.gameObject);
        }
        CheckPages();
        //当“继续”选项出现时，总没有其他选项出现，因此不必像下面一样还要处理一下页数，除非自己作死把行数写满让“继续”按钮没法显示
    }
    /// <summary>
    /// 生成任务列表的选项
    /// </summary>
    private void MakeTalkerQuestOption()
    {
        if (CurrentTalker.QuestInstances.Count < 1) return;
        ClearOptions();
        foreach (Quest quest in CurrentTalker.QuestInstances)
        {
            if (!QuestManager.Instance.HasCompleteQuest(quest) && QuestManager.Instance.IsQuestAcceptable(quest) && QuestManager.IsQuestValid(quest))
            {
                OptionAgent oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(quest.Title + (quest.IsComplete ? "(完成)" : quest.InProcessing ? "(进行中)" : string.Empty), quest);
                if (quest.IsComplete)
                {
                    //保证完成的任务优先显示，方便玩家提交完成的任务
                    oa.transform.SetAsFirstSibling();
                    optionAgents.Insert(0, oa);
                }
                else optionAgents.Add(oa);
            }
        }
        //保证进行中的任务最后显示，方便玩家接取未接取的任务
        for (int i = optionAgents.Count - 1; i >= 0; i--)
        {
            OptionAgent oa = optionAgents[i];
            //若当前选项关联的任务在进行中
            if (oa.MQuest.InProcessing && !oa.MQuest.IsComplete)
            {
                //则从后向前找一个新位置以放置该选项
                for (int j = optionAgents.Count - 1; j > i; j--)
                {
                    //若找到了合适的位置
                    if (!optionAgents[j].MQuest.InProcessing && !optionAgents[j].MQuest.IsComplete)
                    {
                        //则从该位置开始到选项的原位置，逐个前移一位，填充(覆盖)选项的原位置并空出新位置
                        for (int k = i; k < j; k++)
                        {
                            //在k指向目标位置之前，逐个前移
                            optionAgents[k] = optionAgents[k + 1];
                        }
                        //把选项放入新位置，此时选项的原位置即OptionAgents[i]已被填充(覆盖)
                        optionAgents[j] = oa;
                        oa.transform.SetSiblingIndex(j);
                        break;
                    }
                }
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            ZetanUtility.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }
    /// <summary>
    /// 生成已完成任务选项
    /// </summary>
    private void MakeTalkerCmpltQuestOption()
    {
        if (CurrentTalker.QuestInstances.Count < 1) return;
        ClearOptions(OptionType.Option);
        foreach (Quest quest in CurrentTalker.QuestInstances)
        {
            if (!QuestManager.Instance.HasCompleteQuest(quest) && quest.IsComplete)
            {
                OptionAgent oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(quest.Title + "(完成)", quest);
                optionAgents.Add(oa);
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            ZetanUtility.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }
    /// <summary>
    /// 生成对话目标列表的选项
    /// </summary>
    private void MakeTalkerObjectiveOption()
    {
        int index = 1;
        ClearOptions(OptionType.Quest, OptionType.Option);
        foreach (TalkObjective to in CurrentTalker.Data.objectivesTalkToThis.Where(o => !o.IsComplete))
        {
            if (to.AllPrevObjCmplt && !to.HasNextObjOngoing)
            {
                OptionAgent oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(to.runtimeParent.Title, to);
                optionAgents.Add(oa);
                if (index > UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) ZetanUtility.SetActive(oa.gameObject, false);//第一页以外隐藏
                index++;
            }
        }
        index = 1;
        foreach (SubmitObjective so in CurrentTalker.Data.objectivesSubmitToThis.Where(o => !o.IsComplete))
        {
            if (so.AllPrevObjCmplt && !so.HasNextObjOngoing)
            {
                OptionAgent oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(so.DisplayName, so);
                optionAgents.Add(oa);
                if (index > UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) ZetanUtility.SetActive(oa.gameObject, false);//第一页以外隐藏
                index++;
            }
        }
        CheckPages();
    }
    /// <summary>
    /// 生成分支对话选项
    /// </summary>
    private void MakeNormalOption()
    {
        if (wordsToSay.Count < 1 || wordsToSay.Peek().Options.Count < 1) return;
        DialogueWords currentWords = wordsToSay.Peek();
        dialogueDatas.TryGetValue(currentDialog.ID, out DialogueData dialogDataFound);
        if (CurrentType == DialogueType.Normal) ClearOptions(OptionType.Quest, OptionType.Objective);
        else ClearOptions();
        bool isLastWords = currentDialog.IndexOfWords(wordsToSay.Peek()) == currentDialog.Words.Count - 1;
        foreach (WordsOption option in currentWords.Options)
        {
            if (option.OptionType == WordsOptionType.Choice && dialogDataFound != null)
            {
                DialogueWordsData wordsDataFound = dialogDataFound.wordsDatas.Find(x => x.wordsIndex == currentDialog.IndexOfWords(currentWords));
                //这个选择型分支是否完成了
                if (isLastWords || wordsDataFound != null && wordsDataFound.IsOptionCmplt(currentWords.IndexOfOption(option)))
                    continue;//是最后一句话或者选项完成则跳过创建
            }
            if (option.IsValid)
            {
                if (option.OptionType == WordsOptionType.OnlyGet && (option.ShowOnlyWhenNotHave && BackpackManager.Instance.HasItemWithID(option.ItemCanGet.ItemID)
                    || option.OnlyForQuest && option.BindedQuest && !QuestManager.Instance.HasOngoingQuestWithID(option.BindedQuest.ID)))
                    continue;//若已持有当前选项给的道具，或者需任务驱动但任务未接取，则跳过创建
                else if (option.OptionType == WordsOptionType.SubmitAndGet && option.OnlyForQuest && option.BindedQuest
                    && !QuestManager.Instance.HasOngoingQuestWithID(option.BindedQuest.ID))
                    continue;//若需当前选项需任务驱动但任务未接取，则跳过创建
                OptionAgent oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(option.Title, option);
                optionAgents.Add(oa);
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            ZetanUtility.SetActive(optionAgents[i].gameObject, false);
        }
        if (optionAgents.Count < 1)
            MakeContinueOption();//如果所有选择型分支都完成了，则可以进行下一句对话
        CheckPages();
        //Debug.Log("make");
        //Debug.Log(optionAgents.Count);
    }

    public void OptionPageUp()
    {
        if (Page <= 1 || !IsUIOpen || IsPausing) return;
        int leftLineCount = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight);
        if (Page > 0)
        {
            Page--;
            for (int i = 0; i < leftLineCount; i++)
            {
                if ((Page - 1) * leftLineCount + i < optionAgents.Count && (Page - 1) * leftLineCount + i >= 0)
                    ZetanUtility.SetActive(optionAgents[(Page - 1) * leftLineCount + i].gameObject, true);
                if (Page * leftLineCount + i >= 0 && Page * leftLineCount + i < optionAgents.Count)
                    ZetanUtility.SetActive(optionAgents[Page * leftLineCount + i].gameObject, false);
            }
        }
        if (Page <= 1 && MaxPage > 1) SetPageArea(false, true, true);
        else if (Page > 1 && MaxPage > 1) SetPageArea(true, true, true);
        UI.pageText.text = Page.ToString() + "/" + MaxPage.ToString();
    }
    public void OptionPageDown()
    {
        if (Page >= MaxPage || !IsUIOpen || IsPausing) return;
        int leftLineCount = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight);
        if (Page < Mathf.CeilToInt(optionAgents.Count * 1.0f / (leftLineCount * 1.0f)))
        {
            for (int i = 0; i < leftLineCount; i++)
            {
                if ((Page - 1) * leftLineCount + i < optionAgents.Count && (Page - 1) * leftLineCount + i >= 0)
                    ZetanUtility.SetActive(optionAgents[(Page - 1) * leftLineCount + i].gameObject, false);
                if (Page * leftLineCount + i >= 0 && Page * leftLineCount + i < optionAgents.Count)
                    ZetanUtility.SetActive(optionAgents[Page * leftLineCount + i].gameObject, true);
            }
            Page++;
        }
        if (Page >= MaxPage && MaxPage > 1) SetPageArea(true, false, true);
        else if (Page < MaxPage && MaxPage > 1) SetPageArea(true, true, true);
        UI.pageText.text = Page.ToString() + "/" + MaxPage.ToString();
    }

    private void ClearOptions(params OptionType[] exceptions)
    {
        for (int i = 0; i < optionAgents.Count; i++)
        {
            //此时的OptionType.Quest实际上是完成的任务
            if (exceptions.Contains(OptionType.Quest) && optionAgents[i] && optionAgents[i].OptionType == OptionType.Quest)
            {
                if (optionAgents[i].MQuest.IsComplete)
                    continue;
                else optionAgents[i].Recycle();
            }
            else if (optionAgents[i] && !exceptions.Contains(optionAgents[i].OptionType))
            {
                optionAgents[i].Recycle();
            }
        }
        if (exceptions.Length < 1) optionAgents.Clear();
        else optionAgents.RemoveAll(x => !x.gameObject.activeSelf || !x.gameObject);
        CheckPages();
    }

    private void CheckPages()
    {
        MaxPage = Mathf.CeilToInt(optionAgents.Count * 1.0f / ((UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) * 1.0f));
        if (MaxPage > 1)
        {
            SetPageArea(false, true, true);
            UI.pageText.text = Page.ToString() + "/" + MaxPage.ToString();
        }
        else SetPageArea(false, false, false);
    }

    private void SetPageArea(bool activeUp, bool activeDown, bool activeText)
    {
        ZetanUtility.SetActive(UI.pageUpButton.gameObject, activeUp);
        ZetanUtility.SetActive(UI.pageDownButton.gameObject, activeDown);
        ZetanUtility.SetActive(UI.pageText.gameObject, activeText);
    }
    #endregion

    #region 处理每句话
    /// <summary>
    /// 转到下一句话
    /// </summary>
    public void SayNextWords()
    {
        if (wordsToSay.Count < 1) return;
        MakeContinueOption();
        if (wordsToSay.Count > 0 && wordsToSay.Peek().Options.Count > 0)
        {
            MakeTalkerCmpltQuestOption();
            if (AllOptionComplete()) MakeTalkerObjectiveOption();
        }
        MakeNormalOption();
        if (wordsToSay.Count >= 1) currentWords = wordsToSay.Peek();
        if (wordsToSay.Count == 1) HandlingLastWords();//因为Dequeue之后，话就没了，Words.Count就不是1了，而是0，所以要在Dequeue之前做这一步，意思是倒数第二句做这一步
        if (wordsToSay.Count >= 1)
        {
            string talkerName = currentDialog.UseUnifiedNPC ? (currentDialog.UseCurrentTalkerInfo ? CurrentTalker.Info.name : currentDialog.UnifiedNPC.name) : wordsToSay.Peek().TalkerName;
            if (wordsToSay.Peek().TalkerType == TalkerType.Player && PlayerManager.Instance.PlayerInfo)
                talkerName = PlayerManager.Instance.PlayerInfo.name;
            UI.nameText.text = talkerName;
            UI.wordsText.text = HandlingWords(wordsToSay.Peek().Words);
            wordsToSay.Dequeue();
        }
        if (wordsToSay.Count == 0)
        {
            if (wordsOptionInstances.Count > 0)//分支栈不是空的，说明当前对话是其它某句话的一个分支
                HandlingLastOptionWords();//分支处理比较特殊，放到Dequque之后，否则分支最后一句不会讲
            OnFinishDialogueEvent?.Invoke();
        }
    }

    /// <summary>
    /// 处理最后一句对话
    /// </summary>
    private void HandlingLastWords()
    {
        if (CurrentType == DialogueType.Normal && CurrentTalker)
        {
            CurrentTalker.OnTalkFinished();
            MakeTalkerCmpltQuestOption();
            if (CurrentTalker.Data.objectivesTalkToThis != null && CurrentTalker.Data.objectivesTalkToThis.Count > 0
                || CurrentTalker.Data.objectivesSubmitToThis != null && CurrentTalker.Data.objectivesSubmitToThis.Count > 0)
                MakeTalkerObjectiveOption();
            else
            {
                ClearOptions(OptionType.Quest);
                if (wordsToSay.Peek().Options.Count > 0)
                    MakeNormalOption();
            }
            QuestManager.Instance.UpdateUI();
        }
        else if (CurrentType == DialogueType.Objective && (currentTalkObj != null || currentSubmitObj != null)) HandlingLastObjectiveWords();
        else if (CurrentType == DialogueType.Quest && CurrentQuest) HandlingLastQuestWords();
    }
    /// <summary>
    /// 处理最后一句对话型目标的对话
    /// </summary>
    private void HandlingLastObjectiveWords()
    {
        if (currentSubmitObj)
        {
            //双重确认，以防出错
            Quest qParent = currentSubmitObj.runtimeParent;
            var amount = BackpackManager.Instance.GetItemAmount(currentSubmitObj.ItemToSubmit);
            bool submitAble = true;
            if (qParent.CmpltObjctvInOrder)
                foreach (var o in qParent.ObjectiveInstances)
                    if (o is CollectObjective && (o as CollectObjective).LoseItemAtSbmt && o.InOrder && o.IsComplete)
                        if (amount - currentSubmitObj.Amount < o.Amount)
                        {
                            submitAble = false;
                            MessageManager.Instance.New($"该物品为目标[{o.DisplayName}]所需");
                            break;
                        }
            submitAble &= BackpackManager.Instance.TryLoseItem_Boolean(currentSubmitObj.ItemToSubmit, currentSubmitObj.Amount);
            if (submitAble)
            {
                BackpackManager.Instance.LoseItem(currentSubmitObj.ItemToSubmit, currentSubmitObj.Amount);
                currentSubmitObj.UpdateSubmitState(currentSubmitObj.Amount);
            }
            if (currentSubmitObj.IsComplete)
            {
                OptionAgent oa = optionAgents.Find(x => x.TalkObjective == currentSubmitObj);
                if (oa && oa.gameObject)
                {
                    //去掉该对话目标自身的提交型目标选项
                    optionAgents.Remove(oa);
                    oa.Recycle();
                }
                //若该目标是任务的最后一个目标，则可以直接提交任务
                if (qParent.currentQuestHolder == CurrentTalker.Data && qParent.IsComplete && qParent.ObjectiveInstances.IndexOf(currentSubmitObj) == qParent.ObjectiveInstances.Count - 1)
                {
                    oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
                    oa.Init("继续", qParent);
                    optionAgents.Add(oa);
                }
            }
            currentSubmitObj = null;//重置管理器的提交目标以防出错
        }
        else if (currentTalkObj)
        {
            if (!AllOptionComplete()) return;
            currentTalkObj.UpdateTalkState();
            if (currentTalkObj.IsComplete)
            {
                OptionAgent oa = optionAgents.Find(x => x.TalkObjective == currentTalkObj);
                if (oa && oa.gameObject)
                {
                    //去掉该对话目标自身的对话型目标选项
                    optionAgents.Remove(oa);
                    oa.Recycle();
                }
                //该目标是任务的最后一个目标，则可以直接提交任务
                Quest qParent = currentTalkObj.runtimeParent;
                if (qParent.currentQuestHolder == CurrentTalker.Data && qParent.IsComplete && qParent.ObjectiveInstances.IndexOf(currentTalkObj) == qParent.ObjectiveInstances.Count - 1)
                {
                    oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
                    oa.Init("继续", qParent);
                    optionAgents.Add(oa);
                }
            }
            currentTalkObj = null;//重置管理器的对话目标以防出错
        }
        QuestManager.Instance.UpdateUI();
    }
    /// <summary>
    /// 处理最后一句任务的对话
    /// </summary>
    private void HandlingLastQuestWords()
    {
        if (!AllOptionComplete()) return;
        if (!CurrentQuest.InProcessing || CurrentQuest.IsComplete)
        {
            ClearOptions();
            //若是任务对话的最后一句，则根据任务情况弹出确认按钮
            OptionAgent yes = ObjectPool.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
            yes.InitConfirm(CurrentQuest.IsComplete ? "完成" : "接受");
            optionAgents.Add(yes);
            if (!CurrentQuest.IsComplete)
            {
                OptionAgent no = ObjectPool.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
                no.InitBack("拒绝");
                optionAgents.Add(no);
            }
            ShowQuestDescription(CurrentQuest);
        }
    }
    /// <summary>
    /// 处理最后一句分支对话
    /// </summary>
    private void HandlingLastOptionWords()
    {
        WordsOption topOptionInstance = wordsOptionInstances.Pop();
        if (topOptionInstance.runtimeDialogParent)
        {
            DialogueWords topWordsParent = null;
            if (topOptionInstance.runtimeWordsParentIndex > -1 && topOptionInstance.runtimeWordsParentIndex < topOptionInstance.runtimeDialogParent.Words.Count)
                topWordsParent = topOptionInstance.runtimeDialogParent.Words[topOptionInstance.runtimeWordsParentIndex];//找到包含当前分支的语句
            if (topWordsParent != null)
            {
                int indexOfWordsParent = topOptionInstance.runtimeDialogParent.IndexOfWords(topWordsParent);
                string dialogParentID = topOptionInstance.runtimeDialogParent.ID;

                if (topOptionInstance.OptionType == WordsOptionType.Choice && topWordsParent.NeedToChusCorrectOption)//该对话需要选择正确选项，且该选项是选择型选项
                    if (topWordsParent.IsCorrectOption(currentOption))//该选项是正确选项
                                                                      //传入currentOption而不是topOptionInstance是因为前者是存于本地的原始数据，后者是实例化的数据
                    {
                        foreach (WordsOption option in topWordsParent.Options.Where(x => x.OptionType == WordsOptionType.Choice))//则其他选择型选项就跟着完成了
                        {
                            CompleteOption(option, out var result);
                            result.complete = true;
                            var choiceOptions = topWordsParent.Options.Where(x => x.OptionType == WordsOptionType.Choice).ToList();
                            for (int i = 0; i < choiceOptions.Count; i++)
                            {
                                if (result.IsOptionCmplt(i) || !choiceOptions[i].DeleteWhenCmplt) continue;
                                result.complete = false;
                                break;
                            }
                        }
                        StartDialogue(topOptionInstance.runtimeDialogParent, topOptionInstance.runtimeIndexToGoBack, false);
                    }
                    else if (topOptionInstance.DeleteWhenCmplt)//若该选项不是正确选项，但完成后需要删除
                    {
                        CompleteOption(currentOption, out _);
                    }

                void CompleteOption(WordsOption option, out DialogueWordsData result)
                {
                    dialogueDatas.TryGetValue(dialogParentID, out var dFound);
                    if (dFound == null)
                    {
                        dFound = new DialogueData(option.runtimeDialogParent);
                        dialogueDatas.Add(dialogParentID, dFound);
                    }
                    result = dFound.wordsDatas.Find(x => x.wordsIndex == indexOfWordsParent);
                    if (result == null)
                    {
                        result = new DialogueWordsData(indexOfWordsParent);
                        dFound.wordsDatas.Add(result);
                    }
                    int indexOfOption = topWordsParent.IndexOfOption(option);
                    if (!result.cmpltOptionIndexes.Contains(indexOfOption)) result.cmpltOptionIndexes.Add(indexOfOption);//该分支已完成
                    //Debug.Log($"完成选项{indexOfOption}: " + option.Title);
                }

                if (dialogueDatas.TryGetValue(dialogParentID, out var dfind))
                {
                    if (dfind.wordsDatas.TrueForAll(x => x.complete))
                        ExecuteEvents(topWordsParent);
                }
                else if (!topWordsParent.NeedToChusCorrectOption)//找不到，且该句子不需要选择正确选项，可以直接完成
                {
                    ExecuteEvents(topWordsParent);
                }
                if (topWordsParent != null && topWordsParent.NeedToChusCorrectOption && !topWordsParent.IsCorrectOption(currentOption))//选择错误，则说选择错误时应该说的话
                    StartOneWords(new DialogueWords(topWordsParent.TalkerInfo, topWordsParent.WordsWhenChusWB, topWordsParent.TalkerType),
                        topOptionInstance.runtimeDialogParent, topOptionInstance.runtimeIndexToGoBack);
                else if (topOptionInstance.GoBack)//处理普通的带返回的分支
                    StartDialogue(topOptionInstance.runtimeDialogParent, topOptionInstance.runtimeIndexToGoBack, false);
            }
        }
        currentOption = null;
    }

    private Coroutine waitToGoBackRoutine;
    private IEnumerator WaitToGoBack()
    {
        yield return new WaitUntil(() => wordsToSay.Count <= 0);
        if (indexToGoBack < 0) yield break;
        try
        {
            StartDialogue(currentDialog, indexToGoBack, false);
            indexToGoBack = -1;
            StopCoroutine(WaitToGoBack());
        }
        catch
        {
            StopCoroutine(WaitToGoBack());
        }
    }
    #endregion

    #region UI相关
    public override void OpenWindow()
    {
        if (!TalkAble) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        WindowsManager.Instance.PauseAll(true, this);
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteract(false);
        PlayerManager.Instance.PlayerController.controlAble = false;
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        CurrentType = DialogueType.Normal;
        CurrentTalker = null;
        CurrentQuest = null;
        currentDialog = null;
        currentOption = null;
        currentTalkObj = null;
        currentSubmitObj = null;
        wordsOptionInstances.Clear();
        ClearOptions();
        HideQuestDescription();
        if (!BuildingManager.Instance.IsPreviewing) WindowsManager.Instance.PauseAll(false);
        if (WarehouseManager.Instance.IsPausing) WarehouseManager.Instance.PauseDisplay(false);
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        if (ShopManager.Instance.IsPausing) ShopManager.Instance.PauseDisplay(false);
        if (ShopManager.Instance.IsUIOpen) ShopManager.Instance.CloseWindow();
        IsTalking = false;
        UIManager.Instance.EnableJoyStick(true);
        PlayerManager.Instance.PlayerController.controlAble = true;
    }

    public void ShowQuestDescription(Quest quest)
    {
        if (quest == null) return;
        CurrentQuest = quest;
        UI.descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}",
            CurrentQuest.Title,
            CurrentQuest.originalQuestHolder.TalkerName,
            CurrentQuest.Description).ToString();
        UI.moneyText.text = CurrentQuest.RewardMoney > 0 ? CurrentQuest.RewardMoney.ToString() : "无";
        UI.EXPText.text = CurrentQuest.RewardEXP > 0 ? CurrentQuest.RewardEXP.ToString() : "无";
        int befCount = rewardCells.Count;
        for (int i = 0; i < 10 - befCount; i++)
        {
            ItemAgent rwc = ObjectPool.Get(UI.rewardCellPrefab, UI.rewardCellsParent).GetComponent<ItemAgent>();
            rwc.Init();
            rewardCells.Add(rwc);
        }
        foreach (ItemAgent rwc in rewardCells)
            if (rwc) rwc.Empty();
        foreach (ItemInfo info in quest.RewardItems)
            foreach (ItemAgent rw in rewardCells)
            {
                if (rw.IsEmpty)
                {
                    rw.SetItem(info);
                    break;
                }
            }
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
    }
    public void HideQuestDescription()
    {
        CurrentQuest = null;
        UI.descriptionWindow.alpha = 0;
        UI.descriptionWindow.blocksRaycasts = false;
        ItemWindowManager.Instance.CloseWindow();
    }

    public void OpenTalkerWarehouse()
    {
        if (!CurrentTalker || !CurrentTalker.Info.IsWarehouseAgent) return;
        PauseDisplay(true);
        BackpackManager.Instance.PauseDisplay(false);
        WarehouseManager.Instance.Init(CurrentTalker.Data.warehouse);
        WarehouseManager.Instance.OpenWindow();
    }
    public void OpenTalkerShop()
    {
        if (!CurrentTalker || !CurrentTalker.Info.IsVendor) return;
        ShopManager.Instance.Init(CurrentTalker.Data.shop);
        ShopManager.Instance.OpenWindow();
        PauseDisplay(true);
        BackpackManager.Instance.PauseDisplay(false);
        BackpackManager.Instance.OpenWindow();
    }
    public void OpenGiftWindow()
    {
        //TODO 把玩家背包道具读出并展示
        ItemSelectionManager.Instance.StartSelection(ItemSelectionType.Gift, "选择礼物", "确定要送出这些礼物吗？", null);
    }

    public void CanTalk(Talker talker)
    {
        if (IsTalking || !talker) return;
        CurrentTalker = talker;
        TalkAble = true;
        UIManager.Instance.EnableInteract(true, talker.TalkerName);
    }
    public void CannotTalk()
    {
        TalkAble = false;
        IsPausing = false;
        CloseWindow();
        UIManager.Instance.EnableInteract(false);
    }

    private void ShowButtons(bool shop, bool warehouse, bool quest)
    {
        ZetanUtility.SetActive(UI.shopButton.gameObject, shop);
        ZetanUtility.SetActive(UI.warehouseButton.gameObject, warehouse);
        ZetanUtility.SetActive(UI.questButton.gameObject, quest);
    }

    public override void SetUI(DialogueUI UI)
    {
        if (waitToGoBackRoutine != null) StopCoroutine(waitToGoBackRoutine);
        foreach (var oa in optionAgents)
        {
            if (oa && oa.gameObject) oa.Recycle();
        }
        optionAgents.Clear();
        wordsOptionInstances.Clear();
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion

    #region 其它
    public void SendTalkerGifts()
    {
        if (!CurrentTalker || !CurrentTalker.Info.CanDEV_RLAT) return;
        OpenGiftWindow();
    }

    public void ShowTalkerQuest()
    {
        if (CurrentTalker == null) return;
        ZetanUtility.SetActive(UI.questButton.gameObject, false);
        ZetanUtility.SetActive(UI.warehouseButton.gameObject, false);
        ZetanUtility.SetActive(UI.shopButton.gameObject, false);
        GoBackDefault();
        Skip();
        MakeTalkerQuestOption();
    }

    public void Skip()
    {
        dialogueDatas.TryGetValue(currentDialog.ID, out DialogueData find);
        if (find != null)
            for (int i = 0; i < currentDialog.Words.Count; i++)
                for (int j = 0; j < currentDialog.Words[i].Options.Count; j++)
                    if (!find.wordsDatas[i].IsOptionCmplt(j))//只要有一句话的分支有没完成的，则无法跳过
                        return;
        while (wordsToSay.Count > 0)
            SayNextWords();
    }

    public void GoBackDefault()
    {
        currentOption = null;
        wordsOptionInstances.Clear();
        ClearOptions();
        HideQuestDescription();
        StartNormalDialogue(CurrentTalker);
    }

    private bool AllOptionComplete()
    {
        dialogueDatas.TryGetValue(currentDialog.ID, out DialogueData dialogDataFound);
        if (dialogDataFound == null) return false;
        for (int i = 0; i < currentDialog.Words.Count; i++)
        {
            DialogueWordsData wordsDataFound = dialogDataFound.wordsDatas.Find(x => x.wordsIndex == i);
            if (wordsDataFound != null)
                for (int j = 0; j < currentDialog.Words[i].Options.Count; j++)
                    //这个分支是否完成了
                    if (wordsDataFound.IsOptionCmplt(j)) continue;//完成则跳过
                    else return false;//只要有一个没完成，就返回False
        }
        return true;
    }

    private void ExecuteEvents(DialogueWords words)
    {
        foreach (var we in words.Events)
        {
            switch (we.EventType)
            {
                case WordsEventType.Trigger:
                    break;
                case WordsEventType.GetAmity:
                    break;
                case WordsEventType.LoseAmity:
                    break;
                default:
                    break;
            }
        }
    }

    public void RemoveDialogueData(Dialogue dialogue)
    {
        if (!dialogue) return;
        dialogueDatas.Remove(dialogue.ID);
    }

    private string HandlingWords(string words)
    {
        StringBuilder newWords = new StringBuilder();
        StringBuilder keyWordsSB = new StringBuilder();
        bool startKey = false;
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i] == '{' && i + 1 < words.Length)
            {
                startKey = true;
                i++;
            }
            else if (words[i] == '}')
            {
                startKey = false;
                newWords.Append(HandlingName(keyWordsSB.ToString()));
                keyWordsSB.Clear();
            }
            else if (!startKey) newWords.Append(words[i]);
            if (startKey) keyWordsSB.Append(words[i]);
        }

        return newWords.ToString();

        string HandlingName(string keyWords)
        {
            if (keyWords.StartsWith("[NPC]"))//为了性能，建议多此一举
            {
                keyWords = keyWords.Replace("[NPC]", string.Empty);
                GameManager.TalkerInfos.TryGetValue(keyWords, out var talker);
                if (talker) keyWords = talker.name;
                return ZetanUtility.ColorText(keyWords, Color.green);
            }
            else if (keyWords.StartsWith("[ITEM]"))
            {
                keyWords = keyWords.Replace("[ITEM]", string.Empty);
                GameManager.Items.TryGetValue(keyWords, out var item);
                if (item) keyWords = item.name;
                return ZetanUtility.ColorText(keyWords, Color.yellow);
            }
            else if (keyWords.StartsWith("[ENMY]"))
            {
                keyWords = keyWords.Replace("[ENMY]",string.Empty);
                GameManager.EnemyInfos.TryGetValue(keyWords, out var enemy);
                if (enemy) keyWords = enemy.name;
                return ZetanUtility.ColorText(keyWords, Color.red);
            }
            return keyWords;
        }
    }

    public void SaveData(SaveData data)
    {
        foreach (KeyValuePair<string, DialogueData> kvpDialog in dialogueDatas)
        {
            data.dialogueDatas.Add(kvpDialog.Value);
        }
    }
    public void LoadData(SaveData data)
    {
        dialogueDatas.Clear();
        foreach (DialogueData dd in data.dialogueDatas)
        {
            dialogueDatas.Add(dd.dialogID, dd);
        }
    }
    #endregion
}
public enum DialogueType
{
    Normal,
    Quest,
    Objective
}