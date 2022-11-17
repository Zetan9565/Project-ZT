using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.CharacterSystem;
using ZetanStudio.GatheringSystem;

//namespace ZetanStudio
//{
public class PlayerGatheringState : CharacterBusyState
{
    public PlayerGatheringState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.Gathering);
        NotifyCenter.AddListener(GatherManager.GatheringStateChanged, OnCraftCanceled, this);
    }
    protected override void OnExit()
    {
        NotifyCenter.RemoveListener(this);
        if (GatherManager.IsGathering) GatherManager.Cancel();
    }

    protected override void OnUpdate()
    {
        if (GatherManager.IsGathering)
        {
            if (control.ReadValue(CharacterInputNames.Instance.Move, out Vector2 move) && (move.x != 0 || move.y != 0))
            {
                Machine.SetCurrentState<CharacterMoveState>();
            }
            else if (control.ReadTrigger(CharacterInputNames.Instance.Roll))
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

    private void OnCraftCanceled(params object[] msg)
    {
        if (msg.Length > 0 && msg[0] is bool state && !state)
        {
            Debug.Log("end gathering");
            Character.SetMachineState<CharacterIdleState>();
        }
    }
}
//}
