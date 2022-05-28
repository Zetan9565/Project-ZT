using UnityEngine;
using ZetanStudio.Extension;

public abstract class Character : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public string Name => GetData()?.GetInfo().Name;
    public CharacterAnimator Animator { get; protected set; }

    public CharacterController2D Controller { get; protected set; }

    public CharacterStateMachine StateMachine { get; protected set; }

    public CharacterMachineState MachineState => StateMachine?.CurrentState;

    public abstract CharacterData GetData();
    public T GetData<T>() where T : CharacterData
    {
        return GetData() as T;
    }

    public abstract void SetData(CharacterData value);

    public virtual void Init<T>(T data) where T : CharacterData
    {
        SetData(data);
        GetData().entity = this;
        StateMachine = new CharacterStateMachine(this);
        StateMachine.SetCurrentState<CharacterIdleState>();
    }
    public void SetController(CharacterController2D controller)
    {
        Controller = controller;
    }

    public void SetMachineState<T>() where T : CharacterMachineState
    {
        StateMachine?.SetCurrentState<T>();
    }
    public void SetMachineAndCharacterState<T>(CharacterStates main, dynamic sub) where T : CharacterMachineState
    {
        StateMachine?.SetCurrentState<T>();
        SetState(main, sub);
    }

    #region MonoBehaviour
    protected void Awake()
    {
        Animator = this.GetComponentInFamily<CharacterAnimator>();
        if (!Animator) Animator = gameObject.AddComponent<CharacterAnimator>();
        OnAwake();
    }

    private void Update()
    {
        StateMachine?.Update();
        OnUpdate();
    }

    private void LateUpdate()
    {
        StateMachine?.LateUpdate();
        OnLateUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine?.FixedUpdate();
        OnFixedUpdate();
    }

    protected void OnDestroy()
    {
        OnDestroy_();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {

    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {

    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {

    }
    #endregion

    #region 角色状态相关
    public void SetState(CharacterStates mainState, dynamic subState)
    {
        GetData().mainState = mainState;
        GetData().subState = subState;
        OnStateChange(mainState, subState);
    }
    public void SetMainState(CharacterStates state)
    {
        GetData().mainState = state;
        GetData().subState = default;
        OnStateChange(state, default);
    }
    public void SetSubState(dynamic state)
    {
        GetData().subState = state;
        OnStateChange(GetData().mainState, state);
    }

    public bool GetState(out CharacterStates mainState, out dynamic subState)
    {
        if (!GetData())
        {
            mainState = CharacterStates.Abnormal;
            subState = CharacterAbnormalStates.Dead;
            return false;
        }
        else
        {
            mainState = GetData().mainState;
            subState = GetData().subState;
            return true;
        }
    }
    public bool GetMainState(out CharacterStates mainState)
    {
        if (!GetData())
        {
            mainState = CharacterStates.Abnormal;
            return false;
        }
        else
        {
            mainState = GetData().mainState;
            return true;
        }
    }
    public bool GetSubState(out dynamic subState)
    {
        if (!GetData())
        {
            subState = default;
            return false;
        }
        else
        {
            subState = GetData().subState;
            return true;
        }
    }
    #endregion

    #region 虚方法
    protected virtual void OnAwake()
    {

    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnLateUpdate()
    {

    }

    protected virtual void OnFixedUpdate()
    {

    }

    protected virtual void OnDestroy_()
    {

    }

    protected virtual void OnStateChange(CharacterStates main, dynamic sub)
    {

    }
    #endregion
}