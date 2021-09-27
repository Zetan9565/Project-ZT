using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdleState : CharacterNormalState
{
    public CharacterIdleState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
    }

    protected override void OnExit()
    {
        base.OnExit();
    }

    protected override void OnUpdate()
    {
        if (control)
            if (control.RollInput)
            {
                Machine.SetCurrentState<CharacterRollState>();
            }
            else if (control.DashInput)
            {
                Machine.SetCurrentState<CharacterDashState>();
            }
            else if (control.MoveInput.x != 0 || control.MoveInput.y != 0)
            {
                Machine.SetCurrentState<CharacterMoveState>();
            }

    }
}
