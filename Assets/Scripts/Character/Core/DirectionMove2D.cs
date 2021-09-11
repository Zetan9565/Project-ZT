using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterAnimator))]
public class DirectionMove2D : MonoBehaviour, IDirectionMove
{
    [SerializeField]
    private Rigidbody2D mRigidbody;

    private Vector2 moveDiretion;
    public float speed;

    public void Move(Vector2 direction)
    {
        moveDiretion = direction;
    }

    private void Update()
    {
        if (!mRigidbody)
        {
            transform.position += (Vector3)moveDiretion * speed * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (mRigidbody)
        {
            mRigidbody.velocity = moveDiretion * speed;
        }
    }
}