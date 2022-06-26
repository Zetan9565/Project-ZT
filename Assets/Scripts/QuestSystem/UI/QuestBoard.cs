using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestBoard : SingletonWindow<QuestBoard>, IPointerClickHandler
{
    [SerializeField, Label("标题文字")]
    private Text titleText;

    [SerializeField, Label("目标文字")]
    private Text objectiveText;

    public QuestData Quest { get; private set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) WindowsManager.OpenWindowBy<QuestWindow>(this);
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

    protected override bool OnOpen(params object[] args)
    {
        return false;
    }
    protected override bool OnClose(params object[] args)
    {
        return false;
    }

    public void Refresh()
    {
        if (!Quest || Quest.IsFinished)
        {
            Defocus();
            return;
        }
        ZetanUtility.SetActive(gameObject, true);
        titleText.text = $"{(Quest.IsComplete ? $"{Tr("[完成]")}" : string.Empty)}{Quest.Title}";
        StringBuilder objectives = new StringBuilder();
        if (Quest.IsComplete) objectiveText.text = string.Empty;
        else
        {
            var displayObjectives = Quest.Objectives.Where(x => x.Model.Display).ToArray();
            int lineCount = displayObjectives.Length - 1;
            for (int i = 0; i < displayObjectives.Length; i++)
            {
                var objective = displayObjectives[i];
                if (!objective.IsComplete && (!objective.Model.InOrder || objective.AllPrevComplete))
                {
                    string endLine = i == lineCount ? string.Empty : "\n";
                    objectives.AppendFormat("-{0}{1}", objective, endLine);
                }
            }
        }
        objectiveText.text = objectives.ToString();
    }

    protected override void OnAwake()
    {
        Defocus();
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(QuestManager.QuestStateChanged, OnQuestStateChanged, this);
        NotifyCenter.AddListener(QuestManager.ObjectiveUpdate, OnObjectiveUpdate, this);
    }

    private void OnQuestStateChanged(object[] msg)
    {
        if (msg.Length > 0 && msg[0] is QuestData quest && msg[1] is bool ipBef)
            if (Quest == null && quest.InProgress && !ipBef)//任务栏没有在显示的任务，这是新接取的任务
                FocusOnQuest(quest);
            else if (Quest == quest && !quest.InProgress && ipBef)//任务已提交或放弃
                Defocus();
    }

    private void OnObjectiveUpdate(object[] msg)
    {
        if (msg.Length > 0 && msg[0] is ObjectiveData objective && objective.parent == Quest) Refresh();
    }
}