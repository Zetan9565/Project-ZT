using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlInput : CharacterControlInput
{
    protected override void OnAwake()
    {
        Input.Control.Player.Movement.performed += GetMovementInput;
        Input.Control.Player.Movement.canceled += GetMovementInput;
        Input.Control.Player.Flash.started += Flash;
        Input.Control.Player.Roll.started += Roll;
        Input.Control.Player.Action_1.started += Attack;
        Input.Control.Player.Action_1.canceled += Attack;
    }
    private float atkHoldTime;
    private Coroutine atkHoldCouroutine;
    private void Attack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (atkHoldCouroutine != null) StopCoroutine(atkHoldCouroutine);
            atkHoldTime = 0;
            atkHoldCouroutine = StartCoroutine(AttackHold());
        }
        else if (context.canceled)
        {
            if (atkHoldCouroutine != null) StopCoroutine(atkHoldCouroutine);
            if (atkHoldTime > 0.5f) Debug.Log("蓄力释放");
            else Debug.Log("轻击");
        }
    }
    private IEnumerator AttackHold()
    {
        while (true)
        {
            yield return null;
            atkHoldTime += Time.deltaTime;
            if (atkHoldTime > 0.5f) Debug.Log("蓄力中");
        }
    }
    private void Roll(InputAction.CallbackContext context)
    {
        if (Input.IsTyping) return;
        SetTrigger(CharacterInputNames.Instance.Roll);
    }

    private void Flash(InputAction.CallbackContext context)
    {
        if (Input.IsTyping) return;
        SetTrigger(CharacterInputNames.Instance.Flash);
    }

    public void GetMovementInput(InputAction.CallbackContext context)
    {
        if (Input.IsTyping)
        {
            SetValue(CharacterInputNames.Instance.Move, Vector2.zero);
            return;
        }
        if (context.performed)
        {
            var input = context.ReadValue<Vector2>();
            SetValue(CharacterInputNames.Instance.Move, input);
            SetValue(CharacterInputNames.Instance.Direction, input);
        }
        else if (context.canceled)
        {
            SetValue(CharacterInputNames.Instance.Move, Vector2.zero);
        }
    }
}