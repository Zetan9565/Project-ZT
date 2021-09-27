using UnityEngine;

[DisallowMultipleComponent]
public class CharacterSMRunner : MonoBehaviour
{
    public Character Character { get; private set; }

    public CharacterStateMachine StateMachine { get; private set; }

    public CharacterState CurrentState => StateMachine?.CurrentState;

    private void Update()
    {
        StateMachine?.Update();
    }
    private void LateUpdate()
    {
        StateMachine?.LateUpdate();
    }
    private void FixedUpdate()
    {
        StateMachine?.FixedUpdate();
    }

    public void Init(Character character)
    {
        Character = character;
        StateMachine = new CharacterStateMachine(this);
    }

    public void SetCurrentState<T>() where T : CharacterState
    {
        StateMachine?.SetCurrentState<T>();
    }
}