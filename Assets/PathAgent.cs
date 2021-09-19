using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(CharacterController2D), typeof(Seeker))]
public class PathAgent : MonoBehaviour
{
    [DisplayName("停止距离")]
    public float stoppingDistance = 1;
    [DisplayName("选点距离")]
    public float pickNextWaypointDist = 1;
    [DisplayName("自动寻路")]
    public bool autoRepath = true;
    [DisplayName("寻路间隔")]
    public float repathRate = 0.5f;

    public Vector3 destination { get; private set; }
    public Vector3 pathEndPosition { get; set; }
    public Path path { get; private set; }
    public float remainingDistance => (destination - transform.position).magnitude;
    public bool hasPath => path != null && path.vectorPath.Count > 0;
    public Vector3 desiredDirection
    {
        get
        {
            if (isStopped) return Vector3.zero;
            else return (waypoint - transform.position).normalized;
        }
    }

    private bool _isStopped;
    public bool isStopped
    {
        get => _isStopped;
        set
        {
            if (!value) controller.Move(Vector2.zero);
            _isStopped = value;
        }
    }
    public bool hasArrive { get; private set; }

    private Seeker seeker;
    private int waypointIndex;
    private Vector3 waypoint;
    private CharacterController2D controller;
    private float timer;

    private void Awake()
    {
        controller = GetComponent<CharacterController2D>();
        seeker = GetComponent<Seeker>();
        waypointIndex = 0;
        destination = transform.position;
        waypoint = pathEndPosition = destination;
        CheckArrive();
    }

    private void Update()
    {
        CheckArrive();
        CalNextWaypoint();
        if (!isStopped)
        {
            if (hasPath && !hasArrive) controller.Move(desiredDirection);
            else controller.Move(Vector2.zero);
        }
        timer += Time.deltaTime;
        if (timer >= repathRate)
        {
            timer = 0;
            if (autoRepath) SetDestination(destination);
        }
    }

    public void SetDestination(Vector3 destination)
    {
        if (seeker)
        {
            AStarManager.Instance.RequestPath(new PathRequest(transform.position, destination, seeker, Vector2Int.one, OnPathComplete));
            this.destination = destination;
            hasArrive = false;
        }
    }
    private void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            this.path = path;
            waypointIndex = 0;
            waypoint = path.vectorPath[waypointIndex];
            pathEndPosition = path.vectorPath[path.vectorPath.Count - 1];
        }
    }
    private void CalNextWaypoint()
    {
        if (!hasPath) return;
        bool isLast = waypointIndex >= path.vectorPath.Count - 1;
        if (isLast && (waypoint - transform.position).sqrMagnitude <= stoppingDistance * stoppingDistance)
        {
            waypoint = destination;
            return;
        }
        if (!isLast && (waypoint - transform.position).sqrMagnitude <= pickNextWaypointDist * pickNextWaypointDist)
        {
            waypointIndex++;
            if (waypointIndex >= path.vectorPath.Count) waypoint = destination;
            else waypoint = path.vectorPath[waypointIndex];
        }
    }
    public void CheckArrive()
    {
        if (!hasArrive && remainingDistance <= stoppingDistance) hasArrive = true;
        else if (hasArrive && remainingDistance > stoppingDistance) hasArrive = false;
    }
}
