using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/输入管理器")]
public class InputManager : SingletonMonoBehaviour<InputManager>
{
    private CommonInput control;
    public static CommonInput Control => Instance.control;

    public static Vector2 mousePosition => Pointer.current.position.ReadValue();

    public InputCustomInfo customInfo;

    public static bool IsTyping => /*BackpackManager.Instance && BackpackManager.Instance.IsInputFocused ||*/
        NewWindowsManager.IsWindowOpen<PlantWindow>(out var plantWindow) && plantWindow.IsInputFocused;

    private void ShowBuilding(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        else NewWindowsManager.OpenClose<BuildingWindow>();
    }

    private void Awake()
    {
        control = new CommonInput();
        control.Player.Movement.performed += RecordMovement;
        control.Player.Movement.canceled += RecordMovement;
        control.Player.Enable();
        control.UI.Submit.started += Submit;
        control.UI.Cancel.started += Cancel;
        control.UI.ShowQuest.started += ShowQuest;
        control.UI.ShowBackpack.started += ShowBackpack;
        control.UI.ShowBuilding.started += ShowBuilding;
        control.UI.Interact.started += Interact;
        control.UI.Enable();
    }

    private void Update()
    {
        if (IsTyping) return;
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Mouse.current.scroll.ReadValue().y > 0)
        {
            if (NewWindowsManager.IsWindowOpen<DialogueWindow>(out var dialogue) && !dialogue.IsHidden) dialogue.OptionPageUp();
            else if (InteractionPanel.Instance.ScrollAble) InteractionPanel.Instance.Up();
        }
        if (Mouse.current.scroll.ReadValue().y < 0)
        {
            if (NewWindowsManager.IsWindowOpen<DialogueWindow>(out var dialogue) && !dialogue.IsHidden) dialogue.OptionPageDown();
            else if (InteractionPanel.Instance.ScrollAble) InteractionPanel.Instance.Down();
        }
#endif
    }

    private void ShowQuest(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        //QuestManager.Instance.OpenCloseWindow();
    }

    private void ShowBackpack(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        NewWindowsManager.OpenClose<BackpackWindow>();
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        if (NewWindowsManager.IsWindowOpen<LootWindow>(out var loot))
            loot.TakeAll();
        else if (NewWindowsManager.IsWindowOpen<DialogueWindow>(out var window))
        {
            if (window.CurrentType == DialogueType.Normal && window.OptionsCount < 1
                  && window.ShouldShowQuest)
                window.ShowTalkerQuest();
            else if (window.OptionsCount > 0)
                window.FirstOption.OnClick();
        }
        else InteractionPanel.Instance.DoSelectInteract();
    }

    private void Submit(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        if (NewWindowsManager.IsWindowOpen<AmountWindow>(out var amount) && !NewWindowsManager.IsWindowOpen<ConfirmWindow>()) amount.Confirm();
        else if (!NewWindowsManager.IsWindowOpen<AmountWindow>() && NewWindowsManager.IsWindowOpen<ConfirmWindow>(out var confirm)) confirm.Confirm();
    }

    private void Cancel(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        if (NewWindowsManager.IsWindowOpen<BuildingWindow>(out var building) && (building.IsLocating || building.IsPreviewing)) building.GoBack();
        else if (NewWindowsManager.Count > 0) NewWindowsManager.CloseTop();
        else NewWindowsManager.OpenWindow<EscapeWindow>();
    }

    public static bool GetMouseButtonDown(int index)
    {
        if (Mouse.current == null) return false;
        if (index == 0)
            return Mouse.current.leftButton.wasPressedThisFrame;
        else if (index == 1)
            return Mouse.current.rightButton.wasPressedThisFrame;
        else if (index == 2)
            return Mouse.current.middleButton.wasPressedThisFrame;
        return false;
    }

    public static bool GetKeyDown(Key key)
    {
        return Keyboard.current[key].wasPressedThisFrame;
    }

    private Vector2 movement;
    private void RecordMovement(InputAction.CallbackContext context)
    {
        if (context.performed)
            movement = context.ReadValue<Vector2>();
        else movement = Vector2.zero;
    }

    public static float GetAsix(string axisName)
    {
        if (Instance)
        {
            if (axisName == "Horizontal")
                return Instance.movement.x;
            else if (axisName == "Vertical")
                return Instance.movement.y;
        }
        return 0;
    }
}
