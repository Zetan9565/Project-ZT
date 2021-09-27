using UnityEngine;
using ZetanStudio.BehaviourTree;

public class MoveToPoint : Action
{
    [DisplayName("目标点")]
    public SharedVector3 point = Vector3.zero;
    [DisplayName("停止距离")]
    public SharedFloat stopDistance = 1.0f;

    private CharacterControlInput controller;

    public override bool IsValid => point != null && point.IsValid;

    protected override void OnAwake()
    {
        controller = GetComponentInParent<CharacterControlInput>();
    }

    protected override NodeStates OnUpdate()
    {
        if (!controller) return NodeStates.Failure;
        controller.SetMoveInput(GetDirection());
        if (!Arrive()) return NodeStates.Running;
        else return NodeStates.Success;
    }

    protected override void OnEnd()
    {
        controller.SetMoveInput(Vector2.zero);
    }

    private bool Arrive()
    {
        return Vector3.Distance(point, transform.position) <= stopDistance;
    }

    private Vector3 GetDirection()
    {
        return (point - transform.position).normalized;
    }

    public override void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(point, 0.5f);
    }
}