using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.DialogueSystem.UI
{
    using DialogueSystem;
    using Extension;
    using ItemSystem;
    using ItemSystem.UI;
    using ZetanStudio.UI;

    public class DialogueWindow : InteractionWindow<Talker>, IHideable
    {
        #region UI����
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private Text wordsText;

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
        private Button talkButton;
        [SerializeField]
        private Button nextButton;
        [SerializeField]
        private Text nextBtnText;
        [SerializeField]
        private Button rejectButton;
        [SerializeField]
        private Text rejectBtnText;

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
        #endregion

        #region ����ʱ����
        private Talker currentTalker;
        public override Talker Target => currentTalker;
        public bool IsHidden { get; private set; }
        protected override string LangSelector => typeof(Dialogue).Name;
        private DialogueType currentType = DialogueType.Normal;
        private EntryContent root;

        public EntryContent CurrentEntry { get; private set; }
        private DialogueContent currentContent;
        private readonly Stack<DialogueContent> continueContent = new Stack<DialogueContent>();

        public DialogueContentData CurrentEntryData => DialogueManager.GetOrCreateData(CurrentEntry) ?? null;
        private DialogueContentData CurrentData => CurrentEntryData && currentContent ? CurrentEntryData[currentContent] : null;

        private Action next;
        private readonly List<ButtonWithTextData> buttonDatas = new List<ButtonWithTextData>();
        private Coroutine coroutine;

        #region �����������
        private QuestData currentQuest;
        private TalkObjectiveData currentTalkObj;
        private SubmitObjectiveData currentSubmitObj;
        #endregion
        #endregion

        #region �Ի��������
        public void StartWith(Dialogue dialogue) => StartWith(dialogue.Entry);
        public void StartWith(string talker, string content) => StartWith(new EntryContent(talker, content));
        public void StartWith(EntryContent entry)
        {
            if (!entry) return;
            root ??= entry;
            CurrentEntry = entry;
            HandleContent(entry);
        }
        public void ContinueWith(DialogueContent content)
        {
            if (Dialogue.Reachable(CurrentEntry, content)) HandleContent(content);
        }
        /// <summary>
        /// ��������ջ�������ζԻ�����ʱ��������ջ�����ݼ���
        /// </summary>
        public void PushContinuance(DialogueContent content) => continueContent.Push(content);
        private DialogueContent PopContinuance() => continueContent.Count > 0 ? continueContent.Pop() : null;
        #endregion

        #region ����ˢ�����
        private void HandleContent(DialogueContent content)
        {
            if (content == root && currentTalker)
            {
                var info = currentTalker.GetData<TalkerData>().GetInfo<TalkerInformation>();
                ShowButtons(info.CanDEV_RLAT, info.IsVendor, info.IsWarehouseAgent, TalkerHasObjectives(),
                    currentTalker.QuestInstances.Where(x => x.Model.AcceptCondition.IsMeet() && !x.IsSubmitted).Any());
            }
            else ShowButtons(false, false, false, false, false);
            ResetInteraction();
            while (content is DecoratorContent)
            {
                CurrentEntryData[content]?.Access();
                content = content[0]?.Content;
            }
            if (!content.Enter()) return;
            currentContent = content;
            CurrentData?.Access();
            if (content is TextContent text)
            {
                if (text.Talker.ToUpper() == "[NPC]") nameText.text = currentTalker ? currentTalker.TalkerName : Tr("������");
                else if (text.Talker.ToUpper() == "[PLAYER]") nameText.text = PlayerManager.Instance.PlayerInfo.Name;
                else nameText.text = Keywords.HandleKeyWords(text.Talker);
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(Play(text));
                HandleInteraction();
            }
            currentContent?.Events.ForEach(e => e?.Invoke());
        }
        private void HandleLast()
        {
            if (currentQuest) HandleLastQuest();
            else if (currentTalkObj) HandleLastTalkObj();
            else if (currentSubmitObj) HandleLastSubmitObj();
        }
        private void HandleInteraction()
        {
            buttonDatas.Clear();
            if (currentContent.ExitHere)
            {
                if (PopContinuance() is DialogueContent content)
                {
                    currentContent = content;
                    HandleInteraction();
                }
                else HandleLast();
            }
            else
            {
                if (currentContent.Options.Count == 1 && currentContent.Options[0].IsMain)
                    SetNextClick(Tr("����"), () => DoOption(currentContent.Options[0]));
                else foreach (var option in currentContent.Options)
                    {
                        string title = option.Title;
                        bool skip = false;
                        if (option.Content is DecoratorContent)
                        {
                            var temp = option.Content;
                            while (temp is DecoratorContent decorator)
                            {
                                if (!decorator.Decorate(CurrentData, currentContent, option, ref title))
                                {
                                    skip = true;
                                    break;
                                }
                                temp = temp[0]?.Content;
                            }
                        }
                        if (!skip) buttonDatas.Add(new ButtonWithTextData(Tr(title), () => DoOption(option)));
                    }
                RefreshOptions(Tr("����"));
            }
        }
        private void ResetInteraction()
        {
            ZetanUtility.SetActive(optionArea, false);
            ZetanUtility.SetActive(nextButton, false);
            ZetanUtility.SetActive(rejectButton, false);
            next = null;
        }
        private IEnumerator Play(TextContent content)
        {
            var text = Keywords.HandleKeyWords(content.Text, true);
            if (!string.IsNullOrEmpty(text))
            {
                Stack<string> ends = new Stack<string>();
                string temp = string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    string end = ends.Count > 0 ? ends.Peek() : string.Empty;
                    if (checkColor(i))
                    {
                        ends.Push("</color>");
                        temp = temp.Insert(0, "</color>");
                        while (text[i] != '>' && i < text.Length) i++;
                    }
                    else if (checkSize(i))
                    {
                        ends.Push("</size>");
                        temp = temp.Insert(0, "</size>");
                        while (text[i] != '>' && i < text.Length) i++;
                    }
                    else if (checkItalic(i))
                    {
                        ends.Push("</i>");
                        temp = temp.Insert(0, "</i>");
                        i += 2;
                    }
                    else if (checkBold(i))
                    {
                        ends.Push("</b>");
                        temp = temp.Insert(0, "</b>");
                        i += 2;
                    }
                    else if (ends.Count > 0 && i + end.Length < text.Length && text[i..(i + end.Length)] == end)
                    {
                        i += end.Length - 1;
                        temp = temp[0..^ends.Pop().Length];
                    }
                    else
                    {
                        wordsText.text = text[0..(i + 1)] + temp;
                        var interval = getNextUtterInterval(i);
                        if (interval > 0 && !Mathf.Approximately(interval, 0)) yield return new WaitForSecondsRealtime(interval);
                    }
                }

                bool checkColor(int i) => i + 6 < text.Length && text[i..(i + 6)].ToLower() == "<color";
                bool checkSize(int i) => i + 5 < text.Length && text[i..(i + 5)].ToLower() == "<size";
                bool checkBold(int i) => i + 3 < text.Length && text[i..(i + 3)].ToLower() == "<b>";
                bool checkItalic(int i) => i + 3 < text.Length && text[i..(i + 3)].ToLower() == "<i>";
                float getNextUtterInterval(int i)
                {
                    if (i < 0 || i > text.Length - 1 || content.UtterInterval == null || content.UtterInterval.keys.Length < 2) return 0;
                    return content.UtterInterval.Evaluate((i + 1.0f) / text.Length);
                }
            }
            wordsText.text = text;
        }
        #endregion

        #region �������
        private void HandleLastQuest()
        {
            if (!currentQuest.InProgress || currentQuest.IsComplete) ShowQuestDescription(currentQuest);
            if (!currentQuest.IsComplete && !currentQuest.InProgress)
            {
                SetNextClick(Tr("����"), delegate
                {
                    if (QuestManager.AcceptQuest(currentQuest))
                    {
                        currentType = DialogueType.Normal;
                        ShowTalkerQuest();
                    }
                });
            }
            else if (currentQuest.IsComplete && currentQuest.InProgress)
            {
                SetNextClick(Tr("���"), delegate
                {
                    if (QuestManager.SubmitQuest(currentQuest))
                    {
                        currentType = DialogueType.Normal;
                        ShowTalkerQuest();
                    }
                });
            }
            if (!currentQuest.IsComplete && currentQuest.InProgress)
            {
                SetNextClick(Tr("����"), delegate
                {
                    currentType = DialogueType.Normal;
                    ShowTalkerQuest();
                });
            }
            else
            {
                SetRejectClick(currentQuest.InProgress ? Tr("����") : Tr("�ܾ�"), delegate
                {
                    currentType = DialogueType.Normal;
                    ShowTalkerQuest();
                });
            }
        }
        public void ShowTalkerQuest()
        {
            if (!currentTalker) return;
            ReturnToRoot();
            ResetInteraction();
            if (currentTalker.QuestInstances.Where(x => !x.IsSubmitted).Any())
            {
                buttonDatas.Clear();
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
                    buttonDatas.Add(new ButtonWithTextData(quest.Title + "(�����)", delegate
                    {
                        currentQuest = quest;
                        currentType = DialogueType.Special;
                        StartWith(quest.Model.CompleteDialogue);
                    }));
                }
                foreach (var quest in norQuests)
                {
                    buttonDatas.Add(new ButtonWithTextData(quest.Title + (quest.InProgress ? "(������)" : string.Empty), delegate
                    {
                        currentQuest = quest;
                        currentType = DialogueType.Special;
                        StartWith(quest.InProgress ? quest.Model.OngoingDialogue : quest.Model.BeginDialogue);
                    }));
                }
                RefreshOptions(Tr("����"));
            }
        }
        public bool ShouldShowQuest() => currentType == DialogueType.Normal && (CurrentData?.IsDone ?? false) && TalkerHasQuests();
        private bool TalkerHasQuests() => currentTalker && (currentTalker.QuestInstances.Exists(x => x.IsComplete) ||
            currentTalker.QuestInstances.Exists(x => x.Model.AcceptCondition.IsMeet() && !x.InProgress));
        public void ShowQuestDescription(QuestData quest)
        {
            if (quest == null) return;
            currentQuest = quest;
            descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[ί����: {1}]\n{2}", currentQuest.Title,
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

        #region Ŀ�����
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
                //��Ŀ������������һ��Ŀ�꣬�����ֱ���ύ����
                if (qParent.currentQuestHolder == currentTalker.GetData<TalkerData>() && qParent.IsComplete && qParent.Objectives.IndexOf(objective) == qParent.Objectives.Count - 1)
                {
                    SetNextClick(Tr("����"), delegate
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
            ReturnToRoot();
            ResetInteraction();
            buttonDatas.Clear();
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
                                    MessageManager.Instance.New("��������ȷ");
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
                                    MessageManager.Instance.New($"����ƷΪĿ��[{o.DisplayName}]����");
                                    return false;
                                }
                    return BackpackManager.Instance.CanLose(so.Model.ItemToSubmit, so.Model.Amount);
                }
            }
            RefreshOptions(Tr("��̸"));
        }
        private bool ShouldShowObjectives() => currentType == DialogueType.Normal && (CurrentData?.IsDone ?? false) && TalkerHasObjectives();
        private bool TalkerHasObjectives() => currentTalker && (currentTalker.GetData<TalkerData>().objectivesSubmitToThis.Count > 0 ||
            currentTalker.GetData<TalkerData>().objectivesTalkToThis.Count > 0);
        #endregion
        #endregion

        #region �������
        private void SetNextClick(string text, Action action)
        {
            nextBtnText.text = text;
            ZetanUtility.SetActive(nextButton, true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => action?.Invoke());
            next = action;
        }
        private void SetRejectClick(string text, Action action)
        {
            rejectBtnText.text = text;
            ZetanUtility.SetActive(rejectButton, true);
            rejectButton.onClick.RemoveAllListeners();
            rejectButton.onClick.AddListener(() => action?.Invoke());
        }
        public void Next()
        {
            if (next != null) next.Invoke();
            else if (optionList.Count > 0) optionList.Items[0].OnClick();
            else if (ShouldShowObjectives()) ShowTalkerObjective();
            else if (ShouldShowQuest()) ShowTalkerQuest();
        }
        public void ReturnToRoot()
        {
            currentQuest = null;
            currentTalkObj = null;
            currentSubmitObj = null;
            next = null;
            continueContent.Clear();
            currentType = DialogueType.Normal;
            HideQuestDescription();
            StartWith(root);
        }
        private void DoOption(DialogueOption option)
        {
            if (!option.Content) return;
            if (option.Content.IsManual()) option.Content.Manual(this);
            else HandleContent(option.Content);
        }
        private void RefreshOptions(string title = null)
        {
            optionTitle.text = title;
            ZetanUtility.SetActive(optionTitleArea, title != null);
            optionList.Refresh(buttonDatas);
            ZetanUtility.SetActive(optionArea, optionList.Count > 0);
        }
        #endregion

        #region ���ܰ�ť���
        private void ShowButtons(bool gift = true, bool shop = true, bool warehouse = true, bool talk = true, bool quest = true, bool back = true)
        {
            ZetanUtility.SetActive(giftButton.gameObject, gift);
            ZetanUtility.SetActive(shopButton.gameObject, shop);
            ZetanUtility.SetActive(warehouseButton.gameObject, warehouse);
            ZetanUtility.SetActive(talkButton.gameObject, talk);
            ZetanUtility.SetActive(questButton.gameObject, quest);
            ZetanUtility.SetActive(backButton.gameObject, back);
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
            InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, OnSendGift, "��ѡһ������", "ȷ��Ҫ�ͳ����������", 1, i => 1, cancel: () => WindowsManager.HideWindow(this, false));
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

        #region ����
        private void Clear()
        {
            root = null;
            currentTalker = null;
            currentQuest = null;
            currentTalkObj = null;
            currentSubmitObj = null;
            optionList.Clear();
            HideQuestDescription();
            currentType = DialogueType.Normal;
        }
        protected override void OnAwake()
        {
            backButton.onClick.AddListener(ReturnToRoot);
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

        #region ������ʾ���
        public static bool TalkWith(Talker talker)
        {
            if (!talker) return false;
            if (PlayerManager.Instance.CheckIsNormalWithAlert())
            {
                WindowsManager.OpenWindow<DialogueWindow>(talker);
                return true;
            }
            return false;
        }
        protected override bool OnOpen(params object[] args)
        {
            if (args.Length < 1) return false;
            if (args[0] is Talker talker)
            {
                currentTalker = talker;
                StartWith(currentTalker.GetData().GetInfo<TalkerInformation>().DefaultDialogue);
                if (currentTalker.GetData<TalkerData>().Info.IsWarehouseAgent)
                    if (WindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Handler.Inventory == currentTalker.GetData<TalkerData>().Inventory)
                        warehouse.Close(); ;
            }
            else if (args[0] is EntryContent entry) StartWith(entry);
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
            WindowsManager.HideWindow<WarehouseWindow>(false);
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