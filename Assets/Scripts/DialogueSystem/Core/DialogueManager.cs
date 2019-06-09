using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DialogueManager : MonoBehaviour, IWindow
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

    [SerializeField]
    private DialogueUI UI;

    [HideInInspector]
    public UnityEvent OnBeginDialogueEvent;
    [HideInInspector]
    public UnityEvent OnFinishDialogueEvent;

    public DialogueType DialogueType { get; private set; } = DialogueType.Normal;

    private Queue<DialogueWords> Words = new Queue<DialogueWords>();
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
    private TalkObjective talkObjective;
    public Quest CurrentQuest { get; private set; }

    private Dialogue currentDialog;
    private DialogueWords currentWords;
    private BranchDialogue currentBranch;
    private Stack<BranchDialogue> branchDialogInstances = new Stack<BranchDialogue>();
    public bool NPCHasNotAcptQuests
    {
        get
        {
            QuestGiver questGiver = CurrentTalker as QuestGiver;
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
        MyTools.SetActive(UI.wordsText.gameObject, true);
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
        DialogueType = DialogueType.Normal;
        if (talker is QuestGiver && (talker as QuestGiver).QuestInstances.Count > 0)
            MyTools.SetActive(UI.questButton.gameObject, true);
        else MyTools.SetActive(UI.questButton.gameObject, false);
        MyTools.SetActive(UI.warehouseButton.gameObject, talker.IsWarehouseAgent);
        MyTools.SetActive(UI.shopButton.gameObject, talker.IsVendor);
        HideQuestDescription();
        StartDialogue(talker.Info.DefaultDialogue);
        talker.OnTalkBegin();
    }

    public void StartQuestDialogue(Quest quest)
    {
        if (!quest) return;
        CurrentQuest = quest;
        DialogueType = DialogueType.Quest;
        ShowButtons(false, false, false);
        if (!CurrentQuest.IsComplete && !CurrentQuest.IsOngoing) StartDialogue(quest.BeginDialogue);
        else if (!CurrentQuest.IsComplete && CurrentQuest.IsOngoing) StartDialogue(quest.OngoingDialogue);
        else StartDialogue(quest.CompleteDialogue);
    }

    public void StartObjectiveDialogue(TalkObjective talkObjective)
    {
        if (talkObjective == null) return;
        this.talkObjective = talkObjective;
        DialogueType = DialogueType.Objective;
        ShowButtons(false, false, false);
        StartDialogue(talkObjective.Dialogue);
    }

    public void StartBranchDialogue(BranchDialogue branch)
    {
        if (branch == null || branch.IsInvalid) return;
        if (currentWords.NeedToChusRightBranch)
        {
            branchDialogInstances.Push(branch.Cloned);
            branchDialogInstances.Peek().runtimeParent = currentDialog;
            if (currentWords.IndexOfRightBranch == currentWords.Branches.IndexOf(branch))
            {
                branchDialogInstances.Peek().runtimeIndexToGoBack = currentDialog.Words.IndexOf(currentWords) + 1;
            }
            else branchDialogInstances.Peek().runtimeIndexToGoBack = currentDialog.Words.IndexOf(currentWords);
        }
        else if (branch.GoBack)
        {
            branchDialogInstances.Push(branch.Cloned);
            branchDialogInstances.Peek().runtimeParent = currentDialog;
            branchDialogInstances.Peek().runtimeIndexToGoBack = branch.IndexToGo;
        }
        else branchDialogInstances.Push(branch.Cloned);
        currentBranch = branch;
        if (branch.Dialogue && string.IsNullOrEmpty(branch.Words))
            StartDialogue(branchDialogInstances.Peek().Dialogue, branchDialogInstances.Peek().SpecifyIndex);
        else if (!string.IsNullOrEmpty(branch.Words))
        {
            StartOneWords(currentWords.TalkerInfo, branchDialogInstances.Peek().Words, currentWords.TalkerType, currentDialog,
                         branchDialogInstances.Peek().IndexToGo);
            SayNextWords();
        }
    }

    public void StartOneWords(TalkerInfomation talkerInfo, string words, TalkerType talkerType, Dialogue dialogToGoBack, int indexToGoBack)
    {
        if (!UI) return;
        if (waitToGoBackRoutine != null) StopCoroutine(waitToGoBackRoutine);
        if (talkerType == TalkerType.NPC && !talkerInfo || string.IsNullOrEmpty(words))
        {
            return;
        }
        currentDialog = dialogToGoBack;
        IndexToGoBack = indexToGoBack;
        IsTalking = true;
        Words.Clear();
        Words.Enqueue(new DialogueWords(talkerInfo, words, talkerType));
        MakeContinueOption(true);
        MyTools.SetActive(UI.wordsText.gameObject, true);
        SetPageArea(false, false, false);
        waitToGoBackRoutine = StartCoroutine(WaitToGoBack());
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
        if (!(CurrentTalker is QuestGiver)) return;
        ClearOptions();
        foreach (Quest quest in (CurrentTalker as QuestGiver).QuestInstances)
        {
            if (!QuestManager.Instance.HasCompleteQuest(quest) && quest.AcceptAble)
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
            MyTools.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }

    /// <summary>
    /// 生成已完成任务选项
    /// </summary>
    private void MakeTalkerCmpltQuestOption()
    {
        if (!(CurrentTalker is QuestGiver)) return;
        ClearOptions();
        foreach (Quest quest in (CurrentTalker as QuestGiver).QuestInstances)
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
            MyTools.SetActive(optionAgents[i].gameObject, false);
        }
        CheckPages();
    }

    /// <summary>
    /// 生成对话目标列表的选项
    /// </summary>
    private void MakeTalkerObjectiveOption()
    {
        int index = 1;
        ClearOptionsExceptCmlptQuest();
        foreach (TalkObjective to in CurrentTalker.objectivesTalkToThis)
        {
            if (to.AllPrevObjCmplt && !to.HasNextObjOngoing)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(to.runtimeParent.Title, to);
                optionAgents.Add(oa);
                if (index > UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) MyTools.SetActive(oa.gameObject, false);//第一页以外隐藏
                index++;
            }
        }
        CheckPages();
    }

    /// <summary>
    /// 生成分支对话选项
    /// </summary>
    private void MakeBranchDialogueOption()
    {
        if (currentDialog.Words.IndexOf(Words.Peek()) >= currentDialog.Words.Count - 1) return;//最后一句话不支持分支
        if (Words.Peek().Branches.Count < 1) return;
        DialogueWords currentWords = Words.Peek();
        DialogueData find = null;
        if (DialogueDatas.ContainsKey(currentDialog.ID))
            find = DialogueDatas[currentDialog.ID];
        if (DialogueType == DialogueType.Normal) ClearOptions(OptionType.Quest, OptionType.Objective);
        else ClearOptions();
        foreach (BranchDialogue branch in currentWords.Branches)
        {
            if (find != null)
            {
                DialogueWordsData _find = find.wordsDatas.Find(x => x.wordsIndex == currentDialog.Words.IndexOf(currentWords));
                //这个分支是否完成了
                if (_find != null && _find.IsCmpltBranchWithIndex(currentWords.Branches.IndexOf(branch)))
                    continue;//完成则跳过创建
            }
            if (!branch.IsInvalid)
            {
                OptionAgent oa = ObjectPool.Instance.Get(UI.optionPrefab, UI.optionsParent, false).GetComponent<OptionAgent>();
                oa.Init(branch.Title, branch);
                optionAgents.Add(oa);
            }
        }
        //把第一页以外的选项隐藏
        for (int i = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight); i < optionAgents.Count; i++)
        {
            MyTools.SetActive(optionAgents[i].gameObject, false);
        }
        if (optionAgents.Count < 1) MakeContinueOption();//如果分支都完成了，则直接可以进行下一句对话
        CheckPages();
    }

    public void OptionPageUp()
    {
        if (Page <= 1 || !IsUIOpen || IsPausing) return;
        int leftLineCount = UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight);
        if (page > 0)
        {
            Page--;
            for (int i = 0; i < leftLineCount; i++)
            {
                if ((page - 1) * leftLineCount + i < optionAgents.Count && (page - 1) * leftLineCount + i >= 0)
                    MyTools.SetActive(optionAgents[(page - 1) * leftLineCount + i].gameObject, true);
                if (page * leftLineCount + i >= 0 && page * leftLineCount + i < optionAgents.Count)
                    MyTools.SetActive(optionAgents[page * leftLineCount + i].gameObject, false);
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
        if (page < Mathf.CeilToInt(optionAgents.Count * 1.0f / (leftLineCount * 1.0f)))
        {
            for (int i = 0; i < leftLineCount; i++)
            {
                if ((page - 1) * leftLineCount + i < optionAgents.Count && (page - 1) * leftLineCount + i >= 0)
                    MyTools.SetActive(optionAgents[(page - 1) * leftLineCount + i].gameObject, false);
                if (page * leftLineCount + i >= 0 && page * leftLineCount + i < optionAgents.Count)
                    MyTools.SetActive(optionAgents[page * leftLineCount + i].gameObject, true);
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
        Page = 1;
        SetPageArea(false, false, false);
    }

    private void ClearOptionsExceptCmlptQuest()
    {
        for (int i = 0; i < optionAgents.Count; i++)
        {
            if (optionAgents[i] && (optionAgents[i].OptionType != OptionType.Quest || (optionAgents[i].OptionType == OptionType.Quest && !optionAgents[i].MQuest.IsComplete)))
            {
                optionAgents[i].Recycle();
            }
        }
        optionAgents.RemoveAll(x => !x.gameObject.activeSelf || !x.gameObject);
        Page = 1;
        SetPageArea(false, false, false);
    }

    //private void ClearOptionExceptContinue()
    //{
    //    for (int i = 0; i < optionAgents.Count; i++)
    //    {
    //        if (optionAgents[i] && optionAgents[i].OptionType != OptionType.Continue)
    //        {
    //            optionAgents[i].Recycle();
    //        }
    //    }
    //    optionAgents.RemoveAll(x => !x.gameObject.activeSelf || !x.gameObject);
    //    Page = 1;
    //    SetPageArea(false, false, false);
    //}

    private void CheckPages()
    {
        MaxPage = Mathf.CeilToInt(optionAgents.Count * 1.0f / ((UI.lineAmount - (int)(UI.wordsText.preferredHeight / UI.textLineHeight)) * 1.0f));
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
    #endregion

    #region 处理每句话
    /// <summary>
    /// 转到下一句话
    /// </summary>
    public void SayNextWords()
    {
        MakeContinueOption();
        if (Words.Peek().Branches.Count > 0)
        {
            MakeTalkerCmpltQuestOption();
            MakeTalkerObjectiveOption();
        }
        MakeBranchDialogueOption();
        if (Words.Count > 0) currentWords = Words.Peek();
        if (Words.Count == 1) HandlingLastWords();//因为Dequeue之后，话就没了，Words.Count就不是1了，而是0，所以要在此之前做这一步，意思是倒数第二句做这一步
        if (Words.Count > 0)
        {
            string talkerName = Words.Peek().TalkerName;
            if (Words.Peek().TalkerType == TalkerType.Player && PlayerManager.Instance.PlayerInfo)
                talkerName = PlayerManager.Instance.PlayerInfo.Name;
            UI.nameText.text = talkerName;
            UI.wordsText.text = Words.Peek().Words;
            Words.Dequeue();
        }
        if (Words.Count <= 0)
        {
            if (branchDialogInstances.Count > 0)
                HandlingLastBranchWords();//分支处理比较特殊，放到Dequque之后，否则分支最后一句不会讲
            OnFinishDialogueEvent?.Invoke();
        }
    }

    /// <summary>
    /// 处理最后一句对话
    /// </summary>
    private void HandlingLastWords()
    {
        if (DialogueType == DialogueType.Normal && CurrentTalker)
        {
            CurrentTalker.OnTalkFinished();
            MakeTalkerCmpltQuestOption();
            if (CurrentTalker.objectivesTalkToThis != null && CurrentTalker.objectivesTalkToThis.Count > 0) MakeTalkerObjectiveOption();
            else ClearOptionsExceptCmlptQuest();
            QuestManager.Instance.UpdateUI();
        }
        else if (DialogueType == DialogueType.Objective && talkObjective != null) HandlingLastObjectiveWords();
        else if (DialogueType == DialogueType.Quest && CurrentQuest) HandlingLastQuestWords();
    }
    /// <summary>
    /// 处理最后一句对话型目标的对话
    /// </summary>
    private void HandlingLastObjectiveWords()
    {
        if (!AllBranchComplete()) return;
        talkObjective.UpdateTalkState();
        if (talkObjective.IsComplete)
        {
            OptionAgent oa = optionAgents.Find(x => x.TalkObjective == talkObjective);
            if (oa && oa.gameObject)
            {
                //去掉该对话目标自身的对话型目标选项
                optionAgents.Remove(oa);
                oa.Recycle();
            }
            //目标已经完成，不再需要保留在对话人的目标列表里，从对话人的对话型目标里删掉相应信息
            CurrentTalker.objectivesTalkToThis.RemoveAll(x => x == talkObjective);
        }
        talkObjective = null;//重置管理器的对话目标以防出错
        QuestManager.Instance.UpdateUI();
    }
    /// <summary>
    /// 处理最后一句任务的对话
    /// </summary>
    private void HandlingLastQuestWords()
    {
        if (!AllBranchComplete()) return;
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
    private void HandlingLastBranchWords()
    {
        BranchDialogue top = branchDialogInstances.Pop();
        DialogueWords topWordsParent = top.runtimeParent.Words.Find(x => x.Branches.Contains(currentBranch));//找到包含当前分支的语句
        if (topWordsParent != null && topWordsParent.IsRightBranch(currentBranch))
        {
            int indexOfWordsParent = top.runtimeParent.Words.IndexOf(topWordsParent);
            foreach (BranchDialogue branch in topWordsParent.Branches)
            {
                string parentID = top.runtimeParent.ID;
                DialogueWordsData _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfWordsParent);
                if (_find == null)
                {
                    DialogueDatas.Add(parentID, new DialogueData(branch.runtimeParent));
                    _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfWordsParent);
                }
                int indexOfBranch = topWordsParent.Branches.IndexOf(branch);
                _find.cmpltBranchIndexes.Add(indexOfBranch);//该分支已完成
            }
            StartDialogue(top.runtimeParent, top.runtimeIndexToGoBack, false);
        }
        else if (topWordsParent != null && topWordsParent.NeedToChusRightBranch && !topWordsParent.IsRightBranch(currentBranch))
        {
            StartOneWords(topWordsParent.TalkerInfo, topWordsParent.WordsWhenChusWB, topWordsParent.TalkerType, top.runtimeParent, top.runtimeIndexToGoBack);
        }
        else if (top.GoBack && top.runtimeParent.Words.IndexOf(topWordsParent) != top.runtimeParent.Words.Count - 1)//不是最后一句，则处理普通的带返回的分支
        {
            int indexOfFind = top.runtimeParent.Words.IndexOf(topWordsParent);
            if (top.DeleteWhenCmplt)
            {
                string parentID = top.runtimeParent.ID;
                DialogueWordsData _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfFind);
                if (_find == null)
                {
                    DialogueDatas.Add(parentID, new DialogueData(top.runtimeParent));
                    _find = DialogueDatas[parentID].wordsDatas.Find(x => x.wordsIndex == indexOfFind);
                }
                int indexOfBranch = topWordsParent.Branches.IndexOf(top);
                _find.cmpltBranchIndexes.Add(indexOfBranch);//该分支已完成
            }
            StartDialogue(top.runtimeParent, top.runtimeIndexToGoBack, false);
        }
        currentBranch = null;
    }

    private Coroutine waitToGoBackRoutine;
    private IEnumerator WaitToGoBack()
    {
        yield return new WaitUntil(() => Words.Count <= 0);
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
        if (!TalkAble) return;
        if (IsPausing) return;
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
        UI.dialogueWindow.alpha = 0;
        UI.dialogueWindow.blocksRaycasts = false;
        IsUIOpen = false;
        IsPausing = false;
        DialogueType = DialogueType.Normal;
        CurrentTalker = null;
        CurrentQuest = null;
        currentDialog = null;
        currentBranch = null;
        branchDialogInstances.Clear();
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
    public void OpenCloseWindow()
    {

    }
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
        UI.descriptionText.text = string.Format("<b>{0}</b>\n[委托人: {1}]\n{2}",
            CurrentQuest.Title,
            CurrentQuest.OriginalQuestGiver.TalkerName,
            CurrentQuest.Description);
        UI.moneyText.text = CurrentQuest.RewardMoney > 0 ? CurrentQuest.RewardMoney.ToString() : "无";
        UI.EXPText.text = CurrentQuest.RewardEXP > 0 ? CurrentQuest.RewardEXP.ToString() : "无";
        foreach (ItemAgent rwc in UI.rewardCells)
            rwc.Empty();
        foreach (ItemInfo info in quest.RewardItems)
            foreach (ItemAgent rw in UI.rewardCells)
            {
                if (rw.MItemInfo == null)
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
        ItemWindowHandler.Instance.CloseItemWindow();
    }

    public void OpenTalkerWarehouse()
    {
        if (!CurrentTalker || !CurrentTalker.IsWarehouseAgent) return;
        PauseDisplay(true);
        BackpackManager.Instance.PauseDisplay(false);
        WarehouseManager.Instance.Init(CurrentTalker.warehouse);
        WarehouseManager.Instance.OpenWindow();
    }
    public void OpenTalkerShop()
    {
        if (!CurrentTalker || !CurrentTalker.IsVendor) return;
        ShopManager.Instance.Init(CurrentTalker.shop);
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
        MyTools.SetActive(UI.shopButton.gameObject, shop);
        MyTools.SetActive(UI.warehouseButton.gameObject, warehouse);
        MyTools.SetActive(UI.questButton.gameObject, quest);
    }

    public void SetUI(DialogueUI UI)
    {
        if (!UI) return;
        this.UI = UI;
    }
    public void ResetUI()
    {
        optionAgents.Clear();
        IsUIOpen = false;
        IsPausing = false;
        WindowsManager.Instance.Remove(this);
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
        MyTools.SetActive(UI.questButton.gameObject, false);
        MyTools.SetActive(UI.warehouseButton.gameObject, false);
        MyTools.SetActive(UI.shopButton.gameObject, false);
        GotoDefault();
        Skip();
        MakeTalkerQuestOption();
    }

    public void Skip()
    {
        DialogueData find = null;
        if (DialogueDatas.ContainsKey(currentDialog.ID))
            find = DialogueDatas[currentDialog.ID];
        if (find != null)
            foreach (DialogueWords words in currentDialog.Words)
                foreach (BranchDialogue branch in words.Branches)
                    if (!find.wordsDatas[currentDialog.Words.IndexOf(words)].IsCmpltBranchWithIndex(words.Branches.IndexOf(branch)))
                        return;
        while (Words.Count > 0)
            SayNextWords();
    }

    public void GotoDefault()
    {
        currentBranch = null;
        branchDialogInstances.Clear();
        ClearOptions();
        HideQuestDescription();
        StartNormalDialogue(CurrentTalker);
    }

    public bool AllBranchComplete()
    {
        DialogueData find = null;
        if (DialogueDatas.ContainsKey(currentDialog.ID))
            find = DialogueDatas[currentDialog.ID];
        if (find == null) return false;
        foreach (DialogueWords words in currentDialog.Words)
        {
            foreach (BranchDialogue branch in words.Branches)
            {
                DialogueWordsData _find = find.wordsDatas.Find(x => x.wordsIndex == currentDialog.Words.IndexOf(words));
                //这个分支是否完成了
                if (_find != null && _find.IsCmpltBranchWithIndex(words.Branches.IndexOf(branch))) continue;//完成则跳过
                else return false;
            }
        }
        return true;
    }

    public void RemoveDialogueData(Dialogue dialogue)
    {
        if (!dialogue) return;
        if (DialogueDatas.ContainsKey(dialogue.ID))
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