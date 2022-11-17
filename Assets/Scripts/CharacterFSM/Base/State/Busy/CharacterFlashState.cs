using UnityEngine;
using ZetanStudio.CharacterSystem;

public class CharacterFlashState : CharacterBusyState
{
    private Vector2 direction;
    private bool hasFlashStart;

    public CharacterFlashState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.Flash);
        hasFlashStart = false;
        control.ReadValue(CharacterInputNames.Instance.Direction, out direction);
        direction.Normalize();
        animator.PlayFlashAnima(direction);
    }

    protected override void OnExit()
    {
        control.SetTrigger(CharacterInputNames.Instance.Flash);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        bool isDashing = animator.CurrentState.IsTag(CharacterAnimaTags.Flash);
        if (!hasFlashStart && isDashing)
        {
            hasFlashStart = true;
            control.ResetTrigger(CharacterInputNames.Instance.Flash);
            motion.SetPosition(motion.Rigidbody.position + direction * Machine.Params.FlashDistance);
            Machine.SetCurrentState<CharacterIdleState>();
        }
        else if (isDashing)
        {
            control.ResetTrigger(CharacterInputNames.Instance.Flash);
        }
    }
}