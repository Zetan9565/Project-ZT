using System.Collections;
using UnityEngine;
using ZetanStudio.Extension;

public abstract class CharacterMachineState
{
    protected CharacterStateMachine Machine { get; }
    protected Character Character => Machine?.Character;
    protected float EnterTime { get; private set; }

    protected bool IsGrouded => true;

    protected CharacterControlInput control;
    protected CharacterAnimator animator;
    protected Rigidbody2DMotion motion;

    public CharacterMachineState(CharacterStateMachine stateMachine)
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

    /// <summary>
    /// 进入此状态时，默认为空
    /// </summary>
    protected virtual void OnEnter() { }
    /// <summary>
    /// 此状态Update时，默认为空
    /// </summary>
    protected virtual void OnUpdate() { }
    /// <summary>
    /// 此状态LateUpdate时，默认为空
    /// </summary>
    protected virtual void OnLateUpdate() { }
    /// <summary>
    /// 此状态FixedUpdate时，默认为空
    /// </summary>
    protected virtual void OnFixedUpdate() { }
    /// <summary>
    /// 退出此状态时，默认为空
    /// </summary>
    protected virtual void OnExit() { }
    public virtual bool CanTransitTo<T>(Transition transition) where T : CharacterMachineState
    {
        return true;
    }

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

    protected static bool IsState<T>(System.Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }
    protected static bool IsState<TTarget, T>()
    {
        return typeof(TTarget).IsAssignableFrom(typeof(T));
    }

    protected static class Debug
    {
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}