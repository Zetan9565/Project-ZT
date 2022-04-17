using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class QuestWindow : Window
{
    public QuestList questList;

    public CanvasGroup descriptionWindow;

    public Text title;
    //public Image icon;
    public Text giver;
    public Text descriptionText;
    public Text objectiveText;

    public Button abandonButton;
    public Button traceButton;
    public Text traceBtnText;

    public Button desCloseButton;

    public Text moneyText;

    public Text EXPText;

    [SerializeField]
    private TabBar tabBar;

    [SerializeField]
    private ItemGrid rewardList;
    [SerializeField]
    private int maxRewardCount = 10;

    private int page = -1;
    private QuestData selectedQuest;
    private QuestAgent selectedAgent;
    private List<QuestAgentData> toShow = new List<QuestAgentData>();

    protected override void OnAwake()
    {
        tabBar.Refresh(RefreshByTab);
        questList.Selectable = true;
        questList.SetItemModifier(x =>
        {
            x.SubList.Selectable = true;
            x.SubList.SetSelectCallback(OnSelect);
        });
        questList.SetSelectCallback(OnSelect);
        abandonButton.onClick.AddListener(AbandonSelectedQuest);
        traceButton.onClick.AddListener(TraceSelectedQuest);
        desCloseButton.onClick.AddListener(() =>
        {
            ShowOrHideDescription(false);
        });
        ShowOrHideDescription(false);
    }

    private void OnSelect(QuestAgent element)
    {
        if (!element.Data.group && element.Data.quests.Count > 0)
        {
            if (element.IsSelected)
            {
                selectedAgent = element;
                selectedQuest = element.Data.quests[0];
                ShowOrHideDescription(true);
            }
            else if (element.Data.quests[0] == selectedQuest)
            {
                ShowOrHideDescription(false);
            }
        }
    }

    /// <summary>
    /// 放弃当前展示的任务
    /// </summary>
    public void AbandonSelectedQuest()
    {
        if (!selectedQuest) return;
        ConfirmWindow.StartConfirm("已消耗的道具不会退回，确定放弃此任务吗？", delegate
        {
            if (QuestManager.Instance.AbandonQuest(selectedQuest))
            {
                ShowOrHideDescription(false);
            }
        });
    }
    public void TraceSelectedQuest()
    {
        if (QuestBoard.Instance.Quest == selectedQuest) QuestBoard.Instance.Defocus();
        else QuestBoard.Instance.FocusOnQuest(selectedQuest);
        RefreshTraceText();
    }

    private void RefreshByTab(int index)
    {
        switch (index)
        {
            case 1:
                toShow = Convert(QuestManager.Instance.GetInProgressQuests());
                break;
            case 2:
                toShow = Convert(QuestManager.Instance.GetFinishedQuests());
                break;
        }
        RefreshList();
        if (index != page) ShowOrHideDescription(false);
        page = index;
    }

    public void RefreshDescription()
    {
        title.text = selectedQuest.Model.Title;
        giver.text = $"[委托人：{selectedQuest.originalQuestHolder.TalkerName}]";
        descriptionText.text = selectedQuest.Model.Description;
        StringBuilder objectives = new StringBuilder();
        List<ObjectiveData> displayObjectives = selectedQuest.Objectives.FindAll(x => x.Model.Display);
        int lineCount = displayObjectives.Count - 1;
        for (int i = 0; i < displayObjectives.Count; i++)
        {
            string endLine = i == lineCount ? string.Empty : "\n";
            if (selectedQuest.IsFinished)
                objectives.AppendFormat("-{0}{1}", displayObjectives[i].Model.DisplayName, endLine);
            else
                objectives.AppendFormat("-{0}{1}{2}", displayObjectives[i].Model.DisplayName, "[" + displayObjectives[i].CurrentAmount + "/" + displayObjectives[i].Model.Amount + "]", endLine);
        }
        objectiveText.text = objectives.ToString();
        rewardList.Refresh(ItemSlotData.Convert(selectedQuest.Model.RewardItems, maxRewardCount));
        moneyText.text = selectedQuest.Model.RewardMoney > 0 ? selectedQuest.Model.RewardMoney.ToString() : "无";
        EXPText.text = selectedQuest.Model.RewardEXP > 0 ? selectedQuest.Model.RewardEXP.ToString() : "无";

        ZetanUtility.SetActive(abandonButton.gameObject, !selectedQuest.IsFinished && selectedQuest.Model.Abandonable);
        ZetanUtility.SetActive(traceButton.gameObject, !selectedQuest.IsFinished);
        RefreshTraceText();
    }

    private void RefreshTraceText()
    {
        if (QuestBoard.Instance.Quest == selectedQuest) traceBtnText.text = "取消追踪";
        else traceBtnText.text = "追踪";
    }

    private void ShowOrHideDescription(bool show)
    {
        if (!show)
        {
            if (selectedAgent) selectedAgent.IsSelected = false;
            selectedAgent = null;
            selectedQuest = null;
            descriptionWindow.alpha = 0;
            descriptionWindow.blocksRaycasts = false;
        }
        else
        {
            RefreshDescription();
            descriptionWindow.alpha = 1;
            descriptionWindow.blocksRaycasts = true;
        }
        NewWindowsManager.CloseWindow<ItemWindow>();
    }

    protected override bool OnOpen(params object[] args)
    {
        tabBar.SetIndex(1);
        RefreshByTab(1);
        if (openBy is QuestBoard qb)
        {
            bool acceeser(QuestAgent agent)
            {
                int position = agent.SubList.FindPosition(x => x == qb.Quest);
                if (position > 0)
                {
                    questList.SetSelected(agent.Position, true);
                    agent.SubList.SetSelected(position, true);
                    return true;
                }
                return false;
            }
            questList.ForEachWithBreak(acceeser);
        }
        return true;
    }

    protected override bool OnClose(params object[] args)
    {
        toShow = null;
        questList.DeselectAll();
        return true;
    }

    private void RefreshList()
    {
        questList.Refresh(toShow);
    }

    private static List<QuestAgentData> Convert(List<QuestData> origin)
    {
        List<QuestAgentData> results = new List<QuestAgentData>();
        Dictionary<QuestGroup, QuestAgentData> questMap = new Dictionary<QuestGroup, QuestAgentData>();
        foreach (var quest in origin)
        {
            if (quest.Model.Group)
            {
                if (questMap.TryGetValue(quest.Model.Group, out var find))
                {
                    find.quests.Add(quest);
                }
                else
                {
                    find = new QuestAgentData(quest.Model.Group, new QuestData[] { quest });
                    questMap[quest.Model.Group] = find;
                    results.Add(find);
                }
            }
            else
            {
                results.Add(new QuestAgentData(quest));
            }
        }
        return results;
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(QuestManager.QuestStateChanged, OnQuestStateChanged, this);
        NotifyCenter.AddListener(QuestManager.ObjectiveUpdate, OnObjectiveUpdate, this);
    }

    private void OnQuestStateChanged(params object[] msg)
    {
        if (IsOpen && msg.Length > 0 && msg[0] is QuestData)
        {
            RefreshByTab(tabBar.TabIndex);
        }
    }
    private void OnObjectiveUpdate(params object[] msg)
    {
        if (IsOpen && msg.Length > 2 && msg[0] is QuestData quest && quest == selectedQuest)
            RefreshDescription();
    }
}