using System.Collections;
using UnityEngine;

public abstract class CharacterNormalState : CharacterMachineState
{
    public CharacterNormalState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 进入此状态时，默认将角色主状态设置为<see cref="CharacterStates.Normal"/>
    /// </summary>
    protected override void OnEnter()
    {
        Character.SetMainState(CharacterStates.Normal);
    }
}