using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("刚体")]
#endif
    private Rigidbody2D rigidbd;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("移动速度")]
#endif
    private float moveSpeed = 5.0f;

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

    public void Move(Vector2 dir)
    {
        rigidbd.velocity = new Vector2(dir.x * moveSpeed, dir.y * moveSpeed);
        SetAnima(dir);
    }

    void SetAnima(Vector2 dir)
    {
        animator.SetFloat(animaMagnitude, dir.magnitude);
        if (dir != Vector2.zero)
        {
            animator.SetFloat(animHorizontal, dir.x);
            animator.SetFloat(animaVertical, dir.y);
        }
    }
}
