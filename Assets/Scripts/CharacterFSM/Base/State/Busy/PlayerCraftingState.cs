using UnityEngine;

public class PlayerCraftingState : CharacterBusyState
{
    public PlayerCraftingState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    private CraftWindow craft;

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.UI);
        NotifyCenter.AddListener(CraftManager.CraftCanceled, OnCraftCanceled, this);
        craft = WindowsManager.FindWindow<CraftWindow>();
    }

    private void OnCraftCanceled(params object[] msg)
    {
        Machine.SetCurrentState<CharacterIdleState>();
    }

    protected override void OnExit()
    {
        NotifyCenter.RemoveListener(this);
    }

    protected override void OnUpdate()
    {
        if (craft && craft.IsCrafting)
        {
            if (control.ReadValue(CharacterInputNames.Instance.Move, out Vector2 move) && (move.x != 0 || move.y != 0))
            {
                Machine.SetCurrentState<CharacterMoveState>();
            }
        }
    }
}