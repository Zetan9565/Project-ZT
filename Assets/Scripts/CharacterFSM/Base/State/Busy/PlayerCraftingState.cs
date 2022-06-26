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
        craft = WindowsManager.FindWindow<CraftWindow>();
    }

    protected override void OnExit()
    {
        NotifyCenter.RemoveListener(this);
        if (craft.IsCrafting) craft.Interrupt();
    }
}