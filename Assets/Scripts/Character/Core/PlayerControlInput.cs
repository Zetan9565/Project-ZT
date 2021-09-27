using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlInput : CharacterControlInput
{
    private void Awake()
    {
        InputManager.Control.Player.Movement.performed += GetMovementInput;
        InputManager.Control.Player.Movement.canceled += GetMovementInput;
        InputManager.Control.Player.Dash.started += Dash;
        InputManager.Control.Player.Roll.started += Roll;
        InputManager.Control.Player.Action_1.started += Attack;
        InputManager.Control.Player.Action_1.canceled += Attack;
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
        else if(context.canceled)
        {
            if (atkHoldCouroutine != null) StopCoroutine(atkHoldCouroutine);
            Debug.Log(atkHoldTime);
        }
    }
    private IEnumerator AttackHold()
    {
        while (true)
        {
            yield return null;
            atkHoldTime += Time.deltaTime;
        }
    }
    private void Roll(InputAction.CallbackContext context)
    {
        SetRollInput(true);
    }

    private void Dash(InputAction.CallbackContext context)
    {
        SetDashInput(true);
    }

    public void GetMovementInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetMoveInput(context.ReadValue<Vector2>());
        }
        else if(context.canceled)
        {
            SetMoveInput(Vector2.zero);
        }
    }
}