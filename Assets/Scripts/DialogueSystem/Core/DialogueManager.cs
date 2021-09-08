using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ZetanExtends;

public delegate void DialogueListner();
[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/对话管理器")]
public class DialogueManager : WindowHandler<DialogueUI, DialogueManager>
{
    public event DialogueListner OnBeginDialogueEvent;
    public event DialogueListner OnFinishDialogueEvent;

    private Transform talkerRoot;

    public Talker CurrentTalker { get; private set; }
    public bool IsTalking { get; private set; }

    public DialogueType CurrentType { get; private set; } = DialogueType.Normal;

    public bool ShouldShowQuest
    {
        get
        {
            if (!CurrentTalker || !CurrentTalker.QuestInstances.Exists(x => !x.InProgress) || CurrentTalker.QuestInstances.Exists(x => x.IsComplete)) return false;
            return true;
        }
    }

    public DialogueData currentDialogue;
    public DialogueWordsData currentWords;
    public WordsOptionData currentOption;
    private readonly Stack<DialogueData> dialogueToSay = new Stack<DialogueData>();
    private readonly Stack<DialogueWordsData> wordsToSay = new Stack<DialogueWordsData>();
    private readonly Stack<WordsOptionData> choiceOptionSaid = new Stack<WordsOptionData>();

    private readonly Dictionary<string, DialogueData> dialogueDatas = new Dictionary<string, DialogueData>();

    public readonly Dictionary<string, TalkerData> Talkers = new Dictionary<string, TalkerData>();
    private readonly Dictionary<string, List<TalkerData>> talkersMap = new Dictionary<string, List<TalkerData>>();

    #region 选项相关
    private readonly List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();

    private readonly List<ButtonWithText> optionAgents = new List<ButtonWithText>();
    public int OptionsCount => optionAgents.Count;
    public ButtonWithText FirstOption
    {
        get
        {
            if (optionAgents.Count < 1) return null;
            return optionAgents[0];
        }
    }

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
    #endregion

    #region 任务相关
    private QuestData currentQuest;
    private TalkObjectiveData currentTalkObj;
    private SubmitObjectiveData currentSubmitObj;
    private readonly List<ItemSlotBase> rewardCells = new List<ItemSlotBase>();
    #endregion

    #region 开始新对话
    public void BeginNewDialogue()
    {
        if (!CurrentTalker || IsTalking) return;
        StartTalking(CurrentTalker);
        OnBeginDialogueEvent?.Invoke();
    }

    private void StartTalking(Talker talker)
    {
        if (!UI) return;
        CurrentTalker = talker;
        CurrentType = DialogueType.Normal;
        ShowButtons(talker.Data.Info.CanDEV_RLAT, talker.Data.Info.IsVendor, talker.Data.Info.IsWarehouseAgent, talker.QuestInstances.FindAll(q => !q.IsFinished && MiscFuntion.CheckCondition(q.Info.AcceptCondition)).Count > 0);
        HideQuestDescription();
        StartDialogue(talker.DefaultDialogue);
        talker.OnTalkBegin();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (!dialogueDatas.TryGetValue(dialogue.ID, out var currentData))
        {
            currentData = new DialogueData(dialogue);
            dialogueDatas.Add(dialogue.ID, currentData);
        }
        dialogueToSay.Push(currentData);
        wordsToSay.Clear();
        DoSay();
    }

    public void StartOneWords(DialogueWords words)
    {
        wordsToSay.Clear();
        wordsToSay.Push(new DialogueWordsData(words, null));
        DoSay();
    }

    private void DoSay()
    {
        IsTalking = true;
        ZetanUtility.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        if (!IsUIOpen) OpenWindow();
        SayNextWords();
    }
    #endregion

    #region 处理对话选项
    private void HandlingOptions()
    {
        buttonDatas.Clear();
        if (dialogueToSay.Count > 0 || wordsToSay.Count > 0 && (!currentWords.origin.NeedToChusCorrectOption || currentWords.IsDone))
        {
            buttonDatas.Add(new ButtonWithTextData("继续", delegate { SayNextWords(); }));
        }

        if (!currentOption && !currentQuest)
        {
            var cmpltQuest = CurrentTalker.QuestInstances.Where(x => x.InProgress && x.IsComplete);

            if (cmpltQuest != null && cmpltQuest.Count() > 0)
            {
                foreach (var quest in cmpltQuest)
                {
                    buttonDatas.Add(new ButtonWithTextData($"{quest.Info.Title}(已完成)", delegate
                    {
                        currentQuest = quest;
                        ShowButtons(false, false, false, false);
                        StartDialogue(quest.Info.CompleteDialogue);
                    }));
                }
            }
        }
        if (wordsToSay.Count < 1)//最后一句
        {
            //完成剧情用对话时，结束对话
            if (currentWords && currentWords.parent && currentWords.parent.origin.StoryDialogue && currentWords.IsDone)
            {
                buttonDatas.Add(new ButtonWithTextData("结束", delegate
                {
                    OnFinishDialogueEvent?.Invoke();
                    CloseWindow();
                }));
                RefreshOptionUI();
                return;
            }
            if (CurrentType == DialogueType.Gift)
            {
                buttonDatas.Add(new ButtonWithTextData("返回", delegate { GoBackDefault(); }));
            }
            else
            {
                if (currentQuest)//这是由任务引发的对话
                {
                    HandlingLast_Quest();
                }
                else if (currentSubmitObj)//这是由提交目标引发的对话
                {
                    HandlingLast_SumbitObj();
                }
                else if (currentTalkObj)//这是由对话目标引发的对话
                {
                    HandlingLast_TalkObj();
                }
                else if (CurrentTalker)//普通对话
                {
                    HandlingLast_Normal();
                }
            }
            OnFinishDialogueEvent?.Invoke();
        }
        if (currentWords)
        {
            foreach (var option in currentWords.optionDatas)
            {
                if (!option.isDone || option.origin.OptionType != WordsOptionType.Choice)
                {
                    string title = option.origin.Title;
                    if (option.origin.OptionType == WordsOptionType.SubmitAndGet)
                        title += $"(需[{option.origin.ItemToSubmit.ItemName}]{option.origin.ItemToSubmit.Amount}个)";
                    buttonDatas.Add(new ButtonWithTextData(title, delegate
                    {
                        DoOption(option);
                    }));
                }
            }
        }
        RefreshOptionUI();
    }
    private void HandlingQuestOptions()
    {
        var cmpltQuests = new List<QuestData>();
        var norQuests = new List<QuestData>();
        foreach (var quest in CurrentTalker.QuestInstances)
        {
            if (MiscFuntion.CheckCondition(quest.Info.AcceptCondition) && !quest.IsFinished)
                if (quest.IsComplete)
                    cmpltQuests.Add(quest);
                else norQuests.Add(quest);
        }
        buttonDatas.Clear();
        foreach (var quest in cmpltQuests)
        {
            buttonDatas.Add(new ButtonWithTextData(quest.Info.Title + "(完成)", delegate
            {
                currentQuest = quest;
                CurrentType = DialogueType.Quest;
                ShowButtons(false, false, false, false);
                StartDialogue(quest.Info.CompleteDialogue);
            }));
        }
        foreach (var quest in norQuests)
        {
            buttonDatas.Add(new ButtonWithTextData(quest.Info.Title + (quest.InProgress ? "(进行中)" : string.Empty), delegate
            {
                currentQuest = quest;
                CurrentType = DialogueType.Quest;
                ShowButtons(false, false, false, false);
                StartDialogue(quest.InProgress ? quest.Info.OngoingDialogue : quest.Info.BeginDialogue);
            }));
        }
        RefreshOptionUI();
    }

    private void HandlingLast_Quest()
    {
        if (!currentQuest.InProgress || currentQuest.IsComplete) ShowQuestDescription(currentQuest);
        if (!currentQuest.IsComplete && !currentQuest.InProgress)
        {
            buttonDatas.Add(new ButtonWithTextData("接受", delegate
            {
                if (QuestManager.Instance.AcceptQuest(currentQuest))
                {
                    CurrentType = DialogueType.Normal;
                    ShowTalkerQuest();
                }
            }));
        }
        else if (currentQuest.IsComplete && currentQuest.InProgress)
        {
            buttonDatas.Add(new ButtonWithTextData("完成", delegate
            {
                if (QuestManager.Instance.CompleteQuest(currentQuest))
                {
                    CurrentType = DialogueType.Normal;
                    ShowTalkerQuest();
                }
            }));
        }
        buttonDatas.Add(new ButtonWithTextData(currentQuest.InProgress ? "返回" : "拒绝", delegate
        {
            CurrentType = DialogueType.Normal;
            ShowTalkerQuest();
        }));
    }
    private void HandlingLast_TalkObj()
    {
        if (!currentWords.IsDone) return;
        currentTalkObj.UpdateTalkState();
        if (currentTalkObj.IsComplete)
        {
            QuestData qParent = currentTalkObj.runtimeParent;
            //该目标是任务的最后一个目标，则可以直接提交任务
            if (qParent.currentQuestHolder == CurrentTalker.Data && qParent.IsComplete && qParent.ObjectiveInstances.IndexOf(currentTalkObj) == qParent.ObjectiveInstances.Count - 1)
            {
                buttonDatas.Add(new ButtonWithTextData("继续", delegate
                {
                    currentQuest = qParent;
                    CurrentType = DialogueType.Quest;
                    StartDialogue(qParent.Info.CompleteDialogue);
                }));
            }
        }
        currentTalkObj = null;//重置管理器的对话目标以防出错
    }
    private void HandlingLast_SumbitObj()
    {
        //双重确认，以防出错
        if (CheckSumbitAble(currentSubmitObj))
        {
            BackpackManager.Instance.LoseItem(currentSubmitObj.Info.ItemToSubmit, currentSubmitObj.Info.Amount);
            currentSubmitObj.UpdateSubmitState(currentSubmitObj.Info.Amount);
        }
        if (currentSubmitObj.IsComplete)
        {
            QuestData qParent = currentSubmitObj.runtimeParent;
            //若该目标是任务的最后一个目标，则可以直接提交任务
            if (qParent.currentQuestHolder == CurrentTalker.Data && qParent.IsComplete && qParent.ObjectiveInstances.IndexOf(currentSubmitObj) == qParent.ObjectiveInstances.Count - 1)
            {
                buttonDatas.Add(new ButtonWithTextData("继续", delegate
                {
                    currentQuest = qParent;
                    CurrentType = DialogueType.Quest;
                    StartDialogue(qParent.Info.CompleteDialogue);
                }));
            }
        }
        currentSubmitObj = null;//重置管理器的提交目标以防出错
    }
    private bool CheckSumbitAble(SubmitObjectiveData so)
    {
        QuestData qParent = so.runtimeParent;
        var amount = BackpackManager.Instance.GetItemAmount(so.Info.ItemToSubmit);
        bool submitAble = true;
        if (qParent.Info.CmpltObjctvInOrder)
            foreach (var o in qParent.ObjectiveInstances)
                if (o is CollectObjectiveData co && co.Info.LoseItemAtSbmt && o.Info.InOrder && o.IsComplete)
                    if (amount - so.Info.Amount < o.Info.Amount)
                    {
                        submitAble = false;
                        MessageManager.Instance.New($"该物品为目标[{o.Info.DisplayName}]所需");
                        break;
                    }
        submitAble &= BackpackManager.Instance.TryLoseItem_Boolean(so.Info.ItemToSubmit, so.Info.Amount);
        return submitAble;
    }

    private void HandlingLast_Normal()
    {
        foreach (TalkObjectiveData to in CurrentTalker.Data.objectivesTalkToThis.Where(o => !o.IsComplete))
        {
            if (to.AllPrevObjCmplt && !to.HasNextObjOngoing)
            {
                buttonDatas.Add(new ButtonWithTextData(to.runtimeParent.Info.Title, delegate
                {
                    currentTalkObj = to;
                    CurrentType = DialogueType.Objective;
                    ShowButtons(false, false, false, false);
                    StartDialogue(currentTalkObj.Info.Dialogue);
                }));
            }
        }
        foreach (SubmitObjectiveData so in CurrentTalker.Data.objectivesSubmitToThis.Where(o => !o.IsComplete))
        {
            if (so.AllPrevObjCmplt && !so.HasNextObjOngoing)
            {
                buttonDatas.Add(new ButtonWithTextData(so.Info.DisplayName, delegate
                {
                    if (CheckSumbitAble(so))
                    {
                        currentSubmitObj = so;
                        CurrentType = DialogueType.Objective;
                        ShowButtons(false, false, false, false);
                        StartOneWords(new DialogueWords(currentWords.origin.TalkerInfo, currentSubmitObj.Info.WordsWhenSubmit));
                    }
                }));
            }
        }
        CurrentTalker.OnTalkFinished();
    }

    private void RefreshOptionUI()
    {
        ClearOptions();
        foreach (var option in buttonDatas)
        {
            var oa = ObjectPool.Get(UI.optionPrefab, UI.optionsParent).GetComponent<ButtonWithText>();
            oa.Init(option.text, option.callback);
            optionAgents.Add(oa);
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            ZetanUtility.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }
    private void ClearOptions()
    {
        for (int i = 0; i < optionAgents.Count; i++)
        {
            optionAgents[i].Recycle();
        }
        optionAgents.Clear();
        CheckPages();
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
    private void SayNextWords()
    {
        if (wordsToSay.Count > 0)
        {
            currentWords = wordsToSay.Pop();
            DoOneWords(currentWords.origin);
        }
        else if (dialogueToSay.Count > 0)
        {
            currentDialogue = dialogueToSay.Pop();
            DoDialogue(currentDialogue.origin);
            SayNextWords();
        }
    }

    private void DoOneWords(DialogueWords words)
    {
        string talkerName = words.TalkerName;
        if (currentDialogue)
            talkerName = currentDialogue.origin.UseUnifiedNPC ? (currentDialogue.origin.UseCurrentTalkerInfo ? CurrentTalker.Data.Info.name : currentDialogue.origin.UnifiedNPC.name) : talkerName;
        if (words.TalkerType == TalkerType.Player && PlayerManager.Instance.PlayerInfo)
            talkerName = PlayerManager.Instance.PlayerInfo.name;
        UI.nameText.text = talkerName;
        UI.wordsText.text = HandlingContent(words.Content);
        HandlingWords();
    }
    private void HandlingWords()
    {
        if (wordsToSay.Count < 1)
        {
            if (currentOption)//选项引发的对话最后一句
            {
                if (choiceOptionSaid.Count > 0 && this.currentOption == choiceOptionSaid.Peek())
                    choiceOptionSaid.Pop();
                var currentOption = this.currentOption;
                this.currentOption = null;
                if (currentOption.origin.OptionType != WordsOptionType.Choice || currentOption.origin.OptionType == WordsOptionType.Choice && currentOption.origin.DeleteWhenCmplt)
                    currentOption.isDone = true;
                if (currentOption.origin.OptionType == WordsOptionType.Choice && currentOption.parent.origin.IsCorrectOption(currentOption.origin))//选择型选项的最后一句
                {
                    foreach (var option in currentOption.parent.optionDatas.Where(x => x.origin.OptionType == WordsOptionType.Choice))
                    {
                        option.isDone = true;
                    }
                    while (choiceOptionSaid.Count > 0)
                    {
                        var preOption = choiceOptionSaid.Pop();
                        preOption.isDone = true;
                        if (choiceOptionSaid.Count < 1)
                            foreach (var option in preOption.parent.optionDatas.Where(x => x.origin.OptionType == WordsOptionType.Choice))
                            {
                                option.isDone = true;
                            }
                    }
                }
                else if (currentOption.origin.GoBack || currentOption.parent.parent.origin.StoryDialogue)
                {
                    DoDialogue(currentOption.parent.parent.origin, currentOption.indexToGoBack);
                }
            }
            if (currentWords.IsDone)//整句话完成了才触发事件
                ExecuteEvents(currentWords.origin);
        }
        HandlingOptions();
    }

    private void DoDialogue(Dialogue dialogue, int startIndex = 0)
    {
        if (!dialogueDatas.TryGetValue(dialogue.ID, out var findDialog))
        {
            findDialog = new DialogueData(dialogue);
            dialogueDatas.Add(dialogue.ID, findDialog);
        }
        if (startIndex < 0) startIndex = 0;
        wordsToSay.Clear();
        for (int i = findDialog.wordsDatas.Count - 1; i >= startIndex; i--)
        {
            wordsToSay.Push(findDialog.wordsDatas[i]);
        }
        PauseButtons(dialogue.StoryDialogue);
    }

    private void DoOption(WordsOptionData option)
    {
        wordsToSay.Clear();
        currentOption = option;

        if (option.origin.OptionType == WordsOptionType.SubmitAndGet)
        {
            if (option.origin.IsValid)
            {
                if (BackpackManager.Instance.TryLoseItem_Boolean(option.origin.ItemToSubmit, option.origin.ItemCanGet))
                    BackpackManager.Instance.LoseItem(option.origin.ItemToSubmit, option.origin.ItemCanGet);
                else
                {
                    currentOption = null;
                    return;
                }
            }
            else MessageManager.Instance.New("无效的选项");
        }
        if (option.origin.OptionType == WordsOptionType.OnlyGet)
        {
            if (option.origin.IsValid)
            {
                if (!BackpackManager.Instance.TryGetItem_Boolean(option.origin.ItemCanGet))
                {
                    currentOption = null;
                    return;
                }
                else BackpackManager.Instance.GetItem(option.origin.ItemCanGet);
            }
            else MessageManager.Instance.New("无效的选项");
        }
        if (option.origin.OptionType == WordsOptionType.Choice)//是选择型选项
        {
            choiceOptionSaid.Push(option);
            if (option.parent.origin.IsCorrectOption(option.origin))
            {
                DoDialogue(option.parent.parent.origin, option.indexToGoBack);
            }
            else
            {
                NewWords(option.parent.origin.TalkerType != TalkerType.Player, option.parent.origin.WrongChoiceWords);//对话人的类型由句子决定，说的话是句子带的选择错误时说的话
            }
        }
        if (option.origin.HasWordsToSay || option.origin.OptionType == WordsOptionType.BranchWords)
        {
            NewWords(option.origin.TalkerType != TalkerType.Player, option.origin.Words);//对话人的类型由选项决定，说的话的选项自带的
        }
        else if (option.origin.OptionType == WordsOptionType.BranchDialogue)
        {
            DoDialogue(option.origin.Dialogue, option.origin.SpecifyIndex);
        }
        SayNextWords();

        void NewWords(bool condition, string words)
        {
            TalkerInformation talkerInfo = null;
            if (condition)
            {
                if (!option.parent.parent.origin.UseUnifiedNPC)
                    talkerInfo = option.parent.origin.TalkerInfo;
                else if (option.parent.parent.origin.UseUnifiedNPC && option.parent.parent.origin.UseCurrentTalkerInfo)
                    talkerInfo = CurrentTalker.Data.Info;
                else if (option.parent.parent.origin.UseUnifiedNPC && !option.parent.parent.origin.UseCurrentTalkerInfo)
                    talkerInfo = option.parent.parent.origin.UnifiedNPC;
                wordsToSay.Push(new DialogueWordsData(new DialogueWords(talkerInfo, words), null));
            }
            else
            {
                wordsToSay.Push(new DialogueWordsData(new DialogueWords(null, words, TalkerType.Player), null));
            }
        }
    }
    #endregion

    #region UI相关
    public bool Talk(Talker talker)
    {
        if (GatherManager.Instance.IsGathering)
        {
            MessageManager.Instance.New("请先等待采集完成");
            return false;
        }
        if (PlayerManager.Instance.Character.GetMainState(out var state))
        {
            if (state == CharacterState.Normal)
            {
                if (IsTalking || !talker) return false;
                CurrentTalker = talker;
                BeginNewDialogue();
                return true;
            }
            return false;
        }
        return false;
    }
    public void CancelTalk()
    {
        IsPausing = false;
        CloseWindow();
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        WindowsManager.Instance.PauseAll(true, this);
        UIManager.Instance.EnableJoyStick(false);
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(DialogueManager), true);
        PlayerManager.Instance.Controller.ForceStop();
        PlayerManager.Instance.Character.SetState(CharacterState.Busy, CharacterBusyState.Talking);
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        CurrentType = DialogueType.Normal;
        if (CurrentTalker) CurrentTalker.FinishInteraction();
        CurrentTalker = null;
        currentQuest = null;
        currentTalkObj = null;
        currentSubmitObj = null;
        choiceOptionSaid.Clear();
        ClearOptions();
        HideQuestDescription();
        if (!BuildingManager.Instance.IsPreviewing) WindowsManager.Instance.PauseAll(false);
        if (WarehouseManager.Instance.IsPausing) WarehouseManager.Instance.PauseDisplay(false);
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        if (ShopManager.Instance.IsPausing) ShopManager.Instance.PauseDisplay(false);
        if (ShopManager.Instance.IsUIOpen) ShopManager.Instance.CloseWindow();
        IsTalking = false;
        UIManager.Instance.EnableJoyStick(true);
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(DialogueManager), false);
        PlayerManager.Instance.Character.SetState(CharacterState.Normal, CharacterNormalState.Idle);
    }

    public void ShowQuestDescription(QuestData quest)
    {
        if (quest == null) return;
        currentQuest = quest;
        UI.descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}",
            currentQuest.Info.Title,
            currentQuest.originalQuestHolder.TalkerName,
            currentQuest.Info.Description).ToString();
        UI.moneyText.text = currentQuest.Info.RewardMoney > 0 ? currentQuest.Info.RewardMoney.ToString() : "无";
        UI.EXPText.text = currentQuest.Info.RewardEXP > 0 ? currentQuest.Info.RewardEXP.ToString() : "无";
        int befCount = rewardCells.Count;
        for (int i = 0; i < 10 - befCount; i++)
        {
            ItemSlotBase rwc = ObjectPool.Get(UI.rewardCellPrefab, UI.rewardCellsParent).GetComponent<ItemSlotBase>();
            rwc.Init();
            rewardCells.Add(rwc);
        }
        foreach (ItemSlotBase rwc in rewardCells)
            if (rwc) rwc.Empty();
        foreach (ItemInfoBase info in quest.Info.RewardItems)
            foreach (ItemSlotBase rw in rewardCells)
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
        currentQuest = null;
        UI.descriptionWindow.alpha = 0;
        UI.descriptionWindow.blocksRaycasts = false;
        ItemWindowManager.Instance.CloseWindow();
    }

    public void OpenTalkerWarehouse()
    {
        if (!CurrentTalker || !CurrentTalker.Data.Info.IsWarehouseAgent) return;
        PauseDisplay(true);
        BackpackManager.Instance.PauseDisplay(false);
        WarehouseManager.Instance.Manage(CurrentTalker.Data.warehouse);
    }
    public void OpenTalkerShop()
    {
        if (!CurrentTalker || !CurrentTalker.Data.Info.IsVendor) return;
        ShopManager.Instance.Init(CurrentTalker.Data.shop);
        ShopManager.Instance.OpenWindow();
        PauseDisplay(true);
    }
    public void OpenGiftWindow()
    {
        //TODO 把玩家背包道具读出并展示
        ItemSelectionManager.Instance.StartSelection(ItemSelectionType.SelectNum, "挑选一件礼物", "确定要送出这个礼物吗？", 1, 1, null, OnSendGift, delegate
           {
               BackpackManager.Instance.PauseDisplay(false);
               BackpackManager.Instance.CloseWindow();
               PauseDisplay(false);
           });
        PauseDisplay(true);
    }
    private void OnSendGift(IEnumerable<ItemSelectionData> items)
    {
        if (items != null && items.Count() > 0)
        {
            var isd = items.ElementAt(0);
            Dialogue dialogue = CurrentTalker.OnGetGift(isd.source.item);
            if (dialogue)
            {
                BackpackManager.Instance.LoseItem(isd.source, isd.amount);
                CurrentType = DialogueType.Gift;
                ShowButtons(false, false, false, false);
                StartDialogue(dialogue);
            }
        }
        BackpackManager.Instance.PauseDisplay(false);
        BackpackManager.Instance.CloseWindow();
        PauseDisplay(false);
    }

    private void ShowButtons(bool gift, bool shop, bool warehouse, bool quest, bool back = true)
    {
        ZetanUtility.SetActive(UI.giftButton.gameObject, gift);
        ZetanUtility.SetActive(UI.shopButton.gameObject, shop);
        ZetanUtility.SetActive(UI.warehouseButton.gameObject, warehouse);
        ZetanUtility.SetActive(UI.questButton.gameObject, quest);
        ZetanUtility.SetActive(UI.backButton.gameObject, back);
    }

    private void PauseButtons(bool pause)
    {
        ZetanUtility.SetActive(UI.buttonArea, !pause);
    }

    public override void SetUI(DialogueUI UI)
    {
        foreach (var oa in optionAgents)
        {
            if (oa && oa.gameObject) oa.Recycle();
        }
        optionAgents.Clear();
        choiceOptionSaid.Clear();
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion

    #region 其它
    public void Init()
    {
        if (!talkerRoot)
            talkerRoot = new GameObject("Talkers").transform;
        foreach (var ti in Resources.LoadAll<TalkerInformation>("Configuration").Where(x => x.IsValid && x.Enable))
        {
            TalkerData data = new TalkerData(ti);
            if (ti.Scene == ZetanUtility.ActiveScene.name)
            {
                Talker talker = ti.Prefab.gameObject.Instantiate(talkerRoot).GetComponent<Talker>();
                talker.Init(data);
            }
            Talkers.Add(ti.ID, data);
            if (talkersMap.TryGetValue(ti.Scene, out var talkers))
                talkers.Add(data);
            else talkersMap.Add(ti.Scene, new List<TalkerData>() { data });
        }
    }

    public void SendTalkerGifts()
    {
        if (!CurrentTalker || !CurrentTalker.Data.Info.CanDEV_RLAT) return;
        OpenGiftWindow();
    }

    public void ShowTalkerQuest()
    {
        if (CurrentTalker == null) return;
        ZetanUtility.SetActive(UI.questButton.gameObject, false);
        ZetanUtility.SetActive(UI.warehouseButton.gameObject, false);
        ZetanUtility.SetActive(UI.shopButton.gameObject, false);
        GoBackDefault();
        if (CurrentTalker.QuestInstances.Where(x => !x.IsFinished).Count() > 0)
        {
            Skip();
            HandlingQuestOptions();
        }
    }

    public void Skip()
    {
        if (dialogueDatas.TryGetValue(currentDialogue.origin.ID, out var find))
            foreach (var words in find.wordsDatas)
            {
                if (!words.IsDone) return;//只要有一句话的分支有没完成的，则无法跳过
            }
        while (wordsToSay.Count > 0)
            SayNextWords();
    }

    public void GoBackDefault()
    {
        currentOption = null;
        currentDialogue = null;
        currentQuest = null;
        currentSubmitObj = null;
        currentTalkObj = null;
        currentWords = null;
        choiceOptionSaid.Clear();
        ClearOptions();
        HideQuestDescription();
        StartTalking(CurrentTalker);
    }

    private void ExecuteEvents(DialogueWords words)
    {
        foreach (var we in words.Events)
        {
            switch (we.EventType)
            {
                case WordsEventType.Trigger:
                    TriggerManager.Instance.SetTrigger(we.WordsTrigrName, we.TriggerActType == TriggerActionType.Set);
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

    private string HandlingContent(string words)
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

        static string HandlingName(string keyWords)
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
                keyWords = keyWords.Replace("[ENMY]", string.Empty);
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
            data.dialogueDatas.Add(new DialogueSaveData(kvpDialog.Value));
        }
    }
    public void LoadData(SaveData data)
    {
        dialogueDatas.Clear();
        Dialogue[] dialogues = Resources.LoadAll<Dialogue>("Configuration");
        foreach (DialogueSaveData dsd in data.dialogueDatas)
        {
            Dialogue find = dialogues.FirstOrDefault(x => x.ID == dsd.dialogID);
            if (find)
            {
                DialogueData dd = new DialogueData(find);
                for (int i = 0; i < dsd.wordsDatas.Count; i++)
                {
                    for (int j = 0; j < dd.wordsDatas[i].optionDatas.Count; j++)
                    {
                        if (dsd.wordsDatas[i].IsOptionCmplt(j))
                            dd.wordsDatas[i].optionDatas[j].isDone = true;
                    }
                }
                dialogueDatas.Add(dsd.dialogID, dd);
            }
        }
    }
    #endregion
}

public enum DialogueType
{
    Normal,
    Quest,
    Objective,
    Gift,
}