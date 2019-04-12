using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestManager : MonoBehaviour
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
                qga.UpdateGroupStatus();
            }
            else
            {
                qga = ObjectPool.Instance.Get(UI.questGroupPrefab, UI.questListParent).GetComponent<QuestGroupAgent>();
                qga.questGroup = quest.Group;
                qga.Expand(true);
                questGroupAgents.Add(qga);

                qa = ObjectPool.Instance.Get(UI.questPrefab, qga.questListParent).GetComponent<QuestAgent>();
                qga.questAgents.Add(qa);
                qga.UpdateGroupStatus();
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
                    BagManager.Instance.OnGetItemEvent += co.UpdateCollectAmountUp;
                    BagManager.Instance.OnLoseItemEvent += co.UpdateCollectAmountDown;
                    if (co.CheckBagAtAcpt && !loadMode) co.UpdateCollectAmountUp(co.Item.ID, BagManager.Instance.GetItemAmountByID(co.Item.ID));
                }
                catch
                {
                    BagManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BagManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
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
                    if (!o.IsComplete) GameManager.Instance.AllTalker[to.Talker.ID].talkToThisObjectives.Add(to);
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
                    BagManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BagManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
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
                    GameManager.Instance.AllTalker[to.Talker.ID].talkToThisObjectives.RemoveAll(x => x == to);
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
                    cqga.UpdateGroupStatus();
                }
                else
                {
                    cqga = ObjectPool.Instance.Get(UI.questGroupPrefab, UI.cmpltQuestListParent).GetComponent<QuestGroupAgent>();
                    cqga.questGroup = quest.Group;
                    cqga.Expand(true);
                    cmpltQuestGroupAgents.Add(cqga);

                    cqa = ObjectPool.Instance.Get(UI.questPrefab, cqga.questListParent).GetComponent<QuestAgent>();
                    cqga.questAgents.Add(cqa);
                    cqga.UpdateGroupStatus();
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
                    BagManager.Instance.OnGetItemEvent -= co.UpdateCollectAmountUp;
                    BagManager.Instance.OnLoseItemEvent -= co.UpdateCollectAmountDown;
                    if (!loadMode && co.LoseItemAtSbmt) BagManager.Instance.LoseItemByID(co.Item.ID, o.Amount);
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
                    GameManager.Instance.AllTalker[(o as TalkObjective).Talker.ID].talkToThisObjectives.RemoveAll(x => x == (o as TalkObjective));
                }
                if (o is MoveObjective)
                {
                    MoveObjective mo = o as MoveObjective;
                    GameManager.Instance.AllQuestPoint[mo.PointID].OnMoveIntoEvent -= mo.UpdateMoveStatus;
                }
            }
            if (!loadMode)
                foreach (ItemInfo info in quest.RewardItems)
                {
                    BagManager.Instance.GetItem(info.Item);
                }
            //TODO 经验和金钱的处理
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
        UpdateObjectivesUI();
        UI.money_EXPText.text = string.Format("[奖励]\n<size=14>经验:\n{0}\n金币:\n{1}</size>",
            questAgent.MQuest.RewardEXP > 0 ? questAgent.MQuest.RewardEXP.ToString() : "无",
            questAgent.MQuest.RewardMoney > 0 ? questAgent.MQuest.RewardMoney.ToString() : "无");
        foreach (ItemAgent rwc in UI.rewardCells)
        {
            rwc.Item = null;
            rwc.Icon.overrideSprite = null;
        }
        foreach (ItemInfo info in questAgent.MQuest.RewardItems)
            foreach (ItemAgent rwc in UI.rewardCells)
            {
                if (rwc.Item == null)
                {
                    rwc.Item = info.Item;
                    rwc.Icon.overrideSprite = info.Item.Icon;
                    break;
                }
            }
        MyTools.SetActive(UI.abandonButton.gameObject, questAgent.MQuest.IsFinished ? false : questAgent.MQuest.Abandonable);
    }

    public void UpdateObjectivesUI()
    {
        foreach (QuestAgent qa in questAgents)
            qa.UpdateQuestStatus();
        foreach (QuestBoardAgent qba in questBoardAgents)
            qba.UpdateQuestStatus();
        foreach (QuestGroupAgent qga in questGroupAgents)
            qga.UpdateGroupStatus();
        if (SelectedQuest == null) return;
        string objectives = string.Empty;
        QuestAgent cqa = completeQuestAgents.Find(x => x.MQuest == SelectedQuest);
        if (cqa)
        {
            for (int i = 0; i < SelectedQuest.Objectives.Count; i++)
            {
                string endLine = i == SelectedQuest.Objectives.Count - 1 ? string.Empty : "\n";
                objectives += SelectedQuest.Objectives[i].DisplayName + endLine;
            }
            UI.descriptionText.text = string.Format("<size=16><b>{0}</b></size>\n[委托人: {1}]\n{2}\n\n<size=16><b>任务目标</b></size>\n{3}",
                                   SelectedQuest.Title,
                                   SelectedQuest.OriginalQuestGiver.Info.Name,
                                   SelectedQuest.Description,
                                   objectives);
        }
        else
        {
            for (int i = 0; i < SelectedQuest.Objectives.Count; i++)
            {
                string endLine = i == SelectedQuest.Objectives.Count - 1 ? string.Empty : "\n";
                objectives += SelectedQuest.Objectives[i].DisplayName +
                              "[" + SelectedQuest.Objectives[i].CurrentAmount + "/" + SelectedQuest.Objectives[i].Amount + "]" +
                              (SelectedQuest.Objectives[i].IsComplete ? "(达成)" + endLine : endLine);
            }
            UI.descriptionText.text = string.Format("<size=16><b>{0}</b></size>\n[委托人: {1}]\n{2}\n\n<size=16><b>任务目标{3}</b></size>\n{4}",
                                   SelectedQuest.Title,
                                   SelectedQuest.OriginalQuestGiver.Info.Name,
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

    public void CloseQuestWindow()
    {
        UI.questsWindow.alpha = 0;
        UI.questsWindow.blocksRaycasts = false;
        CloseDescriptionWindow();
    }
    public void OpenQuestWindow()
    {
        UI.questsWindow.alpha = 1;
        UI.questsWindow.blocksRaycasts = true;
        DialogueManager.Instance.CloseQuestDescriptionWindow();
    }
    public void SwitchQuestWindow()
    {
        if (UI.questsWindow.alpha == 0)
            OpenQuestWindow();
        else CloseQuestWindow();
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
            qa.Recycle();
        }
    }

    public void SetUI(QuestUI UI)
    {
        if (!UI) return;
        this.UI = UI;
    }
    #endregion
}
