using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestBoardAgent : MonoBehaviour
{
    [HideInInspector]
    public QuestAgent questAgent;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("标题文字")]
#endif
    private Text TitleText;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("目标文字")]
#endif
    private Text ObjectiveText;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("已完成目标的颜色")]
#endif
    private Color cmpltObjectv = Color.grey;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("目标全部完成时的颜色")]
#endif
    private Color cmpltAllObj = Color.yellow;

    public void UpdateStatus()
    {
        if (questAgent.MQuest) TitleText.text = questAgent.MQuest.Title + (questAgent.MQuest.IsComplete ? "(完成)" : string.Empty);
        string objectives = string.Empty;
        if (questAgent.MQuest.IsComplete)
        {
            objectives = "<color=#" + ColorUtility.ToHtmlStringRGB(cmpltAllObj) + ">-已达成所有目标</color>";
            transform.SetAsFirstSibling();//优先显示完成任务
        }
        else
        {
            for (int i = 0; i < questAgent.MQuest.Objectives.Count; i++)
            {
                bool isCmplt = questAgent.MQuest.Objectives[i].IsComplete;
                string endLine = i == questAgent.MQuest.Objectives.Count - 1 ? string.Empty : "\n";
                objectives += (isCmplt ? "<color=#" + ColorUtility.ToHtmlStringRGB(cmpltObjectv) + ">" : string.Empty) + "-" + questAgent.MQuest.Objectives[i].DisplayName +
                              (isCmplt ? "(达成)</color>" + endLine :
                              "[" + questAgent.MQuest.Objectives[i].CurrentAmount + "/" + questAgent.MQuest.Objectives[i].Amount + "]" + endLine);
            }
        }
        ObjectiveText.text = objectives;
    }

    public void OnClick()
    {
        QuestManager.Instance.OpenUI();
        questAgent.OnClick();
    }

    public void Init(QuestAgent qa)
    {
        questAgent = qa;
        UpdateStatus();
    }
}
