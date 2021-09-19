using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Character))]
public class CharacterController2D : MonoBehaviour
{
    public Vector2 input;

    [SerializeField]
    private Rigidbody2D mRigidbody;

    public LayerMask raycastLayer = 0;
    public float distanceOffset = 0.5f;
    public float moveSpeed = 7;
    public float dashForce = 5;
    public Vector2 defaultDirection = Vector2.right;

    private Vector2 latestDirection;
    public float funcSpeed;

    public CharacterAnimator Animator => Character.Animator;

    public Character Character { get; protected set; }

    public Seeker seeker;

    private Path path;
    private List<Vector3> vectorPath;

    public float pickNextDistance = 1;

    public bool HasPath => path != null && vectorPath != null;

    protected virtual void Awake()
    {
        SetCharacter(GetComponent<Character>());
        latestDirection = defaultDirection;
        seeker = GetComponent<Seeker>();
    }

    public virtual bool Roll(Vector2? input = null)
    {
        if (Animator.CurrentState.IsTag(CharacterAnimaTags.Roll) || Animator.CurrentState.IsTag(CharacterAnimaTags.Dash)) return false;
        if (Character.GetMainState(out var state))
        {
            if (state == CharacterState.Normal || state == CharacterState.Attack)
            {
                if (input != null) this.input = input.Value.normalized;
                Character.SetState(CharacterState.Busy, CharacterBusyState.Roll);
                Animator.PlayRollAnima(this.input);
                if (this.input.x != 0 || this.input.y != 0) latestDirection = this.input;
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
                if (input != null) this.input = input.Value.normalized;
                Character.SetState(CharacterState.Busy, CharacterBusyState.Dash);
                Animator.PlayDashAnima(this.input);
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
            if (state == CharacterState.Normal)
            {
                if (input != null) this.input = input.Value.normalized;
                if (this.input.sqrMagnitude != 0)
                {
                    Character.SetSubState(CharacterNormalState.Walk);
                    Animator.PlayMoveAnima(this.input);
                }
                else
                {
                    Character.SetSubState(CharacterNormalState.Idle);
                }
                if (this.input.x != 0 || this.input.y != 0) latestDirection = this.input;
            }
        }
    }

    public virtual void Path(Vector3 point)
    {
        AStarManager.Instance.RequestPath(new PathRequest(Character.Position, point, seeker, Vector2Int.one, SetPath));
    }

    public void SetPath(Path path)
    {
        this.path = path;
        vectorPath = AStarManager.Instance.SimplifyPath(Vector2Int.one, path?.path);
        curPathIndex = 1;
    }

    private int curPathIndex;
    private Vector3 GetDirectionFromPath()
    {
        if (!HasPath)
        {
            Debug.Log("aaaa");
            return Vector3.zero;
        }
        else
        {
            if (curPathIndex >= 0 && curPathIndex < vectorPath.Count)
            {
                if (Vector3.Distance(Character.Position, vectorPath[curPathIndex]) <= 1)
                    curPathIndex++;
                if (curPathIndex >= 0 && curPathIndex < vectorPath.Count)
                {
                    Debug.Log("bbbb");
                    return (vectorPath[curPathIndex] - Character.Position).normalized;
                }
                else
                {
                    Debug.Log("cccc");
                    path = null;
                    vectorPath = null;
                    return Vector3.zero;
                }
            }
            else
            {
                Debug.Log("dddd");
                return Vector3.zero;
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
                    if (!HasPath)
                    {
                        Move();
                    }
                    else
                    {
                        Move(GetDirectionFromPath());
                    }
                    if (!mRigidbody) MoveTransform(moveSpeed * Time.deltaTime * (Vector3)input, input, moveSpeed);
                    break;
                case CharacterState.Busy:
                    switch ((CharacterBusyState)sub)
                    {
                        case CharacterBusyState.Dash:
                            if (!mRigidbody)
                            {
                                Character.SetState(CharacterState.Normal, CharacterNormalState.Idle);
                                MoveTransform(dashForce * latestDirection, latestDirection, moveSpeed * dashForce);
                            }
                            break;
                        case CharacterBusyState.Roll:
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
                    case CharacterState.Normal:
                        mRigidbody.velocity = input * moveSpeed;
                        break;
                    case CharacterState.Busy:
                        switch ((CharacterBusyState)sub)
                        {
                            case CharacterBusyState.Dash:
                                Vector2 addtive = dashForce * latestDirection;
                                if (mRigidbody.collisionDetectionMode == CollisionDetectionMode2D.Discrete)
                                {
                                    var hit = Physics2D.Raycast(mRigidbody.position, latestDirection, addtive.magnitude, raycastLayer);
                                    if (hit.collider) addtive = hit.point - mRigidbody.position;
                                }
                                mRigidbody.MovePosition(mRigidbody.position + addtive);
                                Character.SetState(CharacterState.Normal, CharacterNormalState.Idle);
                                break;
                            case CharacterBusyState.Roll:
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
        path = null;
        vectorPath = null;
    }

    public virtual void SetCharacter(Character character)
    {
        Character = character;
        Character.SetController(this);
    }
}
