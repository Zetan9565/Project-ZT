using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OptionAgent : MonoBehaviour
{
    public Text TitleText;

#if UNITY_EDITOR
    [ReadOnly]
#endif
    public OptionType optionType;

    [HideInInspector]
    public Quest MQuest;

    [HideInInspector]
    public TalkObjective talkObjective;

    public void OnClick()
    {
        switch (optionType)
        {
            case OptionType.Quest:
                DialogueManager.Instance.StartQuestDialogue(MQuest);
                break;
            case OptionType.Objective:
                DialogueManager.Instance.StartObjectiveDialogue(talkObjective);
                break;
            case OptionType.Confirm:
                if (DialogueManager.Instance.DialogueType == DialogueType.Quest)
                    if (!MQuest.IsComplete && QuestManager.Instance.AcceptQuest(MQuest))
                    {
                        DialogueManager.Instance.GotoDefault();
                    }
                    else if (QuestManager.Instance.CompleteQuest(MQuest))
                    {
                        DialogueManager.Instance.GotoDefault();
                    }
                break;
            case OptionType.Back:
                DialogueManager.Instance.CloseQuestDescriptionWindow();
                DialogueManager.Instance.GotoDefault();
                break;
            case OptionType.Continue:
                DialogueManager.Instance.SayNextWords();
                break;
            default:
                break;
        }
    }
}
public enum OptionType
{
    None,
    Quest,
    Objective,
    Confirm,
    Back,
    Continue
}
