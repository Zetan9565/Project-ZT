using UnityEngine;

public class PlayerMakingState : CharacterBusyState
{
    public PlayerMakingState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    private MakingWindow making;

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.UI);
        NotifyCenter.AddListener(MakingManager.MakingCanceled, OnMakingCanceled, this);
        making = WindowsManager.FindWindow<MakingWindow>();
    }

    private void OnMakingCanceled(params object[] msg)
    {
        Machine.SetCurrentState<CharacterIdleState>();
    }

    protected override void OnExit()
    {
        NotifyCenter.RemoveListener(this);
    }

    protected override void OnUpdate()
    {
        if (making && making.IsMaking)
        {
            if (control.ReadValue(CharacterInputNames.Instance.Move, out Vector2 move) && (move.x != 0 || move.y != 0))
            {
                Machine.SetCurrentState<CharacterMoveState>();
            }
        }
    }
}