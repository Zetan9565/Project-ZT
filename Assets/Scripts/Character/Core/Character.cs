using UnityEngine;
using ZetanExtends;

public abstract class Character<T> : Character where T : CharacterData
{
    [SerializeReference, ReadOnly]
    protected T data;

    public override CharacterData GetData()
    {
        return data;
    }
    public override void SetData(CharacterData value)
    {
        data = (T)value;
    }

    public virtual T GetGenericData()
    {
        return data;
    }
    protected void SetGenericData(T value)
    {
        data = value;
    }

    public virtual void Init(T data)
    {
        SetGenericData(data);
        GetData().entity = this;
        StateMachine?.Init(this);
    }
}

[RequireComponent(typeof(CharacterSMRunner))]
public abstract class Character : MonoBehaviour
{
    public Vector3 Position => transform.position;

    public CharacterAnimator Animator { get; protected set; }

    public CharacterController2D Controller { get; protected set; }

    public CharacterSMRunner StateMachine { get; protected set; }

    protected void Awake()
    {
        StateMachine = GetComponent<CharacterSMRunner>();
        Animator = this.GetComponentInFamily<CharacterAnimator>();
        if (!Animator) Animator = gameObject.AddComponent<CharacterAnimator>();
        OnAwake();
    }

    protected void OnDestroy()
    {
        OnDestroy_();
#if UNITY_EDITOR
        if (GetComponent<CharacterSMRunner>() is CharacterSMRunner machine)
            machine.hideFlags = HideFlags.None;
#endif
    }

    protected virtual void OnValidate()
    {
        if (GetComponent<CharacterSMRunner>() is CharacterSMRunner machine)
            machine.hideFlags = HideFlags.HideInInspector;
    }

    public abstract CharacterData GetData();
    public T GetData<T>() where T : CharacterData
    {
        return GetData() as T;
    }
    public abstract void SetData(CharacterData value);

    public void SetState(CharacterStates mainState, dynamic subState)
    {
        GetData().mainState = mainState;
        GetData().subState = subState;
    }
    public void SetMainState(CharacterStates state)
    {
        GetData().mainState = state;
        GetData().subState = default;
    }
    public void SetSubState(dynamic state)
    {
        GetData().subState = state;
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

    public void SetController(CharacterController2D controller)
    {
        Controller = controller;
    }

    protected virtual void OnAwake()
    {

    }

    protected virtual void OnDestroy_()
    {

    }

}