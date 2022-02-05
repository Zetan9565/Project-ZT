using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.BehaviourTree;

[NodeDescription("定点巡逻：按给定的导航点定点巡逻")]
public class Patrol : PathMovement
{
    [DisplayName("随机选点")]
    public bool random;
    [DisplayName("导航点")]
    public SharedVector3List waypoints = new List<Vector3>();
    [DisplayName("巡逻间隔(秒)")]
    public SharedFloat interval = 1f;

    private int waypointIndex;
    private float waypointReachedTime = -1;

    public override bool IsValid => base.IsValid && waypoints != null && waypoints.IsValid;

    protected override void OnStart()
    {
        base.OnStart();
        float distance = Mathf.Infinity;
        float localDistance;
        for (int i = 0; i < waypoints.Value.Count; ++i)
        {
            if ((localDistance = Vector3.Magnitude(transform.position - waypoints.Value[i])) < distance)
            {
                distance = localDistance;
                waypointIndex = i;
            }
        }
        waypointReachedTime = -1;
        SetDestination(Target());
    }

    protected override NodeStates OnUpdate()
    {
        if (waypoints.Value.Count > 0)
        {
            if (HasArrive())
            {
                if (waypointReachedTime == -1)
                    waypointReachedTime = Time.time;
                if (waypointReachedTime + interval.Value <= Time.time)
                {
                    CalculateWaypoint();
                    SetDestination(Target());
                    waypointReachedTime = -1;
                }
            }
            return NodeStates.Running;
        }
        return NodeStates.Failure;
    }

    private void CalculateWaypoint()
    {
        if (random)
        {
            if (waypoints.Value.Count == 1) waypointIndex = 0;
            else
            {
                int indexBef = waypointIndex;
                while (waypointIndex == indexBef)
                {
                    waypointIndex = Random.Range(0, waypoints.Value.Count);
                }
            }
        }
        else waypointIndex = (waypointIndex + 1) % waypoints.Value.Count;
    }

    private Vector3 Target()
    {
        if (waypointIndex >= 0 && waypointIndex < waypoints.Value.Count)
            return waypoints.Value[waypointIndex];
        else return transform.position;
    }

    public override void OnDrawGizmosSelected()
    {
        if (waypoints != null)
        {
            foreach (var waypoint in waypoints.Value)
            {
                Gizmos.DrawSphere(waypoint, 0.5f);
            }
        }
    }
}