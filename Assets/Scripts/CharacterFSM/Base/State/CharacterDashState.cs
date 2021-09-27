using UnityEngine;

public class CharacterDashState : CharacterBusyState
{
    private Vector2 direction;
    private bool hasDashStart;

    public CharacterDashState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();

        hasDashStart = false;
        direction = control.ValidMoveInput;
        animator.PlayDashAnima(direction);
    }

    protected override void OnExit()
    {
        base.OnExit();
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
            control.UseActionInputs();
            motion.SetPosition(motion.Rigidbody.position + direction * Machine.Params.DashDistance);
            Machine.SetCurrentState<CharacterIdleState>();
        }
        else if (isDashing)
        {
            control.UseActionInputs();
        }
    }
}