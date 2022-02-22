using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine
{
    private readonly Dictionary<Type, CharacterMachineStates> stateMap = new Dictionary<Type, CharacterMachineStates>();

    public Character Character { get; }

    public CharacterSMParams Params { get; }

    public CharacterMachineStates CurrentState { get; private set; }

    public CharacterStateMachine(Character character)
    {
        Character = character;
        Params = Character.GetData().GetInfo().SMParams;
        if (!Params) Params = ScriptableObject.CreateInstance<CharacterSMParams>();
        SetCurrentState<CharacterIdleState>();
    }

    public void SetCurrentState<T>() where T : CharacterMachineStates
    {
        Type type = typeof(T);
        if (type.IsAbstract)
        {
            Debug.LogError("状态机尝试设置一个抽象状态：" + type.Name);
            return;
        }
        if (!stateMap.TryGetValue(type, out var state))
        {
            state = (T)Activator.CreateInstance(type, this);
            stateMap.Add(type, state);
        }
        CurrentState?.Exit();
        CurrentState = state;
        CurrentState?.Enter();
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