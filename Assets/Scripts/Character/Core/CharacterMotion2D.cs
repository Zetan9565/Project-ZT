using System;
using System.Collections;
using UnityEngine;

[AddComponentMenu("Zetan Studio/角色行动控制2D")]
public class CharacterMotion2D : MonoBehaviour, IDirectionMove
{
    [SerializeField]
    private Rigidbody2D mRigidbody;

    public LayerMask raycastLayer = 0;
    public float distanceOffset = 0.5f;
    public float moveSpeed = 7;
    public float dashForce = 5;
    public float rollForce = 5;
    public float rollStopSpeed = 5;
    public float rollSpeedDownMult = 5;
    public Vector2 defaultDirection = Vector2.right;

    private State state;
    private Vector2 moveDirection;
    private Vector2 latestDirection;
    private float rollSpeed;

    private Action rollCallback;
    private Action dashCallback;

    private enum State
    {
        Normal,
        Dash,
        Roll,
    }

    #region MonoBehaviour
    private void Awake()
    {
        latestDirection = defaultDirection;
    }

    void Update()
    {
        switch (state)
        {
            case State.Normal:
                if (!mRigidbody) MoveTransform((Vector3)moveDirection * moveSpeed * Time.deltaTime, moveDirection, moveSpeed);
                break;
            case State.Dash:
                if (!mRigidbody)
                {
                    state = State.Normal;
                    MoveTransform(dashForce * latestDirection, latestDirection, moveSpeed * dashForce);
                    dashCallback?.Invoke();
                    dashCallback = null;
                }
                break;
            case State.Roll:
                rollSpeed -= rollSpeed * rollSpeedDownMult * Time.deltaTime;
                //rollSpeed = Mathf.Lerp(rollSpeed, 0, rollSpeedDownMult * Time.deltaTime);
                if (rollSpeed < rollStopSpeed)
                {
                    state = State.Normal;
                    rollCallback?.Invoke();
                    rollCallback = null;
                    break;
                }
                if (!mRigidbody) MoveTransform((Vector3)latestDirection * rollSpeed * Time.deltaTime, latestDirection, rollSpeed);
                break;
            default:
                break;
        }

        void MoveTransform(Vector3 additive, Vector2 direction, float distance)
        {
            var hit = Physics2D.CircleCast(transform.position, distanceOffset, direction, distance, raycastLayer);
            if (hit.collider) CalAdditive(hit.distance);
            transform.position += additive;

            void CalAdditive(float distance)
            {
                float offsetDistance = distance - distanceOffset;
                if (offsetDistance < 0)
                {
                    additive = Vector3.zero;
                }
                else if (offsetDistance < additive.magnitude)
                    additive = additive.normalized * offsetDistance;
            }
        }
    }

    private void FixedUpdate()
    {
        if (mRigidbody)
        {
            switch (state)
            {
                case State.Normal:
                    mRigidbody.velocity = moveDirection * moveSpeed;
                    break;
                case State.Dash:
                    state = State.Normal;
                    Vector2 addtive = dashForce * latestDirection;
                    if (mRigidbody.collisionDetectionMode == CollisionDetectionMode2D.Discrete)
                    {
                        var hit = Physics2D.Raycast(mRigidbody.position, latestDirection, addtive.magnitude, raycastLayer);
                        if (hit.collider) addtive = hit.point - mRigidbody.position;
                    }
                    mRigidbody.MovePosition(mRigidbody.position + addtive);
                    dashCallback?.Invoke();
                    dashCallback = null;
                    break;
                case State.Roll:
                    mRigidbody.velocity = latestDirection * rollSpeed;
                    break;
                default:
                    break;
            }
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        switch (state)
        {
            case State.Normal:
                DrawLine(moveDirection, moveSpeed);
                break;
            case State.Dash:
                DrawLine(latestDirection, moveSpeed * dashForce);
                break;
            case State.Roll:
                DrawLine(latestDirection, rollSpeed);
                break;
            default:
                break;
        }

        void DrawLine(Vector2 direction, float distance)
        {
            Color colorBef = Gizmos.color;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * distance);
            Vector3 start1 = transform.position + new Vector3(-direction.y, direction.x).normalized * distanceOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start1, start1 + (Vector3)direction * (distance - distanceOffset));
            Vector3 start2 = transform.position + new Vector3(direction.y, -direction.x).normalized * distanceOffset;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(start2, start2 + (Vector3)direction * (distance - distanceOffset));
            Gizmos.color = colorBef;
            var hit = Physics2D.CircleCast(transform.position, distanceOffset, direction, distance, raycastLayer);
            if (hit.collider) Gizmos.DrawSphere(hit.point, 0.5f);
        }
    }
    #endif
    #endregion

    public void Move(Vector2 direction)
    {
        if (state != State.Normal) return;
        moveDirection = direction;
        if (direction.x != 0 || direction.y != 0) latestDirection = direction;
    }

    public void Roll(Vector2 direction, Action callback = null)
    {
        if (state != State.Normal) return;
        state = State.Roll;
        rollSpeed = moveSpeed * rollForce;
        if (direction.x != 0 || direction.y != 0) latestDirection = direction;
        rollCallback = callback;
    }

    public void Dash(Vector2 direction, Action callback = null)
    {
        if (state != State.Normal) return;
        state = State.Dash;
        if (direction.x != 0 || direction.y != 0) latestDirection = direction;
        dashCallback = callback;
    }

    public void ForceStop()
    {
        moveDirection = Vector2.zero;
        if (mRigidbody) mRigidbody.velocity = Vector2.zero;
        switch (state)
        {
            case State.Normal:
                break;
            case State.Dash:
                break;
            case State.Roll:
                state = State.Normal;
                rollCallback?.Invoke();
                rollCallback = null;
                break;
            default:
                break;
        }
    }
}