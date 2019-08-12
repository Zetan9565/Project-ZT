using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestManager : SingletonMonoBehaviour<QuestManager>, IWindow
{
    [SerializeField]
    private QuestUI UI;

    public bool IsPausing { get; private set; }
    public bool IsUIOpen { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            return UI.windowCanvas;
        }
    }

    public readonly List<ItemAgent> rewardCells = new List<ItemAgent>();
    private readonly List<QuestAgent> questAgents = new List<QuestAgent>();
    private readonly List<QuestAgent> completeQuestAgents = new List<QuestAgent>();
    private readonly List<QuestGroupAgent> questGroupAgents = new List<QuestGroupAgent>();
    private readonly List<QuestGroupAgent> cmpltQuestGroupAgents = new List<QuestGroupAgent>();
    private readonly List<QuestBoardAgent> questBoardAgents = new List<QuestBoardAgent>();

    [SerializeField, Header("任务列表")]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private List<Quest> questsOngoing = new List<Quest>();
    public List<Quest> QuestsOngoing
    {
        get
        {
            return questsOngoing;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private List<Quest> questsComplete = new List<Quest>();
    public List<Quest> QuestsComplete
    {
        get
        {
            return questsComplete;
        }
    }

    private Quest selectedQuest;

    #region 任务处理相关
    /// <summary>
    /// 接取任务
    /// </summary>
    /// <param name="quest">要接取的任务</param>
    /// <param name="loadMode">是否读档模式</param>
    public bool AcceptQuest(Quest quest, bool loadMode = false)
    {
        if (!quest)
        {
            MessageManager.Instance.NewMessage("空任务");
            return false;
        }
        if (HasOngoingQuest(quest))
        {
            MessageManager.Instance.NewMessage("已经在执行");
            return false;
        }
        QuestAgent qa;
        if (quest.Group)
        {
            QuestGroupAgent qga = questGroupAgents.Find(x => x.questGroup == quest.Group);
            if (qga)
            {
                qa = ObjectPool.Instance.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qa.parent = qga;
                qga.questAgents.Add(qa);
            }
            else
            {
                qga = ObjectPool.Instance.Get(UI.questGroupPrefab, UI.questListParent).GetComponent<QuestGroupAgent>();
                qga.questGroup = quest.Group;
                qga.IsExpanded = true;
                questGroupAgents.Add(qga);

                qa = ObjectPool.Instance.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qa.parent = qga;
                qga.questAgents.Add(qa);
            }
        }
        else qa = ObjectPool.Instance.Get(UI.questPrefab, UI.questListParent).GetComponent<QuestAgent>();
        qa.Init(quest);
        questAgents.Add(qa);
        QuestBoardAgent qba = ObjectPool.Instance.Get(UI.boardQuestPrefab, UI.questBoardArea).GetComponent<QuestBoardAgent>();
        qba.Init(qa);
        questBoardAgents.Add(qba);
        foreach (Objective o in quest.Objectives)
        {
            if (o is CollectObjective)
            {
                CollectObjective co = o as CollectObjective;
                try
                {
                    BackpackManager.Instance.OnGetItemEvent += co.UpdateCollectAmountUp;
                    BackpackManager.Instance.OnLoseItemEvent += co.UpdateCollectAmountDown;
                    if (co.CheckBagAtAcpt && !loadMode) co.UpdateCollectAmountUp(co.Item, BackpackManager.Instance.GetItemAmount(co.Item.ID));
                }
                catch
                {
                    BackpackManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BackpackManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                    MessageManager.Instance.NewMessage(string.Format("[尝试使用null道具] 任务名称：" + quest.Title));
                    continue;
                }
            }
            else if (o is KillObjective)
            {
                KillObjective ko = o as KillObjective;
                try
                {
                    switch (ko.ObjectiveType)
                    {
                        case KillObjectiveType.Specific:
                            foreach (Enemy enemy in GameManager.Enermies[ko.Enemy.ID])
                                enemy.OnDeathEvent += ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enermies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent += ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enermies.Select(x => x.Value))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent += ko.UpdateKillAmount;
                            break;
                    }
                }
                catch
                {
                    MessageManager.Instance.NewMessage(string.Format("[找不到敌人] ID: {0}", ko.Enemy.ID));
                    continue;
                }
            }
            else if (o is TalkObjective)
            {
                TalkObjective to = o as TalkObjective;
                try
                {
                    if (!o.IsComplete) GameManager.Talkers[to.Talker.ID].objectivesTalkToThis.Add(to);
                }
                catch
                {
                    MessageManager.Instance.NewMessage(string.Format("[找不到NPC] ID: {0}", to.Talker.ID));
                    continue;
                }
            }
            else if (o is MoveObjective)
            {
                MoveObjective mo = o as MoveObjective;
                try
                {
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent += mo.UpdateMoveState;
                }
                catch
                {
                    MessageManager.Instance.NewMessage(string.Format("[找不到任务点] ID: {0}", mo.PointID));
                    continue;
                }
            }
            else if (o is CustomObjective)
            {
                CustomObjective cuo = o as CustomObjective;
                TriggerManager.Instance.OnTriggerSetEvent += cuo.UpdateTriggerState;
                if (cuo.CheckStateAtAcpt) TriggerManager.Instance.SetTrigger(cuo.TriggerName, TriggerManager.Instance.GetTriggerState(cuo.TriggerName));
            }
        }
        quest.IsOngoing = true;
        QuestsOngoing.Add(quest);
        if (!quest.SbmtOnOriginalNPC)
        {
            try
            {
                (GameManager.Talkers[quest.NPCToSubmit.ID] as QuestGiver).TransferQuestToThis(quest);
            }
            catch
            {
                MessageManager.Instance.NewMessage(string.Format("[找不到NPC] ID: {0}", quest.NPCToSubmit));
            }
        }
        if (!loadMode)
            MessageManager.Instance.NewMessage("接取了任务 [" + quest.Title + "]");
        if (QuestsOngoing.Count > 0)
        {
            UI.questBoard.alpha = 1;
            UI.questBoard.blocksRaycasts = true;
        }
        UpdateUI();
        return true;
    }
    /// <summary>
    /// 放弃任务
    /// </summary>
    /// <param name="quest">要放弃的任务</param>
    public bool AbandonQuest(Quest quest)
    {
        if (HasOngoingQuest(quest) && quest && quest.Abandonable)
        {
            quest.IsOngoing = false;
            QuestsOngoing.Remove(quest);
            foreach (Objective o in quest.Objectives)
            {
                if (o is CollectObjective)
                {
                    CollectObjective co = o as CollectObjective;
                    co.CurrentAmount = 0;
                    BackpackManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BackpackManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                }
                if (o is KillObjective)
                {
                    KillObjective ko = o as KillObjective;
                    ko.CurrentAmount = 0;
                    switch (ko.ObjectiveType)
                    {
                        case KillObjectiveType.Specific:
                            foreach (Enemy enemy in GameManager.Enermies[ko.Enemy.ID])
                                enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enermies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enermies.Select(x => x.Value))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                    }
                }
                if (o is TalkObjective)
                {
                    TalkObjective to = o as TalkObjective;
                    to.CurrentAmount = 0;
                    GameManager.Talkers[to.Talker.ID].objectivesTalkToThis.RemoveAll(x => x == to);
                    DialogueManager.Instance.RemoveDialogueData(to.Dialogue);
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    mo.CurrentAmount = 0;
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveState;
                }
                else if (o is CustomObjective)
                {
                    CustomObjective cuo = o as CustomObjective;
                    cuo.CurrentAmount = 0;
                    TriggerManager.Instance.OnTriggerSetEvent -= cuo.UpdateTriggerState;
                }
            }
            if (!quest.SbmtOnOriginalNPC)
            {
                quest.OriginalQuestGiver.TransferQuestToThis(quest);
            }
            if (QuestsOngoing.Count < 1)
            {
                UI.questBoard.alpha = 0;
                UI.questBoard.blocksRaycasts = false;
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// 放弃当前展示的任务
    /// </summary>
    public void AbandonSelectedQuest()
    {
        if (!selectedQuest) return;
        ConfirmManager.Instance.NewConfirm("已消耗的道具不会退回，确定放弃此任务吗？", delegate
        {
            if (AbandonQuest(selectedQuest))
            {
                RemoveQuestAgentByQuest(selectedQuest);
                HideDescription();
            }
        });
    }
    /// <summary>
    /// 完成任务
    /// </summary>
    /// <param name="quest">要放弃的任务</param>
    /// <param name="loadMode">是否读档模式</param>
    /// <returns>是否成功完成任务</returns>
    public bool CompleteQuest(Quest quest, bool loadMode = false)
    {
        if (!quest) return false;
        if (HasOngoingQuest(quest) && quest.IsComplete)
        {
            if (!loadMode)
            {
                foreach (ItemInfo rwi in quest.RewardItems)
                    if (!BackpackManager.Instance.TryGetItem_Boolean(rwi))
                        return false;
                List<Quest> questsReqThisQuestItem = new List<Quest>();
                foreach (Objective o in quest.Objectives)
                {
                    if (o is CollectObjective)
                    {
                        CollectObjective co = o as CollectObjective;
                        questsReqThisQuestItem = QuestsRequiredItem(co.Item,
                            BackpackManager.Instance.GetItemAmount(co.Item) - o.Amount).ToList();
                    }
                    if (questsReqThisQuestItem.Contains(quest) && questsReqThisQuestItem.Count > 1)
                    //需要道具的任务群包含该任务且数量多于一个，说明有其他任务对该任务需提交的道具存在依赖
                    {
                        MessageManager.Instance.NewMessage("提交失败！其他任务对该任务需提交物品有需求");
                        return false;
                    }
                }
            }
            quest.IsOngoing = false;
            QuestsOngoing.Remove(quest);
            RemoveQuestAgentByQuest(quest);
            quest.CurrentQuestGiver.QuestInstances.Remove(quest);
            QuestsComplete.Add(quest);
            QuestAgent cqa;
            if (quest.Group)
            {
                QuestGroupAgent cqga = cmpltQuestGroupAgents.Find(x => x.questGroup == quest.Group);
                if (cqga)
                {
                    cqa = ObjectPool.Instance.Get(UI.questPrefab, cqga.questListParent).GetComponent<QuestAgent>();
                    cqa.parent = cqga;
                    cqga.questAgents.Add(cqa);
                    cqga.UpdateStatus();
                }
                else
                {
                    cqga = ObjectPool.Instance.Get(UI.questGroupPrefab, UI.cmpltQuestListParent).GetComponent<QuestGroupAgent>();
                    cqga.questGroup = quest.Group;
                    cqga.IsExpanded = true;
                    cmpltQuestGroupAgents.Add(cqga);

                    cqa = ObjectPool.Instance.Get(UI.questPrefab, cqga.questListParent).GetComponent<QuestAgent>();
                    cqa.parent = cqga;
                    cqga.questAgents.Add(cqa);
                    cqga.UpdateStatus();
                }
            }
            else cqa = ObjectPool.Instance.Get(UI.questPrefab, UI.cmpltQuestListParent).GetComponent<QuestAgent>();
            cqa.Init(quest, true);
            completeQuestAgents.Add(cqa);
            foreach (Objective o in quest.Objectives)
            {
                if (o is CollectObjective)
                {
                    CollectObjective co = o as CollectObjective;
                    BackpackManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BackpackManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                    if (!loadMode && co.LoseItemAtSbmt) BackpackManager.Instance.LoseItem(co.Item, o.Amount);
                }
                if (o is KillObjective)
                {
                    KillObjective ko = o as KillObjective;
                    switch (ko.ObjectiveType)
                    {
                        case KillObjectiveType.Specific:
                            foreach (Enemy enemy in GameManager.Enermies[ko.Enemy.ID])
                                enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enermies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enermies.Select(x => x.Value))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                    }
                }
                if (o is TalkObjective)
                {
                    GameManager.Talkers[(o as TalkObjective).Talker.ID].objectivesTalkToThis.RemoveAll(x => x == (o as TalkObjective));
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveState;
                }
                else if (o is CustomObjective)
                {
                    CustomObjective cuo = o as CustomObjective;
                    TriggerManager.Instance.OnTriggerSetEvent -= cuo.UpdateTriggerState;
                }
            }
            if (!loadMode)
            {
                //TODO 经验的处理
                BackpackManager.Instance.GetMoney(quest.RewardMoney);
                foreach (ItemInfo info in quest.RewardItems)
                {
                    BackpackManager.Instance.GetItem(info);
                }
                MessageManager.Instance.NewMessage("提交了任务 [" + quest.Title + "]");
            }
            HideDescription();
            if (QuestsOngoing.Count < 1)
            {
                UI.questBoard.alpha = 0;
                UI.questBoard.blocksRaycasts = false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 追踪当前展示任务进行中的目标
    /// </summary>
    public void TraceSelectedQuest()
    {
        TraceQuest(selectedQuest);
    }

    public void TraceQuest(Quest quest)
    {
        if (!quest || !AStarManager.Instance || !PlayerManager.Instance.PlayerController.Unit) return;
        if (quest.IsComplete && quest.CurrentQuestGiver)
        {
            PlayerManager.Instance.PlayerController.Unit.IsFollowingTarget = false;
            PlayerManager.Instance.PlayerController.Unit.ShowPath(true);
            PlayerManager.Instance.PlayerController.Unit.SetDestination(quest.CurrentQuestGiver.transform.position, false);
        }
        else if (quest.Objectives.Count > 0)
            using (var objectiveEnum = quest.Objectives.GetEnumerator())
            {
                Vector3 destination = default;
                Objective currentObj = null;
                List<Objective> concurrentObj = new List<Objective>();
                while (objectiveEnum.MoveNext())
                {
                    currentObj = objectiveEnum.Current;
                    if (!currentObj.IsComplete)
                    {
                        if (currentObj.Concurrent && currentObj.AllPrevObjCmplt)
                        {
                            if (!(currentObj is CollectObjective))
                                concurrentObj.Add(currentObj);
                        }
                        else break;
                    }
                }
                if (concurrentObj.Count > 0)
                {
                    int index = Random.Range(0, concurrentObj.Count);//如果目标可以同时进行，则随机选一个
                    currentObj = concurrentObj[index];
                }
                if (currentObj is TalkObjective)
                {
                    TalkObjective to = currentObj as TalkObjective;
                    if (GameManager.Talkers.ContainsKey(to.Talker.ID))
                        destination = GameManager.Talkers[to.Talker.ID].transform.position;
                }
                else if (currentObj is KillObjective)
                {
                    KillObjective ko = currentObj as KillObjective;
                    if (GameManager.Enermies.ContainsKey(ko.Enemy.ID))
                    {
                        Enemy enemy = GameManager.Enermies[ko.Enemy.ID].FirstOrDefault();
                        if (enemy) destination = enemy.transform.position;
                    }
                }
                else if (currentObj is MoveObjective)
                {
                    MoveObjective mo = currentObj as MoveObjective;
                    if (GameManager.QuestPoints[mo.PointID])
                        destination = GameManager.QuestPoints[mo.PointID].transform.position;
                }
                if (destination != default)
                {
                    PlayerManager.Instance.PlayerController.Unit.IsFollowingTarget = false;
                    PlayerManager.Instance.PlayerController.Unit.ShowPath(true);
                }
                PlayerManager.Instance.PlayerController.Unit.SetDestination(destination, false);
            }
    }
    #endregion

    #region UI相关
    public void UpdateUI()
    {
        using (var qaEnum = questAgents.GetEnumerator())
            while (qaEnum.MoveNext())
                qaEnum.Current.UpdateStatus();

        using (var qbaEnum = questBoardAgents.GetEnumerator())
            while (qbaEnum.MoveNext())
                qbaEnum.Current.UpdateStatus();

        using (var qgaEnum = questGroupAgents.GetEnumerator())
            while (qgaEnum.MoveNext())
                qgaEnum.Current.UpdateStatus();

        if (selectedQuest == null) return;
        StringBuilder objectives = new StringBuilder();
        QuestAgent cqa = completeQuestAgents.Find(x => x.MQuest == selectedQuest);
        if (cqa)
        {
            int lineCount = selectedQuest.Objectives.FindAll(x => x.Display).Count - 1;
            for (int i = 0; i < selectedQuest.Objectives.Count; i++)
            {
                if (selectedQuest.Objectives[i].Display)
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives.Append(selectedQuest.Objectives[i].DisplayName + endLine);
                }
            }
            UI.descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}\n\n<b>任务目标</b>\n{3}",
                                    selectedQuest.Title,
                                    selectedQuest.OriginalQuestGiver.TalkerName,
                                    selectedQuest.Description,
                                    objectives.ToString()).ToString();
        }
        else
        {
            int lineCount = selectedQuest.Objectives.FindAll(x => x.Display).Count - 1;
            for (int i = 0; i < selectedQuest.Objectives.Count; i++)
            {
                if (selectedQuest.Objectives[i].Display)
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives.Append(selectedQuest.Objectives[i].DisplayName +
                                  "[" + selectedQuest.Objectives[i].CurrentAmount + "/" + selectedQuest.Objectives[i].Amount + "]" +
                                  (selectedQuest.Objectives[i].IsComplete ? "(达成)" + endLine : endLine));
                }
            }
            UI.descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}\n\n<b>任务目标{3}</b>\n{4}",
                                   selectedQuest.Title,
                                   selectedQuest.OriginalQuestGiver.TalkerName,
                                   selectedQuest.Description,
                                   selectedQuest.IsComplete ? "(完成)" : selectedQuest.IsOngoing ? "(进行中)" : string.Empty,
                                   objectives.ToString()).ToString();
        }
    }

    public void ShowDescription(QuestAgent questAgent)
    {
        if (!questAgent.MQuest) return;
        if (selectedQuest && selectedQuest != questAgent.MQuest)
        {
            QuestAgent qa = questAgents.Find(x => x.MQuest == selectedQuest);
            if (qa) qa.Deselect();
            else
            {
                qa = completeQuestAgents.Find(x => x.MQuest == selectedQuest);
                if (qa) qa.Deselect();
            }
        }
        questAgent.Select();
        selectedQuest = questAgent.MQuest;
        UpdateUI();
        UI.moneyText.text = selectedQuest.RewardMoney > 0 ? selectedQuest.RewardMoney.ToString() : "无";
        UI.EXPText.text = selectedQuest.RewardEXP > 0 ? selectedQuest.RewardEXP.ToString() : "无";

        int befCount = rewardCells.Count;
        for (int i = 0; i < 10 - befCount; i++)
        {
            ItemAgent rwc = ObjectPool.Instance.Get(UI.rewardCellPrefab, UI.rewardCellsParent).GetComponent<ItemAgent>();
            rwc.Init();
            rewardCells.Add(rwc);
        }
        foreach (ItemAgent rwc in rewardCells)
            if (rwc) rwc.Empty();
        foreach (ItemInfo info in selectedQuest.RewardItems)
            foreach (ItemAgent rwc in rewardCells)
            {
                if (rwc.IsEmpty)
                {
                    rwc.InitItem(info);
                    break;
                }
            }
        MyUtilities.SetActive(UI.abandonButton.gameObject, questAgent.MQuest.IsFinished ? false : questAgent.MQuest.Abandonable);
        MyUtilities.SetActive(UI.traceButton.gameObject, questAgent.MQuest.IsFinished ? false : true);
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
        ItemWindowManager.Instance.CloseItemWindow();
    }
    public void HideDescription()
    {
        QuestAgent qa = questAgents.Find(x => x.MQuest == selectedQuest);
        if (qa) qa.Deselect();
        else
        {
            qa = completeQuestAgents.Find(x => x.MQuest == selectedQuest);
            if (qa) qa.Deselect();
        }
        selectedQuest = null;
        UI.descriptionWindow.alpha = 0;
        UI.descriptionWindow.blocksRaycasts = false;
    }

    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        if (DialogueManager.Instance.IsTalking) return;
        UI.questsWindow.alpha = 1;
        UI.questsWindow.blocksRaycasts = true;
        DialogueManager.Instance.HideQuestDescription();
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
        UIManager.Instance.EnableJoyStick(false);
        TriggerManager.Instance.SetTrigger("Open Quest", true);
    }
    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.questsWindow.alpha = 0;
        UI.questsWindow.blocksRaycasts = false;
        HideDescription();
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
        IsPausing = false;
        UIManager.Instance.EnableJoyStick(true);
    }
    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen)
            OpenWindow();
        else CloseWindow();
    }
    public void PauseDisplay(bool pause)
    {
        if (!UI | !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.questsWindow.alpha = 1;
            UI.questsWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.questsWindow.alpha = 0;
            UI.questsWindow.blocksRaycasts = false;
            HideDescription();
        }
        IsPausing = pause;
    }
    #endregion

    #region 其它
    public bool HasOngoingQuest(Quest quest)
    {
        return QuestsOngoing.Contains(quest);
    }
    public bool HasOngoingQuestWithID(string questID)
    {
        return QuestsOngoing.Exists(x => x.ID == questID);
    }

    public bool HasCompleteQuest(Quest quest)
    {
        return QuestsComplete.Contains(quest);
    }
    public bool HasCompleteQuestWithID(string questID)
    {
        return QuestsComplete.Exists(x => x.ID == questID);
    }
    private void RemoveQuestAgentByQuest(Quest quest)
    {
        QuestAgent qa = questAgents.Find(x => x.MQuest == quest);
        if (qa)
        {
            QuestBoardAgent qba = questBoardAgents.Find(x => x.questAgent == qa);
            if (qba)
            {
                qba.questAgent = null;
                questBoardAgents.Remove(qba);
                ObjectPool.Instance.Put(qba.gameObject);
            }
            QuestGroupAgent qga = questGroupAgents.Find(x => x.questAgents.Contains(qa));
            if (qga)
            {
                qga.questAgents.Remove(qa);
                if (qga.questAgents.Count < 1)
                {
                    questGroupAgents.Remove(qga);
                    qga.Recycle();
                }
            }
            questAgents.Remove(qa);
            qa.Recycle();
        }
    }

    /// <summary>
    /// 判定是否有某个任务需要某数量的某个道具
    /// </summary>
    /// <param name="item">要判定的道具ID</param>
    /// <param name="leftAmount">要判定的数量</param>
    /// <returns>是否需要该道具</returns>
    public bool HasQuestRequiredItem(ItemBase item, int leftAmount)
    {
        return QuestsRequiredItem(item, leftAmount).Count() > 0;
    }
    public IEnumerable<Quest> QuestsRequiredItem(ItemBase item, int leftAmount)
    {
        return QuestsOngoing.FindAll(x => x.RequiredItem(item, leftAmount)).AsEnumerable();
    }

    public void SetUI(QuestUI UI)
    {
        foreach (var qa in questAgents)
        {
            if (qa) qa.Recycle();
        }
        questAgents.Clear();
        foreach (var qba in questBoardAgents)
        {
            if (qba)
            {
                qba.questAgent = null;
                ObjectPool.Instance.Put(qba.gameObject);
            }
        }
        questBoardAgents.Clear();
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion
}