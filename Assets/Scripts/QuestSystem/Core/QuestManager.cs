using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/任务管理器")]
public class QuestManager : SingletonMonoBehaviour<QuestManager>, IWindowHandler, IOpenCloseAbleWindow
{
    [SerializeField]
    private QuestUI UI;

    public bool IsPausing { get; private set; }
    public bool IsUIOpen { get; private set; }

    public Canvas CanvasToSort => UI.windowCanvas;

    public QuestFlagsAgent QuestFlagsPrefab => UI ? UI.questFlagsPrefab : null;
    public Transform QuestFlagsPanel => UI ? UI.questFlagsPanel : null;

    private readonly List<ItemAgent> rewardCells = new List<ItemAgent>();
    private readonly List<QuestAgent> questAgents = new List<QuestAgent>();
    private readonly List<QuestAgent> completeQuestAgents = new List<QuestAgent>();
    private readonly List<QuestGroupAgent> questGroupAgents = new List<QuestGroupAgent>();
    private readonly List<QuestGroupAgent> cmpltQuestGroupAgents = new List<QuestGroupAgent>();
    private readonly List<QuestBoardAgent> questBoardAgents = new List<QuestBoardAgent>();
    private readonly Dictionary<Objective, MapIcon> questIcons = new Dictionary<Objective, MapIcon>();

    public delegate void QuestStatusListener();
    public event QuestStatusListener OnQuestStatusChange;

    [SerializeField, Header("任务列表")]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private List<Quest> questsOngoing = new List<Quest>();
    public List<Quest> QuestsOngoing => questsOngoing;

    [SerializeField]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private List<Quest> questsComplete = new List<Quest>();
    public List<Quest> QuestsComplete => questsComplete;

    private Quest selectedQuest;

    #region 任务处理相关
    /// <summary>
    /// 接取任务
    /// </summary>
    /// <param name="quest">要接取的任务</param>
    /// <param name="loadMode">是否读档模式</param>
    public bool AcceptQuest(Quest quest, bool loadMode = false)
    {
        if (!quest || !QuestIsValid(quest))
        {
            MessageManager.Instance.NewMessage("无效任务");
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
        foreach (Objective o in quest.ObjectiveInstances)
        {
            o.OnStateChangeEvent += OnObjectiveStateChange;
            if (o is CollectObjective)
            {
                CollectObjective co = o as CollectObjective;
                BackpackManager.Instance.OnGetItemEvent += co.UpdateCollectAmount;
                BackpackManager.Instance.OnLoseItemEvent += co.UpdateCollectAmountDown;
                if (co.CheckBagAtStart && !loadMode) co.UpdateCollectAmount(co.Item, BackpackManager.Instance.GetItemAmount(co.Item.ID));
            }
            if (o is KillObjective)
            {
                KillObjective ko = o as KillObjective;
                switch (ko.ObjectiveType)
                {
                    case KillObjectiveType.Specific:
                        foreach (Enemy enemy in GameManager.Enemies[ko.Enemy.ID])
                            enemy.OnDeathEvent += ko.UpdateKillAmount;
                        break;
                    case KillObjectiveType.Race:
                        foreach (List<Enemy> enemies in
                            GameManager.Enemies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                            foreach (Enemy enemy in enemies)
                                enemy.OnDeathEvent += ko.UpdateKillAmount;
                        break;
                    case KillObjectiveType.Any:
                        foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                            foreach (Enemy enemy in enemies)
                                enemy.OnDeathEvent += ko.UpdateKillAmount;
                        break;
                }
            }
            if (o is TalkObjective)
                if (!o.IsComplete) GameManager.TalkerDatas[(o as TalkObjective).NPCToTalk.ID].objectivesTalkToThis.Add(o as TalkObjective);
            if (o is MoveObjective)
                GameManager.QuestPoints[(o as MoveObjective).PointID].OnMoveIntoEvent += (o as MoveObjective).UpdateMoveState;
            if (o is SubmitObjective)
                if (!o.IsComplete) GameManager.TalkerDatas[(o as SubmitObjective).NPCToSubmit.ID].objectivesSubmitToThis.Add(o as SubmitObjective);
            if (o is CustomObjective)
            {
                CustomObjective cuo = o as CustomObjective;
                TriggerManager.Instance.RegisterTriggerEvent(cuo.UpdateTriggerState);
                var state = TriggerManager.Instance.GetTriggerState(cuo.TriggerName);
                if (cuo.CheckStateAtAcpt && state != TriggerState.NotExist)
                    TriggerManager.Instance.SetTrigger(cuo.TriggerName, state == TriggerState.On ? true : false);
            }
        }
        quest.IsOngoing = true;
        QuestsOngoing.Add(quest);
        if (quest.NPCToSubmit)
            GameManager.TalkerDatas[quest.NPCToSubmit.ID].TransferQuestToThis(quest);
        if (!loadMode)
            MessageManager.Instance.NewMessage("接取了任务 [" + quest.Title + "]");
        if (QuestsOngoing.Count > 0)
        {
            UI.questBoard.alpha = 1;
            UI.questBoard.blocksRaycasts = true;
        }
        UpdateUI();
        Objective firstObj = quest.ObjectiveInstances[0];
        CreateObjectiveIcon(firstObj);
        OnQuestStatusChange?.Invoke();
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
            foreach (Objective o in quest.ObjectiveInstances)
            {
                o.OnStateChangeEvent -= OnObjectiveStateChange;
                if (o is CollectObjective)
                {
                    CollectObjective co = o as CollectObjective;
                    co.CurrentAmount = 0;
                    BackpackManager.Instance.OnGetItemEvent -= co.UpdateCollectAmount;
                    BackpackManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                }
                if (o is KillObjective)
                {
                    KillObjective ko = o as KillObjective;
                    ko.CurrentAmount = 0;
                    switch (ko.ObjectiveType)
                    {
                        case KillObjectiveType.Specific:
                            foreach (Enemy enemy in GameManager.Enemies[ko.Enemy.ID])
                                enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enemies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                    }
                }
                if (o is TalkObjective)
                {
                    TalkObjective to = o as TalkObjective;
                    to.CurrentAmount = 0;
                    GameManager.TalkerDatas[to.NPCToTalk.ID].objectivesTalkToThis.RemoveAll(x => x == to);
                    DialogueManager.Instance.RemoveDialogueData(to.Dialogue);
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    mo.CurrentAmount = 0;
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveState;
                }
                if (o is SubmitObjective)
                {
                    SubmitObjective so = o as SubmitObjective;
                    so.CurrentAmount = 0;
                    GameManager.TalkerDatas[so.NPCToSubmit.ID].objectivesSubmitToThis.RemoveAll(x => x == so);
                }
                if (o is CustomObjective)
                {
                    CustomObjective cuo = o as CustomObjective;
                    cuo.CurrentAmount = 0;
                    TriggerManager.Instance.DeleteTriggerEvent(cuo.UpdateTriggerState);
                }
                RemoveObjectiveIcon(o);
            }
            if (quest.NPCToSubmit)
                quest.originalQuestHolder.TransferQuestToThis(quest);
            if (QuestsOngoing.Count < 1)
            {
                UI.questBoard.alpha = 0;
                UI.questBoard.blocksRaycasts = false;
            }
            OnQuestStatusChange?.Invoke();
            return true;
        }
        else if (!quest.Abandonable)
            ConfirmManager.Instance.NewConfirm("该任务无法放弃。");
        OnQuestStatusChange?.Invoke();
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
                RemoveUIElementByQuest(selectedQuest);
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
                    if (!BackpackManager.Instance.TryGetItem_Boolean(rwi)) return false;
                List<Quest> questsReqThisQuestItem = new List<Quest>();
                foreach (Objective o in quest.ObjectiveInstances)
                {
                    if (o is CollectObjective)
                    {
                        CollectObjective co = o as CollectObjective;
                        questsReqThisQuestItem = QuestsRequireItem(co.Item, BackpackManager.Instance.GetItemAmount(co.Item) - o.Amount).ToList();
                    }
                    if (questsReqThisQuestItem.Contains(quest) && questsReqThisQuestItem.Count > 1)
                    //需要道具的任务群包含该任务且数量多于一个，说明有其他任务对该任务需提交的道具存在依赖
                    {
                        MessageManager.Instance.NewMessage("提交失败！其他任务对该任务需提交物品存在依赖");
                        return false;
                    }
                }
            }
            quest.IsOngoing = false;
            QuestsOngoing.Remove(quest);
            RemoveUIElementByQuest(quest);
            quest.currentQuestHolder.questInstances.Remove(quest);
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
            foreach (Objective o in quest.ObjectiveInstances)
            {
                o.OnStateChangeEvent -= OnObjectiveStateChange;
                if (o is CollectObjective)
                {
                    CollectObjective co = o as CollectObjective;
                    BackpackManager.Instance.OnGetItemEvent -= co.UpdateCollectAmount;
                    BackpackManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                    if (!loadMode && co.LoseItemAtSbmt) BackpackManager.Instance.LoseItem(co.Item, o.Amount);
                }
                if (o is KillObjective)
                {
                    KillObjective ko = o as KillObjective;
                    switch (ko.ObjectiveType)
                    {
                        case KillObjectiveType.Specific:
                            foreach (Enemy enemy in GameManager.Enemies[ko.Enemy.ID])
                                enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enemies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                                foreach (Enemy enemy in enemies)
                                    enemy.OnDeathEvent -= ko.UpdateKillAmount;
                            break;
                    }
                }
                if (o is TalkObjective)
                {
                    GameManager.TalkerDatas[(o as TalkObjective).NPCToTalk.ID].objectivesTalkToThis.RemoveAll(x => x == (o as TalkObjective));
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    GameManager.QuestPoints[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveState;
                }
                if (o is SubmitObjective)
                {
                    GameManager.TalkerDatas[(o as SubmitObjective).NPCToSubmit.ID].objectivesSubmitToThis.RemoveAll(x => x == (o as SubmitObjective));
                }
                if (o is CustomObjective)
                {
                    CustomObjective cuo = o as CustomObjective;
                    TriggerManager.Instance.DeleteTriggerEvent(cuo.UpdateTriggerState);
                }
                RemoveObjectiveIcon(o);
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
            OnQuestStatusChange?.Invoke();
            return true;
        }
        OnQuestStatusChange?.Invoke();
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
        if (!quest || !QuestIsValid(quest) || !AStarManager.Instance || !PlayerManager.Instance.PlayerController.Unit) return;
        if (quest.IsComplete && GameManager.Talkers[quest.currentQuestHolder.TalkerID])
        {
            PlayerManager.Instance.PlayerController.Unit.IsFollowingTarget = false;
            PlayerManager.Instance.PlayerController.Unit.ShowPath(true);
            PlayerManager.Instance.PlayerController.Unit.SetDestination(GameManager.Talkers[quest.currentQuestHolder.TalkerID].transform.position, false);
        }
        else if (quest.ObjectiveInstances.Count > 0)
            using (var objectiveEnum = quest.ObjectiveInstances.GetEnumerator())
            {
                Vector3 destination = default;
                Objective currentObj = null;
                List<Objective> parallelObj = new List<Objective>();
                while (objectiveEnum.MoveNext())
                {
                    currentObj = objectiveEnum.Current;
                    if (!currentObj.IsComplete)
                    {
                        if (currentObj.Parallel && currentObj.AllPrevObjCmplt)
                        {
                            if (!(currentObj is CollectObjective))
                                parallelObj.Add(currentObj);
                        }
                        else break;
                    }
                }
                if (parallelObj.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, parallelObj.Count);//如果目标可以同时进行，则随机选一个
                    currentObj = parallelObj[index];
                }
                if (currentObj is TalkObjective)
                {
                    TalkObjective to = currentObj as TalkObjective;
                    GameManager.TalkerDatas.TryGetValue(to.NPCToTalk.ID, out TalkerData talkerFound);
                    if (talkerFound)
                    {
                        destination = talkerFound.currentPosition;
                        SetDestination();
                    }
                }
                else if (currentObj is KillObjective)
                {
                    KillObjective ko = currentObj as KillObjective;
                    GameManager.Enemies.TryGetValue(ko.Enemy.ID, out List<Enemy> enemiesFound);
                    if (enemiesFound != null && enemiesFound.Count > 0)
                    {
                        Enemy enemy = enemiesFound.FirstOrDefault();
                        if (enemy)
                        {
                            destination = enemy.transform.position;
                            SetDestination();
                        }
                    }
                }
                else if (currentObj is MoveObjective)
                {
                    MoveObjective mo = currentObj as MoveObjective;
                    GameManager.QuestPoints.TryGetValue(mo.PointID, out QuestPoint pointFound);
                    if (pointFound)
                    {
                        destination = pointFound.transform.position;
                        SetDestination();
                    }
                }
                else if (currentObj is SubmitObjective)
                {
                    SubmitObjective so = currentObj as SubmitObjective;
                    GameManager.TalkerDatas.TryGetValue(so.NPCToSubmit.ID, out TalkerData talkerFound);
                    if (talkerFound)
                    {
                        destination = talkerFound.currentPosition;
                        SetDestination();
                    }
                }

                void SetDestination()
                {
                    PlayerManager.Instance.PlayerController.Unit.IsFollowingTarget = false;
                    PlayerManager.Instance.PlayerController.Unit.ShowPath(true);
                    PlayerManager.Instance.PlayerController.Unit.SetDestination(destination, false);
                }
            }
    }

    private void OnObjectiveStateChange(Objective objective, bool befCmplt)
    {
        if (!befCmplt && objective.IsComplete)
        {
            UpdateNextCollectObjectives(objective);
            //Debug.Log("\"" + objective.DisplayName + "\"" + "从没完成变成完成");
            Objective nextToDo = null;
            Quest quest = objective.runtimeParent;
            List<Objective> parallelObj = new List<Objective>();
            for (int i = 0; i < quest.ObjectiveInstances.Count - 1; i++)
            {
                if (quest.ObjectiveInstances[i] == objective)
                {
                    for (int j = i - 1; j > -1; j--)//往前找可以并行的目标
                    {
                        Objective prevObj = quest.ObjectiveInstances[j];
                        if (!prevObj.Parallel) break;//只要碰到一个不能并行的，就中断
                        else parallelObj.Add(prevObj);
                    }
                    for (int j = i + 1; j < quest.ObjectiveInstances.Count; j++)//往后找可以并行的目标
                    {
                        Objective nextObj = quest.ObjectiveInstances[j];
                        if (!nextObj.Parallel)//只要碰到一个不能并行的，就中断
                        {
                            if (nextObj.AllPrevObjCmplt && !nextObj.IsComplete)
                                nextToDo = nextObj;//同时，若该不能并行目标的所有前置目标都完成了，那么它就是下一个要做的目标
                            break;
                        }
                        else parallelObj.Add(nextObj);
                    }
                    break;
                }
            }
            if (!nextToDo)//当目标不能并行时此变量才不为空，所以此时表示所有后置目标都是可并行的，或者不存在后置目标
            {
                parallelObj.RemoveAll(x => x.IsComplete);//把所有已完成的可并行目标去掉
                /*if (parallelObj.Count > 0)//剩下未完成的可并行目标，则随机选一个作为下一个要做的目标
                    nextToDo = parallelObj[Random.Range(0, parallelObj.Count)];*/
                foreach (var obj in parallelObj)
                    CreateObjectiveIcon(obj);
            }
            else CreateObjectiveIcon(nextToDo);
            RemoveObjectiveIcon(objective);
            OnQuestStatusChange?.Invoke();
        }
        //else Debug.Log("无操作");
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
            int lineCount = selectedQuest.ObjectiveInstances.FindAll(x => x.Display).Count - 1;
            for (int i = 0; i < selectedQuest.ObjectiveInstances.Count; i++)
            {
                if (selectedQuest.ObjectiveInstances[i].Display)
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives.Append(selectedQuest.ObjectiveInstances[i].DisplayName + endLine);
                }
            }
            UI.descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}\n\n<b>任务目标</b>\n{3}",
                                    selectedQuest.Title,
                                    selectedQuest.originalQuestHolder.TalkerName,
                                    selectedQuest.Description,
                                    objectives.ToString()).ToString();
        }
        else
        {
            int lineCount = selectedQuest.ObjectiveInstances.FindAll(x => x.Display).Count - 1;
            for (int i = 0; i < selectedQuest.ObjectiveInstances.Count; i++)
            {
                if (selectedQuest.ObjectiveInstances[i].Display)
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives.Append(selectedQuest.ObjectiveInstances[i].DisplayName +
                                  "[" + selectedQuest.ObjectiveInstances[i].CurrentAmount + "/" + selectedQuest.ObjectiveInstances[i].Amount + "]" +
                                  (selectedQuest.ObjectiveInstances[i].IsComplete ? "(达成)" + endLine : endLine));
                }
            }
            UI.descriptionText.text = new StringBuilder().AppendFormat("<b>{0}</b>\n[委托人: {1}]\n{2}\n\n<b>任务目标{3}</b>\n{4}",
                                   selectedQuest.Title,
                                   selectedQuest.originalQuestHolder.TalkerName,
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
                    rwc.SetItem(info);
                    break;
                }
            }
        ZetanUtility.SetActive(UI.abandonButton.gameObject, questAgent.MQuest.IsFinished ? false : questAgent.MQuest.Abandonable);
        ZetanUtility.SetActive(UI.traceButton.gameObject, questAgent.MQuest.IsFinished ? false : true);
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
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
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
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
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
            UI.window.alpha = 1;
            UI.window.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.window.alpha = 0;
            UI.window.blocksRaycasts = false;
            HideDescription();
        }
        IsPausing = pause;
    }

    private void CreateObjectiveIcon(Objective objective)
    {
        if (!objective || !objective.ShowMapIcon) return;
        //Debug.Log("Create icon for " + objective.DisplayName);
        Vector3 destination;
        if (objective is TalkObjective)
        {
            TalkObjective to = objective as TalkObjective;
            GameManager.TalkerDatas.TryGetValue(to.NPCToTalk.ID, out TalkerData talkerFound);
            if (talkerFound)
            {
                destination = talkerFound.currentPosition;
                CreateIcon();
            }
        }
        else if (objective is KillObjective)
        {
            KillObjective ko = objective as KillObjective;
            GameManager.Enemies.TryGetValue(ko.Enemy.ID, out List<Enemy> enemiesFound);
            if (enemiesFound != null && enemiesFound.Count > 0)
            {
                Enemy enemy = enemiesFound.FirstOrDefault();
                if (enemy)
                {
                    destination = enemy.transform.position;
                    CreateIcon();
                }
            }
        }
        else if (objective is MoveObjective)
        {
            MoveObjective mo = objective as MoveObjective;
            GameManager.QuestPoints.TryGetValue(mo.PointID, out QuestPoint pointFound);
            if (pointFound)
            {
                destination = pointFound.transform.position;
                CreateIcon();
            }
        }
        else if (objective is SubmitObjective)
        {
            SubmitObjective so = objective as SubmitObjective;
            GameManager.TalkerDatas.TryGetValue(so.NPCToSubmit.ID, out TalkerData talkerFound);
            if (talkerFound)
            {
                destination = talkerFound.currentPosition;
                CreateIcon();
            }
        }

        void CreateIcon()
        {
            var icon = UI.questIcon ? (objective is KillObjective ?
                MapManager.Instance.CreateMapIcon(UI.questIcon, new Vector2(48, 48), destination, true, 144f, MapIconType.Objective, false, objective.DisplayName) :
                MapManager.Instance.CreateMapIcon(UI.questIcon, new Vector2(48, 48), destination, true, MapIconType.Objective, false, objective.DisplayName)) :
                MapManager.Instance.CreateDefaultMark(destination, true, objective.DisplayName, false);
            if (icon)
            {
                questIcons.TryGetValue(objective, out MapIcon iconExist);
                if (iconExist)
                {
                    MapManager.Instance.RemoveMapIcon(iconExist, true);
                    questIcons[objective] = icon;
                }
                else questIcons.Add(objective, icon);
            }
        }
    }
    private void RemoveObjectiveIcon(Objective objective)
    {
        if (!objective) return;
        //Debug.Log("Try remove icon for " + objective.DisplayName);
        questIcons.TryGetValue(objective, out MapIcon icon);
        if (icon) MapManager.Instance.RemoveMapIcon(icon, true);
        //else Debug.Log("remove failed for " + objective.DisplayName);
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
    private bool HasCompleteQuestWithID(string questID)
    {
        return QuestsComplete.Exists(x => x.ID == questID);
    }

    private void RemoveUIElementByQuest(Quest quest)
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
    /// 更新某个收集类任务目标，用于在其他前置目标完成时，更新后置收集类目标
    /// </summary>
    private void UpdateNextCollectObjectives(Objective NextObjective)
    {
        Objective tempObj = NextObjective;
        CollectObjective co;
        while (tempObj != null)
        {
            if (!(tempObj is CollectObjective) && tempObj.InOrder && tempObj.NextObjective != null && tempObj.NextObjective.InOrder && tempObj.OrderIndex < tempObj.NextObjective.OrderIndex)
            {
                //若相邻后置目标不是收集类目标，该后置目标按顺序执行，其相邻后置也按顺序执行，且两者不可同时执行，则说明无法继续更新后置的收集类目标
                return;
            }
            if (tempObj is CollectObjective)
            {
                co = tempObj as CollectObjective;
                if (co.CheckBagAtStart) co.CurrentAmount = BackpackManager.Instance.GetItemAmount(co.Item.ID);
            }
            tempObj = tempObj.NextObjective;
        }
    }

    public bool QuestIsAcceptAble(Quest quest)
    {
        bool calFailed = false;
        if (string.IsNullOrEmpty(quest.ConditionRelational)) return quest.AcceptConditions.TrueForAll(x => EligibleCondition(x));
        if (quest.AcceptConditions.Count < 1) calFailed = true;
        else
        {
            //Debug.Log(quest.Title);
            var cr = quest.ConditionRelational.Replace(" ", "").ToCharArray();//删除所有空格才开始计算
            List<string> RPN = new List<string>();//逆波兰表达式
            string indexStr = string.Empty;//数字串
            Stack<char> optStack = new Stack<char>();//运算符栈
            for (int i = 0; i < cr.Length; i++)
            {
                char c = cr[i];
                string item;
                if (c < '0' || c > '9')
                {
                    if (!string.IsNullOrEmpty(indexStr))
                    {
                        item = indexStr;
                        indexStr = string.Empty;
                        GetRPNItem(item);
                    }
                    if (c == '(' || c == ')' || c == '+' || c == '*' || c == '~')
                    {
                        item = c + "";
                        GetRPNItem(item);
                    }
                    else
                    {
                        calFailed = true;
                        break;
                    }//既不是数字也不是运算符，直接放弃计算
                }
                else
                {
                    indexStr += c;//拼接数字
                    if (i + 1 >= cr.Length)
                    {
                        item = indexStr;
                        indexStr = string.Empty;
                        GetRPNItem(item);
                    }
                }
            }
            while (optStack.Count > 0)
                RPN.Add(optStack.Pop() + "");
            Stack<bool> values = new Stack<bool>();
            foreach (var item in RPN)
            {
                Debug.Log(item);
                if (int.TryParse(item, out int index))
                {
                    if (index >= 0 && index < quest.AcceptConditions.Count)
                        values.Push(EligibleCondition(quest.AcceptConditions[index]));
                    else
                    {
                        //Debug.Log("return 1");
                        return true;
                    }
                }
                else if (values.Count > 1)
                {
                    if (item == "+") values.Push(values.Pop() | values.Pop());
                    else if (item == "~") values.Push(!values.Pop());
                    else if (item == "*") values.Push(values.Pop() & values.Pop());
                }
                else if (item == "~") values.Push(!values.Pop());
            }
            if (values.Count == 1)
            {
                //Debug.Log("return 2");
                return values.Pop();
            }

            void GetRPNItem(string item)
            {
                //Debug.Log(item);
                if (item == "+" || item == "*" || item == "~")//遇到运算符
                {
                    char opt = item[0];
                    if (optStack.Count < 1) optStack.Push(opt);//栈空则直接入栈
                    else while (optStack.Count > 0)//栈不空则出栈所有优先级大于或等于opt的运算符后才入栈opt
                        {
                            char top = optStack.Peek();
                            if (top + "" == item || top == '~' || top == '*' && opt == '+')
                            {
                                RPN.Add(optStack.Pop() + "");
                                if (optStack.Count < 1)
                                {
                                    optStack.Push(opt);
                                    break;
                                }
                            }
                            else
                            {
                                optStack.Push(opt);
                                break;
                            }
                        }
                }
                else if (item == "(") optStack.Push('(');
                else if (item == ")")
                {
                    while (optStack.Count > 0)
                    {
                        char opt = optStack.Pop();
                        if (opt == '(') break;
                        else RPN.Add(opt + "");
                    }
                }
                else if (int.TryParse(item, out _)) RPN.Add(item);//遇到数字
            }
        }
        if (!calFailed)
        {
            //Debug.Log("return 3");
            return true;
        }
        else
        {
            foreach (QuestAcceptCondition qac in quest.AcceptConditions)
                if (!EligibleCondition(qac))
                {
                    //Debug.Log("return 4");
                    return false;
                }
            //Debug.Log("return 5");
            return true;
        }
    }

    public bool QuestIsValid(Quest quest)
    {
        if (quest.ObjectiveInstances.Count < 1) return false;
        if (string.IsNullOrEmpty(quest.ID) || string.IsNullOrEmpty(quest.Title)) return false;
        if (quest.NPCToSubmit && !GameManager.TalkerDatas.ContainsKey(quest.NPCToSubmit.ID)) return false;
        foreach (var co in quest.CollectObjectives)
            if (!co.IsValid) return false;
        foreach (var ko in quest.KillObjectives)
            if (!ko.IsValid) return false;
            else if (!GameManager.Enemies.ContainsKey(ko.Enemy.ID)) return false;
        foreach (var to in quest.TalkObjectives)
            if (!to.IsValid) return false;
            else if (!GameManager.TalkerDatas.ContainsKey(to.NPCToTalk.ID)) return false;
        foreach (var mo in quest.MoveObjectives)
            if (!mo.IsValid) return false;
            else if (!GameManager.QuestPoints.ContainsKey(mo.PointID)) return false;
        foreach (var so in quest.SubmitObjectives)
            if (!so.IsValid) return false;
            else if (!GameManager.TalkerDatas.ContainsKey(so.NPCToSubmit.ID)) return false;
        foreach (var cuo in quest.CustomObjectives)
            if (!cuo.IsValid) return false;
        return true;
    }

    /// <summary>
    /// 是否符合条件
    /// </summary>
    private bool EligibleCondition(QuestAcceptCondition condition)
    {
        switch (condition.AcceptCondition)
        {
            case QuestCondition.CompleteQuest: return QuestManager.Instance.HasCompleteQuestWithID(condition.CompleteQuest.ID);
            case QuestCondition.HasItem: return BackpackManager.Instance.HasItemWithID(condition.OwnedItem.ID);
            case QuestCondition.LevelEquals: return PlayerManager.Instance.PlayerInfo.level == condition.Level;
            case QuestCondition.LevelLargeThen: return PlayerManager.Instance.PlayerInfo.level > condition.Level;
            //case QuestCondition.LevelLargeOrEqualsThen: return PlayerManager.Instance.PlayerInfo.level >= level;
            case QuestCondition.LevelLessThen: return PlayerManager.Instance.PlayerInfo.level < condition.Level;
            //case QuestCondition.LevelLessOrEqualsThen: return PlayerManager.Instance.PlayerInfo.level <= level;
            case QuestCondition.TriggerSet:
                var state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist ? (state == TriggerState.On ? true : false) : false;
            case QuestCondition.TriggerReset:
                state = TriggerManager.Instance.GetTriggerState(condition.TriggerName);
                return state != TriggerState.NotExist ? (state == TriggerState.Off ? true : false) : false;
            default: return true;
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
        return QuestsRequireItem(item, leftAmount).Count() > 0;
    }
    private IEnumerable<Quest> QuestsRequireItem(ItemBase item, int leftAmount)
    {
        return QuestsOngoing.FindAll(x => BackpackManager.Instance.QuestRequireItem(x, item, leftAmount)).AsEnumerable();
    }

    public void SaveData(SaveData data)
    {
        foreach (Quest quest in QuestsOngoing)
        {
            data.ongoingQuestDatas.Add(new QuestData(quest));
        }
        foreach (Quest quest in QuestsComplete)
        {
            data.completeQuestDatas.Add(new QuestData(quest));
        }
    }

    public void LoadQuest(SaveData data)
    {
        QuestsOngoing.Clear();
        foreach (QuestData questData in data.ongoingQuestDatas)
        {
            HandlingQuestData(questData);
            UpdateUI();
        }
        QuestsComplete.Clear();
        foreach (QuestData questData in data.completeQuestDatas)
        {
            Quest quest = HandlingQuestData(questData);
            CompleteQuest(quest, true);
        }
    }
    private Quest HandlingQuestData(QuestData questData)
    {
        TalkerData questGiver = GameManager.TalkerDatas[questData.originalGiverID];
        if (!questGiver) return null;
        Quest quest = questGiver.questInstances.Find(x => x.ID == questData.questID);
        if (!quest) return null;
        AcceptQuest(quest, true);
        foreach (ObjectiveData od in questData.objectiveDatas)
        {
            foreach (Objective o in quest.ObjectiveInstances)
            {
                if (o.runtimeID == od.objectiveID)
                {
                    o.CurrentAmount = od.currentAmount;
                    break;
                }
            }
        }
        return quest;
    }
    #endregion
}