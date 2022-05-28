using UnityEngine;
using ZetanStudio.BehaviourTree;
using ZetanStudio.BehaviourTree.Nodes;

[Group("Movement")]
public class MoveToPoint : Action
{
    [Label("目标点")]
    public SharedVector3 point = Vector3.zero;
    [Label("停止距离")]
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
        controller.SetValue(CharacterInputNames.Instance.Move, GetDirection());
        controller.SetValue(CharacterInputNames.Instance.Direction, GetDirection());
        if (!Arrive()) return NodeStates.Running;
        else return NodeStates.Success;
    }

    protected override void OnEnd()
    {
        controller.SetValue(CharacterInputNames.Instance.Move, Vector2.zero);
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