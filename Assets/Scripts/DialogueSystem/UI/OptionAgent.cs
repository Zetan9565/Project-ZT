using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OptionAgent : MonoBehaviour
{
    private Text titleText;

    public OptionType OptionType { get; private set; }

    public Quest MQuest { get; private set; }

    public TalkObjective TalkObjective { get; private set; }
    public SubmitObjective SubmitObjective { get; private set; }

    public WordsOption BranchDialogue { get; private set; }

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

    public void Init(string text, SubmitObjective objective)
    {
        titleText.text = text;
        OptionType = OptionType.Objective;
        SubmitObjective = objective;
    }

    public void Init(string text, WordsOption option)
    {
        titleText.text = text;
        if (option.OptionType == WordsOptionType.SubmitAndGet)
        {
            if (option.IsValid)
                titleText.text += string.Format("(需[{0}]{1}个)", option.ItemToSubmit.ItemName, option.ItemToSubmit.Amount);
        }
        OptionType = OptionType.Option;
        BranchDialogue = option;
    }

    public void OnClick()
    {
        switch (OptionType)
        {
            case OptionType.Quest:
                DialogueManager.Instance.StartQuestDialogue(MQuest);
                break;
            case OptionType.Objective:
                if (TalkObjective) DialogueManager.Instance.StartObjectiveDialogue(TalkObjective);
                else DialogueManager.Instance.StartObjectiveDialogue(SubmitObjective);
                break;
            case OptionType.Confirm:
                if (DialogueManager.Instance.CurrentType == DialogueType.Quest)
                    if (!DialogueManager.Instance.CurrentQuest.IsComplete && QuestManager.Instance.AcceptQuest(DialogueManager.Instance.CurrentQuest))
                        DialogueManager.Instance.GoBackDefault();
                    else if (QuestManager.Instance.CompleteQuest(DialogueManager.Instance.CurrentQuest))
                        DialogueManager.Instance.GoBackDefault();
                break;
            case OptionType.Back:
                DialogueManager.Instance.HideQuestDescription();
                DialogueManager.Instance.GoBackDefault();
                break;
            case OptionType.Continue:
                DialogueManager.Instance.SayNextWords();
                break;
            case OptionType.Option:
                DialogueManager.Instance.StartOptionDialogue(BranchDialogue);
                break;
            default:
                break;
        }
    }

    public void Recycle()
    {
        titleText.text = string.Empty;
        TalkObjective = null;
        SubmitObjective = null;
        MQuest = null;
        BranchDialogue = null;
        OptionType = OptionType.None;
        ObjectPool.Put(gameObject);
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
    Option
}
