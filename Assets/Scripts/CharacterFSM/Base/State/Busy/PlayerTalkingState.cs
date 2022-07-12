using ZetanStudio.DialogueSystem.UI;

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
        if (msg.Length > 0 && msg[0] is System.Type type && Window.IsType<DialogueWindow>(type) && msg[0] is WindowStates states && states == WindowStates.Closed)
        {
            Debug.Log("end talking");
            Character.SetMachineState<CharacterIdleState>();
        }
    }
}
