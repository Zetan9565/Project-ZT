using System;
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

    public static bool IsTyping => BackpackManager.Instance && BackpackManager.Instance.IsInputFocused ||
        PlantManager.Instance && PlantManager.Instance.IsInputFocused ||
        WarehouseManager.Instance && WarehouseManager.Instance.IsInputFocused;

    private void ShowBuilding(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        BuildingManager.Instance.OpenCloseWindow();
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
            if (DialogueManager.Instance.IsUIOpen && !DialogueManager.Instance.IsPausing) DialogueManager.Instance.OptionPageUp();
            else if (InteractionManager.Instance.ScrollAble) InteractionManager.Instance.Up();
        }
        if (Mouse.current.scroll.ReadValue().y < 0)
        {
            if (DialogueManager.Instance.IsUIOpen && !DialogueManager.Instance.IsPausing) DialogueManager.Instance.OptionPageDown();
            else if (InteractionManager.Instance.ScrollAble) InteractionManager.Instance.Down();
        }
#endif
    }

    private void ShowQuest(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        QuestManager.Instance.OpenCloseWindow();
    }

    private void ShowBackpack(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        BackpackManager.Instance.OpenCloseWindow();
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
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

    private void Submit(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        if (AmountManager.Instance.IsUIOpen && !ConfirmManager.Instance.IsUIOpen) AmountManager.Instance.Confirm();
        else if (!AmountManager.Instance.IsUIOpen && ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.Confirm();
    }

    private void Cancel(InputAction.CallbackContext context)
    {
        if (IsTyping) return;
        if (ConfirmManager.Instance.IsUIOpen) ConfirmManager.Instance.Cancel();
        else if (AmountManager.Instance.IsUIOpen) AmountManager.Instance.Cancel();
        else if (BuildingManager.Instance.IsLocating || BuildingManager.Instance.IsPreviewing) BuildingManager.Instance.GoBack();
        else if (WindowsManager.Instance.WindowsCount > 0) WindowsManager.Instance.CloseTop();
        else EscapeMenuManager.Instance.OpenWindow();
    }

    public static bool GetMouseButtonDown(int index)
    {
        if (Mouse.current == null) return false;
        if (index == 0)
            return Mouse.current.leftButton.isPressed;
        else if (index == 1)
            return Mouse.current.rightButton.isPressed;
        else if (index == 2)
            return Mouse.current.middleButton.isPressed;
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
