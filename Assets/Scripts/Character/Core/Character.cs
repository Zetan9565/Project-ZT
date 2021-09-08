using UnityEngine;
using ZetanExtends;

public class Character : MonoBehaviour
{
    [SerializeReference]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    protected CharacterData data;
    public CharacterData Data { get => data; protected set => data = value; }

    public Transform Body { get; private set; }

    public CharacterAnimator Animator { get; private set; }
    public CharacterMotion2D Motion { get; protected set; }

    public CharacterController2D Controller { get; private set; }

    public Vector3 Position => transform.position;

    private void Awake()
    {
        Body = transform.FindOrCreate("Body");
        Animator = Body.GetComponent<CharacterAnimator>();
        Motion = Body.GetComponent<CharacterMotion2D>();
        OnAwake();
    }

    private void OnDestroy()
    {
        OnDestroy_();
    }

    public bool IsInit { get; protected set; }

    public void Init(CharacterData data)
    {
        Data = data;
        Data.entity = this;
    }

    public void SetState(CharacterState mainState, dynamic subState)
    {
        Data.mainState = mainState;
        Data.subState = subState;
    }
    public void SetMainState(CharacterState state)
    {
        Data.mainState = state;
        Data.subState = default;
    }
    public void SetSubState(dynamic state)
    {
        Data.subState = state;
    }

    public bool GetState(out CharacterState mainState, out dynamic subState)
    {
        if (!Data)
        {
            mainState = CharacterState.Abnormal;
            subState = CharacterAbnormalState.Dead;
            return false;
        }
        else
        {
            mainState = Data.mainState;
            subState = Data.subState;
            return true;
        }
    }
    public bool GetMainState(out CharacterState mainState)
    {
        if (!Data)
        {
            mainState = CharacterState.Abnormal;
            return false;
        }
        else
        {
            mainState = Data.mainState;
            return true;
        }
    }
    public bool GetSubState(out dynamic subState)
    {
        if (!Data)
        {
            subState = default;
            return false;
        }
        else
        {
            subState = Data.subState;
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