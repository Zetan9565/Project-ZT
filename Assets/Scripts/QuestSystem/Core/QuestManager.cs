using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestManager : MonoBehaviour, IWindow
{
    private static QuestManager instance;
    public static QuestManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<QuestManager>();
            return instance;
        }
    }

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

    private List<QuestAgent> questAgents = new List<QuestAgent>();
    private List<QuestAgent> completeQuestAgents = new List<QuestAgent>();
    private List<QuestGroupAgent> questGroupAgents = new List<QuestGroupAgent>();
    private List<QuestGroupAgent> cmpltQuestGroupAgents = new List<QuestGroupAgent>();

    private List<QuestBoardAgent> questBoardAgents = new List<QuestBoardAgent>();

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

    private Quest SelectedQuest;

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
        if (HasQuest(quest))
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
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent += mo.UpdateMoveStatus;
                }
                catch
                {
                    MessageManager.Instance.NewMessage(string.Format("[找不到任务点] ID: {0}", mo.PointID));
                    continue;
                }
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
        if (HasQuest(quest) && quest && quest.Abandonable)
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
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    mo.CurrentAmount = 0;
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveStatus;
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
        if (!SelectedQuest) return;
        ConfirmHandler.Instance.NewConfirm("已消耗的道具不会退回，确定放弃此任务吗？", delegate
        {
            if (AbandonQuest(SelectedQuest))
            {
                RemoveQuestAgentByQuest(SelectedQuest);
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
        if (HasQuest(quest) && quest.IsComplete)
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
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveStatus;
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

    public void TraceSelectedQuest()
    {
        if (!SelectedQuest) return;
        if (SelectedQuest.IsComplete && SelectedQuest.CurrentQuestGiver)
        {
            PlayerManager.Instance.PlayerController.Unit.IsFollowingTarget = false;
            PlayerManager.Instance.PlayerController.Unit.ShowPath(true);
            PlayerManager.Instance.PlayerController.Unit.SetDestination(SelectedQuest.CurrentQuestGiver.transform.position, false);
            return;
        }
        if (SelectedQuest.Objectives.Count > 0)
            using (var objectiveEnum = SelectedQuest.Objectives.GetEnumerator())
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
                            concurrentObj.Add(currentObj);
                        else break;
                    }
                }
                if (concurrentObj.Count > 0)
                {
                    int index = Random.Range(0, concurrentObj.Count);
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

        if (SelectedQuest == null) return;
        string objectives = string.Empty;
        QuestAgent cqa = completeQuestAgents.Find(x => x.MQuest == SelectedQuest);
        if (cqa)
        {
            int lineCount = SelectedQuest.Objectives.FindAll(x => x.Display).Count - 1;
            for (int i = 0; i < SelectedQuest.Objectives.Count; i++)
            {
                if (SelectedQuest.Objectives[i].Display)
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives += SelectedQuest.Objectives[i].DisplayName + endLine;
                }
            }
            UI.descriptionText.text = string.Format("<b>{0}</b>\n[委托人: {1}]\n{2}\n\n<b>任务目标</b>\n{3}",
                                   SelectedQuest.Title,
                                   SelectedQuest.OriginalQuestGiver.TalkerName,
                                   SelectedQuest.Description,
                                   objectives);
        }
        else
        {
            int lineCount = SelectedQuest.Objectives.FindAll(x => x.Display).Count - 1;
            for (int i = 0; i < SelectedQuest.Objectives.Count; i++)
            {
                if (SelectedQuest.Objectives[i].Display)
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives += SelectedQuest.Objectives[i].DisplayName +
                                  "[" + SelectedQuest.Objectives[i].CurrentAmount + "/" + SelectedQuest.Objectives[i].Amount + "]" +
                                  (SelectedQuest.Objectives[i].IsComplete ? "(达成)" + endLine : endLine);
                }
            }
            UI.descriptionText.text = string.Format("<b>{0}</b>\n[委托人: {1}]\n{2}\n\n<b>任务目标{3}</b>\n{4}",
                                   SelectedQuest.Title,
                                   SelectedQuest.OriginalQuestGiver.TalkerName,
                                   SelectedQuest.Description,
                                   SelectedQuest.IsComplete ? "(完成)" : SelectedQuest.IsOngoing ? "(进行中)" : string.Empty,
                                   objectives);
        }
    }

    public void ShowDescription(QuestAgent questAgent)
    {
        if (!questAgent.MQuest) return;
        DialogueManager.Instance.HideQuestDescription();
        if (SelectedQuest && SelectedQuest != questAgent.MQuest)
        {
            QuestAgent qa = questAgents.Find(x => x.MQuest == SelectedQuest);
            if (qa) qa.Deselect();
            else
            {
                qa = completeQuestAgents.Find(x => x.MQuest == SelectedQuest);
                if (qa) qa.Deselect();
            }
        }
        questAgent.Select();
        SelectedQuest = questAgent.MQuest;
        UpdateUI();
        UI.moneyText.text = SelectedQuest.RewardMoney > 0 ? SelectedQuest.RewardMoney.ToString() : "无";
        UI.EXPText.text = SelectedQuest.RewardEXP > 0 ? SelectedQuest.RewardEXP.ToString() : "无";
        foreach (ItemAgent rwc in UI.rewardCells)
            rwc.Clear();
        foreach (ItemInfo info in SelectedQuest.RewardItems)
            foreach (ItemAgent rwc in UI.rewardCells)
            {
                if (rwc.MItemInfo == null)
                {
                    rwc.InitItem(info);
                    break;
                }
            }
        MyTools.SetActive(UI.abandonButton.gameObject, questAgent.MQuest.IsFinished ? false : questAgent.MQuest.Abandonable);
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
        ItemWindowHandler.Instance.CloseItemWindow();
    }
    public void HideDescription()
    {
        QuestAgent qa = questAgents.Find(x => x.MQuest == SelectedQuest);
        if (qa) qa.Deselect();
        else
        {
            qa = completeQuestAgents.Find(x => x.MQuest == SelectedQuest);
            if (qa) qa.Deselect();
        }
        SelectedQuest = null;
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
    public bool HasQuest(Quest quest)
    {
        return QuestsOngoing.Contains(quest);
    }
    public bool HasCompleteQuest(Quest quest)
    {
        return QuestsComplete.Contains(quest);
    }
    public bool HasCmpltQuestWithID(string questID)
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
            qa.OnRecycle();
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
        if (!UI) return;
        this.UI = UI;
    }

    public void ResetUI()
    {
        questAgents.Clear();
        IsUIOpen = false;
        IsPausing = false;
    }
    #endregion
}
