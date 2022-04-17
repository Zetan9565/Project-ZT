public class CharacterTalkingState : CharacterBusyState
{
    public CharacterTalkingState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.Talking);
    }
}