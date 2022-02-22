using System.Collections;
using UnityEngine;

public abstract class CharacterBusyState : CharacterMachineStates
{
    protected CharacterBusyState(CharacterStateMachine stateMachine) : base(stateMachine) { }
}