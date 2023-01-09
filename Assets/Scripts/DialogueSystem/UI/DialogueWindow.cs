using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.DialogueSystem.UI
{
    using CharacterSystem;
    using DialogueSystem;
    using Extension;
    using InteractionSystem.UI;
    using InventorySystem;
    using InventorySystem.UI;
    using ItemSystem;
    using ItemSystem.UI;
    using PlayerSystem;
    using QuestSystem;
    using ZetanStudio.UI;

    public class DialogueWindow : InteractionWindow<Talker>, IHideable
    {
        #region UI声明
#if ZTDS_ENABLE_PORTRAIT
        [SerializeField]
        private Image leftPortrait;
        [SerializeField]
        private Image rightPortrait;
#endif
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private Text textText;
        [SerializeField]
        private Button textButton;

        [SerializeField]
        private Transform buttonArea;
        [SerializeField]
        private Button homeButton;
        [SerializeField]
        private Button giftButton;
        [SerializeField]
        private Button warehouseButton;
        [SerializeField]
        private Button shopButton;
        [SerializeField]
        private Button questButton;
        [SerializeField]
        private Button talkButton;
        [SerializeField]
        private Button nextButton;
        [SerializeField]
        private Text nextButtonText;

        [SerializeField]
        private GameObject optionArea;
        [SerializeField]
        private GameObject optionTitleArea;
        [SerializeField]
        private Text optionTitle;
        [SerializeField]
        private TabbedBar optionTab;
        [SerializeField]
        private ButtonWithTextList optionList;

        [SerializeField]
        private CanvasGroup descriptionWindow;
        [SerializeField]
        private Text descriptionText;
        [SerializeField]
        private ItemGrid rewardList;
        [SerializeField]
        private float skipDelay = 0.5f;
        [SerializeField]
        private bool closeAtExit = true;
        #endregion

        #region 运行时声明
        private Talker currentTalker;
        public override Talker Target => currentTalker;
        public bool IsHidden { get; private set; }
        protected override string LangSelector => typeof(Dialogue).Name;
        private DialogueType currentType = DialogueType.Normal;
        private EntryNode home;

        public EntryNode CurrentEntry { get; private set; }
        private DialogueNode currentNode;
        private readonly Stack<DialogueNode> continueNodes = new Stack<DialogueNode>();

        public DialogueData CurrentEntryData => DialogueManager.GetOrCreateData(CurrentEntry) ?? null;
        private DialogueData CurrentData => CurrentEntryData && currentNode ? CurrentEntryData[currentNode] : null;

        private Action next;

        #region 逐字相关
        private string targetText;
        private Coroutine coroutine;
        private float playTime;
        public bool IsPlaying => coroutine != null;
        #endregion

        #region 任务相关声明
        private QuestData currentQuest;
        private TalkObjectiveData currentTalkObj;
        private SubmitObjectiveData currentSubmitObj;
        #endregion
        #endregion

        #region 对话发起相关
        public void StartWith(Dialogue dialogue) => StartWith(dialogue.Entry);
        public void StartWith(string talker, string content) => StartWith(new EntryNode(talker, content));
        public void StartWith(EntryNode entry)
        {
            if (!entry) return;
            home ??= entry;
            CurrentEntry = entry;
            HandleNode(entry);
        }
        public void ContinueWith(DialogueNode node)
        {
            if (Dialogue.Reachable(CurrentEntry, node)) HandleNode(node);
        }
        /// <summary>
        /// 将内容入栈。当本次对话结束时，将会以栈顶内容继续
        /// </summary>
        public void PushContinuance(DialogueNode node) => continueNodes.Push(node);
        private DialogueNode PopContinuance() => continueNodes.Count > 0 ? continueNodes.Pop() : null;
        #endregion

        #region 内容刷新相关
        private void HandleNode(DialogueNode node)
        {
            HandleSpecial(ref node);
            if (!node || !node.OnEnter()) return;
            if (node == home && currentTalker)
            {
                var info = currentTalker.GetData<TalkerData>().GetInfo<TalkerInformation>();
                ShowButtons(info.CanDEV_RLAT, info.IsVendor, info.IsWarehouseAgent, TalkerHasObjectives(),
                    currentTalker.QuestInstances.Where(x => x.Model.AcceptCondition.IsMeet() && !x.IsSubmitted).Any());
            }
            else ShowButtons(false, false, false, false, false);
            ResetInteraction();
            currentNode = node;
            CurrentData?.Access();
            if (node is SentenceNode sentence)
            {
#if ZTDS_ENABLE_PORTRAIT
                SetPortrait(sentence.Portrait, sentence.PortrSide);
#endif
                nameText.text = ConvertPlayerOrNPC(Keyword.HandleKeywords(Tr(sentence.Talker)));
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(Write(sentence));
            }
            else if (node is BlockerNode) DoOption(node[0]);
            (currentNode as IEventNode)?.Events.ForEach(e =>
            {
                if (e != null && CurrentData.EventStates.TryGetValue(e.ID, out var state) && !state && e.Invoke())
                    CurrentData.AccessEvent(e.ID);
            });
        }

        private void HandleSpecial(ref DialogueNode node)
        {
            while (node is DecoratorNode)
            {
                CurrentEntryData[node]?.Access();
                node = node[0]?.Next;
            }
            while (node is ConditionNode)
            {
                CurrentEntryData[node]?.Access();
                node = node[0]?.Next;
            }
            while (node is BranchNode branch)
            {
                CurrentEntryData[node]?.Access();
                node = branch.GetBranch(CurrentEntryData);
            }
        }

        private string ConvertPlayerOrNPC(string text)
        {
            if (Regex.IsMatch(text, @"{\[NPC\]}", RegexOptions.IgnoreCase))
                text = Regex.Replace(text, @"{\[NPC\]}", currentTalker ? currentTalker.TalkerName : Tr("神秘人"), RegexOptions.IgnoreCase);
            if (Regex.IsMatch(text, @"{\[NPC\]}", RegexOptions.IgnoreCase))
                text = Regex.Replace(text, @"{\[PLAYER\]}", PlayerManager.Instance.PlayerInfo.Name, RegexOptions.IgnoreCase);
            return text;
        }

        private void HandleLastSentence()
        {
            if (currentQuest) HandleLastQuest();
            else if (currentTalkObj) HandleLastTalkObj();
            else if (currentSubmitObj) HandleLastSubmitObj();
            else if (closeAtExit) SetNextClick(Tr("结束"), () => Close());
        }
        private void HandleInteraction()
        {
            List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
            if (currentNode.ExitHere)
            {
                if (PopContinuance() is DialogueNode node)
                {
                    currentNode = node;
                    HandleInteraction();
                }
                else HandleLastSentence();
            }
            else
            {
                if (currentNode.Options.Count == 1 && currentNode[0].IsMain && currentNode[0].Next is not ExternalOptionsNode)
                    SetNextClick(Tr("继续"), () => DoOption(currentNode[0]));
                else
                {
                    IList<DialogueOption> options = null;
                    if (currentNode[0]?.Next is ExternalOptionsNode sorter)
                    {
                        options = sorter.GetOptions(CurrentEntryData, currentNode);
                        CurrentEntryData[sorter]?.Access();
                    }
                    else options = currentNode.Options;
                    //是否所有可使对话结束的选项(以下简称"结束选项")都被剔除了
                    bool needReserve = options.Count > 0 && options
                        .Where(opt => opt.Next && opt.Next.Exitable).All(opt =>
                        {
                            return opt.Next is ConditionNode condition && !condition.Check(CurrentData);
                        });
                    bool reserved = false;//是否已经保留了一个结束选项
                    foreach (var option in options)
                    {
                        bool culled = false;
                        if (option.Next is ConditionNode condition)
                        {
                            if ((!condition.Exitable || !needReserve) && !condition.Check(CurrentData))
                                culled = true;//如果不在此选项结束对话或无需保留，则按需标记剔除
                            else if (needReserve && reserved && condition.Exitable)
                                culled = true;//如果需要保留且已经保留了一个结束选项，然后这个也是结束选项，也剔除掉
                            if (!reserved) reserved = needReserve && condition.Exitable;
                        }
                        if (!culled)
                        {
                            string title = Keyword.HandleKeywords(Tr(option.Title));
                            var temp = option.Next;
                            while (temp is DecoratorNode decorator)
                            {
                                decorator.Decorate(CurrentEntryData, ref title);
                                temp = temp[0]?.Next;
                            }
                            buttonDatas.Add(new ButtonWithTextData(Tr(title), () => DoOption(option)));
                        }
                    }
                }
                RefreshOptions(buttonDatas, Tr("互动"));
            }
        }
        private void ResetInteraction()
        {
            Utility.SetActive(optionArea, false);
            Utility.SetActive(nextButton, false);
            next = null;
        }
        private IEnumerator Write(SentenceNode sentence)
        {
            targetText = ConvertPlayerOrNPC(Keyword.HandleKeywords(Tr(sentence.Text), true));
            if (!string.IsNullOrEmpty(targetText))
            {
                Stack<string> ends = new Stack<string>();
                string suffix = string.Empty;
                float interval = 0;
                for (int i = 0; i < targetText.Length; i++)
                {
                    string end = ends.Count > 0 ? ends.Peek() : string.Empty;
                    if (Regex.Match(safeString(i, 20),
                                    @"^(<color=?>|<color=""\w+"">|<color='\w+'>|<color=#*[a-fA-F\d]{6}>|<color=#*[a-fA-F\d]{8}>)",
                                    RegexOptions.IgnoreCase) is Match match && match.Success)
                    {
                        ends.Push("</color>");
                        suffix = suffix.Insert(0, "</color>");
                        i += match.Value.Length - 1;
                    }
                    else if (Regex.Match(safeString(i, 10), @"^(<size>|<size=\d*>)", RegexOptions.IgnoreCase) is Match match2 && match2.Success)
                    {
                        ends.Push("</size>");
                        suffix = suffix.Insert(0, "</size>");
                        i += match2.Value.Length - 1;
                    }
                    else if (Regex.Match(safeString(i, 3), @"^(<i>)", RegexOptions.IgnoreCase) is Match match3 && match3.Success)
                    {
                        ends.Push("</i>");
                        suffix = suffix.Insert(0, "</i>");
                        i += match3.Value.Length - 1;
                    }
                    else if (Regex.Match(safeString(i, 3), @"^(<b>)", RegexOptions.IgnoreCase) is Match match4 && match4.Success)
                    {
                        ends.Push("</b>");
                        suffix = suffix.Insert(0, "</b>");
                        i += match4.Value.Length - 1;
                    }
                    else if (ends.Count > 0 && i + end.Length <= targetText.Length && targetText[i..(i + end.Length)] == end)
                    {
                        i += end.Length - 1;
                        suffix = suffix[ends.Pop().Length..^0];
                    }
                    else
                    {
                        playTime += interval;
                        //前段正常显示，后段设置为透明，这样能在一定程度时保持每个字的位置
                        var remainder = Regex.Replace(targetText[i..^0],
                                                      @"<color=?>|<color=""\w+"">|<color='\w+'>|<color=#*[a-fA-F\d]{6}>|<color=#*[a-fA-F\d]{8}>|<\/color>|<b>|<\/b>|<i>|<\/i>",
                                                      "", RegexOptions.IgnoreCase);
                        var prefix = string.Empty;
                        var ms = Regex.Matches(remainder, @"<size>|<size=\d{0,3}>|<\/size>", RegexOptions.IgnoreCase);
                        var tags = new Queue<Match>(Regex.Matches(targetText[0..i], @"<size=?>|<size=\d{0,3}>", RegexOptions.IgnoreCase));
                        for (int j = 0; j < ms.Count; j++)
                        {
                            if (tags.Count > 0 && Regex.IsMatch(ms[j].Value, @"</size>", RegexOptions.IgnoreCase) && (j == 0 || !Regex.IsMatch(ms[j - 1].Value, @"<size=?>|<size=\d{0,3}>", RegexOptions.IgnoreCase)))
                                prefix += tags.Dequeue().Value;
                        }
                        remainder = prefix + remainder;
                        var result = targetText[0..i] + suffix + (!string.IsNullOrEmpty(remainder) ? $"<color>{remainder}</color>" : string.Empty);
                        textText.text = result;
                        if (i < targetText.Length && !char.IsPunctuation(targetText[i]))//标点符号不进行逐字，直接跟随上一个字符出来
                        {
                            interval = getNextUtterInterval(i);
                            if (interval > 0 && !Mathf.Approximately(interval, 0)) yield return new WaitForSecondsRealtime(interval);
                        }
                        else interval = 0;
                    }
                }

                string safeString(int i, int length) => targetText[i..(i + length <= targetText.Length ? i + length : ^0)];
                float getNextUtterInterval(int i)
                {
                    if (i < 0 || i > targetText.Length - 1 || (sentence.SpeakInterval?.keys.Length ?? 0) < 2) return 0;
                    return sentence.SpeakInterval.Evaluate((i + 1.0f) / targetText.Length);
                }
            }
            textText.text = targetText;
            HandleInteraction();
            coroutine = null;
            playTime = 0;
        }
        public void Skip()
        {
            if (IsPlaying && playTime >= skipDelay)
            {
                StopCoroutine(coroutine);
                textText.text = targetText;
                HandleInteraction();
                coroutine = null;
            }
        }

#if ZTDS_ENABLE_PORTRAIT
        private void SetPortrait(Sprite portrait, PortraitSide side)
        {
            if (portrait)
                switch (side)
                {
                    case PortraitSide.Left:
                        if (leftPortrait)
                        {
                            leftPortrait.overrideSprite = portrait;
                            leftPortrait.SetNativeSize();
                        }
                        Utility.SetActive(leftPortrait, true);
                        Utility.SetActive(rightPortrait, false);
                        break;
                    case PortraitSide.Right:
                        Utility.SetActive(leftPortrait, false);
                        if (rightPortrait)
                        {
                            rightPortrait.overrideSprite = portrait;
                            rightPortrait.SetNativeSize();
                        }
                        Utility.SetActive(rightPortrait, true);
                        break;
                }
            else
            {
                if (!leftPortrait || !rightPortrait) return;
                leftPortrait.overrideSprite = null;
                Utility.SetActive(leftPortrait, false);
                rightPortrait.overrideSprite = null;
                Utility.SetActive(rightPortrait, false);
            }
        }
#endif
        #endregion

        #region 任务相关
        private void HandleLastQuest()
        {
            if (!currentQuest.InProgress || currentQuest.IsComplete) ShowQuestDescription(currentQuest);
            List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
            if (!currentQuest.IsComplete)
                if (!currentQuest.InProgress)
                {
                    buttonDatas.Add(new ButtonWithTextData(Tr("接受"), delegate
                    {
                        if (QuestManager.AcceptQuest(currentQuest))
                        {
                            currentType = DialogueType.Normal;
                            ShowTalkerQuest();
                        }
                    }));
                    buttonDatas.Add(new ButtonWithTextData(Tr("拒绝"), delegate
                    {
                        currentType = DialogueType.Normal;
                        ShowTalkerQuest();
                    }));
                    RefreshOptions(buttonDatas, Tr("任务"));
                }
                else
                {
                    SetNextClick(Tr("返回"), delegate
                    {
                        currentType = DialogueType.Normal;
                        ShowTalkerQuest();
                    });
                }
            else if (currentQuest.InProgress)
            {
                buttonDatas.Add(new ButtonWithTextData(Tr("完成"), delegate
                {
                    if (QuestManager.SubmitQuest(currentQuest))
                    {
                        currentType = DialogueType.Normal;
                        ShowTalkerQuest();
                    }
                }));
            }
        }
        public void ShowTalkerQuest()
        {
            if (!currentTalker) return;
            BackHomeImmediate();
            ResetInteraction();
            if (currentTalker.QuestInstances.Where(x => !x.IsSubmitted).Any())
            {
                List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
                var cmpltQuests = new List<QuestData>();
                var norQuests = new List<QuestData>();
                foreach (var quest in currentTalker.QuestInstances)
                {
                    if (quest.Model.AcceptCondition.IsMeet() && !quest.IsSubmitted)
                        if (quest.IsComplete) cmpltQuests.Add(quest);
                        else norQuests.Add(quest);
                }
                foreach (var quest in cmpltQuests)
                {
                    buttonDatas.Add(new ButtonWithTextData(quest.Title + "(已完成)", delegate
                    {
                        currentQuest = quest;
                        currentType = DialogueType.Special;
                        StartWith(quest.Model.CompleteDialogue);
                    }));
                }
                foreach (var quest in norQuests)
                {
                    buttonDatas.Add(new ButtonWithTextData(quest.Title + (quest.InProgress ? "(进行中)" : string.Empty), delegate
                    {
                        currentQuest = quest;
                        currentType = DialogueType.Special;
                        StartWith(quest.InProgress ? quest.Model.OngoingDialogue : quest.Model.BeginDialogue);
                    }));
                }
                RefreshOptions(buttonDatas, Tr("任务"));
            }
        }
        public bool ShouldShowQuest() => currentNode.ExitHere && currentType == DialogueType.Normal && (CurrentData?.IsDone ?? false) && TalkerHasQuests();
        private bool TalkerHasQuests() => currentTalker && (currentTalker.QuestInstances.Exists(x => x.IsComplete) ||
            currentTalker.QuestInstances.Exists(x => x.Model.AcceptCondition.IsMeet() && !x.InProgress));
        public void ShowQuestDescription(QuestData quest)
        {
            if (quest == null) return;
            currentQuest = quest;
            descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}", currentQuest.Title,
                currentQuest.originalQuestHolder.TalkerName, currentQuest.Description).ToString();
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

        #region 目标相关
        private void HandleLastTalkObj()
        {
            if (!CurrentData.IsDone) return;
            currentTalkObj.UpdateTalkState();
            TryShowQuest(currentTalkObj);
            currentTalkObj = null;
            currentType = DialogueType.Normal;
        }
        private void HandleLastSubmitObj()
        {
            currentSubmitObj.UpdateSubmitState();
            TryShowQuest(currentSubmitObj);
            currentSubmitObj = null;
            currentType = DialogueType.Normal;
        }
        private void TryShowQuest(ObjectiveData objective)
        {
            if (objective.IsComplete)
            {
                QuestData qParent = objective.parent;
                //该目标是任务的最后一个目标，则可以直接提交任务
                if (qParent.currentQuestHolder == currentTalker.GetData<TalkerData>() && qParent.IsComplete && qParent.Objectives.IndexOf(objective) == qParent.Objectives.Count - 1)
                {
                    SetNextClick(Tr("继续"), delegate
                    {
                        currentQuest = qParent;
                        currentType = DialogueType.Special;
                        StartWith(qParent.Model.CompleteDialogue);
                    });
                }
            }
        }
        public void ShowTalkerObjective()
        {
            if (!currentTalker) return;
            BackHomeImmediate();
            ResetInteraction();
            List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
            foreach (TalkObjectiveData to in currentTalker.GetData<TalkerData>().objectivesTalkToThis.Where(o => !o.IsComplete))
            {
                if (to.AllPrevComplete && !to.AnyNextOngoing)
                {
                    buttonDatas.Add(new ButtonWithTextData(to.parent.Title, delegate
                    {
                        currentTalkObj = to;
                        currentType = DialogueType.Special;
                        StartWith(currentTalkObj.Model.Dialogue);
                    }));
                }
            }
            foreach (SubmitObjectiveData so in currentTalker.GetData<TalkerData>().objectivesSubmitToThis.Where(o => !o.IsComplete))
            {
                if (so.AllPrevComplete && !so.AnyNextOngoing)
                {
                    buttonDatas.Add(new ButtonWithTextData(so.DisplayName, delegate
                    {
                        if (checkSumbitAble())
                        {
                            currentSubmitObj = so;
                            currentType = DialogueType.Special;
                            WindowsManager.HideWindow(this, true);
                            InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, amountLimit: amount, selectCondition: condition, cancel: cancel);

                            bool confirm(IEnumerable<CountedItem> items)
                            {
                                if (CountedItem.GetAmount(items, so.Model.ItemToSubmit) != so.Model.Amount)
                                {
                                    MessageManager.Instance.New(Tr("数量不正确"));
                                    return false;
                                }
                                if (BackpackManager.Instance.Lose(items)) StartWith(so.Dialogue);
                                WindowsManager.HideWindow(this, false);
                                return true;
                            }
                            int amount(ItemData item) => so.Model.ItemToSubmit == item.Model ? so.Model.Amount : 0;
                            bool condition(ItemData item) => so.Model.ItemToSubmit == item?.Model;
                            void cancel() => WindowsManager.HideWindow(this, false);
                        }
                    }));
                }

                bool checkSumbitAble()
                {
                    QuestData qParent = so.parent;
                    var amount = BackpackManager.Instance.GetAmount(so.Model.ItemToSubmit);
                    if (qParent.Model.InOrder)
                        foreach (var o in qParent.Objectives)
                            if (o is CollectObjectiveData co && co.Model.LoseItemAtSbmt && o.Model.InOrder && o.IsComplete)
                                if (amount - so.Model.Amount < o.Model.Amount)
                                {
                                    MessageManager.Instance.New(Tr("该物品为目标[{0}]所需", o.DisplayName));
                                    return false;
                                }
                    return BackpackManager.Instance.CanLose(so.Model.ItemToSubmit, so.Model.Amount);
                }
            }
            RefreshOptions(buttonDatas, Tr("交谈"));
        }
        private bool ShouldShowObjectives() => currentType == DialogueType.Normal && (CurrentData?.IsDone ?? false) && TalkerHasObjectives();
        private bool TalkerHasObjectives() => currentTalker && (currentTalker.GetData<TalkerData>().objectivesSubmitToThis.Count > 0 ||
            currentTalker.GetData<TalkerData>().objectivesTalkToThis.Count > 0);
        #endregion
        #endregion

        #region 操作相关
        private void SetNextClick(string text, Action action)
        {
            nextButtonText.text = text;
            Utility.SetActive(nextButton, true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => action?.Invoke());
            next = action;
        }
        public void Next()
        {
            if (IsPlaying) Skip();
            else if (next != null) next.Invoke();
            else if (optionList.Count > 0) optionList.Items[0].OnClick();
            else if (ShouldShowObjectives()) ShowTalkerObjective();
            else if (ShouldShowQuest()) ShowTalkerQuest();
        }
        public void BackHome()
        {
            currentQuest = null;
            currentSubmitObj = null;
            next = null;
            continueNodes.Clear();
            currentType = DialogueType.Normal;
            HideQuestDescription();
            StartWith(home);
        }
        private void BackHomeImmediate()
        {
            BackHome();
            if (IsPlaying)
            {
                StopCoroutine(coroutine);
                textText.text = targetText;
                coroutine = null;
            }
        }
        private void OnTextClick()
        {
            if (IsPlaying) Skip();
            else next?.Invoke();
        }
        private void DoOption(DialogueOption option)
        {
            if (!option || !option.Next) return;
            if (option.Next.IsManual()) option.Next.DoManual(this);
            else HandleNode(option.Next);
        }
        private void RefreshOptions(List<ButtonWithTextData> buttonDatas, string title = null)
        {
            optionTitle.text = title;
            Utility.SetActive(optionTitleArea, title != null);
            optionList.Refresh(buttonDatas);
            Utility.SetActive(optionArea, optionList.Count > 0);
        }
        #endregion

        #region 功能按钮相关
        private void ShowButtons(bool gift = true, bool shop = true, bool warehouse = true, bool talk = true, bool quest = true, bool back = true)
        {
            Utility.SetActive(giftButton.gameObject, gift);
            Utility.SetActive(shopButton.gameObject, shop);
            Utility.SetActive(warehouseButton.gameObject, warehouse);
            Utility.SetActive(talkButton.gameObject, talk);
            Utility.SetActive(questButton.gameObject, quest);
            Utility.SetActive(homeButton.gameObject, back);
        }
        private void OpenTalkerWarehouse()
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
        private void OpenTalkerShop()
        {
            if (!currentTalker || !currentTalker.GetData<TalkerData>().Info.IsVendor) return;
            var shop = WindowsManager.OpenWindow<ShopWindow>(currentTalker.GetData<TalkerData>().shop);
            if (shop)
            {
                shop.onClose += () => WindowsManager.HideWindow(this, false);
                WindowsManager.HideWindow(this, true);
            }
        }
        private void OpenGiftWindow()
        {
            InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, OnSendGift, "挑选一件礼物", "确定要送出这个礼物吗？", 1, i => 1, cancel: () => WindowsManager.HideWindow(this, false));
            WindowsManager.HideWindow(this, true);
        }
        private void OnSendGift(IEnumerable<CountedItem> items)
        {
            if (items != null && items.Any())
            {
                var isd = items.ElementAt(0);
                Dialogue dialogue = currentTalker.OnGetGift(isd.source.Model);
                if (dialogue)
                {
                    BackpackManager.Instance.Lose(isd.source, isd.amount);
                    currentType = DialogueType.Special;
                    ShowButtons(false, false, false, false);
                    StartWith(dialogue);
                }
            }
            WindowsManager.HideWindow(this, false);
        }
        #endregion

        #region 其它
        private void Clear()
        {
            home = null;
            currentTalker = null;
            currentQuest = null;
            currentTalkObj = null;
            currentSubmitObj = null;
            next = null;
            optionList.Clear();
            HideQuestDescription();
            currentType = DialogueType.Normal;
        }
        protected override void OnAwake()
        {
            textButton.onClick.AddListener(OnTextClick);
            homeButton.onClick.AddListener(BackHome);
            giftButton.onClick.AddListener(OpenGiftWindow);
            shopButton.onClick.AddListener(OpenTalkerShop);
            warehouseButton.onClick.AddListener(OpenTalkerWarehouse);
            questButton.onClick.AddListener(ShowTalkerQuest);
            talkButton.onClick.AddListener(ShowTalkerObjective);
        }
        protected override void OnInterrupt()
        {
            IsHidden = false;
        }
        #endregion

        #region 窗口显示相关
        protected override bool OnOpen(params object[] args)
        {
            if (args.Length < 1) return false;
            Clear();
            if (args[0] is Talker talker)
            {
                if (PlayerManager.Instance.CheckIsNormalWithAlert())
                {
                    currentTalker = talker;
                    StartWith(currentTalker.GetData().GetInfo<TalkerInformation>().DefaultDialogue);
                    if (currentTalker.GetData<TalkerData>().Info.IsWarehouseAgent)
                        if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Handler.Inventory == currentTalker.GetData<TalkerData>().Inventory)
                            warehouse.Close();
                }
                else return false;
            }
            else if (args[0] is EntryNode entry) StartWith(entry);
            else if (args[0] is string talker2 && args[1] is string content) StartWith(talker2, content);
            else return false;
            WindowsManager.HideAllExcept(true, this);
            PlayerManager.Instance.Player.SetMachineState<PlayerTalkingState>();
            return base.OnOpen(args);
        }
        protected override bool OnClose(params object[] args)
        {
            base.OnClose(args);
            Clear();
            WindowsManager.HideAll(false);
            WindowsManager.CloseWindow<WarehouseWindow>();
            WindowsManager.CloseWindow<ShopWindow>();
            PlayerManager.Instance.Player.SetMachineState<CharacterIdleState>();
            IsHidden = false;
            return true;
        }
        public void Hide(bool hide, params object[] args)
        {
            if (!IsOpen) return;
            IHideable.HideHelper(content, hide);
            IsHidden = hide;
        }
        #endregion

        private enum DialogueType
        {
            Normal,
            Special,
        }
    }
}