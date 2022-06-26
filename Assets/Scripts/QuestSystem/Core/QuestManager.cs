using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using ZetanStudio;
using ZetanStudio.ItemSystem;

public static class QuestManager
{
    private static readonly Dictionary<ObjectiveData, List<MapIcon>> questIcons = new Dictionary<ObjectiveData, List<MapIcon>>();

    private static readonly List<QuestData> questsInProgress = new List<QuestData>();
    public static ReadOnlyCollection<QuestData> QuestInProgress => questsInProgress.AsReadOnly();

    private static readonly List<QuestData> questsFinished = new List<QuestData>();//分开存储完成任务可减少不必要的检索开销
    public static ReadOnlyCollection<QuestData> QuestFinished => questsFinished.AsReadOnly();

    #region 任务处理相关
    /// <summary>
    /// 接取任务
    /// </summary>
    /// <param name="quest">要接取的任务</param>
    public static bool AcceptQuest(QuestData quest)
    {
        if (!quest || !IsQuestValid(quest.Model))
        {
            MessageManager.Instance.New(Tr("无效任务"));
            return false;
        }
        if (!MiscFuntion.CheckCondition(quest.Model.AcceptCondition) && !SaveManager.Instance.IsLoading)
        {
            MessageManager.Instance.New(Tr("未满足任务接取条件"));
            return false;
        }
        if (HasOngoingQuest(quest))
        {
            MessageManager.Instance.New(Tr("已经在执行"));
            return false;
        }
        ObjectiveData currentObjective = quest.Objectives[0];
        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            var o = quest.Objectives[i];
            o.OnStateChangeEvent += OnObjectiveStateChange;
            if (o is CollectObjectiveData co)
            {
                BackpackManager.Instance.Inventory.OnItemAmountChanged += co.UpdateCollectAmount;
                if (o.AllPrevComplete)
                {
                    if (co.Model.CheckBagAtStart && !SaveManager.Instance.IsLoading) co.CurrentAmount = BackpackManager.Instance.GetAmount(co.Model.ItemToCollect);
                    else if (!co.Model.CheckBagAtStart && !SaveManager.Instance.IsLoading) co.amountWhenStart = BackpackManager.Instance.GetAmount(co.Model.ItemToCollect);
                }
            }
            if (o is KillObjectiveData ko)
            {
                switch (ko.Model.KillType)
                {
                    case KillObjectiveType.Specific:
                        GameManager.Enemies[ko.Model.Enemy.ID].ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        break;
                    case KillObjectiveType.Race:
                        foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Model.Race))
                            enemies.ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        break;
                    case KillObjectiveType.Group:
                        foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && ko.Model.Group.Contains(x[0].Info.ID)))
                        {
                            enemies.ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        }
                        break;
                    case KillObjectiveType.Any:
                        foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                            enemies.ForEach(e => e.OnDeathEvent += ko.UpdateKillAmount);
                        break;
                }
            }
            if (o is TalkObjectiveData to)
                if (!o.IsComplete)
                {
                    var talker = DialogueManager.Talkers[to.Model.NPCToTalk.ID];
                    talker.objectivesTalkToThis.Add(to);
                    o.OnStateChangeEvent += talker.TryRemoveObjective;
                }
            if (o is MoveObjectiveData mo)
                mo.targetPoint = CheckPointManager.CreateCheckPoint(mo.Model.AuxiliaryPos, mo.UpdateMoveState);
            if (o is SubmitObjectiveData so)
                if (!o.IsComplete)
                {
                    var talker = DialogueManager.Talkers[so.Model.NPCToSubmit.ID];
                    talker.objectivesSubmitToThis.Add(so);
                    o.OnStateChangeEvent += talker.TryRemoveObjective;
                }
            if (o is TriggerObjectiveData cuo)
            {
                TriggerManager.RegisterTriggerEvent(cuo.UpdateTriggerState);
                var state = TriggerManager.GetTriggerState(cuo.Model.TriggerName);
                if (cuo.Model.CheckStateAtAcpt && state != TriggerState.NotExist)
                    TriggerManager.SetTrigger(cuo.Model.TriggerName, state == TriggerState.On);
            }
        }
        quest.InProgress = true;
        questsInProgress.Add(quest);
        if (quest.Model.NPCToSubmit)
            DialogueManager.Talkers[quest.Model.NPCToSubmit.ID].TransferQuestToThis(quest);
        if (!SaveManager.Instance.IsLoading) MessageManager.Instance.New(Tr("接取了任务{0}", quest.Title));
        quest.latestHandleDays = TimeManager.Instance.Days;
        CreateObjectiveMapIcon(quest.Objectives[0]);
        NotifyCenter.PostNotify(QuestStateChanged, quest, false);
        return true;
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    /// <param name="quest">要放弃的任务</param>
    /// <returns>是否成功完成任务</returns>
    public static bool CompleteQuest(QuestData quest)
    {
        if (!quest) return false;
        if (HasOngoingQuest(quest) && quest.IsComplete)
        {
            if (!SaveManager.Instance.IsLoading)
            {
                if (!BackpackManager.Instance.CanGet(quest.Model.RewardItems)) return false;
                List<QuestData> questsReqThisItem = new List<QuestData>();
                foreach (ObjectiveData o in quest.Objectives)
                {
                    if (o is CollectObjectiveData co)
                    {
                        questsReqThisItem = FindQuestsRequiredItem(co.Model.ItemToCollect, BackpackManager.Instance.GetAmount(co.Model.ItemToCollect) - o.Model.Amount).ToList();
                    }
                    if (questsReqThisItem.Contains(quest) && questsReqThisItem.Count > 1)
                    //需要道具的任务群包含该任务且数量多于一个，说明有其他任务对该任务需提交的道具存在依赖
                    {
                        MessageManager.Instance.New(Tr("暂时无法提交，其他任务对此任务需提交的物品存在依赖"));
                        return false;
                    }
                }
            }
            quest.InProgress = false;
            questsInProgress.Remove(quest);
            quest.currentQuestHolder.questInstances.Remove(quest);
            questsFinished.Add(quest);
            foreach (ObjectiveData o in quest.Objectives)
            {
                o.OnStateChangeEvent -= OnObjectiveStateChange;
                if (o is CollectObjectiveData co)
                {
                    BackpackManager.Instance.Inventory.OnItemAmountChanged -= co.UpdateCollectAmount;
                    if (!SaveManager.Instance.IsLoading && co.Model.LoseItemAtSbmt) BackpackManager.Instance.Lose(co.Model.ItemToCollect, o.Model.Amount);
                }
                if (o is KillObjectiveData ko)
                {
                    switch (ko.Model.KillType)
                    {
                        case KillObjectiveType.Specific:
                            GameManager.Enemies[ko.Model.Enemy.ID].ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                        case KillObjectiveType.Race:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Model.Race))
                            {
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            }
                            break;
                        case KillObjectiveType.Group:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && ko.Model.Group.Contains(x[0].Info.ID)))
                            {
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            }
                            break;
                        case KillObjectiveType.Any:
                            foreach (List<Enemy> enemies in GameManager.Enemies.Values)
                                enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                            break;
                    }
                }
                if (o is TalkObjectiveData to)
                {
                    var talker = DialogueManager.Talkers[to.Model.NPCToTalk.ID];
                    talker.objectivesTalkToThis.RemoveAll(x => x == to);
                    o.OnStateChangeEvent -= talker.TryRemoveObjective;
                }
                if (o is MoveObjectiveData mo)
                {
                    mo.targetPoint = null;
                    CheckPointManager.RemoveCheckPointListener(mo.Model.AuxiliaryPos, mo.UpdateMoveState);
                }
                if (o is SubmitObjectiveData so)
                {
                    var talker = DialogueManager.Talkers[so.Model.NPCToSubmit.ID];
                    talker.objectivesSubmitToThis.RemoveAll(x => x == so);
                    o.OnStateChangeEvent -= talker.TryRemoveObjective;
                }
                if (o is TriggerObjectiveData cuo)
                {
                    TriggerManager.DeleteTriggerListner(cuo.UpdateTriggerState);
                }
                RemoveObjectiveMapIcon(o);
            }
            if (!SaveManager.Instance.IsLoading)
            {
                BackpackManager.Instance.Get(quest.Model.RewardItems);
                MessageManager.Instance.New(Tr("提交了任务{0}", quest.Title));
            }
            quest.latestHandleDays = TimeManager.Instance.Days;
            NotifyCenter.PostNotify(QuestStateChanged, quest, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 放弃任务
    /// </summary>
    /// <param name="quest">要放弃的任务</param>
    public static bool AbandonQuest(QuestData quest)
    {
        if (!quest.Model.Abandonable) ConfirmWindow.StartConfirm(Tr("该任务无法放弃。"));
        else if (HasOngoingQuest(quest) && quest && quest.Model.Abandonable)
        {
            if (HasQuestNeedAsCondition(quest.Model, out var findQuest))
            {
                //MessageManager.Instance.New($"由于任务[{bindQuest.Title}]正在进行，无法放弃该任务。");
                ConfirmWindow.StartConfirm(Tr("由于任务[{0}]正在进行，无法放弃该任务。", findQuest.Title));
            }
            else
            {
                bool isCmplt = quest.IsComplete;
                quest.InProgress = false;
                questsInProgress.Remove(quest);
                foreach (ObjectiveData o in quest.Objectives)
                {
                    o.OnStateChangeEvent -= OnObjectiveStateChange;
                    if (o is CollectObjectiveData)
                    {
                        CollectObjectiveData co = o as CollectObjectiveData;
                        co.CurrentAmount = 0;
                        co.amountWhenStart = 0;
                        BackpackManager.Instance.Inventory.OnItemAmountChanged -= co.UpdateCollectAmount;
                    }
                    if (o is KillObjectiveData ko)
                    {
                        ko.CurrentAmount = 0;
                        switch (ko.Model.KillType)
                        {
                            case KillObjectiveType.Specific:
                                GameManager.Enemies[ko.Model.Enemy.ID].ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                                break;
                            case KillObjectiveType.Race:
                                foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == ko.Model.Race))
                                {
                                    enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                                }
                                break;
                            case KillObjectiveType.Group:
                                foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && ko.Model.Group.Contains(x[0].Info.ID)))
                                {
                                    enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                                }
                                break;
                            case KillObjectiveType.Any:
                                foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                                {
                                    enemies.ForEach(e => e.OnDeathEvent -= ko.UpdateKillAmount);
                                }
                                break;
                        }
                    }
                    if (o is TalkObjectiveData to)
                    {
                        to.CurrentAmount = 0;
                        DialogueManager.Talkers[to.Model.NPCToTalk.ID].objectivesTalkToThis.RemoveAll(x => x == to);
                        DialogueManager.RemoveDialogueData(to.Model.Dialogue);
                    }
                    if (o is MoveObjectiveData mo)
                    {
                        mo.CurrentAmount = 0;
                        mo.targetPoint = null;
                        CheckPointManager.RemoveCheckPointListener(mo.Model.AuxiliaryPos, mo.UpdateMoveState);
                    }
                    if (o is SubmitObjectiveData so)
                    {
                        so.CurrentAmount = 0;
                        DialogueManager.Talkers[so.Model.NPCToSubmit.ID].objectivesSubmitToThis.RemoveAll(x => x == so);
                    }
                    if (o is TriggerObjectiveData cuo)
                    {
                        cuo.CurrentAmount = 0;
                        TriggerManager.DeleteTriggerListner(cuo.UpdateTriggerState);
                    }
                    RemoveObjectiveMapIcon(o);
                }
                if (quest.Model.NPCToSubmit)
                    quest.originalQuestHolder.TransferQuestToThis(quest);
                quest.latestHandleDays = TimeManager.Instance.Days;
                NotifyCenter.PostNotify(QuestStateChanged, quest, true);
                return true;
            }
        }
        MessageManager.Instance.New(Tr("该任务未在进行"));
        return false;
    }

    private static void OnObjectiveStateChange(ObjectiveData objective, bool befCmplt)
    {
        if (!SaveManager.Instance.IsLoading)
        {
            if (objective.CurrentAmount > 0)
            {
                string message = objective.DisplayName + (objective.IsComplete ? $"({Tr("完成")}" : $"[{objective.AmountString}]");
                MessageManager.Instance.New(message);
            }
            if (objective.parent.IsComplete) MessageManager.Instance.New($"[{Tr("任务")}]{objective.parent.Title}({Tr("已完成")})");
        }
        if (!befCmplt && objective.IsComplete)
        {
            UpdateNextCollectObjectives(objective);
            //Debug.Log("\"" + objective.DisplayName + "\"" + "从没完成变成完成");
            ObjectiveData nextToDo = null;
            QuestData quest = objective.parent;
            List<ObjectiveData> parallelObj = new List<ObjectiveData>();
            for (int i = 0; i < quest.Objectives.Count - 1; i++)
            {
                if (quest.Objectives[i] == objective)
                {
                    for (int j = i - 1; j > -1; j--)//往前找可以并行的目标
                    {
                        ObjectiveData prevObj = quest.Objectives[j];
                        if (!prevObj.Parallel) break;//只要碰到一个不能并行的，就中断
                        else parallelObj.Add(prevObj);
                    }
                    for (int j = i + 1; j < quest.Objectives.Count; j++)//往后找可以并行的目标
                    {
                        ObjectiveData nextObj = quest.Objectives[j];
                        if (!nextObj.Parallel)//只要碰到一个不能并行的，就中断
                        {
                            if (nextObj.AllPrevComplete && !nextObj.IsComplete)
                                nextToDo = nextObj;//同时，若该非并行目标的所有前置目标都完成了，那么它就是下一个要做的目标
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
        }
        //else Debug.Log("无操作");
        NotifyCenter.PostNotify(ObjectiveUpdate, objective, befCmplt);
    }
    #endregion

    #region UI相关
    private static void CreateObjectiveMapIcon(ObjectiveData objective)
    {
        if (!objective || !objective.Model.ShowMapIcon) return;
        //Debug.Log("Create icon for " + objective.DisplayName);
        if (objective is TalkObjectiveData to)
        {
            if (DialogueManager.Talkers.TryGetValue(to.Model.NPCToTalk.ID, out TalkerData talkerFound))
            {
                if (talkerFound.currentScene == ZetanUtility.ActiveScene.name)
                    CreateIcon(talkerFound.currentPosition);
            }
        }
        else if (objective is SubmitObjectiveData so)
        {
            if (DialogueManager.Talkers.TryGetValue(so.Model.NPCToSubmit.ID, out TalkerData talkerFound))
            {
                if (talkerFound.currentScene == ZetanUtility.ActiveScene.name)
                    CreateIcon(talkerFound.currentPosition);
            }
        }
        else if (objective.Model.AuxiliaryPos && objective.Model.AuxiliaryPos.Scene == ZetanUtility.ActiveScene.name)
            foreach (var position in objective.Model.AuxiliaryPos.Positions)
            {
                CreateIcon(position);
            }

        void CreateIcon(Vector3 destination)
        {
            var icon = MiscSettings.Instance.QuestIcon ? (objective is KillObjectiveData ?
                MapManager.Instance.CreateMapIcon(MiscSettings.Instance.QuestIcon, new Vector2(48, 48), destination, true, 144f, MapIconType.Objective, false, objective.DisplayName) :
                MapManager.Instance.CreateMapIcon(MiscSettings.Instance.QuestIcon, new Vector2(48, 48), destination, true, MapIconType.Objective, false, objective.DisplayName)) :
                MapManager.Instance.CreateDefaultMark(destination, true, false, objective.DisplayName);
            if (icon)
            {
                if (questIcons.TryGetValue(objective, out var iconsExist))
                {
                    iconsExist.Add(icon);
                }
                else questIcons.Add(objective, new List<MapIcon>() { icon });
            }
        }
    }
    private static void RemoveObjectiveMapIcon(ObjectiveData objective)
    {
        if (!objective) return;
        if (questIcons.TryGetValue(objective, out var icons))
        {
            foreach (var icon in icons)
            {
                MapManager.Instance.RemoveMapIcon(icon, true);
            }
            questIcons.Remove(objective);
        }
    }
    #endregion

    #region 其它
    public static bool HasOngoingQuest(QuestData quest)
    {
        return questsInProgress.Contains(quest);
    }
    public static bool HasOngoingQuest(string ID)
    {
        return questsInProgress.Exists(x => x.Model.ID == ID);
    }

    public static bool HasCompleteQuest(QuestData quest)
    {
        return questsFinished.Contains(quest);
    }
    public static bool HasCompleteQuestWithID(string questID)
    {
        return questsFinished.Exists(x => x.Model.ID == questID);
    }

    public static bool HasQuestNeedAsCondition(Quest quest, out QuestData findQuest)
    {
        findQuest = questsInProgress.Find(x => x.Model.AcceptCondition.Conditions.Exists(y => y.Type == ConditionType.AcceptQuest && y.RelatedQuest.ID == quest.ID));
        return findQuest != null;
    }

    public static QuestData FindQuest(string ID)
    {
        return questsInProgress.Find(x => x.Model.ID == ID) ?? questsFinished.Find(x => x.Model.ID == ID);
    }

    /// <summary>
    /// 更新某个收集类任务目标，用于在其他前置目标完成时，更新其后置收集类目标
    /// </summary>
    private static void UpdateNextCollectObjectives(ObjectiveData objective)
    {
        if (!objective || !objective.nextObjective) return;
        ObjectiveData nextObjective = objective.nextObjective;
        while (nextObjective != null)
        {
            if (nextObjective is not CollectObjectiveData && nextObjective.Model.InOrder && nextObjective.nextObjective != null && nextObjective.nextObjective.Model.InOrder && nextObjective.Model.Priority < nextObjective.nextObjective.Model.Priority)
            {
                //若相邻后置目标不是收集类目标，该后置目标按顺序执行，其相邻后置也按顺序执行，且两者不可同时执行，则说明无法继续更新后置的收集类目标
                return;
            }
            if (nextObjective is CollectObjectiveData co)
            {
                if (co.Model.CheckBagAtStart && !SaveManager.Instance.IsLoading) co.CurrentAmount = BackpackManager.Instance.GetAmount(co.Model.ItemToCollect);
                else if (!co.Model.CheckBagAtStart && !SaveManager.Instance.IsLoading) co.amountWhenStart = BackpackManager.Instance.GetAmount(co.Model.ItemToCollect);
            }
            nextObjective = nextObjective.nextObjective;
        }
    }

    /// <summary>
    /// 任务是否有效
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    public static bool IsQuestValid(Quest quest)
    {
        if (string.IsNullOrEmpty(quest.ID) || string.IsNullOrEmpty(quest.Title)) return false;
        if (quest.NPCToSubmit && !DialogueManager.Talkers.ContainsKey(quest.NPCToSubmit.ID)) return false;
        foreach (var obj in quest.Objectives)
            if (!obj.IsValid) return false;
        return true;
    }

    /// <summary>
    /// 判定是否有某个任务需要某数量的某个道具
    /// </summary>
    /// <param name="item">要判定的道具ID</param>
    /// <param name="leftAmount">要判定的数量</param>
    /// <returns>是否需要该道具</returns>
    public static bool HasQuestRequiredItem(Item item, int leftAmount)
    {
        return FindQuestsRequiredItem(item, leftAmount).Count() > 0;
    }
    private static IEnumerable<QuestData> FindQuestsRequiredItem(Item item, int leftAmount)
    {
        return questsInProgress.FindAll(quest =>
        {
            if (quest.Model.InOrder)
            {
                foreach (ObjectiveData o in quest.Objectives)
                {
                    //当目标是收集类目标且在提交任务同时会失去相应道具时，才进行判断
                    if (o is CollectObjectiveData co && item == co.Model.ItemToCollect && co.Model.LoseItemAtSbmt)
                    {
                        if (o.IsComplete && o.Model.InOrder)
                        {
                            //如果剩余的道具数量不足以维持该目标完成状态
                            if (o.Model.Amount > leftAmount)
                            {
                                ObjectiveData tempObj = o.nextObjective;
                                while (tempObj != null)
                                {
                                    //则判断是否有后置目标在进行，以保证在打破该目标的完成状态时，后置目标不受影响
                                    if (tempObj.CurrentAmount > 0 && tempObj.Model.Priority > o.Model.Priority)
                                    {
                                        //Debug.Log("Required");
                                        return true;
                                    }
                                    tempObj = tempObj.nextObjective;
                                }
                            }
                            //Debug.Log("NotRequired3");
                            return false;
                        }
                        //Debug.Log("NotRequired2");
                        return false;
                    }
                }
            }
            //Debug.Log("NotRequired1");
            return false;
        }).AsEnumerable();
    }

    [SaveMethod]
    public static void SaveData(SaveData saveData)
    {
        var inProgress = new SaveDataItem();
        saveData.data["questsInProgress"] = inProgress;
        foreach (QuestData quest in questsInProgress)
        {
            var qs = new SaveDataItem();
            qs.stringData["ID"] = quest.Model.ID;
            qs.stringData["giverID"] = quest.originalQuestHolder.TalkerID;
            inProgress.dataList.Add(qs);
            foreach (var obj in quest.Objectives)
            {
                qs.intData[obj.ID] = obj.CurrentAmount;
            }
        }
        var finished = new SaveDataItem();
        saveData.data["questsFinished"] = finished;
        foreach (QuestData quest in questsFinished)
        {
            var qs = new SaveDataItem();
            qs.stringData["ID"] = quest.Model.ID;
            qs.stringData["giverID"] = quest.originalQuestHolder.TalkerID;
            finished.dataList.Add(qs);
        }
    }

    [InitMethod]
    public static void Init()
    {
        foreach (var quest in questsInProgress)
            foreach (ObjectiveData o in quest.Objectives)
                RemoveObjectiveMapIcon(o);
        questsInProgress.Clear();
        questsFinished.Clear();
        NotifyCenter.AddListener(NotifyCenter.CommonKeys.TriggerChanged, OnTriggerChange);
    }

    [LoadMethod]
    public static void LoadData(SaveData saveData)
    {
        questsInProgress.Clear();
        if (saveData.data.TryGetValue("questsInProgress", out var quests))
        {
            foreach (var quest in quests.dataList)
            {
                HandlingQuestData(quest, false);
            }
        }
        questsFinished.Clear();
        if (saveData.data.TryGetValue("questsFinished", out quests))
        {
            foreach (var quest in quests.dataList)
            {
                HandlingQuestData(quest, true);
            }
        }

        static void HandlingQuestData(SaveDataItem questData, bool finished)
        {
            if (!questData.stringData.TryGetValue("giverID", out var giverID)) return;
            if (!DialogueManager.Talkers.TryGetValue(giverID, out var questGiver)) return;
            if (!questData.stringData.TryGetValue("ID", out var ID)) return;
            QuestData quest = questGiver.questInstances.Find(x => x.Model.ID == ID);
            if (!quest) return;
            AcceptQuest(quest);
            if (!finished)
                foreach (var od in questData.intData)
                {
                    foreach (ObjectiveData o in quest.Objectives)
                    {
                        if (o.ID == od.Key)
                        {
                            o.CurrentAmount = od.Value;
                            break;
                        }
                    }
                }
            else
            {
                foreach (var o in quest.Objectives)
                {
                    o.CurrentAmount = o.Model.Amount;
                }
                CompleteQuest(quest);
            }
        }
    }

    public static void OnTriggerChange(params object[] args)
    {
        //TODO 处理触发器改变时
    }

    public static void OnLoadScene()
    {
        //TODO 重新遍历对话人、敌人等来绑定对话类、杀敌类、提交类等的回调
    }
    #endregion

    #region 消息
    /// <summary>
    /// 任务更新消息，格式：([发生变化的任务：<see cref="QuestData"/>], [任务之前的进行状态：<see cref="bool"/>])
    /// </summary>
    public const string QuestStateChanged = "QuestStateChanged";
    /// <summary>
    /// 目标更新消息，格式：([发生变化的目标：<see cref="ObjectiveData"/>]，[目标之前的完成状态：<see cref="bool"/>])
    /// </summary>
    public const string ObjectiveUpdate = "ObjectiveUpdate";
    #endregion

    #region 语言相关
    public static string Tr(string text)
    {
        return LM.Tr(typeof(QuestManager).Name, text);
    }
    public static string Tr(string text, params object[] args)
    {
        return LM.Tr(typeof(QuestManager).Name, text, args);
    }
    #endregion
}