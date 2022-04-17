using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestBoard : SingletonMonoBehaviour<QuestBoard>, IPointerClickHandler
{
    [SerializeField, DisplayName("标题文字")]
    private Text titleText;

    [SerializeField, DisplayName("目标文字")]
    private Text objectiveText;

    public QuestData Quest { get; private set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) NewWindowsManager.OpenWindowBy<QuestWindow>(this);
    }

    public void FocusOnQuest(QuestData quest)
    {
        Quest = quest;
        Refresh();
    }

    public void Defocus()
    {
        Quest = null;
        titleText.text = string.Empty;
        objectiveText.text = string.Empty;
        ZetanUtility.SetActive(gameObject, false);
    }

    public void Refresh()
    {
        if (!Quest || Quest.IsFinished)
        {
            Defocus();
            return;
        }
        ZetanUtility.SetActive(gameObject, true);
        titleText.text = Quest.Model.Title + (Quest.IsComplete ? "(完成)" : string.Empty);
        StringBuilder objectives = new StringBuilder();
        if (Quest.IsComplete) objectiveText.text = string.Empty;
        else
        {
            for (int i = 0; i < Quest.Objectives.Count; i++)
            {
                var objective = Quest.Objectives[i];
                if (objective.Model.Display && !objective.IsComplete)
                {
                    objectives.Append('-');
                    objectives.Append(objective.Model.DisplayName);
                    if (objective is not TalkObjectiveData && objective is not MoveObjectiveData)
                    {
                        objectives.Append('[');
                        objectives.Append(objective.AmountString);
                        objectives.Append(']');
                    }
                    if (i + 1 >= Quest.Objectives.Count || !Quest.Objectives[i + 1].CanParallelWith(objective)) break;
                    if (i < Quest.Objectives.Count - 1) objectives.Append('\n');
                }
            }
        }
        objectiveText.text = objectives.ToString();
    }

    private void Start()
    {
        NotifyCenter.AddListener(QuestManager.QuestStateChanged, OnQuestStateChanged, this);
        NotifyCenter.AddListener(QuestManager.ObjectiveUpdate, OnObjectiveUpdate, this);
        Defocus();
    }

    private void OnQuestStateChanged(object[] msg)
    {
        if (msg.Length > 0 && msg[0] is QuestData quest && msg[1] is bool ipBef)
            if (Quest == null && quest.InProgress && !ipBef)//任务栏没有在显示的任务，这是新接取的任务
                FocusOnQuest(quest);
            else if (!quest.InProgress && ipBef)//任务已提交或放弃
                Defocus();
    }

    private void OnDestroy()
    {
        NotifyCenter.RemoveListener(this);
    }

    private void OnObjectiveUpdate(object[] msg)
    {
        if (msg.Length > 0 && msg[0] is QuestData quest && quest == Quest) Refresh();
    }
}