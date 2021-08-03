using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/输入管理器")]
public class InputManager : SingletonMonoBehaviour<InputManager>
{
    public InputCustomInfo customInfo;

    public static bool IsTyping => BackpackManager.Instance && BackpackManager.Instance.IsInputFocused ||
        PlantManager.Instance && PlantManager.Instance.IsInputFocused ||
        WarehouseManager.Instance && WarehouseManager.Instance.IsInputFocused;

    void Update()
    {
        if (IsTyping) return;
        //#if UNITY_STANDALONE
        if (Input.GetKeyDown(customInfo.QuestWindowButton))
            QuestManager.Instance.OpenCloseWindow();
        if (Input.GetKeyDown(customInfo.BuildingButton))
            BuildingManager.Instance.OpenCloseWindow();
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (DialogueManager.Instance.IsUIOpen && !DialogueManager.Instance.IsPausing) DialogueManager.Instance.OptionPageUp();
            else if (InteractionManager.Instance.ScrollAble) InteractionManager.Instance.Up();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (DialogueManager.Instance.IsUIOpen && !DialogueManager.Instance.IsPausing) DialogueManager.Instance.OptionPageDown();
            else if (InteractionManager.Instance.ScrollAble) InteractionManager.Instance.Down();
        }
        if (Input.GetKeyDown(customInfo.BackpackButton))
        {
            BackpackManager.Instance.OpenCloseWindow();
        }
        if (Input.GetKeyDown(customInfo.TraceButton))
        {
            PlayerManager.Instance.PlayerController.Trace();
        }
        //#endif
        if (Input.GetKeyDown(customInfo.InteractiveButton) || Input.GetButtonDownMobile("Interactive"))
        {
            if (LootManager.Instance.IsPicking)
                LootManager.Instance.TakeAll();
            else if (DialogueManager.Instance.IsTalking)
            {
                if (DialogueManager.Instance.CurrentType == DialogueType.Normal && DialogueManager.Instance.OptionsCount < 1
                      && DialogueManager.Instance.ShouldShowQuest)
                    DialogueManager.Instance.ShowTalkerQuest();
                else if (DialogueManager.Instance.OptionsCount > 0)
                    DialogueManager.Instance.FirstOption.OnClick();
            }
            else InteractionManager.Instance.DoSelectInteract();
        }
        if (Input.GetButtonDown("Cancel") || Input.GetButtonDownMobile("Cancel"))
        {
            if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.Cancel();
            else if (AmountManager.Instance.IsUIOpen) AmountManager.Instance.Cancel();
            else if (BuildingManager.Instance.IsLocating || BuildingManager.Instance.IsPreviewing) BuildingManager.Instance.GoBack();
            else if (WindowsManager.Instance.WindowsCount > 0) WindowsManager.Instance.CloseTop();
            else EscapeMenuManager.Instance.OpenWindow();
        }
        if (Input.GetButtonDown("Submit") || Input.GetButtonDownMobile("Submit"))
        {
            if (AmountManager.Instance.IsUIOpen && !ConfirmManager.Instance.IsUIOpen) AmountManager.Instance.Confirm();
            else if (!AmountManager.Instance.IsUIOpen && ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.Confirm();
        }
    }
}
