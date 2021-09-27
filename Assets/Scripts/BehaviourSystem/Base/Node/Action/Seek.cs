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
    [DisplayName("寻路频率")]
    public SharedFloat repathRate = 0.5f;

    private float pathTime;
    private bool repath;

    public override bool IsValid => base.IsValid && (point != null && point.IsValid || target != null && target.IsValid) && radius != null && radius.IsValid && repathRate != null && repathRate.IsValid;

    protected override void OnStart()
    {
        base.OnStart();
        SetDestination(GetTarget());
        pathTime = Time.time;
        repath = pathAgent.autoRepath;
        pathAgent.autoRepath = false;
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
    protected override void OnEnd()
    {
        pathAgent.autoRepath = repath;
    }
    public override void OnDrawGizmosSelected()
    {
        if (transform) ZetanUtility.DrawGizmosCircle(transform.position, radius);
    }

    private Vector3 GetTarget()
    {
        if (target.Value) return target.Value.transform.position;
        return point;
    }
}