using Pathfinding;
using UnityEngine;
using ZetanExtends;

[RequireComponent(typeof(Seeker))]
public class PathAgent : MonoBehaviour
{
    [DisplayName("停止距离")]
    public float stoppingDistance = 1;
    [DisplayName("选点距离")]
    public float pickNextWaypointDist = 1;
    [DisplayName("自动重寻")]
    public bool autoRepath = true;
    [DisplayName("寻路间隔")]
    public float repathRate = 0.5f;

#if UNITY_EDITOR
    [SerializeField, DisplayName("显示Seeker")]
    private bool showSeeker;
    [SerializeField, DisplayName("路径Gizmos")]
    private bool gizmosPath;
    [SerializeField, DisplayName("细节Gizmos")]
    private bool gizmosDetail;
#endif

    public Vector3 Destination { get; private set; }
    public Vector3 PathEndPosition { get; set; }
    public Path Path { get; private set; }
    public float RemainingDistance => (Destination - transform.position).magnitude;
    public bool HasPath => Path != null && Path.vectorPath.Count > 0;
    public Vector3 DesiredDirection
    {
        get
        {
            if (IsStopped) return Vector3.zero;
            else return (waypoint - transform.position).normalized;
        }
    }

    private bool _isStopped;
    public bool IsStopped
    {
        get => _isStopped;
        set
        {
            if (!value) controller.SetMoveInput(Vector2.zero);
            _isStopped = value;
        }
    }
    public bool HasArrive { get; private set; }

    private Seeker seeker;
    private int waypointIndex;
    private Vector3 waypoint;
    private CharacterControlInput controller;
    private float timer;

    private void Awake()
    {
        controller = this.GetOrAddComponent<CharacterControlInput>();
        seeker = GetComponent<Seeker>();
        waypointIndex = 0;
        Destination = transform.position;
        waypoint = PathEndPosition = Destination;
        CheckArrive();
    }

    private void Update()
    {
        CheckArrive();
        CalNextWaypoint();
        if (!IsStopped)
        {
            if (HasPath && !HasArrive) controller.SetMoveInput(DesiredDirection);
            else controller.SetMoveInput(Vector2.zero);
        }
        if (autoRepath)
        {
            timer += Time.deltaTime;
            if (timer >= repathRate)
            {
                timer = 0;
                SetDestination(Destination);
            }
        }
    }

    private void OnValidate()
    {
        if (GetComponent<Seeker>() is Seeker seeker)
        {
            if (!showSeeker) seeker.hideFlags = HideFlags.HideInInspector;
            else seeker.hideFlags = HideFlags.None;
            seeker.drawGizmos = gizmosPath;
            seeker.detailedGizmos = gizmosDetail;
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (GetComponent<Seeker>() is Seeker seeker)
            seeker.hideFlags = HideFlags.None;
#endif
    }

    public void SetDestination(Vector3 destination)
    {
        if (seeker)
        {
            AStarManager.Instance.RequestPath(new PathRequest(transform.position, destination, seeker, Vector2Int.one, OnPathComplete));
            this.Destination = destination;
            HasArrive = false;
        }
    }
    private void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            this.Path = path;
            waypointIndex = 0;
            waypoint = path.vectorPath[waypointIndex];
            PathEndPosition = path.vectorPath[path.vectorPath.Count - 1];
        }
    }
    private void CalNextWaypoint()
    {
        if (!HasPath) return;
        bool isLast = waypointIndex >= Path.vectorPath.Count - 1;
        if (isLast && (waypoint - transform.position).sqrMagnitude <= stoppingDistance * stoppingDistance)
        {
            waypoint = Destination;
            return;
        }
        if (!isLast && (waypoint - transform.position).sqrMagnitude <= pickNextWaypointDist * pickNextWaypointDist)
        {
            waypointIndex++;
            if (waypointIndex >= Path.vectorPath.Count) waypoint = Destination;
            else waypoint = Path.vectorPath[waypointIndex];
        }
    }
    private void CheckArrive()
    {
        if (!HasArrive && (Destination - transform.position).sqrMagnitude <= stoppingDistance * stoppingDistance) HasArrive = true;
        else if (HasArrive && (Destination - transform.position).sqrMagnitude > stoppingDistance * stoppingDistance) HasArrive = false;
    }
}
