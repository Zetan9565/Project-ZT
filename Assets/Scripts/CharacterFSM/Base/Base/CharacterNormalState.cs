using System.Collections;
using UnityEngine;

public abstract class CharacterNormalState : CharacterMachineStates
{
    public CharacterNormalState(CharacterStateMachine stateMachine) : base(stateMachine) { }
}