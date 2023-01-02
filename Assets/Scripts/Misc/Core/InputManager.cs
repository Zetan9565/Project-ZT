using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZetanStudio
{
    using DialogueSystem.UI;
    using InteractionSystem.UI;
    using InventorySystem.UI;
    using ItemSystem.UI;
    using QuestSystem.UI;
    using ZetanStudio.UI;

    public static class InputManager
    {
        private static CommonInput control;
        public static CommonInput Control => control;

#pragma warning disable IDE1006 // 命名样式
        public static Vector2 mousePosition => Pointer.current.position.ReadValue();
#pragma warning restore IDE1006 // 命名样式

        public static bool IsTyping => EventSystem.current && EventSystem.current.currentSelectedGameObject is GameObject go && go.GetComponent<InputField>();

        private static void ShowStructure(InputAction.CallbackContext context)
        {
            if (IsTyping) return;
            else WindowsManager.OpenClose<StructureWindow>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            control = new CommonInput();
            control.Player.Movement.performed += RecordMovement;
            control.Player.Movement.canceled += RecordMovement;
            control.Player.Enable();
            control.UI.Submit.started += Submit;
            control.UI.Cancel.started += Cancel;
            control.UI.ShowQuest.started += ShowQuest;
            control.UI.ShowBackpack.started += ShowBackpack;
            control.UI.ShowBuilding.started += ShowStructure;
            control.UI.Interact.started += Interact;
            control.UI.Enable();
            movement = Vector2.zero;
        }

        private static void ShowQuest(InputAction.CallbackContext context)
        {
            if (IsTyping) return;
            WindowsManager.OpenClose<QuestWindow>();
        }

        private static void ShowBackpack(InputAction.CallbackContext context)
        {
            if (IsTyping) return;
            WindowsManager.OpenClose<BackpackWindow>();
        }

        private static void Interact(InputAction.CallbackContext context)
        {
            if (IsTyping) return;
            if (WindowsManager.IsWindowOpen<LootWindow>(out var loot))
                loot.TakeAll();
            else if (WindowsManager.IsWindowOpen<DialogueWindow>(out var window))
                window.Next();
            else InteractionPanel.Instance.DoSelectInteract();
        }

        private static void Submit(InputAction.CallbackContext context)
        {
            if (IsTyping) return;
            if (WindowsManager.IsWindowOpen<AmountWindow>(out var amount) && !WindowsManager.IsWindowOpen<ConfirmWindow>()) amount.Confirm();
            else if (!WindowsManager.IsWindowOpen<AmountWindow>() && WindowsManager.IsWindowOpen<ConfirmWindow>(out var confirm)) confirm.Confirm();
        }

        private static void Cancel(InputAction.CallbackContext context)
        {
            if (IsTyping) return;
            if (WindowsManager.IsWindowOpen<StructureWindow>(out var structure) && (structure.IsLocating || structure.IsPreviewing)) structure.GoBack();
            else if (WindowsManager.Count > 0) WindowsManager.CloseTop();
            else WindowsManager.OpenWindow<EscapeWindow>();
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
        public static bool GetMouseButtonUp(int index)
        {
            if (Mouse.current == null) return false;
            if (index == 0)
                return Mouse.current.leftButton.wasReleasedThisFrame;
            else if (index == 1)
                return Mouse.current.rightButton.wasReleasedThisFrame;
            else if (index == 2)
                return Mouse.current.middleButton.wasReleasedThisFrame;
            return false;
        }
        public static bool GetMouseButton(int index)
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

        public static bool GetPointerDown()
        {
            return Pointer.current.press.wasPressedThisFrame;
        }
        public static bool GetPointerUp()
        {
            return Pointer.current.press.wasReleasedThisFrame;
        }
        public static bool GetPointer()
        {
            return Pointer.current.press.isPressed;
        }

        public static bool GetTouchDown(int index)
        {
            try
            {
                return Touchscreen.current.touches[index].press.wasPressedThisFrame;
            }
            catch
            {
                return false;
            }
        }
        public static bool GetTouchUp(int index)
        {
            try
            {
                return Touchscreen.current.touches[index].press.wasReleasedThisFrame;
            }
            catch
            {
                return false;
            }
        }
        public static bool GetTouch(int index)
        {
            try
            {
                return Touchscreen.current.touches[index].press.isPressed;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetKeyDown(Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }
        public static bool GetKeyUp(Key key)
        {
            return Keyboard.current[key].wasReleasedThisFrame;
        }
        public static bool GetKey(Key key)
        {
            return Keyboard.current[key].isPressed;
        }

        private static Vector2 movement;
        private static void RecordMovement(InputAction.CallbackContext context)
        {
            if (context.performed) movement = context.ReadValue<Vector2>();
            else movement = Vector2.zero;
        }

        public static float GetAsix(string axisName)
        {
            if (axisName == "Horizontal") return movement.x;
            else if (axisName == "Vertical") return movement.y;
            else if (axisName == "Mouse ScrollWheel") return control.Player.MouseWheel.ReadValue<float>();
            else return 0;
        }
    }
}