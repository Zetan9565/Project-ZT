using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OptionAgent : MonoBehaviour
{
    private Text titleText;

    public OptionType OptionType { get; private set; }

    public Quest MQuest { get; private set; }

    public TalkObjective TalkObjective { get; private set; }

    public BranchDialogue BranchDialogue { get; private set; }

    private void Awake()
    {
        titleText = GetComponentInChildren<Text>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void InitContinue(string text)
    {
        titleText.text = text;
        OptionType = OptionType.Continue;
    }

    public void InitBack(string text)
    {
        titleText.text = text;
        OptionType = OptionType.Back;
    }

    public void InitConfirm(string text)
    {
        titleText.text = text;
        OptionType = OptionType.Confirm;
    }

    public void Init(string text, Quest quest)
    {
        if (!titleText)
            titleText = GetComponentInChildren<Text>();
        titleText.text = text;
        OptionType = OptionType.Quest;
        MQuest = quest;
    }

    public void Init(string text, TalkObjective objective)
    {
        titleText.text = text;
        OptionType = OptionType.Objective;
        TalkObjective = objective;
    }

    public void Init(string text, BranchDialogue branch)
    {
        titleText.text = text;
        OptionType = OptionType.Branch;
        BranchDialogue = branch;
    }

    public void OnClick()
    {
        switch (OptionType)
        {
            case OptionType.Quest:
                DialogueManager.Instance.StartQuestDialogue(MQuest);
                break;
            case OptionType.Objective:
                DialogueManager.Instance.StartObjectiveDialogue(TalkObjective);
                break;
            case OptionType.Confirm:
                if (DialogueManager.Instance.CurrentType == DialogueType.Quest)
                    if (!DialogueManager.Instance.CurrentQuest.IsComplete && QuestManager.Instance.AcceptQuest(DialogueManager.Instance.CurrentQuest))
                        DialogueManager.Instance.GotoDefault();
                    else if (QuestManager.Instance.CompleteQuest(DialogueManager.Instance.CurrentQuest))
                        DialogueManager.Instance.GotoDefault();
                break;
            case OptionType.Back:
                DialogueManager.Instance.HideQuestDescription();
                DialogueManager.Instance.GotoDefault();
                break;
            case OptionType.Continue:
                DialogueManager.Instance.SayNextWords();
                break;
            case OptionType.Branch:
                DialogueManager.Instance.StartBranchDialogue(BranchDialogue);
                break;
            default:
                break;
        }
    }

    public void Recycle()
    {
        titleText.text = string.Empty;
        TalkObjective = null;
        MQuest = null;
        BranchDialogue = null;
        OptionType = OptionType.None;
        ObjectPool.Instance.Put(gameObject);
    }
}
public enum OptionType
{
    None,
    Quest,
    Objective,
    Confirm,
    Back,
    Continue,
    Branch
}
