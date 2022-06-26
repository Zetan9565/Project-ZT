using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character))]
public class PlayerController2D : CharacterController2D
{
    [SerializeField]
#if UNITY_EDITOR
    [Label("寻路代理")]
#endif
    private AStarUnit unit;
    public AStarUnit Unit
    {
        get
        {
            return unit;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Input.Control.Player.Movement.performed += GetMovementInput;
        Input.Control.Player.Movement.canceled += GetMovementInput;
        Input.Control.Player.Flash.started += Flash;
        Input.Control.Player.Roll.started += Roll;
        Input.Control.Player.Action_1.started += Attack;
    }

    private bool canAttack;
    private void Attack(InputAction.CallbackContext context)
    {
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterStates.Normal || state == CharacterStates.Attack)
            {
                if (state == CharacterStates.Normal) canAttack = true;
                if (canAttack)
                {
                    ForceStop();
                    if (inputCoroutine != null) StopCoroutine(inputCoroutine);
                    inputCoroutine = StartCoroutine(WaitAttackInputDelay());
                    if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
                    timeoutCoroutine = StartCoroutine(WaitAttackInputTimeout());
                    Animator.PlayAttackAnima();
                }
            }
        }
    }

    private Coroutine inputCoroutine;
    private IEnumerator WaitAttackInputDelay()
    {
        canAttack = false;
        yield return new WaitForSecondsRealtime(0.03f);
        canAttack = true;
    }

    private Coroutine timeoutCoroutine;
    private IEnumerator WaitAttackInputTimeout()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        Animator.ResetAttackAnima();
    }

    private void Roll(InputAction.CallbackContext context)
    {
        if (Roll()) canAttack = false;
    }

    private void Flash(InputAction.CallbackContext context)
    {
        Flash();
    }

    private bool isTrace;

    public void GetMovementInput(InputAction.CallbackContext context)
    {
        if (context.performed) input = context.ReadValue<Vector2>();
        else input = Vector2.zero;
    }

    public void Trace()
    {
        isTrace = Unit ? (Unit.HasPath ? !isTrace : false) : false;
    }

    public void ResetPath()
    {
        if (Unit) Unit.ResetPath();
    }
}