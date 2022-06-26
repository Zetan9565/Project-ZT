using UnityEngine;

public class CharacterIdleState : CharacterNormalState
{
    public CharacterIdleState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterNormalStates.Idle);
    }

    protected override void OnExit()
    {
        base.OnExit();
    }

    protected override void OnUpdate()
    {
        if (control)
        {
            if (control.ReadTrigger(CharacterInputNames.Instance.Roll))
            {
                Machine.SetCurrentState<CharacterRollState>();
            }
            else if (control.ReadTrigger(CharacterInputNames.Instance.Flash))
            {
                Machine.SetCurrentState<CharacterFlashState>();
            }
            else if (control.ReadValue(CharacterInputNames.Instance.Move, out Vector2 move) && (move.x != 0 || move.y != 0))
            {
                Machine.SetCurrentState<CharacterMoveState>();
            }
        }
    }
}
