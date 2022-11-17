using UnityEngine;
using ZetanStudio;
using ZetanStudio.BehaviourTree;
using ZetanStudio.BehaviourTree.Nodes;

[Group("Movement"), Description("随机巡逻：在给定的范围内随机巡逻")]
public class RandomPatrol : PathMovement
{
    [Label("范围而不是边界")]
    public bool useRange = true;
    [Label("范围半径"), HideIf("useRange", false)]
    public SharedFloat range = 10f;
    [Label("盲区半径"), HideIf("useRange", false), Tooltip("不会在此范围内取点")]
    public SharedFloat blindRange = 0f;
    [Label("边界右上角"), HideIf("useRange", true)]
    public SharedVector3 boundMin;
    [Label("边界左下角"), HideIf("useRange", true)]
    public SharedVector3 boundMax;
    [Label("巡逻间隔")]
    public SharedFloat interval = 1f;

    private float waypointReachedTime = -1;

    public override bool IsValid => base.IsValid && (useRange && range != null && range.IsValid && blindRange != null && blindRange.IsValid
        || !useRange && boundMin != null && boundMax != null && boundMin.IsValid && boundMax.IsValid);

    protected override void OnStart()
    {
        base.OnStart();
        SetDestination(Target());
        waypointReachedTime = -1;
    }

    private Vector3 Target()
    {
        if (useRange) return new Vector3(transform.position.x + blindRange + Random.Range(-range + blindRange, range - blindRange),
            transform.position.y + blindRange + Random.Range(-range + blindRange, range - blindRange));
        else return new Vector3(Random.Range(boundMin.Value.x, boundMax.Value.x), Random.Range(boundMin.Value.y, boundMax.Value.y));
    }

    protected override NodeStates OnUpdate()
    {
        if (HasArrive())
        {
            if (waypointReachedTime == -1)
                waypointReachedTime = Time.time;
            if (waypointReachedTime + interval.Value <= Time.time)
            {
                SetDestination(Target());
                waypointReachedTime = -1;
            }
        }
        return NodeStates.Running;
    }

    public override void OnDrawGizmosSelected()
    {
        if (useRange && transform) Gizmos.DrawWireSphere(transform.position, range);
        else
        {
            Gizmos.DrawWireCube(Utility.CenterBetween(boundMin, boundMax), Utility.SizeBetween(boundMin, boundMax));
        }
    }
}