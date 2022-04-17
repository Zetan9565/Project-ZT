using UnityEngine;

public class CharacterDashState : CharacterBusyState
{
    private Vector2 direction;
    private bool hasDashStart;

    public CharacterDashState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.Dash);
        hasDashStart = false;
        control.ReadValue(CharacterInputNames.Instance.Direction, out direction);
        animator.PlayDashAnima(direction);
    }

    protected override void OnExit()
    {
        control.SetTrigger(CharacterInputNames.Instance.Dash);
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        bool isDashing = animator.CurrentState.IsTag(CharacterAnimaTags.Dash);
        if (!hasDashStart && isDashing)
        {
            hasDashStart = true;
            control.ResetTrigger(CharacterInputNames.Instance.Dash);
            motion.SetPosition(motion.Rigidbody.position + direction * Machine.Params.DashDistance);
            Machine.SetCurrentState<CharacterIdleState>();
        }
        else if (isDashing)
        {
            control.ResetTrigger(CharacterInputNames.Instance.Dash);
        }
    }
}