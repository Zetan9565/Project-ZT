using UnityEngine;
using ZetanExtends;
using ZetanStudio.BehaviourTree;

public abstract class PathMovement : Action
{
    [DisplayName("停止距离")]
    public SharedFloat arriveDistance = 1;
    [DisplayName("选点距离")]
    public SharedFloat pickNextWaypointDist = 1;

    protected PathAgent pathAgent;
    protected bool pathFailed;

    public override bool IsValid
    {
        get
        {
            return arriveDistance != null && arriveDistance.IsValid && pickNextWaypointDist != null && pickNextWaypointDist.IsValid;
        }
    }

    protected override void OnAwake()
    {
        pathAgent = gameObject.GetOrAddComponent<PathAgent>();
        pathAgent.stoppingDistance = arriveDistance;
        pathAgent.pickNextWaypointDist = pickNextWaypointDist;
    }
    protected override void OnEnd()
    {
        Stop();
    }
    protected override void OnReset()
    {
        Stop();
    }

    public void SetDestination(Vector3 destination)
    {
        if (pathAgent)
        {
            pathAgent.IsStopped = false;
            pathAgent.SetDestination(destination);
            pathFailed = false;
        }
        else pathFailed = true;
    }
    public void Stop()
    {
        pathAgent.IsStopped = true;
    }
    protected bool HasArrive()
    {
        if (!pathAgent) return true;
        else return pathAgent.HasArrive;
    }
}