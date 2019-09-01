using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DialogueManager : SingletonMonoBehaviour<DialogueManager>, IWindowHandler
{
    [SerializeField]
    private DialogueUI UI;

    [HideInInspector]
    public UnityEvent OnBeginDialogueEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnFinishDialogueEvent = new UnityEvent();

    public DialogueType CurrentType { get; private set; } = DialogueType.Normal;

    private readonly Queue<DialogueWords> Words = new Queue<DialogueWords>();
    public Dictionary<string, DialogueData> DialogueDatas { get; private set; } = new Dictionary<string, DialogueData>();

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

    private readonly List<ItemAgent> rewardCells = new List<ItemAgent>();
    private List<OptionAgent> optionAgents = new List<OptionAgent>();

    public int OptionsCount
    {
        get
        {
            return optionAgents.Count;
        }
    }
    public OptionAgent FirstOption
    {
        get
        {
            if (optionAgents.Count < 1) return null;
            return optionAgents[0];
        }
    }

    public Talker CurrentTalker { get; private set; }
    private TalkObjective currentTalkObj;
    private SubmitObjective currentSubmitObj;
    public Quest CurrentQuest { get; private set; }

    private Dialogue currentDialog;
    private DialogueWords currentWords;
    private WordsOption currentOption;
    private readonly Stack<WordsOption> wordsOptionInstances = new Stack<WordsOption>();
    public bool NPCHasNotAcptQuests
    {
        get
        {
            Talker questGiver = CurrentTalker;
            if (!questGiver || !questGiver.QuestInstances.Exists(x => !x.IsOngoing) || questGiver.QuestInstances.Exists(x => x.IsComplete)) return false;
            return true;
        }
    }

    public bool IsTalking { get; private set; }
    public bool TalkAble { get; private set; }

    public int IndexToGoBack { get; private set; } = -1;

    public bool IsUIOpen { get; private set; }

    public bool IsPausing { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            if (!UI) return null;
            return UI.windowCanvas;
        }
    }

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
        if (!DialogueDatas.ContainsKey(dialogue.ID)) DialogueDatas.Add(dialogue.ID, new DialogueData(dialogue));
        IsTalking = true;
        Words.Clear();
        if (startIndex < 0) startIndex = 0;
        else if (startIndex > dialogue.Words.Count - 1) startIndex = dialogue.Words.Count - 1;
        for (int i = startIndex; i < dialogue.Words.Count; i++)
            Words.Enqueue(dialogue.Words[i]);
        if (sayImmediately) SayNextWords();
        else MakeContinueOption(true);
        ZetanUtilities.SetActive(UI.wordsText.gameObject, true);
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
        if (talker.QuestInstances.Count > 0)
            ZetanUtilities.SetActive(UI.questButton.gameObject, true);
        else ZetanUtilities.SetActive(UI.questButton.gameObject, false);
        ZetanUtilities.SetActive(UI.warehouseButton.gameObject, talker.Info.IsWarehouseAgent);
        ZetanUtilities.SetActive(UI.shopButton.gameObject, talker.Info.IsVendor);
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
        if (!CurrentQuest.IsComplete && !CurrentQuest.IsOngoing) StartDialogue(quest.BeginDialogue);
        else if (!CurrentQuest.IsComplete && CurrentQuest.IsOngoing) StartDialogue(quest.OngoingDialogue);
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
        Quest parent = submitObj.runtimeParent;
        var amount = BackpackManager.Instance.GetItemAmount(submitObj.ItemToSubmit);
        bool submitAble = true;
        if (parent.CmpltObjctvInOrder)
            foreach (var o in parent.ObjectiveInstances)
                if (o is CollectObjective)
                    if (o.InOrder && o.IsComplete)
                        if (amount - submitObj.Amount < o.Amount)
                        {
                            submitAble = false;
                            MessageManager.Instance.NewMessage("该物品为其它目标所需");
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
            if (BackpackManager.Instance.TryLoseItem_Boolean(option.ItemToSubmit))
            {
                if (option.ItemCanGet && option.ItemCanGet.item)
                {
                    BackpackManager.Instance.MBackpack.weightLoad -= option.ItemToSubmit.item.Weight * option.ItemToSubmit.Amount;
                    int leftAmount = BackpackManager.Instance.GetItemAmount(option.ItemToSubmit.item) - option.ItemToSubmit.Amount;
                    if (leftAmount == 0) BackpackManager.Instance.MBackpack.backpackSize--;
                    if (!BackpackManager.Instance.TryGetItem_Boolean(option.ItemCanGet))
                    {
                        BackpackManager.Instance.MBackpack.weightLoad += option.ItemToSubmit.item.Weight * option.ItemToSubmit.Amount;
                        if (leftAmount == 0) BackpackManager.Instance.MBackpack.backpackSize++;
                        return;
                    }
                    else
                    {
                        BackpackManager.Instance.MBackpack.weightLoad += option.ItemToSubmit.item.Weight * option.ItemToSubmit.Amount;
                        if (leftAmount == 0) BackpackManager.Instance.MBackpack.backpackSize++;
                        BackpackManager.Instance.LoseItem(option.ItemToSubmit.item, option.ItemToSubmit.Amount);
                        BackpackManager.Instance.GetItem(option.ItemCanGet);
                    }
                }
                else BackpackManager.Instance.LoseItem(option.ItemToSubmit.item, option.ItemToSubmit.Amount);
            }
            else return;
        }
        if (option.OptionType == WordsOptionType.OnlyGet && option.IsValid)
        {
            if (!BackpackManager.Instance.TryGetItem_Boolean(option.ItemCanGet)) return;
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
            if (optionInstance.runtimeWordsParentIndex > -1 && optionInstance.runtimeWordsParentIndex < currentDialog.Words.Count)
                talkerInfo = currentDialog.Words[optionInstance.runtimeWordsParentIndex].TalkerInfo;
            if (option.GoBack) StartOneWords(new DialogueWords(talkerInfo, optionInstance.Words, optionInstance.TalkerType), currentDialog, optionInstance.runtimeIndexToGoBack);
            else StartOneWords(new DialogueWords(talkerInfo, optionInstance.Words, optionInstance.TalkerType));
            SayNextWords();
        }
    }

    public void StartOneWords(DialogueWords words, Dialogue dialogToGoBack = null, int indexToGoBack = -1)
    {
        if (!UI || !words.IsValid) return;
        IsTalking = true;
        Words.Clear();
        Words.Enqueue(words);
        MakeContinueOption(true);
        ZetanUtilities.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        if (dialogToGoBack && indexToGoBack > -1)
        {
            if (waitToGoBackRoutine != null) StopCoroutine(waitToGoBackRoutine);
            currentDialog = dialogToGoBack;
            IndexToGoBack = indexToGoBack;
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
        if (Words.Count > 1 || force)
        {
            //如果还有话没说完，弹出一个“继续”按钮
            if (!oa)
            {
                oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
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
            ObjectPool.Instance.Put(oa.gameObject);
        }
        CheckPages();
        //当“继续”选项出现时，总没有其他选项出现，因此不必像下面一样还要处理一下页数，除非自己作死把行数写满让“继续”按钮没法显示
    }
    /// <summary>
    /// 生成任务列表的选项
    /// </summary>
    private void MakeTalkerQuestOption()
    {
        if (!CurrentTalker.Info.IsQuestGiver) return;
        ClearOptions();
        foreach (Quest quest in CurrentTalker.QuestInstances)
        {
            if (!QuestManager.Instance.HasCompleteQuest(quest) && quest.AcceptAble && quest.IsValid)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(quest.Title + (quest.IsComplete ? "(完成)" : quest.IsOngoing ? "(进行中)" : string.Empty), quest);
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
            if (oa.MQuest.IsOngoing && !oa.MQuest.IsComplete)
            {
                //则从后向前找一个新位置以放置该选项
                for (int j = optionAgents.Count - 1; j > i; j--)
                {
                    //若找到了合适的位置
                    if (!optionAgents[j].MQuest.IsOngoing && !optionAgents[j].MQuest.IsComplete)
                    {
                        //则从该位置开始到选项的原位置，逐个前移一位，填补(覆盖)选项的原位置并空出新位置
                        for (int k = i; k < j; k++)
                        {
                            //在k指向目标位置之前，逐个前移
                            optionAgents[k] = optionAgents[k + 1];
                        }
                        //把选项放入新位置，此时选项的原位置即OptionAgents[i]已被填补(覆盖)
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
            ZetanUtilities.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }
    /// <summary>
    /// 生成已完成任务选项
    /// </summary>
    private void MakeTalkerCmpltQuestOption()
    {
        if (!CurrentTalker.Info.IsQuestGiver) return;
        ClearOptions(OptionType.Branch);
        foreach (Quest quest in CurrentTalker.QuestInstances)
        {
            if (!QuestManager.Instance.HasCompleteQuest(quest) && quest.AcceptAble && quest.IsComplete)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(quest.Title + "(完成)", quest);
                optionAgents.Add(oa);
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            ZetanUtilities.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }
    /// <summary>
    /// 生成对话目标列表的选项
    /// </summary>
    private void MakeTalkerObjectiveOption()
    {
        int index = 1;
        ClearOptions(OptionType.Quest, OptionType.Branch);
        foreach (TalkObjective to in CurrentTalker.Data.objectivesTalkToThis)
        {
            if (to.AllPrevObjCmplt && !to.HasNextObjOngoing)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(to.runtimeParent.Title, to);
                optionAgents.Add(oa);
                if (index > UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) ZetanUtilities.SetActive(oa.gameObject, false);//第一页以外隐藏
                index++;
            }
        }
        index = 1;
        foreach (SubmitObjective so in CurrentTalker.Data.objectivesSubmitToThis)
        {
            if (so.AllPrevObjCmplt && !so.HasNextObjOngoing)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(so.DisplayName, so);
                optionAgents.Add(oa);
                if (index > UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) ZetanUtilities.SetActive(oa.gameObject, false);//第一页以外隐藏
                index++;
            }
        }
        CheckPages();
    }
    /// <summary>
    /// 生成分支对话选项
    /// </summary>
    private void MakeWordsOptionOption()
    {
        if (Words.Count < 1 || Words.Peek().Options.Count < 1) return;
        DialogueWords currentWords = Words.Peek();
        DialogueDatas.TryGetValue(currentDialog.ID, out DialogueData dialogDataFound);
        if (CurrentType == DialogueType.Normal) ClearOptions(OptionType.Quest, OptionType.Objective);
        else ClearOptions();
        bool isLastWords = currentDialog.IndexOfWords(Words.Peek()) == currentDialog.Words.Count - 1;
        foreach (WordsOption option in currentWords.Options)
        {
            if (option.OptionType == WordsOptionType.Choice && dialogDataFound != null)
            {
                DialogueWordsData wordsDataFound = dialogDataFound.wordsDatas.Find(x => x.wordsIndex == currentDialog.IndexOfWords(currentWords));
                //这个选择型分支是否完成了
                if (isLastWords || wordsDataFound != null && wordsDataFound.IsCmpltOptionWithIndex(currentWords.IndexOfOption(option)))
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
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(option.Title, option);
                optionAgents.Add(oa);
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            ZetanUtilities.SetActive(optionAgents[i].gameObject, false);
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
                    ZetanUtilities.SetActive(optionAgents[(Page - 1) * leftLineCount + i].gameObject, true);
                if (Page * leftLineCount + i >= 0 && Page * leftLineCount + i < optionAgents.Count)
                    ZetanUtilities.SetActive(optionAgents[Page * leftLineCount + i].gameObject, false);
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
                    ZetanUtilities.SetActive(optionAgents[(Page - 1) * leftLineCount + i].gameObject, false);
                if (Page * leftLineCount + i >= 0 && Page * leftLineCount + i < optionAgents.Count)
                    ZetanUtilities.SetActive(optionAgents[Page * leftLineCount + i].gameObject, true);
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
            //此时的OptionType.Quest实际上是OptionType.CmpltQuest，完成的任务
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
        ZetanUtilities.SetActive(UI.pageUpButton.gameObject, activeUp);
        ZetanUtilities.SetActive(UI.pageDownButton.gameObject, activeDown);
        ZetanUtilities.SetActive(UI.pageText.gameObject, activeText);
    }
    #endregion

    #region 处理每句话
    /// <summary>
    /// 转到下一句话
    /// </summary>
    public void SayNextWords()
    {
        MakeContinueOption();
        if (Words.Count > 0 && Words.Peek().Options.Count > 0)
        {
            MakeTalkerCmpltQuestOption();
            if (AllOptionComplete()) MakeTalkerObjectiveOption();
        }
        MakeWordsOptionOption();
        if (Words.Count > 0) currentWords = Words.Peek();
        if (Words.Count == 1) HandlingLastWords();//因为Dequeue之后，话就没了，Words.Count就不是1了，而是0，所以要在此之前做这一步，意思是倒数第二句做这一步
        if (Words.Count > 0)
        {
            string talkerName = currentDialog.UseUnifiedNPC ? (currentDialog.UseTalkerInfo ? CurrentTalker.Info.Name : currentDialog.UnifiedNPC.Name) : Words.Peek().TalkerName;
            if (Words.Peek().TalkerType == TalkerType.Player && PlayerManager.Instance.PlayerInfo)
                talkerName = PlayerManager.Instance.PlayerInfo.Name;
            UI.nameText.text = talkerName;
            UI.wordsText.text = Words.Peek().Words;
            Words.Dequeue();
        }
        if (Words.Count <= 0)
        {
            if (wordsOptionInstances.Count > 0)
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
                if (Words.Peek().Options.Count > 0)
                    MakeWordsOptionOption();
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
            Quest parent = currentSubmitObj.runtimeParent;
            var amount = BackpackManager.Instance.GetItemAmount(currentSubmitObj.ItemToSubmit);
            bool submitAble = true;
            if (parent.CmpltObjctvInOrder)
                foreach (var o in parent.ObjectiveInstances)
                    if (o is CollectObjective)
                        if (o.InOrder && o.IsComplete)
                            if (amount - currentSubmitObj.Amount < o.Amount)
                            {
                                submitAble = false;
                                MessageManager.Instance.NewMessage("该物品为其它目标所需");
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
                //目标已经完成，不再需要保留在对话人的目标列表里，从对话人的提交型目标里删掉相应信息
                CurrentTalker.Data.objectivesSubmitToThis.RemoveAll(x => x == currentSubmitObj);
                //该目标是任务的最后一个目标，则可以直接提交任务
                if (parent.IsComplete && parent.ObjectiveInstances.IndexOf(currentSubmitObj) == parent.ObjectiveInstances.Count - 1)
                {
                    oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
                    oa.Init("继续", parent);
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
                //目标已经完成，不再需要保留在对话人的目标列表里，从对话人的对话型目标里删掉相应信息
                CurrentTalker.Data.objectivesTalkToThis.RemoveAll(x => x == currentTalkObj);
                //该目标是任务的最后一个目标，则可以直接提交任务
                Quest parent = currentTalkObj.runtimeParent;
                if (parent.IsComplete && parent.ObjectiveInstances.IndexOf(currentSubmitObj) == parent.ObjectiveInstances.Count - 1)
                {
                    oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
                    oa.Init("继续", parent);
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
        if (!CurrentQuest.IsOngoing || CurrentQuest.IsComplete)
        {
            ClearOptions();
            //若是任务对话的最后一句，则根据任务情况弹出确认按钮
            OptionAgent yes = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
            yes.InitConfirm(CurrentQuest.IsComplete ? "完成" : "接受");
            optionAgents.Add(yes);
            if (!CurrentQuest.IsComplete)
            {
                OptionAgent no = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent).GetComponent<OptionAgent>();
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
            if (topWordsParent != null && topWordsParent.IsCorrectOption(currentOption))
            {
                if (topOptionInstance.OptionType == WordsOptionType.Choice)
                {
                    int indexOfWordsParent = topOptionInstance.runtimeDialogParent.IndexOfWords(topWordsParent);
                    foreach (WordsOption option in topWordsParent.Options)
                    {
                        string parentID = topOptionInstance.runtimeDialogParent.ID;
                        DialogueWordsData _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfWordsParent);
                        if (_find == null)
                        {
                            DialogueDatas.Add(parentID, new DialogueData(option.runtimeDialogParent));
                            _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfWordsParent);
                        }
                        int indexOfBranch = topWordsParent.IndexOfOption(option);
                        _find.cmpltBranchIndexes.Add(indexOfBranch);//该分支已完成
                    }
                    StartDialogue(topOptionInstance.runtimeDialogParent, topOptionInstance.runtimeIndexToGoBack, false);
                }
            }
            else
            {
                if (topOptionInstance.OptionType == WordsOptionType.Choice && topOptionInstance.DeleteWhenCmplt)
                {
                    int indexOfWords = topOptionInstance.runtimeDialogParent.IndexOfWords(topWordsParent);
                    string parentID = topOptionInstance.runtimeDialogParent.ID;
                    DialogueWordsData _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfWords);
                    if (_find == null)
                    {
                        DialogueDatas.Add(parentID, new DialogueData(topOptionInstance.runtimeDialogParent));
                        _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfWords);
                    }
                    int indexOfBranch = topWordsParent.IndexOfOption(currentOption);
                    _find.cmpltBranchIndexes.Add(indexOfBranch);//该分支已完成
                }
                if (topWordsParent != null && topWordsParent.NeedToChusCorrectOption && !topWordsParent.IsCorrectOption(currentOption))//选择错误，则说选择错误时应该说的话
                    StartOneWords(new DialogueWords(topWordsParent.TalkerInfo, topWordsParent.WordsWhenChusWB, topWordsParent.TalkerType),
                        topOptionInstance.runtimeDialogParent, topOptionInstance.runtimeIndexToGoBack);
                else if (topOptionInstance.GoBack)
                    //处理普通的带返回的分支
                    StartDialogue(topOptionInstance.runtimeDialogParent, topOptionInstance.runtimeIndexToGoBack, false);
            }
        }
        currentOption = null;
    }

    private Coroutine waitToGoBackRoutine;
    private IEnumerator WaitToGoBack()
    {
        yield return new WaitUntil(() => Words.Count <= 0);
        if (IndexToGoBack < 0) yield break;
        try
        {
            StartDialogue(currentDialog, IndexToGoBack, false);
            IndexToGoBack = -1;
            StopCoroutine(WaitToGoBack());
        }
        catch
        {
            StopCoroutine(WaitToGoBack());
        }
    }
    #endregion

    #region UI相关
    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!TalkAble) return;
        if (IsPausing) return;
        if (IsUIOpen) return;
        UI.dialogueWindow.alpha = 1;
        UI.dialogueWindow.blocksRaycasts = true;
        WindowsManager.Instance.PauseAll(true, this, WarehouseManager.Instance);
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteractive(false);
    }
    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        UI.dialogueWindow.alpha = 0;
        UI.dialogueWindow.blocksRaycasts = false;
        IsUIOpen = false;
        IsPausing = false;
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
        IsTalking = false;
        if (!BuildingManager.Instance.IsPreviewing) WindowsManager.Instance.PauseAll(false);
        WindowsManager.Instance.Remove(this);
        if (WarehouseManager.Instance.IsPausing) WarehouseManager.Instance.PauseDisplay(false);
        WarehouseManager.Instance.CloseWindow();
        if (ShopManager.Instance.IsPausing) ShopManager.Instance.PauseDisplay(false);
        ShopManager.Instance.CloseWindow();
        UIManager.Instance.EnableJoyStick(true);
    }

    void IWindowHandler.OpenCloseWindow() { }

    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.dialogueWindow.alpha = 1;
            UI.dialogueWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.dialogueWindow.alpha = 0;
            UI.dialogueWindow.blocksRaycasts = false;
        }
        IsPausing = pause;
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
            ItemAgent rwc = ObjectPool.Instance.Get(UI.rewardCellPrefab, UI.rewardCellsParent).GetComponent<ItemAgent>();
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
                    rw.InitItem(info);
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
        ItemWindowManager.Instance.CloseItemWindow();
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
    }

    public void CanTalk(Talker talker)
    {
        if (IsTalking || !talker) return;
        CurrentTalker = talker;
        TalkAble = true;
        UIManager.Instance.EnableInteractive(true, talker.TalkerName);
    }
    public void CannotTalk()
    {
        TalkAble = false;
        CloseWindow();
        UIManager.Instance.EnableInteractive(false);
    }

    private void ShowButtons(bool shop, bool warehouse, bool quest)
    {
        ZetanUtilities.SetActive(UI.shopButton.gameObject, shop);
        ZetanUtilities.SetActive(UI.warehouseButton.gameObject, warehouse);
        ZetanUtilities.SetActive(UI.questButton.gameObject, quest);
    }

    public void SetUI(DialogueUI UI)
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

    public void LoadTalkerQuest()
    {
        if (CurrentTalker == null) return;
        ZetanUtilities.SetActive(UI.questButton.gameObject, false);
        ZetanUtilities.SetActive(UI.warehouseButton.gameObject, false);
        ZetanUtilities.SetActive(UI.shopButton.gameObject, false);
        GotoDefault();
        Skip();
        MakeTalkerQuestOption();
    }

    public void Skip()
    {
        DialogueDatas.TryGetValue(currentDialog.ID, out DialogueData find);
        if (find != null)
            for (int i = 0; i < currentDialog.Words.Count; i++)
                for (int j = 0; j < currentDialog.Words[i].Options.Count; j++)
                    if (!find.wordsDatas[i].IsCmpltOptionWithIndex(j))
                        return;
        while (Words.Count > 0)
            SayNextWords();
    }

    public void GotoDefault()
    {
        currentOption = null;
        wordsOptionInstances.Clear();
        ClearOptions();
        HideQuestDescription();
        StartNormalDialogue(CurrentTalker);
    }

    public bool AllOptionComplete()
    {
        DialogueDatas.TryGetValue(currentDialog.ID, out DialogueData dialogDataFound);
        if (dialogDataFound == null) return false;
        for (int i = 0; i < currentDialog.Words.Count; i++)
        {
            DialogueWordsData wordsDataFound = dialogDataFound.wordsDatas.Find(x => x.wordsIndex == i);
            if (wordsDataFound != null)
                for (int j = 0; j < currentDialog.Words[i].Options.Count; j++)
                    //这个分支是否完成了
                    if (wordsDataFound.IsCmpltOptionWithIndex(j)) continue;//完成则跳过
                    else return false;//只要有一个没完成，就返回False
        }
        return true;
    }

    public void RemoveDialogueData(Dialogue dialogue)
    {
        if (!dialogue) return;
        DialogueDatas.Remove(dialogue.ID);
    }
    #endregion
}
public enum DialogueType
{
    Normal,
    Quest,
    Objective
}