using System.Collections;
using UnityEngine;
using ZetanStudio.BehaviourTree;

[NodeDescription("寻路至目标结点：沿着找到的路径移动至目标点")]
public class Seek : PathMovement
{
    [DisplayName("目标点")]
    public SharedVector3 point;
    [DisplayName("目标")]
    public SharedGameObject target;
    [DisplayName("半径")]
    public SharedFloat radius = 1;

    private float pathTime;

    public override bool IsValid => base.IsValid && point != null && point.IsValid;

    protected override void OnStart()
    {
        base.OnStart();
        SetDestination(GetTarget());
        pathTime = Time.time;
    }
    protected override NodeStates OnUpdate()
    {
        if (pathFailed) return NodeStates.Failure;
        if (HasArrive()) return NodeStates.Success;
        if (pathTime + repathRate <= Time.time)
        {
            pathTime = Time.time;
            SetDestination(GetTarget());
        }
        return NodeStates.Running;
    }
    public override void OnDrawGizmosSelected()
    {
        if (transform) ZetanUtility.DrawGizmosCircle(transform.position, radius, Vector3.forward);
    }

    private Vector3 GetTarget()
    {
        if (target.Value) return target.Value.transform.position;
        return point;
    }
}