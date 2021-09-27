using System.Collections;
using UnityEngine;

public abstract class CharacterBusyState : CharacterState
{
    protected CharacterBusyState(CharacterStateMachine stateMachine) : base(stateMachine) { }
}