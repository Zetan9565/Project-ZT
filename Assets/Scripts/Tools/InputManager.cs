using UnityEngine;

public class InputManager : MonoBehaviour
{
    public InputCustomInfo customInfo;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(customInfo.QuestWindowButton))
            QuestManager.Instance.SwitchQuestWindow();
        if (Input.GetKeyDown(customInfo.TalkButton))
        {
            if (DialogueManager.Instance.TalkAble)
            {
                if (!DialogueManager.Instance.IsTalking)
                    DialogueManager.Instance.BeginNewDialogue();
                else if (DialogueManager.Instance.OptionAgents.Count > 0)
                    DialogueManager.Instance.OptionAgents[0].OnClick();
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (DialogueManager.Instance.IsTalking)
                DialogueManager.Instance.GotoDefault();
        }
    }
}
