public class PlayerTalkingState : CharacterTalkingState
{
    public PlayerTalkingState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        NotifyCenter.AddListener(Window.WindowStateChanged, OnWindowStateChanged, this);
    }

    protected override void OnExit()
    {
        NotifyCenter.RemoveListener(this);
    }

    private void OnWindowStateChanged(params object[] msg)
    {
        if (msg.Length > 0 && msg[0] is string name && Window.IsName<DialogueWindow>(name))
        {
            Debug.Log("end talking");
            Character.SetMachineState<CharacterIdleState>();
        }
    }
}
