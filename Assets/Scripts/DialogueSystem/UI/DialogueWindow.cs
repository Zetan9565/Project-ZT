using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DialogueWindow : InteractionWindow<Talker>, IHideable
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text wordsText;
    [SerializeField]
    private float textLineHeight = 22.35832f;
    [SerializeField]
    private int lineAmount = 5;

    [SerializeField]
    private Transform buttonArea;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button giftButton;
    [SerializeField]
    private Button warehouseButton;
    [SerializeField]
    private Button shopButton;
    [SerializeField]
    private Button questButton;

    [SerializeField]
    private ButtonWithTextList optionList;
    [SerializeField]
    private Button pageUpButton;
    [SerializeField]
    private Button pageDownButton;
    [SerializeField]
    private Text pageText;

    [SerializeField]
    private CanvasGroup descriptionWindow;
    [SerializeField]
    private Text descriptionText;
    [SerializeField]
    private ItemGrid rewardList;

    public event DialogueListner OnBeginDialogueEvent;
    public event DialogueListner OnFinishDialogueEvent;
    public override Talker Target => currentTalker;
    private Talker currentTalker;

    public DialogueType CurrentType { get; private set; } = DialogueType.Normal;
    public bool IsHidden { get; protected set; }

    public bool ShouldShowQuest
    {
        get
        {
            if (!currentTalker || !currentTalker.QuestInstances.Exists(x => !x.InProgress) || currentTalker.QuestInstances.Exists(x => x.IsComplete)) return false;
            return true;
        }
    }

    private DialogueData currentDialogue;
    private DialogueWordsData currentWords;
    private WordsOptionData currentOption;
    private readonly Stack<DialogueData> dialogueToSay = new Stack<DialogueData>();
    private readonly Stack<DialogueWordsData> wordsToSay = new Stack<DialogueWordsData>();
    private readonly Stack<WordsOptionData> choiceOptionSaid = new Stack<WordsOptionData>();

    #region 选项相关
    private readonly List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
    public int OptionsCount => optionList.Count;
    public ButtonWithText FirstOption
    {
        get
        {
            if (optionList.Count < 1) return null;
            return optionList[0];
        }
    }

    private int maxPage = 1;
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
    #endregion

    #region 任务相关
    private QuestData currentQuest;
    private TalkObjectiveData currentTalkObj;
    private SubmitObjectiveData currentSubmitObj;
    #endregion

    #region 开始新对话
    public void BeginNewDialogue()
    {
        if (!currentTalker) return;
        StartTalking(currentTalker);
        OnBeginDialogueEvent?.Invoke();
    }

    private void StartTalking(Talker talker)
    {
        currentTalker = talker;
        CurrentType = DialogueType.Normal;
        ShowButtons(talker.GetData<TalkerData>().Info.CanDEV_RLAT, talker.GetData<TalkerData>().Info.IsVendor, talker.GetData<TalkerData>().Info.IsWarehouseAgent, talker.QuestInstances.FindAll(q => !q.IsFinished && MiscFuntion.CheckCondition(q.Model.AcceptCondition)).Count > 0);
        HideQuestDescription();
        StartDialogue(talker.DefaultDialogue);
        talker.OnTalkBegin();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueToSay.Push(DialogueManager.Instance.GetOrCreateDialogueData(dialogue));
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
        ZetanUtility.SetActive(wordsText.gameObject, true);
        SetPageArea(false, false, false);
        SayNextWords();
    }
    #endregion

    #region 处理对话选项
    private void HandlingOptions()
    {
        buttonDatas.Clear();
        if (dialogueToSay.Count > 0 || wordsToSay.Count > 0 && (!currentWords.model.NeedToChusCorrectOption || currentWords.IsDone))
        {
            buttonDatas.Add(new ButtonWithTextData("继续", delegate { SayNextWords(); }));
        }

        if (!currentOption && !currentQuest)
        {
            var cmpltQuest = currentTalker.QuestInstances.Where(x => x.InProgress && x.IsComplete);

            if (cmpltQuest != null && cmpltQuest.Count() > 0)
            {
                foreach (var quest in cmpltQuest)
                {
                    buttonDatas.Add(new ButtonWithTextData($"{quest.Model.Title}(已完成)", delegate
                    {
                        currentQuest = quest;
                        ShowButtons(false, false, false, false);
                        StartDialogue(quest.Model.CompleteDialogue);
                    }));
                }
            }
        }
        if (wordsToSay.Count < 1)//最后一句
        {
            //完成剧情用对话时，结束对话
            if (currentWords && currentWords.parent && currentWords.parent.model.StoryDialogue && currentWords.IsDone)
            {
                buttonDatas.Add(new ButtonWithTextData("结束", delegate
                {
                    OnFinishDialogueEvent?.Invoke();
                    Close();
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
                else if (currentTalker)//普通对话
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
                if (!option.isDone || option.model.OptionType != WordsOptionType.Choice)
                {
                    string title = option.model.Title;
                    if (option.model.OptionType == WordsOptionType.SubmitAndGet)
                        title += $"(需[{option.model.ItemToSubmit.ItemName}]{option.model.ItemToSubmit.Amount}个)";
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
        foreach (var quest in currentTalker.QuestInstances)
        {
            if (MiscFuntion.CheckCondition(quest.Model.AcceptCondition) && !quest.IsFinished)
                if (quest.IsComplete)
                    cmpltQuests.Add(quest);
                else norQuests.Add(quest);
        }
        buttonDatas.Clear();
        foreach (var quest in cmpltQuests)
        {
            buttonDatas.Add(new ButtonWithTextData(quest.Model.Title + "(完成)", delegate
            {
                currentQuest = quest;
                CurrentType = DialogueType.Quest;
                ShowButtons(false, false, false, false);
                StartDialogue(quest.Model.CompleteDialogue);
            }));
        }
        foreach (var quest in norQuests)
        {
            buttonDatas.Add(new ButtonWithTextData(quest.Model.Title + (quest.InProgress ? "(进行中)" : string.Empty), delegate
            {
                currentQuest = quest;
                CurrentType = DialogueType.Quest;
                ShowButtons(false, false, false, false);
                StartDialogue(quest.InProgress ? quest.Model.OngoingDialogue : quest.Model.BeginDialogue);
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
            QuestData qParent = currentTalkObj.parent;
            //该目标是任务的最后一个目标，则可以直接提交任务
            if (qParent.currentQuestHolder == currentTalker.GetData<TalkerData>() && qParent.IsComplete && qParent.Objectives.IndexOf(currentTalkObj) == qParent.Objectives.Count - 1)
            {
                buttonDatas.Add(new ButtonWithTextData("继续", delegate
                {
                    currentQuest = qParent;
                    CurrentType = DialogueType.Quest;
                    StartDialogue(qParent.Model.CompleteDialogue);
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
            BackpackManager.Instance.LoseItem(currentSubmitObj.Model.ItemToSubmit, currentSubmitObj.Model.Amount);
            currentSubmitObj.UpdateSubmitState(currentSubmitObj.Model.Amount);
        }
        if (currentSubmitObj.IsComplete)
        {
            QuestData qParent = currentSubmitObj.parent;
            //若该目标是任务的最后一个目标，则可以直接提交任务
            if (qParent.currentQuestHolder == currentTalker.GetData<TalkerData>() && qParent.IsComplete && qParent.Objectives.IndexOf(currentSubmitObj) == qParent.Objectives.Count - 1)
            {
                buttonDatas.Add(new ButtonWithTextData("继续", delegate
                {
                    currentQuest = qParent;
                    CurrentType = DialogueType.Quest;
                    StartDialogue(qParent.Model.CompleteDialogue);
                }));
            }
        }
        currentSubmitObj = null;//重置管理器的提交目标以防出错
    }
    private bool CheckSumbitAble(SubmitObjectiveData so)
    {
        QuestData qParent = so.parent;
        var amount = BackpackManager.Instance.GetAmount(so.Model.ItemToSubmit);
        if (qParent.Model.CmpltObjctvInOrder)
            foreach (var o in qParent.Objectives)
                if (o is CollectObjectiveData co && co.Model.LoseItemAtSbmt && o.Model.InOrder && o.IsComplete)
                    if (amount - so.Model.Amount < o.Model.Amount)
                    {
                        MessageManager.Instance.New($"该物品为目标[{o.Model.DisplayName}]所需");
                        return false;
                    }
        return BackpackManager.Instance.CanLose(so.Model.ItemToSubmit, so.Model.Amount);
    }

    private void HandlingLast_Normal()
    {
        foreach (TalkObjectiveData to in currentTalker.GetData<TalkerData>().objectivesTalkToThis.Where(o => !o.IsComplete))
        {
            if (to.AllPrevComplete && !to.AnyNextOngoing)
            {
                buttonDatas.Add(new ButtonWithTextData(to.parent.Model.Title, delegate
                {
                    currentTalkObj = to;
                    CurrentType = DialogueType.Objective;
                    ShowButtons(false, false, false, false);
                    StartDialogue(currentTalkObj.Model.Dialogue);
                }));
            }
        }
        foreach (SubmitObjectiveData so in currentTalker.GetData<TalkerData>().objectivesSubmitToThis.Where(o => !o.IsComplete))
        {
            if (so.AllPrevComplete && !so.AnyNextOngoing)
            {
                buttonDatas.Add(new ButtonWithTextData(so.Model.DisplayName, delegate
                {
                    if (CheckSumbitAble(so))
                    {
                        currentSubmitObj = so;
                        CurrentType = DialogueType.Objective;
                        ShowButtons(false, false, false, false);
                        StartOneWords(new DialogueWords(currentWords.model.TalkerInfo, currentSubmitObj.Model.WordsWhenSubmit));
                    }
                }));
            }
        }
        currentTalker.OnTalkFinished();
    }

    private void RefreshOptionUI()
    {
        //把第一页以外的选项隐藏
        List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
        for (int i = 0; i < lineAmount - (int)(wordsText.preferredHeight / textLineHeight) && i < this.buttonDatas.Count; i++)
        {
            buttonDatas.Add(this.buttonDatas[i]);
        }
        optionList.Refresh(buttonDatas);
        CheckPages();
    }
    private void ClearOptions()
    {
        optionList.Clear();
        CheckPages();
    }

    public static float CalculateLineHeight(Text text)
    {
        var extents = text.cachedTextGenerator.rectExtents.size * 0.5f;
        var setting = text.GetGenerationSettings(extents);
        var lineHeight = text.cachedTextGeneratorForLayout.GetPreferredHeight("A", setting);
        return lineHeight * text.lineSpacing / setting.scaleFactor;
    }

    public void OptionPageUp()
    {
        if (Page <= 1 || !IsOpen || IsHidden) return;
        int leftLineCount = lineAmount - (int)(wordsText.preferredHeight / textLineHeight);
        if (Page > 0)
        {
            Page--;
            List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
            for (int i = 0; i < leftLineCount && i + Page < this.buttonDatas.Count; i++)
            {
                buttonDatas.Add(this.buttonDatas[i + Page]);
            }
            optionList.Refresh(buttonDatas);
        }
        if (Page <= 1 && maxPage > 1) SetPageArea(false, true, true);
        else if (Page > 1 && maxPage > 1) SetPageArea(true, true, true);
        pageText.text = Page.ToString() + "/" + maxPage.ToString();
    }
    public void OptionPageDown()
    {
        if (Page >= maxPage || !IsOpen || IsHidden) return;
        int leftLineCount = lineAmount - (int)(wordsText.preferredHeight / textLineHeight);
        if (Page < Mathf.CeilToInt(buttonDatas.Count * 1.0f / (leftLineCount * 1.0f)))
        {
            Page++;
            List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
            for (int i = 0; i < leftLineCount && i + Page < this.buttonDatas.Count; i++)
            {
                buttonDatas.Add(this.buttonDatas[i + Page]);
            }
            optionList.Refresh(buttonDatas);
        }
        if (Page >= maxPage && maxPage > 1) SetPageArea(true, false, true);
        else if (Page < maxPage && maxPage > 1) SetPageArea(true, true, true);
        pageText.text = Page.ToString() + "/" + maxPage.ToString();
    }
    private void CheckPages()
    {
        maxPage = Mathf.CeilToInt(buttonDatas.Count * 1.0f / ((lineAmount - (int)(wordsText.preferredHeight / textLineHeight)) * 1.0f));
        if (maxPage > 1)
        {
            SetPageArea(false, true, true);
            pageText.text = Page.ToString() + "/" + maxPage.ToString();
        }
        else SetPageArea(false, false, false);
    }
    private void SetPageArea(bool activeUp, bool activeDown, bool activeText)
    {
        ZetanUtility.SetActive(pageUpButton.gameObject, activeUp);
        ZetanUtility.SetActive(pageDownButton.gameObject, activeDown);
        ZetanUtility.SetActive(pageText.gameObject, activeText);
    }
    #endregion

    #region 处理每句话
    private void SayNextWords()
    {
        if (wordsToSay.Count > 0)
        {
            currentWords = wordsToSay.Pop();
            DoOneWords(currentWords.model);
        }
        else if (dialogueToSay.Count > 0)
        {
            currentDialogue = dialogueToSay.Pop();
            DoDialogue(currentDialogue.model);
            SayNextWords();
        }
    }

    private void DoOneWords(DialogueWords words)
    {
        string talkerName = words.TalkerName;
        if (currentDialogue)
            talkerName = currentDialogue.model.UseUnifiedNPC ? (currentDialogue.model.UseCurrentTalkerInfo ? currentTalker.GetData<TalkerData>().Info.Name : currentDialogue.model.UnifiedNPC.Name) : talkerName;
        if (words.TalkerType == TalkerType.Player && PlayerManager.Instance.PlayerInfo)
            talkerName = PlayerManager.Instance.PlayerInfo.Name;
        nameText.text = talkerName;
        wordsText.text = MiscFuntion.HandlingKeyWords(words.Content, true);
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
                if (currentOption.model.OptionType != WordsOptionType.Choice || currentOption.model.OptionType == WordsOptionType.Choice && currentOption.model.DeleteWhenCmplt)
                    currentOption.isDone = true;
                if (currentOption.model.OptionType == WordsOptionType.Choice && currentOption.parent.model.IsCorrectOption(currentOption.model))//选择型选项的最后一句
                {
                    foreach (var option in currentOption.parent.optionDatas.Where(x => x.model.OptionType == WordsOptionType.Choice))
                    {
                        option.isDone = true;
                    }
                    while (choiceOptionSaid.Count > 0)
                    {
                        var preOption = choiceOptionSaid.Pop();
                        preOption.isDone = true;
                        if (choiceOptionSaid.Count < 1)
                            foreach (var option in preOption.parent.optionDatas.Where(x => x.model.OptionType == WordsOptionType.Choice))
                            {
                                option.isDone = true;
                            }
                    }
                }
                else if (currentOption.model.GoBack || currentOption.parent.parent.model.StoryDialogue)
                {
                    DoDialogue(currentOption.parent.parent.model, currentOption.indexToGoBack);
                }
            }
            if (currentWords.IsDone)//整句话完成了才触发事件
                ExecuteEvents(currentWords.model);
        }
        HandlingOptions();
    }

    private void DoDialogue(Dialogue dialogue, int startIndex = 0)
    {
        var findDialog = DialogueManager.Instance.GetOrCreateDialogueData(dialogue);
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

        if (option.model.OptionType == WordsOptionType.SubmitAndGet)
        {
            if (option.model.IsValid)
            {
                if (BackpackManager.Instance.CanLose(option.model.ItemToSubmit.item, option.model.ItemToSubmit.Amount, (ItemWithAmount)option.model.ItemCanGet))
                    BackpackManager.Instance.LoseItem(option.model.ItemToSubmit.item, option.model.ItemToSubmit.Amount, (ItemWithAmount)option.model.ItemCanGet);
                else
                {
                    currentOption = null;
                    return;
                }
            }
            else MessageManager.Instance.New("无效的选项");
        }
        if (option.model.OptionType == WordsOptionType.OnlyGet)
        {
            if (option.model.IsValid)
            {
                if (!BackpackManager.Instance.CanGet(option.model.ItemCanGet.item, option.model.ItemCanGet.Amount))
                {
                    currentOption = null;
                    return;
                }
                else BackpackManager.Instance.GetItem(option.model.ItemCanGet.item, option.model.ItemCanGet.Amount);
            }
            else MessageManager.Instance.New("无效的选项");
        }
        if (option.model.OptionType == WordsOptionType.Choice)//是选择型选项
        {
            choiceOptionSaid.Push(option);
            if (option.parent.model.IsCorrectOption(option.model))
            {
                DoDialogue(option.parent.parent.model, option.indexToGoBack);
            }
            else
            {
                NewWords(option.parent.model.TalkerType != TalkerType.Player, option.parent.model.WrongChoiceWords);//对话人的类型由句子决定，说的话是句子带的选择错误时说的话
            }
        }
        if (option.model.HasWordsToSay || option.model.OptionType == WordsOptionType.BranchWords)
        {
            NewWords(option.model.TalkerType != TalkerType.Player, option.model.Words);//对话人的类型由选项决定，说的话是选项自带的
        }
        else if (option.model.OptionType == WordsOptionType.BranchDialogue)
        {
            DoDialogue(option.model.Dialogue, option.model.SpecifyIndex);
        }
        SayNextWords();

        void NewWords(bool condition, string words)
        {
            TalkerInformation talkerInfo = null;
            if (condition)
            {
                if (!option.parent.parent.model.UseUnifiedNPC)
                    talkerInfo = option.parent.model.TalkerInfo;
                else if (option.parent.parent.model.UseUnifiedNPC && option.parent.parent.model.UseCurrentTalkerInfo)
                    talkerInfo = currentTalker.GetData<TalkerData>().Info;
                else if (option.parent.parent.model.UseUnifiedNPC && !option.parent.parent.model.UseCurrentTalkerInfo)
                    talkerInfo = option.parent.parent.model.UnifiedNPC;
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
    public static bool TalkWith(Talker talker)
    {
        if (!talker) return false;
        if (GatherManager.Instance.IsGathering)
        {
            MessageManager.Instance.New("请先等待采集完成");
            return false;
        }
        if (PlayerManager.Instance.CheckIsNormalWithAlert())
        {
            WindowsManager.OpenWindow<DialogueWindow>(talker);
            return true;
        }
        return false;
    }
    public void CancelTalk()
    {
        IsHidden = false;
        Close();
    }

    protected override bool OnOpen(params object[] args)
    {
        if (args == null || args.Length < 1) return false;
        if (currentTalker && currentTalker.GetData<TalkerData>().Info.IsWarehouseAgent)
            if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Handler.Inventory == currentTalker.GetData<TalkerData>().Inventory)
                warehouse.Close(); ;
        Clear();
        WindowsManager.HideAllExcept(true, this);
        PlayerManager.Instance.Player.SetMachineState<PlayerTalkingState>();
        currentTalker = args[0] as Talker;
        BeginNewDialogue();
        return base.OnClose(args);
    }
    protected override bool OnClose(params object[] args)
    {
        base.OnClose(args);
        CurrentType = DialogueType.Normal;
        Clear();
        WindowsManager.HideAll(false);
        WindowsManager.HideWindow<WarehouseWindow>(false);
        WindowsManager.CloseWindow<WarehouseWindow>();
        WindowsManager.CloseWindow<ShopWindow>();
        PlayerManager.Instance.Player.SetMachineState<CharacterIdleState>();
        IsHidden = false;
        return true;
    }

    private void Clear()
    {
        currentTalker = null;
        currentQuest = null;
        currentTalkObj = null;
        currentSubmitObj = null;
        choiceOptionSaid.Clear();
        ClearOptions();
        HideQuestDescription();
    }

    public void Hide(bool hide, params object[] args)
    {
        if (!IsOpen) return;
        content.alpha = hide ? 0 : 1;
        content.blocksRaycasts = !hide;
        IsHidden = hide;
    }

    public void ShowQuestDescription(QuestData quest)
    {
        if (quest == null) return;
        currentQuest = quest;
        descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}",
            currentQuest.Model.Title,
            currentQuest.originalQuestHolder.TalkerName,
            currentQuest.Model.Description).ToString();
        rewardList.Refresh(ItemSlotData.Convert(currentQuest.Model.RewardItems, 10));
        descriptionWindow.alpha = 1;
        descriptionWindow.blocksRaycasts = true;
    }
    public void HideQuestDescription()
    {
        currentQuest = null;
        descriptionWindow.alpha = 0;
        descriptionWindow.blocksRaycasts = false;
        WindowsManager.CloseWindow<ItemWindow>();
    }

    public void OpenTalkerWarehouse()
    {
        if (!currentTalker || !currentTalker.GetData<TalkerData>().Info.IsWarehouseAgent) return;
        var warehouse = WindowsManager.OpenWindow<WarehouseWindow>(WarehouseWindow.OpenType.Store, currentTalker.GetData<TalkerData>(),
            WindowsManager.FindWindow<BackpackWindow>());
        if (warehouse)
        {
            warehouse.onClose += () => WindowsManager.HideWindow(this, false);
            WindowsManager.HideWindow(this, true);
        }
    }
    public void OpenTalkerShop()
    {
        if (!currentTalker || !currentTalker.GetData<TalkerData>().Info.IsVendor) return;
        var shop = WindowsManager.OpenWindow<ShopWindow>(currentTalker.GetData<TalkerData>().shop);
        if (shop)
        {
            shop.onClose += () => WindowsManager.HideWindow(this, false);
            WindowsManager.HideWindow(this, true);
        }
    }
    public void OpenGiftWindow()
    {
        InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, OnSendGift, "挑选一件礼物", "确定要送出这个礼物吗？", 1, 1, cancel: () => WindowsManager.HideWindow(this, false));
        WindowsManager.HideWindow(this, true);
    }
    private void OnSendGift(IEnumerable<ItemWithAmount> items)
    {
        if (items != null && items.Count() > 0)
        {
            var isd = items.ElementAt(0);
            Dialogue dialogue = currentTalker.OnGetGift(isd.source.Model_old);
            if (dialogue)
            {
                BackpackManager.Instance.LoseItem(isd.source, isd.amount);
                CurrentType = DialogueType.Gift;
                ShowButtons(false, false, false, false);
                StartDialogue(dialogue);
            }
        }
        WindowsManager.HideWindow(this, false);
    }

    private void ShowButtons(bool gift, bool shop, bool warehouse, bool quest, bool back = true)
    {
        ZetanUtility.SetActive(giftButton.gameObject, gift);
        ZetanUtility.SetActive(shopButton.gameObject, shop);
        ZetanUtility.SetActive(warehouseButton.gameObject, warehouse);
        ZetanUtility.SetActive(questButton.gameObject, quest);
        ZetanUtility.SetActive(backButton.gameObject, back);
    }

    private void PauseButtons(bool pause)
    {
        ZetanUtility.SetActive(buttonArea, !pause);
    }

    #endregion

    #region 其它
    protected override void OnAwake()
    {
        giftButton.onClick.AddListener(SendTalkerGifts);
        warehouseButton.onClick.AddListener(OpenTalkerWarehouse);
        shopButton.onClick.AddListener(OpenTalkerShop);
        backButton.onClick.AddListener(GoBackDefault);
        questButton.onClick.AddListener(ShowTalkerQuest);
        pageUpButton.onClick.AddListener(OptionPageUp);
        pageDownButton.onClick.AddListener(OptionPageDown);
        //textLineHeight = CalculateLineHeight(wordsText);
    }

    public void SendTalkerGifts()
    {
        if (!currentTalker || !currentTalker.GetData<TalkerData>().Info.CanDEV_RLAT) return;
        OpenGiftWindow();
    }

    public void ShowTalkerQuest()
    {
        if (currentTalker == null) return;
        ZetanUtility.SetActive(questButton.gameObject, false);
        ZetanUtility.SetActive(warehouseButton.gameObject, false);
        ZetanUtility.SetActive(shopButton.gameObject, false);
        GoBackDefault();
        if (currentTalker.QuestInstances.Where(x => !x.IsFinished).Count() > 0)
        {
            Skip();
            HandlingQuestOptions();
        }
    }

    public void Skip()
    {
        foreach (var words in currentDialogue.wordsDatas)
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
        StartTalking(currentTalker);
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
                    //TODO 增加好感
                    break;
                case WordsEventType.LoseAmity:
                    //TODO 减少好感
                    break;
                default:
                    break;
            }
        }
    }
    #endregion
}