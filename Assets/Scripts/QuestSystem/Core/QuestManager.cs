using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/任务管理器")]
public class QuestManager : WindowHandler<QuestUI, QuestManager>, IOpenCloseAbleWindow
{
    public QuestFlag QuestFlagsPrefab => UI ? UI.questFlagPrefab : null;

    private readonly List<ItemAgent> rewardCells = new List<ItemAgent>();
    private readonly List<QuestAgent> progressQuestAgents = new List<QuestAgent>();
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
    private List<Quest> questsInProgress = new List<Quest>();

    [SerializeField]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private List<Quest> questsComplete = new List<Quest>();

    private Quest selectedQuest;

    #region 任务处理相关
    /// <summary>
    /// 接取任务
    /// </summary>
    /// <param name="quest">要接取的任务</param>
    public bool AcceptQuest(Quest quest)
    {
        if (!quest || !IsQuestValid(quest))
        {
            MessageManager.Instance.New("无效任务");
            return false;
        }
        if (!quest.AcceptCondition.IsMeet() && !SaveManager.Instance.IsLoading)
        {
            MessageManager.Instance.New("未满足任务接取条件");
            return false;
        }
        if (HasOngoingQuest(quest))
        {
            MessageManager.Instance.New("已经在执行");
            return false;
        }
        QuestAgent qa;
        if (quest.Group)
        {
            QuestGroupAgent qga = questGroupAgents.Find(x => x.questGroup == quest.Group);
            if (qga)
            {
                qa = ObjectPool.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qa.parent = qga;
                qga.questAgents.Add(qa);
            }
            else
            {
                qga = ObjectPool.Get(UI.questGroupPrefab, UI.questListParent).GetComponent<QuestGroupAgent>();
                qga.questGroup = quest.Group;
                qga.IsExpanded = true;
                questGroupAgents.Add(qga);

                qa = ObjectPool.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qa.parent = qga;
                qga.questAgents.Add(qa);
            }
        }
        else qa = ObjectPool.Get(UI.questPrefab, UI.questListParent).GetComponent<QuestAgent>();
        qa.Init(quest);
        progressQuestAgents.Add(qa);
        QuestBoardAgent qba = ObjectPool.Get(UI.boardQuestPrefab, UI.questBoardArea).GetComponent<QuestBoardAgent>();
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
                if (co.CheckBagAtStart && !SaveManager.Instance.IsLoading) co.UpdateCollectAmount(co.Item.ID, BackpackManager.Instance.GetItemAmount(co.Item.ID));
                else if (!co.CheckBagAtStart && !SaveManager.Instance.IsLoading) co.amountWhenStart = BackpackManager.Instance.GetItemAmount(co.Item.ID);
            }
            if (o is KillObjective)
            {
                KillObjective ko = o as KillObjective;
                switch (ko.ObjectiveType)
                {
                    case KillObjectiveType.Specific:
                        GameManager.Enemies[ko.Enemy.ID].ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        break;
                    case KillObjectiveType.Race:
                        foreach (List<Enemy> enemies in
                            GameManager.Enemies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                            enemies.ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        break;
                    case KillObjectiveType.Any:
                        foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                            enemies.ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        break;
                }
            }
            if (o is TalkObjective)
                if (!o.IsComplete)
                {
                    var talker = GameManager.TalkerDatas[(o as TalkObjective).NPCToTalk.ID];
                    talker.objectivesTalkToThis.Add(o as TalkObjective);
                    o.OnStateChangeEvent += talker.TryRemoveObjective;
                }
            if (o is MoveObjective)
                GameManager.QuestPoints[(o as MoveObjective).PointID].ForEach(x => x.OnMoveIntoEvent += (o as MoveObjective).UpdateMoveState);
            if (o is SubmitObjective)
                if (!o.IsComplete)
                {
                    var talker = GameManager.TalkerDatas[(o as SubmitObjective).NPCToSubmit.ID];
                    talker.objectivesSubmitToThis.Add(o as SubmitObjective);
                    o.OnStateChangeEvent += talker.TryRemoveObjective;
                }
            if (o is CustomObjective)
            {
                CustomObjective cuo = o as CustomObjective;
                TriggerManager.Instance.RegisterTriggerEvent(cuo.UpdateTriggerState);
                var state = TriggerManager.Instance.GetTriggerState(cuo.TriggerName);
                if (cuo.CheckStateAtAcpt && state != TriggerState.NotExist)
                    TriggerManager.Instance.SetTrigger(cuo.TriggerName, state == TriggerState.On ? true : false);
            }
        }
        quest.InProgress = true;
        questsInProgress.Add(quest);
        if (quest.NPCToSubmit)
            GameManager.TalkerDatas[quest.NPCToSubmit.ID].TransferQuestToThis(quest);
        if (!SaveManager.Instance.IsLoading) MessageManager.Instance.New($"接取了任务 [{quest.Title}]");
        if (questsInProgress.Count > 0)
        {
            UI.questBoard.alpha = 1;
            UI.questBoard.blocksRaycasts = true;
        }
        UpdateUI();
        Objective firstObj = quest.ObjectiveInstances[0];
        CreateObjectiveMapIcon(firstObj);
        OnQuestStatusChange?.Invoke();
        return true;
    }
    /// <summary>
    /// 完成任务
    /// </summary>
    /// <param name="quest">要放弃的任务</param>
    /// <returns>是否成功完成任务</returns>
    public bool CompleteQuest(Quest quest)
    {
        if (!quest) return false;
        if (HasOngoingQuest(quest) && quest.IsComplete)
        {
            if (!SaveManager.Instance.IsLoading)
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
                        MessageManager.Instance.New("提交失败！其他任务对该任务需提交的物品存在依赖");
                        return false;
                    }
                }
            }
            quest.InProgress = false;
            questsInProgress.Remove(quest);
            RemoveUIElementByQuest(quest);
            quest.currentQuestHolder.questInstances.Remove(quest);
            questsComplete.Add(quest);
            QuestAgent cqa;
            if (quest.Group)
            {
                QuestGroupAgent cqga = cmpltQuestGroupAgents.Find(x => x.questGroup == quest.Group);
                if (cqga)
                {
                    cqa = ObjectPool.Get(UI.questPrefab, cqga.questListParent).GetComponent<QuestAgent>();
                    cqa.parent = cqga;
                    cqga.questAgents.Add(cqa);
                    cqga.UpdateStatus();
                }
                else
                {
                    cqga = ObjectPool.Get(UI.questGroupPrefab, UI.cmpltQuestListParent).GetComponent<QuestGroupAgent>();
                    cqga.questGroup = quest.Group;
                    cqga.IsExpanded = true;
                    cmpltQuestGroupAgents.Add(cqga);

                    cqa = ObjectPool.Get(UI.questPrefab, cqga.questListParent).GetComponent<QuestAgent>();
                    cqa.parent = cqga;
                    cqga.questAgents.Add(cqa);
                    cqga.UpdateStatus();
                }
            }
            else cqa = ObjectPool.Get(UI.questPrefab, UI.cmpltQuestListParent).GetComponent<QuestAgent>();
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
                    if (!SaveManager.Instance.IsLoading && co.LoseItemAtSbmt) BackpackManager.Instance.LoseItem(co.Item, o.Amount);
                }
                if (o is KillObjective)
                {
                    KillObjective ko = o as KillObjective;
                    switch (ko.ObjectiveType)
                    {
                        case KillObjectiveType.Specific:
                            GameManager.Enemies[ko.Enemy.ID].ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enemies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                    }
                }
                if (o is TalkObjective)
                {
                    var talker = GameManager.TalkerDatas[(o as TalkObjective).NPCToTalk.ID];
                    talker.objectivesTalkToThis.RemoveAll(x => x == (o as TalkObjective));
                    o.OnStateChangeEvent -= talker.TryRemoveObjective;
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    GameManager.QuestPoints[mo.PointID].ForEach(x => x.OnMoveIntoEvent -= mo.UpdateMoveState);
                }
                if (o is SubmitObjective)
                {
                    var talker = GameManager.TalkerDatas[(o as SubmitObjective).NPCToSubmit.ID];
                    talker.objectivesSubmitToThis.RemoveAll(x => x == (o as SubmitObjective));
                    o.OnStateChangeEvent -= talker.TryRemoveObjective;
                }
                if (o is CustomObjective)
                {
                    CustomObjective cuo = o as CustomObjective;
                    TriggerManager.Instance.DeleteTriggerListner(cuo.UpdateTriggerState);
                }
                RemoveObjectiveMapIcon(o);
            }
            if (!SaveManager.Instance.IsLoading)
            {
                //TODO 经验的处理
                BackpackManager.Instance.GetMoney(quest.RewardMoney);
                foreach (ItemInfo info in quest.RewardItems)
                {
                    BackpackManager.Instance.GetItem(info);
                }
                MessageManager.Instance.New($"提交了任务 [{quest.Title}]");
            }
            HideDescription();
            if (questsInProgress.Count < 1)
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
    /// 放弃任务
    /// </summary>
    /// <param name="quest">要放弃的任务</param>
    public bool AbandonQuest(Quest quest)
    {
        if (HasOngoingQuest(quest) && quest && quest.Abandonable)
        {
            quest.InProgress = false;
            questsInProgress.Remove(quest);
            foreach (Objective o in quest.ObjectiveInstances)
            {
                o.OnStateChangeEvent -= OnObjectiveStateChange;
                if (o is CollectObjective)
                {
                    CollectObjective co = o as CollectObjective;
                    co.CurrentAmount = 0;
                    co.amountWhenStart = 0;
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
                            GameManager.Enemies[ko.Enemy.ID].ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in
                                GameManager.Enemies.Select(x => x.Value).Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Race))
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
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
                    GameManager.QuestPoints[mo.PointID].ForEach(x => x.OnMoveIntoEvent -= mo.UpdateMoveState);
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
                    TriggerManager.Instance.DeleteTriggerListner(cuo.UpdateTriggerState);
                }
                RemoveObjectiveMapIcon(o);
            }
            if (quest.NPCToSubmit)
                quest.originalQuestHolder.TransferQuestToThis(quest);
            if (questsInProgress.Count < 1)
            {
                UI.questBoard.alpha = 0;
                UI.questBoard.blocksRaycasts = false;
            }
            OnQuestStatusChange?.Invoke();
            return true;
        }
        else if (!quest.Abandonable) ConfirmManager.Instance.New("该任务无法放弃。");
        OnQuestStatusChange?.Invoke();
        return false;
    }
    /// <summary>
    /// 放弃当前展示的任务
    /// </summary>
    public void AbandonSelectedQuest()
    {
        if (!selectedQuest) return;
        ConfirmManager.Instance.New("已消耗的道具不会退回，确定放弃此任务吗？", delegate
        {
            if (AbandonQuest(selectedQuest))
            {
                RemoveUIElementByQuest(selectedQuest);
                HideDescription();
            }
        });
    }

    public void TraceQuest(Quest quest)
    {
        if (!quest || !IsQuestValid(quest) || !AStarManager.Instance || !PlayerManager.Instance.PlayerController.Unit) return;
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
                    int index = Random.Range(0, parallelObj.Count);//如果目标可以同时进行，则随机选一个
                    currentObj = parallelObj[index];
                }
                if (!currentObj.CanNavigate) return;
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
                    GameManager.QuestPoints.TryGetValue(mo.PointID, out var pointsFound);
                    if (pointsFound != null)
                    {
                        destination = pointsFound[Random.Range(0, pointsFound.Count)].transform.position;
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
    /// <summary>
    /// 追踪当前展示任务进行中的目标
    /// </summary>
    public void TraceSelectedQuest()
    {
        TraceQuest(selectedQuest);
    }

    private void OnObjectiveStateChange(Objective objective, bool befCmplt)
    {
        if (!SaveManager.Instance.IsLoading)
        {
            if (objective.CurrentAmount > 0)
            {
                string message = objective.DisplayName + (objective.IsComplete ? "(完成)" : $"[{objective.CurrentAmount}/{objective.Amount}]");
                MessageManager.Instance.New(message);
            }
            if (objective.runtimeParent.IsComplete) MessageManager.Instance.New($"[任务]{objective.runtimeParent.Title}(已完成)");
        }
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
                    CreateObjectiveMapIcon(obj);
            }
            else CreateObjectiveMapIcon(nextToDo);
            RemoveObjectiveMapIcon(objective);
            OnQuestStatusChange?.Invoke();
        }
        //else Debug.Log("无操作");
    }
    #endregion

    #region UI相关
    public void UpdateUI()
    {
        using (var qaEnum = progressQuestAgents.GetEnumerator())
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
                                   selectedQuest.IsComplete ? "(完成)" : selectedQuest.InProgress ? "(进行中)" : string.Empty,
                                   objectives.ToString()).ToString();
        }
    }

    public void ShowDescription(QuestAgent questAgent)
    {
        if (!questAgent.MQuest) return;
        if (selectedQuest && selectedQuest != questAgent.MQuest)
        {
            QuestAgent qa = progressQuestAgents.Find(x => x.MQuest == selectedQuest);
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
            ItemAgent rwc = ObjectPool.Get(UI.rewardCellPrefab, UI.rewardCellsParent).GetComponent<ItemAgent>();
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
        ItemWindowManager.Instance.CloseWindow();
    }
    public void HideDescription()
    {
        QuestAgent qa = progressQuestAgents.Find(x => x.MQuest == selectedQuest);
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

    public override void OpenWindow()
    {
        if (DialogueManager.Instance.IsTalking) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        DialogueManager.Instance.HideQuestDescription();
        UIManager.Instance.EnableJoyStick(false);
        TriggerManager.Instance.SetTrigger("Open Quest", true);
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        HideDescription();
        UIManager.Instance.EnableJoyStick(true);
    }
    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) OpenWindow();
        else CloseWindow();
    }
    public override void PauseDisplay(bool pause)
    {
        if (!IsPausing && pause) HideDescription();
        base.PauseDisplay(pause);
    }

    private void RemoveUIElementByQuest(Quest quest)
    {
        QuestAgent qa = progressQuestAgents.Find(x => x.MQuest == quest);
        if (qa)
        {
            QuestBoardAgent qba = questBoardAgents.Find(x => x.questAgent == qa);
            if (qba)
            {
                qba.questAgent = null;
                questBoardAgents.Remove(qba);
                ObjectPool.Put(qba.gameObject);
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
            progressQuestAgents.Remove(qa);
            qa.Recycle();
        }
    }

    private void CreateObjectiveMapIcon(Objective objective)
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
            GameManager.QuestPoints.TryGetValue(mo.PointID, out var pointsFound);
            if (pointsFound != null)
            {
                destination = pointsFound[Random.Range(0, pointsFound.Count)].transform.position;
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
                MapManager.Instance.CreateDefaultMark(destination, true, false, objective.DisplayName);
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
    private void RemoveObjectiveMapIcon(Objective objective)
    {
        if (!objective) return;
        //Debug.Log("Try remove icon for " + objective.DisplayName);
        questIcons.TryGetValue(objective, out MapIcon icon);
        if (icon) MapManager.Instance.RemoveMapIcon(icon, true);
        //else Debug.Log("remove failed for " + objective.DisplayName);
    }

    public override void SetUI(QuestUI UI)
    {
        foreach (var qba in questBoardAgents)
        {
            if (qba)
            {
                qba.questAgent.Recycle();
                qba.Recycle();
            }
        }
        progressQuestAgents.Clear();
        questBoardAgents.Clear();
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion

    #region 其它
    public bool HasOngoingQuest(Quest quest)
    {
        return questsInProgress.Contains(quest);
    }
    public bool HasOngoingQuestWithID(string questID)
    {
        return questsInProgress.Exists(x => x.ID == questID);
    }

    public bool HasCompleteQuest(Quest quest)
    {
        return questsComplete.Contains(quest);
    }
    public bool HasCompleteQuestWithID(string questID)
    {
        return questsComplete.Exists(x => x.ID == questID);
    }

    /// <summary>
    /// 更新某个收集类任务目标，用于在其他前置目标完成时，更新其后置收集类目标
    /// </summary>
    private void UpdateNextCollectObjectives(Objective objective)
    {
        if (!objective || !objective.NextObjective) return;
        Objective nextObjective = objective.NextObjective;
        CollectObjective co;
        while (nextObjective != null)
        {
            if (!(nextObjective is CollectObjective) && nextObjective.InOrder && nextObjective.NextObjective != null && nextObjective.NextObjective.InOrder && nextObjective.OrderIndex < nextObjective.NextObjective.OrderIndex)
            {
                //若相邻后置目标不是收集类目标，该后置目标按顺序执行，其相邻后置也按顺序执行，且两者不可同时执行，则说明无法继续更新后置的收集类目标
                return;
            }
            if (nextObjective is CollectObjective)
            {
                co = nextObjective as CollectObjective;
                if (co.CheckBagAtStart) co.CurrentAmount = BackpackManager.Instance.GetItemAmount(co.Item.ID);
            }
            nextObjective = nextObjective.NextObjective;
        }
    }

    /// <summary>
    /// 任务是否有效
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    public static bool IsQuestValid(Quest quest)
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
        return questsInProgress.FindAll(x => BackpackManager.Instance.DoQuestRequireItem(x, item, leftAmount)).AsEnumerable();
    }

    public void SaveData(SaveData data)
    {
        foreach (Quest quest in questsInProgress)
        {
            data.inProgressQuestDatas.Add(new QuestData(quest));
        }
        foreach (Quest quest in questsComplete)
        {
            data.completeQuestDatas.Add(new QuestData(quest));
        }
    }

    public void Init()
    {
        foreach (var quest in questsInProgress)
            foreach (Objective o in quest.ObjectiveInstances)
                RemoveObjectiveMapIcon(o);
        questsInProgress.Clear();
        questsComplete.Clear();
        questBoardAgents.ForEach(qba => { qba.questAgent.Recycle(); qba.Recycle(); });
        progressQuestAgents.Clear();
        questBoardAgents.Clear();
        completeQuestAgents.ForEach(cqa => cqa.Recycle());
        completeQuestAgents.Clear();
        questGroupAgents.ForEach(qga => qga.Recycle());
        questGroupAgents.Clear();
        UI.questBoard.alpha = 0;
        UI.questBoard.blocksRaycasts = false;
    }

    public void LoadQuest(SaveData data)
    {
        questsInProgress.Clear();
        foreach (QuestData questData in data.inProgressQuestDatas)
        {
            HandlingQuestData(questData);
            UpdateUI();
        }
        questsComplete.Clear();
        foreach (QuestData questData in data.completeQuestDatas)
        {
            Quest quest = HandlingQuestData(questData);
            CompleteQuest(quest);
        }
    }
    private Quest HandlingQuestData(QuestData questData)
    {
        TalkerData questGiver = GameManager.TalkerDatas[questData.originalGiverID];
        if (!questGiver) return null;
        Quest quest = questGiver.questInstances.Find(x => x.ID == questData.questID);
        if (!quest) return null;
        AcceptQuest(quest);
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