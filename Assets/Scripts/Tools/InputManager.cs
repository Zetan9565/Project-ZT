using UnityEngine;

[DisallowMultipleComponent]
public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<InputManager>();
            return instance;
        }
    }

    public InputCustomInfo customInfo;

    void Update()
    {
#if UNITY_STANDALONE
        if (Input.GetKeyDown(customInfo.QuestWindowButton))
            QuestManager.Instance.OpenCloseWindow();
        if (Input.GetKeyDown(customInfo.BuildingButton))
            BuildingManager.Instance.OpenCloseWindow();
        if (DialogueManager.Instance.IsUIOpen && !DialogueManager.Instance.IsPausing)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                DialogueManager.Instance.OptionPageUp();
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                DialogueManager.Instance.OptionPageDown();
            }
        }
        if (Input.GetKeyDown(customInfo.BackpackButton))
        {
            BackpackManager.Instance.OpenCloseWindow();
        }
#endif
        if (Input.GetKeyDown(customInfo.InteractiveButton) || Input.GetButtonDownMobile("Interactive"))
        {
            if (DialogueManager.Instance.TalkAble)
            {
                if (!DialogueManager.Instance.IsTalking)
                    DialogueManager.Instance.BeginNewDialogue();
                else if (DialogueManager.Instance.DialogueType == DialogueType.Normal && DialogueManager.Instance.OptionsCount < 1
                    && DialogueManager.Instance.NPCHasNotAcptQuests)
                    DialogueManager.Instance.LoadTalkerQuest();
                else if (DialogueManager.Instance.OptionsCount > 0)
                    DialogueManager.Instance.FirstOption.OnClick();
            }
            else if (WarehouseManager.Instance.StoreAble)
                WarehouseManager.Instance.OpenWindow();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (WindowsManager.Instance.WindowsCount > 0) WindowsManager.Instance.CloseTop();
            else EscapeMenuManager.Instance.OpenWindow();
        }
    }
}
