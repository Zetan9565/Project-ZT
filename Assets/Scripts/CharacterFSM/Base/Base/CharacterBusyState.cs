public abstract class CharacterBusyState : CharacterMachineState
{
    protected CharacterBusyState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 进入此状态时，默认将角色主状态设置为<see cref="CharacterStates.Busy"/>
    /// </summary>
    protected override void OnEnter()
    {
        Character.SetMainState(CharacterStates.Busy);
    }
}