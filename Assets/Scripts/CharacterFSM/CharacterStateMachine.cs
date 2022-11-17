using System;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio;

public class CharacterStateMachine
{
    private readonly Dictionary<Type, CharacterMachineState> stateMap = new Dictionary<Type, CharacterMachineState>();

    public Character Character { get; }

    public CharacterSMParams Params { get; }

    public CharacterMachineState CurrentState { get; private set; }

    public event Action<CharacterMachineState, CharacterMachineState> OnStateChanged;

    public bool IsAtState<T>() where T : CharacterMachineState
    {
        return CurrentState is T;
    }

    public CharacterStateMachine(Character character)
    {
        Character = character;
        Params = Character.GetData().GetInfo().SMParams;
        if (!Params) Params = ScriptableObject.CreateInstance<CharacterSMParams>();
    }

    public bool SetCurrentState<T>(Transition transition = null) where T : CharacterMachineState
    {
        Type type = typeof(T);
        if (type.IsAbstract)
        {
            Debug.LogError($"{Character}的状态机尝试设置一个抽象状态：{type.Name}");
            return false;
        }
        if (!stateMap.TryGetValue(type, out var state))
        {
            state = (T)Activator.CreateInstance(type, this);
            stateMap.Add(type, state);
        }
        if (CurrentState != null && !CurrentState.CanTransitTo<T>(transition))
        {
#if DEBUG
            Debug.Log($"{Character} 无法从 {CurrentState.GetType().Name} 状态进入 {type.Name} 状态，过渡条件：{Utility.SerializeObject(transition, false)}");
#endif
            return false;
        }
        var prev = CurrentState;
        prev?.Exit();
        CurrentState = state;
        CurrentState?.Enter();
        OnStateChanged?.Invoke(prev, CurrentState);
#if false
        if (prev != null) Debug.Log($"{Character} 从 {prev.GetType().Name} 状态进入 {type.Name} 状态");
        else Debug.Log($"{Character} 进入 {type.Name} 状态");
#endif
        return true;
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void LateUpdate()
    {
        CurrentState?.LateUpdate();
    }

    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}