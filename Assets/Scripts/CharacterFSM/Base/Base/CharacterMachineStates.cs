using System.Collections;
using UnityEngine;
using ZetanExtends;

public abstract class CharacterMachineStates
{
    protected CharacterStateMachine Machine { get; }
    protected float EnterTime { get; private set; }

    protected CharacterControlInput control;
    protected CharacterAnimator animator;
    protected Rigidbody2DMotion motion;

    public CharacterMachineStates(CharacterStateMachine stateMachine)
    {
        Machine = stateMachine;
        control = Machine.Character.GetComponentInFamily<CharacterControlInput>();
        animator = Machine.Character.GetComponentInFamily<CharacterAnimator>();
        motion = Machine.Character.GetComponentInFamily<Rigidbody2DMotion>();
    }

    public void Enter()
    {
        //Debug.Log($"{Machine.Character.Data.GetInfo().name}进入{GetType().Name}状态"); 
        EnterTime = Time.time;
        OnEnter();
    }
    public void Update()
    {
        OnUpdate();
    }
    public void LateUpdate()
    {
        OnLateUpdate();
    }
    public void FixedUpdate()
    {
        OnFixedUpdate();
    }
    public void Exit()
    {
        //Debug.Log($"{Machine.Character.Data.GetInfo().name}退出{GetType().Name}状态");
        OnExit();
    }

    protected virtual void OnEnter() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnLateUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnExit() { }

    protected Coroutine StartCoroutine(IEnumerator routine)
    {
        if (Machine == null || !Machine.Character) return null;
        return Machine.Character.StartCoroutine(routine);
    }
    protected void StopCoroutine(Coroutine routine)
    {
        if (Machine == null || !Machine.Character) return;
        Machine.Character.StopCoroutine(routine);
    }
    protected void StopAllCoroutines()
    {
        if (Machine == null || !Machine.Character) return;
        Machine.Character.StopAllCoroutines();
    }
}