using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterController2D : MonoBehaviour
{
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public Vector2 input;

    public CharacterAnimator Animator => Character.Animator;

    public CharacterMotion2D Motion => Character.Motion;

    public Character Character { get; protected set; }

    protected virtual void Awake()
    {
        SetCharacter(GetComponent<Character>());
    }

    public virtual bool Roll(Vector2? input = null)
    {
        if (Animator.CurrentState.IsTag(CharacterAnimaTags.Roll) || Animator.CurrentState.IsTag(CharacterAnimaTags.Dash)) return false;
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterState.Normal || state == CharacterState.Attack)
            {
                if (input == null) input = this.input;
                Character.SetState(CharacterState.Busy, CharacterBusyState.Roll);
                Motion.Roll(input.Value, delegate
                {
                    if (Character.GetState(out var main, out var sub) && main == CharacterState.Busy && sub == CharacterBusyState.Roll)
                        Character.SetState(CharacterState.Normal, CharacterNormalState.Idle);
                });
                Animator.PlayRollAnima(input.Value);
                return true;
            }
        }
        return false;
    }

    public virtual bool Dash(Vector2? input = null)
    {
        if (Animator.CurrentState.IsTag(CharacterAnimaTags.Roll) || Animator.CurrentState.IsTag(CharacterAnimaTags.Dash)) return false;
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterState.Normal)
            {
                if (input == null) input = this.input;
                Character.SetState(CharacterState.Busy, CharacterBusyState.Dash);
                Motion.Dash(input.Value, () => { Character.SetState(CharacterState.Normal, CharacterNormalState.Idle); });
                Animator.PlayDashAnima(input.Value);
                return true;
            }
        }
        return false;
    }

    public virtual void Move(Vector2? input = null)
    {
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterState.Normal)
            {
                if (input == null) input = this.input;
                if (input.Value.sqrMagnitude != 0)
                {
                    Character.SetSubState(CharacterNormalState.Walk);
                    Animator.PlayMoveAnima(input.Value);
                }
                else
                {
                    Character.SetSubState(CharacterNormalState.Idle);
                }
                Motion.Move(input.Value);
            }
        }
    }

    protected virtual void Update()
    {
        if (Character.GetState(out var main, out var sub))
        {
            switch (main)
            {
                case CharacterState.Normal:
                    Move();
                    break;
                case CharacterState.Abnormal:
                    break;
                case CharacterState.Gather:
                    break;
                case CharacterState.Attack:
                    Motion.Move(Vector2.zero);
                    break;
                case CharacterState.Busy:
                    if ((CharacterBusyState)sub == CharacterBusyState.Roll || (CharacterBusyState)sub == CharacterBusyState.Dash)
                        Motion.Move(Vector2.zero);
                    break;
                default:
                    break;
            }
        }
        Animator.SetDesiredSpeed(input);
    }

    public virtual void ForceStop()
    {
        Motion.ForceStop();
        Animator.SetAnimaState((int)CharacterState.Normal, (int)CharacterNormalState.Idle);
    }

    public virtual void SetCharacter(Character character)
    {
        Character = character;
        Character.SetController(this);
    }
}
