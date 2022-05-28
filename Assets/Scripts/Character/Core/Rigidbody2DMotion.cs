using UnityEngine;
using ZetanStudio.Extension;

[AddComponentMenu("Zetan Studio/2D刚体运动控制")]
[RequireComponent(typeof(Rigidbody2D))]
public class Rigidbody2DMotion : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;
    public Rigidbody2D Rigidbody => _rigidbody;

    public LayerMask raycastLayer = 0;

    private void Awake()
    {
        _rigidbody = this.GetComponentInFamily<Rigidbody2D>();
    }

    public void SetVelocity(Vector2 velocity)
    {
        _rigidbody.velocity = velocity;
    }

    public void SetPosition(Vector2 position)
    {
        Vector2 addtive = position - _rigidbody.position;
        if (_rigidbody.collisionDetectionMode == CollisionDetectionMode2D.Discrete)
        {
            var hit = Physics2D.Raycast(_rigidbody.position, addtive, addtive.magnitude, raycastLayer);
            if (hit.collider) addtive = hit.point - _rigidbody.position;
        }
        _rigidbody.MovePosition(_rigidbody.position + addtive);
    }

    private void OnDrawGizmosSelected()
    {
        if (_rigidbody) Gizmos.DrawRay(_rigidbody.position, _rigidbody.velocity);
    }
}