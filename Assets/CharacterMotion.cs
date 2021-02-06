using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterMotion : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("动画控制器")]
#endif
    private Animator animator;
    public Animator Animator => animator;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("2D 刚体")]
#endif
    private Rigidbody2D mRigidbody;

#if UNITY_EDITOR
    [DisplayName("移动速度")]
#endif
    public float moveSpeed = 5.0f;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("Horizontal参数名")]
#endif
    private string animaHorizontal = "Horizontal";
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("Vertical参数名")]
#endif
    private string animaVertical = "Vertical";

    public Character character;

    void Awake()
    {
        animator = GetComponent<Animator>();
        AStarUnit unit = GetComponent<AStarUnit>();
        if (unit) unit.onAnima += SetMoveAnima;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Move(Vector2 input)
    {
        if (moveSpeed < 0) return;
        if (!character || !character.Data || character.Data.CanMove)
        {
            Vector2 move = (Vector2)transform.position + input.normalized * moveSpeed * Time.deltaTime;
            if (mRigidbody) mRigidbody.MovePosition(move);
            else transform.position = move;
            SetMoveAnima(input.normalized);
        }
    }

    public void SetMoveAnima(Vector2 input)
    {
        if (input != Vector2.zero)
        {
            Animator.SetInteger("aState", (int)CharacterState.Walk);
            Animator.SetFloat(animaHorizontal, input.x);
            Animator.SetFloat(animaVertical, input.y);
        }
        else
        {
            Animator.SetInteger("aState", (int)CharacterState.Idle);
        }
    }
}