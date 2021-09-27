using System.Collections;
using UnityEngine;

public class CharacterMoveState : CharacterNormalState
{
    public CharacterMoveState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
    }

    protected override void OnExit()
    {
        base.OnExit();
    }

    protected override void OnFixedUpdate()
    {
        if (control.RollInput)
        {
            animator.SetDesiredSpeed(Vector2.zero);
            Machine.SetCurrentState<CharacterRollState>();
        }
        else if (control.DashInput)
        {
            motion.SetVelocity(Vector2.zero);
            animator.SetDesiredSpeed(Vector2.zero);
            Machine.SetCurrentState<CharacterDashState>();
        }
        else if (control.MoveInput.x != 0 || control.MoveInput.y != 0)
        {
            motion.SetVelocity(control.MoveInput * Machine.Params.WalkSpeed);
            animator.PlayMoveAnima(control.MoveInput);
            animator.SetDesiredSpeed(control.MoveInput);
        }
        else
        {
            motion.SetVelocity(Vector2.zero);
            animator.SetDesiredSpeed(Vector2.zero);
            Machine.SetCurrentState<CharacterIdleState>();
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }
}