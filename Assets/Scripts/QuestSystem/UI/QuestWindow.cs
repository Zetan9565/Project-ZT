﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.QuestSystem.UI
{
    using ItemSystem.UI;
    using ZetanStudio;
    using ZetanStudio.UI;

    public class QuestWindow : Window, IHideable
    {
        [SerializeField]
        private QuestList questList;

        [SerializeField]
        private CanvasGroup descriptionWindow;

        [SerializeField]
        private Text title;
        //[SerializeField]
        //private Image icon;
        [SerializeField]
        private Text giver;
        [SerializeField]
        private Text descriptionText;
        [SerializeField]
        private Text objectiveText;

        [SerializeField]
        private Button abandonButton;
        [SerializeField]
        private Button traceButton;
        [SerializeField]
        private Text traceBtnText;

        [SerializeField]
        private Button desCloseButton;

        [SerializeField]
        private TabbedBar tabBar;

        [SerializeField]
        private ItemGrid rewardList;
        [SerializeField]
        private int maxRewardCount = 10;

        private int page = -1;
        private QuestData selectedQuest;
        private QuestAgent selectedAgent;
        private List<QuestAgentData> toShow = new List<QuestAgentData>();

        public bool IsHidden { get; private set; }

        protected override void OnAwake()
        {
            tabBar.Refresh(RefreshByTab);
            questList.Selectable = true;
            questList.SetItemModifier(x =>
            {
                ///因为questList的Prefab存在自我引用问题(Prefab系统的祖传BUG），需要替换为真正的Prefab而不是自我引用
                if (x.SubList.Prefab == x) x.SubList.PreplacePrefab(questList.Prefab);
                x.SubList.Selectable = true;
                x.SubList.SetSelectCallback(OnSelectQuest);
            });
            questList.SetSelectCallback(OnSelectQuest);
            abandonButton.onClick.AddListener(AbandonSelectedQuest);
            traceButton.onClick.AddListener(TraceSelectedQuest);
            desCloseButton.onClick.AddListener(() =>
            {
                ShowOrHideDescription(false);
            });
            ShowOrHideDescription(false);
        }

        private void OnSelectQuest(QuestAgent element)
        {
            if (element && element.Data.quests.Count > 0)
            {
                if (!element.Data.group)
                {
                    selectedAgent = element;
                    selectedQuest = element.Data.quests[0];
                    ShowOrHideDescription(true);
                }
            }
            else ShowOrHideDescription(false);
        }

        /// <summary>
        /// 放弃当前展示的任务
        /// </summary>
        public void AbandonSelectedQuest()
        {
            if (!selectedQuest) return;
            ConfirmWindow.StartConfirm(Tr("已消耗的道具不会退回，确定放弃此任务吗？"), delegate
            {
                if (QuestManager.AbandonQuest(selectedQuest))
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
                    toShow = Convert(QuestManager.QuestInProgress);
                    break;
                case 2:
                    toShow = Convert(QuestManager.QuestFinished);
                    break;
            }
            RefreshList();
            if (index != page) ShowOrHideDescription(false);
            page = index;
        }

        public void RefreshDescription()
        {
            title.text = selectedQuest.Title;
            giver.text = $"[{Tr("委托人")}：{selectedQuest.originalQuestHolder.TalkerName}]";
            descriptionText.text = selectedQuest.Description;
            StringBuilder objectives = new StringBuilder();
            var displayObjectives = selectedQuest.Objectives.Where(x => x.Model.Display).ToArray();
            int lineCount = displayObjectives.Length - 1;
            for (int i = 0; i < displayObjectives.Length; i++)
            {
                string endLine = i == lineCount ? string.Empty : "\n";
                if (selectedQuest.IsSubmitted)
                    objectives.AppendFormat("-{0}{1}", displayObjectives[i].DisplayName, endLine);
                else
                    objectives.AppendFormat("-{0}{1}{2}", displayObjectives[i], displayObjectives[i].IsComplete ? $" {Tr("(达成)")}" : string.Empty, endLine);
            }
            objectiveText.text = objectives.ToString();
            rewardList.Refresh(ItemSlotData.Convert(selectedQuest.Model.RewardItems, maxRewardCount));

            Utility.SetActive(abandonButton.gameObject, !selectedQuest.IsSubmitted && selectedQuest.Model.Abandonable);
            Utility.SetActive(traceButton.gameObject, !selectedQuest.IsSubmitted);
            RefreshTraceText();
        }

        private void RefreshTraceText()
        {
            if (QuestBoard.Instance.Quest == selectedQuest) traceBtnText.text = Tr("取消追踪");
            else traceBtnText.text = Tr("追踪");
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
            WindowsManager.CloseWindow<ItemWindow>();
        }

        protected override bool OnOpen(params object[] args)
        {
            tabBar.SetIndex(1);
            RefreshByTab(1);
            if (openBy is QuestBoard qb)
            {
                bool acceeser(QuestAgent agent)
                {
                    int index = agent.SubList.FindIndex(x => x.Data.quests.Contains(qb.Quest));
                    if (index > -1)
                    {
                        agent.SetSelected();
                        agent.SubList.SetSelected(index, true);
                        return true;
                    }
                    return false;
                }
                questList.ForEach(acceeser);
            }
            return true;
        }

        protected override bool OnClose(params object[] args)
        {
            toShow = null;
            questList.ClearSelection();
            return true;
        }

        private void RefreshList()
        {
            questList.Refresh(toShow);
        }

        private static List<QuestAgentData> Convert(IList<QuestData> origin)
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
            NotifyCenter.AddListener(QuestManager.QuestAcceptStateChanged, OnQuestStateChanged, this);
            NotifyCenter.AddListener(QuestManager.ObjectiveStateUpdate, OnObjectiveUpdate, this);
        }

        private void OnQuestStateChanged(params object[] msg)
        {
            if (IsOpen && msg.Length > 0 && msg[0] is QuestData)
            {
                RefreshByTab(tabBar.SelectedIndex);
            }
        }
        private void OnObjectiveUpdate(params object[] msg)
        {
            if (IsOpen && msg.Length > 1 && msg[0] is ObjectiveData objective)
            {
                questList.RefreshItemIf(x => x.Data.quests.Contains(objective.parent));
                if (objective.parent == selectedQuest) RefreshDescription();
            }
        }

        public void Hide(bool hide, params object[] args)
        {
            if (!IsOpen) return;
            IHideable.HideHelper(content, hide);
            IsHidden = hide;
        }
    }
}