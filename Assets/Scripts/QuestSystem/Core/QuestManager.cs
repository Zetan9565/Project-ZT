using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestManager : MonoBehaviour, IOpenCloseable
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
        if (!quest) return false;
        if (HasQuest(quest)) return false;
        QuestAgent qa;
        if (quest.Group)
        {
            QuestGroupAgent qga = questGroupAgents.Find(x => x.questGroup == quest.Group);
            if (qga)
            {
                qa = ObjectPool.Instance.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qga.questAgents.Add(qa);
                qga.UpdateStatus();
            }
            else
            {
                qga = ObjectPool.Instance.Get(UI.questGroupPrefab, UI.questListParent).GetComponent<QuestGroupAgent>();
                qga.questGroup = quest.Group;
                qga.Expand(true);
                questGroupAgents.Add(qga);

                qa = ObjectPool.Instance.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qga.questAgents.Add(qa);
                qga.UpdateStatus();
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
                    if (co.CheckBagAtAcpt && !loadMode) co.UpdateCollectAmountUp(co.Item, BackpackManager.Instance.GetItemAmountByID(co.Item.ID));
                }
                catch
                {
                    BackpackManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BackpackManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                    Debug.LogWarning("[尝试使用null道具] 任务名称：" + quest.Title);
                    continue;
                }
            }
            else if (o is KillObjective)
            {
                KillObjective ko = o as KillObjective;
                try
                {
                    foreach (Enemy enemy in GameManager.Instance.AllEnermy[ko.Enemy.ID])
                        enemy.OnDeathEvent += ko.UpdateKillAmount;
                }
                catch
                {
                    Debug.LogWarningFormat("[找不到敌人] ID: {0}", ko.Enemy.ID);
                    continue;
                }
            }
            else if (o is TalkObjective)
            {
                TalkObjective to = o as TalkObjective;
                try
                {
                    if (!o.IsComplete) GameManager.Instance.AllTalker[to.Talker.ID].objectivesTalkToThis.Add(to);
                }
                catch
                {
                    Debug.LogWarningFormat("[找不到NPC] ID: {0}", to.Talker.ID);
                    continue;
                }
            }
            else if (o is MoveObjective)
            {
                MoveObjective mo = o as MoveObjective;
                try
                {
                    GameManager.Instance.AllQuestPoint[mo.PointID].OnMoveIntoEvent += mo.UpdateMoveStatus;
                }
                catch
                {
                    Debug.LogWarningFormat("[找不到任务点] ID: {0}", mo.PointID);
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
                (GameManager.Instance.AllTalker[quest.NPCToSubmit.ID] as QuestGiver).TransferQuestToThis(quest);
            }
            catch
            {
                Debug.LogWarningFormat("[找不到NPC] ID: {0}", quest.NPCToSubmit);
            }
        }
        if(!loadMode)
            MessageManager.Instance.NewMessage("接取了任务 [" + quest.Title + "]");
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
                    foreach (Enemy enemy in GameManager.Instance.AllEnermy[ko.Enemy.ID])
                    {
                        enemy.OnDeathEvent -= ko.UpdateKillAmount;
                    }
                }
                if (o is TalkObjective)
                {
                    TalkObjective to = o as TalkObjective;
                    to.CurrentAmount = 0;
                    GameManager.Instance.AllTalker[to.Talker.ID].objectivesTalkToThis.RemoveAll(x => x == to);
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    mo.CurrentAmount = 0;
                    GameManager.Instance.AllQuestPoint[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveStatus;
                }
            }
            if (!quest.SbmtOnOriginalNPC)
            {
                quest.OriginalQuestGiver.TransferQuestToThis(quest);
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
        if (AbandonQuest(SelectedQuest))
        {
            RemoveQuestAgentByQuest(SelectedQuest);
            CloseDescriptionWindow();
        }
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
                    questsReqThisQuestItem = BackpackManager.Instance.QuestsRequiredItem(co.Item,
                        BackpackManager.Instance.GetItemAmountByItem(co.Item) - o.Amount).ToList();
                }
                if (questsReqThisQuestItem.Contains(quest) && questsReqThisQuestItem.Count > 1)
                {
                    MessageManager.Instance.NewMessage("其他任务对该任务需提交物品有需求");
                    return false;
                }
            }
        }
        if (HasQuest(quest) && quest.IsComplete)
        {
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
                    cqga.questAgents.Add(cqa);
                    cqga.UpdateStatus();
                }
                else
                {
                    cqga = ObjectPool.Instance.Get(UI.questGroupPrefab, UI.cmpltQuestListParent).GetComponent<QuestGroupAgent>();
                    cqga.questGroup = quest.Group;
                    cqga.Expand(true);
                    cmpltQuestGroupAgents.Add(cqga);

                    cqa = ObjectPool.Instance.Get(UI.questPrefab, cqga.questListParent).GetComponent<QuestAgent>();
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
                    foreach (Enemy enermy in GameManager.Instance.AllEnermy[(o as KillObjective).Enemy.ID])
                    {
                        enermy.OnDeathEvent -= (o as KillObjective).UpdateKillAmount;
                    }
                }
                if (o is TalkObjective)
                {
                    GameManager.Instance.AllTalker[(o as TalkObjective).Talker.ID].objectivesTalkToThis.RemoveAll(x => x == (o as TalkObjective));
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    GameManager.Instance.AllQuestPoint[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveStatus;
                }
            }
            if (!loadMode)
            {
                //TODO 经验和金钱的处理
                foreach (ItemInfo info in quest.RewardItems)
                {
                    BackpackManager.Instance.GetItem(info);
                }
                MessageManager.Instance.NewMessage("提交了任务 [" + quest.Title + "]");
            }
            CloseDescriptionWindow();
            return true;
        }
        return false;
    }
    #endregion

    #region UI相关
    public void InitDescription(QuestAgent questAgent)
    {
        if (!questAgent.MQuest) return;
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
                if (rwc.itemInfo == null)
                {
                    rwc.Init(info);
                    break;
                }
            }
        MyTools.SetActive(UI.abandonButton.gameObject, questAgent.MQuest.IsFinished ? false : questAgent.MQuest.Abandonable);
    }

    public void UpdateUI()
    {
        foreach (QuestAgent qa in questAgents)
            qa.UpdateStatus();
        foreach (QuestBoardAgent qba in questBoardAgents)
            qba.UpdateStatus();
        foreach (QuestGroupAgent qga in questGroupAgents)
            qga.UpdateStatus();
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
            UI.descriptionText.text = string.Format("<size=16><b>{0}</b></size>\n[委托人: {1}]\n{2}\n\n<size=16><b>任务目标</b></size>\n{3}",
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
            UI.descriptionText.text = string.Format("<size=16><b>{0}</b></size>\n[委托人: {1}]\n{2}\n\n<size=16><b>任务目标{3}</b></size>\n{4}",
                                   SelectedQuest.Title,
                                   SelectedQuest.OriginalQuestGiver.TalkerName,
                                   SelectedQuest.Description,
                                   SelectedQuest.IsComplete ? "(完成)" : SelectedQuest.IsOngoing ? "(进行中)" : string.Empty,
                                   objectives);
        }
    }

    public void CloseDescriptionWindow()
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
    public void OpenDescriptionWindow(QuestAgent questAgent)
    {
        DialogueManager.Instance.CloseQuestDescriptionWindow();
        InitDescription(questAgent);
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
    }

    public void CloseUI()
    {
        if (IsPausing) return;
        UI.questsWindow.alpha = 0;
        UI.questsWindow.blocksRaycasts = false;
        CloseDescriptionWindow();
        IsUIOpen = false;
    }
    public void OpenUI()
    {
        if (IsPausing) return;
        UI.questsWindow.alpha = 1;
        UI.questsWindow.blocksRaycasts = true;
        DialogueManager.Instance.CloseQuestDescriptionWindow();
        WindowsManager.Instance.PushWindow(this);
        IsUIOpen = true;
    }
    public void OpenCloseUI()
    {
        if (UI.questsWindow.alpha == 0)
            OpenUI();
        else CloseUI();
    }

    public void PauseDisplay(bool state)
    {
        if (!IsUIOpen) return;
        if (!state)
        {
            UI.questsWindow.alpha = 1;
            UI.questsWindow.blocksRaycasts = true;
            UI.descriptionWindow.alpha = 1;
            UI.descriptionWindow.blocksRaycasts = true;
        }
        else
        {
            UI.questsWindow.alpha = 0;
            UI.questsWindow.blocksRaycasts = false;
            UI.descriptionWindow.alpha = 0;
            UI.descriptionWindow.blocksRaycasts = false;
        }
        IsPausing = state;
    }

    public bool IsPausing { get; private set; }
    public bool IsUIOpen { get; private set; }

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

    public void SetUI(QuestUI UI)
    {
        if (!UI) return;
        this.UI = UI;
    }
    #endregion
}
