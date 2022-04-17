public abstract class CharacterAbnormalState : CharacterMachineState
{
    protected CharacterAbnormalState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    /// <summary>
    /// 进入此状态时，默认将角色主状态设置为<see cref="CharacterStates.Abnormal"/>
    /// </summary>
    protected override void OnEnter()
    {
        Character.SetMainState(CharacterStates.Abnormal);
    }
}