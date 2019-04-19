using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public InputCustomInfo customInfo;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(customInfo.QuestWindowButton))
            QuestManager.Instance.OpenCloseUI();
        if (Input.GetKeyDown(customInfo.InteractiveButton))
        {
            if (DialogueManager.Instance.TalkAble)
            {
                if (!DialogueManager.Instance.IsTalking)
                    DialogueManager.Instance.BeginNewDialogue();
                else if (DialogueManager.Instance.DialogueType == DialogueType.Normal && DialogueManager.Instance.OptionAgents.Count < 1 && DialogueManager.Instance.HasNotAcptQuests)
                {
                    DialogueManager.Instance.LoadTalkerQuest();
                }
                else if (DialogueManager.Instance.OptionAgents.Count > 0)
                    DialogueManager.Instance.OptionAgents[0].OnClick();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DialogueManager.Instance.IsTalking)
                DialogueManager.Instance.CloseDialogueWindow();
            WindowsManager.Instance.CloseTopWindow();
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (DialogueManager.Instance.IsTalking)
                DialogueManager.Instance.OptionPageUp();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (DialogueManager.Instance.IsTalking)
                DialogueManager.Instance.OptionPageDown();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            BackpackManager.Instance.OpenCloseUI();
        }
    }
}
