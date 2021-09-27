using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine
{
    private readonly Dictionary<Type, CharacterState> stateMap = new Dictionary<Type, CharacterState>();

    public CharacterSMRunner Runner { get; }

    public Character Character { get; }

    public CharacterSMParams Params { get; }

    public CharacterState CurrentState { get; private set; }

    public CharacterStateMachine(CharacterSMRunner runner)
    {
        Runner = runner;
        Character = runner.Character;
        Params = Character.GetData().GetInfo().SMParams;
        if (!Params) Params = ScriptableObject.CreateInstance<CharacterSMParams>();
        SetCurrentState<CharacterIdleState>();
    }

    public void SetCurrentState<T>() where T : CharacterState
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