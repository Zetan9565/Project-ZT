using System.Collections;
using UnityEngine;

public class CharacterMoveState : CharacterNormalState
{
    public CharacterMoveState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterNormalStates.Walk);
    }

    protected override void OnExit()
    {
        animator.SetDesiredSpeed(Vector2.zero);
    }

    protected override void OnFixedUpdate()
    {
        if (control.ReadValue(CharacterInputNames.Instance.Move, out Vector2 move) && (move.x != 0 || move.y != 0))
        {
            motion.SetVelocity(move * Machine.Params.WalkSpeed);
            animator.PlayMoveAnima(move);
            animator.SetDesiredSpeed(move);
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
        if (control.ReadTrigger(CharacterInputNames.Instance.Roll))
        {
            animator.SetDesiredSpeed(Vector2.zero);
            Machine.SetCurrentState<CharacterRollState>();
        }
        else if (control.ReadTrigger(CharacterInputNames.Instance.Flash))
        {
            motion.SetVelocity(Vector2.zero);
            animator.SetDesiredSpeed(Vector2.zero);
            Machine.SetCurrentState<CharacterFlashState>();
        }
    }
}