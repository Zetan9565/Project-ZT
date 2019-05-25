using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("2D 刚体")]
#endif
    private Rigidbody2D rigidbd;

#if UNITY_EDITOR
    [DisplayName("移动速度")]
#endif
    public float moveSpeed = 5.0f;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("动画控制器")]
#endif
    private Animator animator;
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("Horizontal参数名")]
#endif
    private string animHorizontal = "Horizontal";
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("Vertical参数名")]
#endif
    private string animaVertical = "Vertical";
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("Magnitude参数名")]
#endif
    private string animaMagnitude = "Move";

    public void Move(Vector2 input)
    {
        if (moveSpeed < 0) return;
        rigidbd.velocity = new Vector2(input.x * moveSpeed, input.y * moveSpeed);
        SetAnima(input);
    }

    public void SetAnima(Vector2 input)
    {
        animator.SetFloat(animaMagnitude, input.magnitude);
        if (input != Vector2.zero)
        {
            animator.SetFloat(animHorizontal, input.x);
            animator.SetFloat(animaVertical, input.y);
        }
    }
}
