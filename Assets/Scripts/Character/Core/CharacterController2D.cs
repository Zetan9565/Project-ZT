using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterController2D : MonoBehaviour
{
    public Vector2 input;

    [SerializeField]
    private Rigidbody2D mRigidbody;

    public LayerMask raycastLayer = 0;
    public float distanceOffset = 0.5f;
    public float moveSpeed = 7;
    public float flashForce = 5;
    public Vector2 defaultDirection = Vector2.right;

    private Vector2 latestDirection;
    public float funcSpeed;

    public CharacterAnimator Animator => Character.Animator;

    public Character Character { get; protected set; }

    protected virtual void Awake()
    {
        SetCharacter(GetComponent<Character>());
        latestDirection = defaultDirection;
    }

    public virtual bool Roll(Vector2? input = null)
    {
        if (Animator.CurrentState.IsTag(CharacterAnimaTags.Roll) || Animator.CurrentState.IsTag(CharacterAnimaTags.Flash)) return false;
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterStates.Normal || state == CharacterStates.Attack)
            {
                if (input != null) this.input = input.Value.normalized;
                Character.SetState(CharacterStates.Busy, CharacterBusyStates.Roll);
                Animator.PlayRollAnima(this.input);
                if (this.input.x != 0 || this.input.y != 0) latestDirection = this.input;
                return true;
            }
        }
        return false;
    }

    public virtual bool Flash(Vector2? input = null)
    {
        if (Animator.CurrentState.IsTag(CharacterAnimaTags.Roll) || Animator.CurrentState.IsTag(CharacterAnimaTags.Flash)) return false;
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterStates.Normal)
            {
                if (input != null) this.input = input.Value.normalized;
                Character.SetState(CharacterStates.Busy, CharacterBusyStates.Flash);
                Animator.PlayFlashAnima(this.input);
                if (this.input.x != 0 || this.input.y != 0) latestDirection = this.input;
                return true;
            }
        }
        return false;
    }

    public virtual void Move(Vector2? input = null)
    {
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterStates.Normal)
            {
                if (input != null) this.input = input.Value.normalized;
                if (this.input.sqrMagnitude != 0)
                {
                    Character.SetSubState(CharacterNormalStates.Walk);
                    Animator.PlayMoveAnima(this.input);
                }
                else
                {
                    Character.SetSubState(CharacterNormalStates.Idle);
                }
                if (this.input.x != 0 || this.input.y != 0) latestDirection = this.input;
            }
        }
    }

    protected virtual void Update()
    {
        if (Character.GetState(out var main, out var sub))
        {
            switch (main)
            {
                case CharacterStates.Normal:
                    Move();
                    if (!mRigidbody) MoveTransform(moveSpeed * Time.deltaTime * (Vector3)input, input, moveSpeed);
                    break;
                case CharacterStates.Busy:
                    switch ((CharacterBusyStates)sub)
                    {
                        case CharacterBusyStates.Flash:
                            if (!mRigidbody)
                            {
                                Character.SetState(CharacterStates.Normal, CharacterNormalStates.Idle);
                                MoveTransform(flashForce * latestDirection, latestDirection, moveSpeed * flashForce);
                            }
                            break;
                        case CharacterBusyStates.Roll:
                            if (!mRigidbody) MoveTransform(funcSpeed * Time.deltaTime * (Vector3)latestDirection, latestDirection, funcSpeed);
                            break;
                        default:
                            input = Vector2.zero;
                            break;
                    }
                    break;
                default:
                    input = Vector2.zero;
                    break;
            }
        }
        Animator.SetDesiredSpeed(input);
    }

    private void MoveTransform(Vector3 additive, Vector2 direction, float distance)
    {
        var hit = Physics2D.CircleCast(transform.position, distanceOffset, direction, distance, raycastLayer);
        if (hit.collider) CalAdditive(hit.distance);
        transform.position += additive;

        void CalAdditive(float distance)
        {
            float offsetDistance = distance - distanceOffset;
            if (offsetDistance < 0) additive = Vector3.zero;
            else if (offsetDistance < additive.magnitude)
                additive = additive.normalized * offsetDistance;
        }
    }

    private void FixedUpdate()
    {
        if (Character.GetState(out var main, out var sub))
            if (mRigidbody)
            {
                switch (main)
                {
                    case CharacterStates.Normal:
                        mRigidbody.velocity = input * moveSpeed;
                        break;
                    case CharacterStates.Busy:
                        switch ((CharacterBusyStates)sub)
                        {
                            case CharacterBusyStates.Flash:
                                Vector2 addtive = flashForce * latestDirection;
                                if (mRigidbody.collisionDetectionMode == CollisionDetectionMode2D.Discrete)
                                {
                                    var hit = Physics2D.Raycast(mRigidbody.position, latestDirection, addtive.magnitude, raycastLayer);
                                    if (hit.collider) addtive = hit.point - mRigidbody.position;
                                }
                                mRigidbody.MovePosition(mRigidbody.position + addtive);
                                Character.SetState(CharacterStates.Normal, CharacterNormalStates.Idle);
                                break;
                            case CharacterBusyStates.Roll:
                                mRigidbody.velocity = latestDirection * funcSpeed;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
    }

    public virtual void ForceStop()
    {
        input = Vector2.zero;
        if (mRigidbody) mRigidbody.velocity = Vector2.zero;
        funcSpeed = 0;
    }

    public virtual void SetCharacter(Character character)
    {
        Character = character;
        Character.SetController(this);
    }
}
