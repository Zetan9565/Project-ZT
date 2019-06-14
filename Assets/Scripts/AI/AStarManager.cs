using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AstarPath))]
[AddComponentMenu("ZetanStudio/AI/A*寻路管理器")]
public class AStarManager : SingletonMonoBehaviour<AStarManager>
{
    #region Gizmos相关
    [SerializeField]
    private bool gizmosPriview = true;
    [SerializeField]
    private Color priviewColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.15f);

    [SerializeField]
    private bool gizmosEdge = true;
    [SerializeField]
    private Color edgeColor = Color.white;

    [SerializeField]
    private bool gizmosGrid = true;
    [SerializeField]
    private Color gridColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.15f);

    [SerializeField]
    private bool expendGraphs = true;
    #endregion

    #region 初始化相关变量
    [SerializeField, Tooltip("长和宽都推荐使用2的幂数")]
    private Vector2 worldSize = new Vector2(48, 48);

    [SerializeField]
    private bool threeD;
    public bool ThreeD
    {
        get
        {
            return threeD;
        }
    }

    [SerializeField, Range(0.2f, 2f)]
    private float baseCellSize = 1;
    public float BaseCellSize
    {
        get
        {
            return baseCellSize;
        }
    }

    [SerializeField, Tooltip("以单元格倍数为单位，至少是 1 倍，2D空间下 Y 数值无效")]
    private Vector2Int[] unitSizes = new Vector2Int[] { Vector2Int.one };

    [SerializeField]
    private float worldHeight = 20.0f;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private LayerMask unwalkableLayer;

    [SerializeField, Tooltip("以单元格倍数为单位"), Range(0.25f, 0.5f)]
    private float castRadiusMultiple = 0.5f;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("球形", "胶囊体", "点射线")]
#endif
    private ColliderType castCheckType = ColliderType.Capsule;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("无", "普通", "附加", "游戏中", "错误")]
#endif
    private PathLog pathLog;
    #endregion

    #region 实时变量
    public AstarPath PathFinder
    {
        get
        {
            if (!AstarPath.active)
            {
                AstarPath.active = FindObjectOfType<AstarPath>();
                if (!AstarPath.active)
                    AstarPath.active = gameObject.AddComponent<AstarPath>();
            }
            return AstarPath.active;
        }
    }

    public readonly Dictionary<Vector2Int, NavGraph> graphs = new Dictionary<Vector2Int, NavGraph>();
    #endregion

    #region 网格相关函数
    private void CreateGraphs()
    {
        foreach (Vector2Int unitSize in unitSizes)
            CreateGraph(unitSize);
    }

    private void CreateGraph(Vector2Int unitSize)
    {
        if (unitSize.x < 1 || unitSize.y < 1 || graphs.ContainsKey(unitSize)) return;

        var gridGragh = PathFinder.data.AddGraph(typeof(GridGraph)) as GridGraph;
        gridGragh.SetDimensions(Mathf.RoundToInt(worldSize.x / (unitSize.x * BaseCellSize)), Mathf.RoundToInt(worldSize.y / (unitSize.x * BaseCellSize)), unitSize.x * BaseCellSize);
        gridGragh.name = unitSize.ToString();
        gridGragh.center = Vector3.zero;
        gridGragh.rotation = ThreeD ? gridGragh.rotation : new Vector3(gridGragh.rotation.y - 90, 270, 90);
        gridGragh.cutCorners = false;
        gridGragh.collision.mask = unwalkableLayer;
        gridGragh.collision.type = castCheckType;
        if (ThreeD)
        {
            gridGragh.collision.fromHeight = worldHeight;
            gridGragh.collision.height = unitSize.y * BaseCellSize;
            gridGragh.collision.heightMask = groundLayer;
            gridGragh.collision.heightCheck = true;
        }
        gridGragh.collision.use2D = !ThreeD;
        gridGragh.collision.diameter = castRadiusMultiple * 2;
        gridGragh.Scan();
        graphs.Add(unitSize, gridGragh);
    }

    public void UpdateGraphs()
    {
        PathFinder.Scan();
    }

    public void UpdateGraphs(Vector3 fromPoint, Vector3 toPoint)
    {
        PathFinder.UpdateGraphs(new Bounds(MyUtilities.CenterOf(fromPoint, toPoint), MyUtilities.SizeOf(fromPoint, toPoint) / 2));
    }
    #endregion

    #region 路径相关函数
    public void FindPath(PathRequest request)
    {
        if (!graphs.ContainsKey(request.unitSize))
        {
            CreateGraph(request.unitSize);
        }
        request.seeker.StartPath(request.start, request.goal, request.pathCallback, GraphMask.FromGraph(graphs[request.unitSize]));
    }

    public List<Vector3> SimplifyPath(Vector2Int unitSize, List<GraphNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        if (path.Count < 1) return waypoints;
        PathToWaypoints();
        StraightenPath();
        return waypoints;

        void PathToWaypoints(bool simplify = true)
        {
            if (simplify)
            {
                Vector2 oldDir = Vector3.zero;
                for (int i = 2; i < path.Count; i++)
                {
                    Vector2 newDir = new Vector2(path[i - 1].position.x, ThreeD ? path[i - 1].position.z : path[i - 1].position.y)
                        - new Vector2(path[i].position.x, ThreeD ? path[i].position.z : path[i].position.y);
                    if (newDir != oldDir)//方向不一样时才使用前面的点
                        waypoints.Add((Vector3)path[i - 1].position);
                    else if (i == path.Count - 1) waypoints.Add((Vector3)path[i].position);//即使方向一样，也强制把终点点也加进去
                    oldDir = newDir;
                }
            }
            else foreach (GraphNode node in path)
                    waypoints.Add((Vector3)node.position);
        }

        void StraightenPath()
        {
            if (waypoints.Count < 1) return;
            List<Vector3> toRemove = new List<Vector3>();
            Vector3 from = waypoints[0];
            for (int i = 2; i < waypoints.Count; i++)
                if (CanGoStraight(unitSize, from, waypoints[i]))
                    toRemove.Add(waypoints[i - 1]);
                else from = waypoints[i - 1];
            foreach (Vector3 point in toRemove)
                waypoints.Remove(point);
        }
    }

    private bool CanGoStraight(Vector2Int unitSize, Vector3 from, Vector3 to)
    {
        int cellSize = unitSize.x;
        int cellHeight = unitSize.y;
        Vector3 dir = (to - from).normalized;
        float dis = Vector3.Distance(from, to);
        float checkRadius = cellSize * castRadiusMultiple;
        if (!threeD)
        {
            bool hit = Physics2D.Raycast(from, dir, dis, unwalkableLayer);
            if (!hit)//射不中，则进行第二次检测
            {
                float x1 = -dir.y / dir.x;
                Vector3 point1 = from + new Vector3(x1, 1).normalized * checkRadius;
                bool hit1 = Physics2D.Raycast(point1, dir, dis, unwalkableLayer);
                if (!hit1)//射不中，进行第三次检测
                {
                    float x2 = dir.y / dir.x;
                    Vector3 point2 = from + new Vector3(x2, -1).normalized * checkRadius;
                    bool hit2 = Physics2D.Raycast(point2, dir, dis, unwalkableLayer);
                    if (!hit2) return true;
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        else
        {
            bool hit = Physics.Raycast(from, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
            if (!hit)
            {
                float x1 = -dir.z / dir.x;
                Vector3 point1 = from + new Vector3(x1, 0, 1).normalized * checkRadius;//左边
                bool hit1 = Physics.Raycast(point1, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                if (!hit1)
                {
                    float x2 = dir.z / dir.x;
                    Vector3 point2 = from + new Vector3(x2, 0, -1).normalized * checkRadius;//右边
                    bool hit2 = Physics.Raycast(point2, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                    if (!hit2)
                    {
                        float x3 = -dir.y / dir.x;
                        Vector3 point3 = from + new Vector3(x3, 1, 0).normalized * checkRadius;//底部
                        bool hit3 = Physics.Raycast(point3, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                        if (!hit3)
                        {
                            for (int i = 1; i <= cellHeight; i++)//上部
                            {
                                float x4 = -dir.y / dir.x;
                                Vector3 point4 = from + new Vector3(x4, -1, 0).normalized * (checkRadius * (1 + 2 * (i - 1)));
                                bool hit4 = Physics.Raycast(point4, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                                if (hit4) return false;
                            }
                            return true;
                        }
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        PathFinder.hideFlags = HideFlags.NotEditable;
    }

    private void Start()
    {
        CreateGraphs();
    }

    private void OnDrawGizmos()
    {
        if (gizmosPriview)
        {
            Vector2Int gridSize = Vector2Int.RoundToInt(worldSize / BaseCellSize);
            Vector3 nodeWorldPos;
            Vector3 axisOrigin;
            if (!ThreeD)
                axisOrigin = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0)
                    - Vector3.right * (worldSize.x / 2) - Vector3.up * (worldSize.y / 2);
            else
                axisOrigin = new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z))
                    - Vector3.right * (worldSize.x / 2) - Vector3.forward * (worldSize.y / 2);

            for (int x = 0; x < gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (!ThreeD)
                    {
                        Gizmos.color = priviewColor;
                        nodeWorldPos = axisOrigin + Vector3.right * (x + 0.5f) * BaseCellSize + Vector3.up * (y + 0.5f) * BaseCellSize;
                        bool hitUnwkbl;
                        switch (castCheckType)
                        {
                            case ColliderType.Sphere:
                            case ColliderType.Capsule:
                                hitUnwkbl = Physics2D.OverlapCircle(nodeWorldPos, BaseCellSize * castRadiusMultiple, unwalkableLayer);
                                break;
                            case ColliderType.Ray:
                                hitUnwkbl = Physics2D.Raycast(nodeWorldPos, Vector2.zero, 0, unwalkableLayer);
                                break;
                            default:
                                hitUnwkbl = false;
                                break;
                        }
                        if (!hitUnwkbl) Gizmos.DrawWireCube(nodeWorldPos, new Vector3(1, 1, 0) * BaseCellSize);
                        else
                        {
                            Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, priviewColor.a);
                            Gizmos.DrawCube(nodeWorldPos, Vector2.one * BaseCellSize);
                        }
                    }
                    else
                    {
                        Gizmos.color = new Color(priviewColor.r, priviewColor.g, priviewColor.b, priviewColor.a * 2f);
                        nodeWorldPos = axisOrigin + Vector3.right * (x + 0.5f) * BaseCellSize + Vector3.forward * (y + 0.5f) * BaseCellSize;
                        float height;
                        if (Physics.Raycast(nodeWorldPos + Vector3.up * (worldHeight + 0.01f), Vector3.down, out RaycastHit hit, worldHeight + 0.01f, groundLayer, QueryTriggerInteraction.Ignore))
                            height = hit.point.y;
                        else height = worldHeight + 0.01f;
                        if (height > worldHeight) Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, priviewColor.a);
                        nodeWorldPos += Vector3.up * height;
                        bool hitUnwkbl;
                        switch (castCheckType)
                        {
                            case ColliderType.Sphere:
                                hitUnwkbl = Physics.CheckSphere(nodeWorldPos, BaseCellSize * castRadiusMultiple, unwalkableLayer, QueryTriggerInteraction.Ignore);
                                break;
                            case ColliderType.Capsule:
                                hitUnwkbl = Physics.CheckCapsule(nodeWorldPos, nodeWorldPos + Vector3.up * (worldHeight - nodeWorldPos.y),
                                    BaseCellSize * castRadiusMultiple, unwalkableLayer, QueryTriggerInteraction.Ignore);
                                break;
                            case ColliderType.Ray:
                                hitUnwkbl = Physics.Raycast(nodeWorldPos + Vector3.up * worldHeight, Vector3.down, worldHeight, unwalkableLayer, QueryTriggerInteraction.Ignore);
                                break;
                            default:
                                hitUnwkbl = false;
                                break;
                        }
                        if (!hitUnwkbl) Gizmos.DrawCube(nodeWorldPos, Vector3.one * BaseCellSize * 0.95f);
                        else
                        {
                            Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, priviewColor.a * 2f);
                            Gizmos.DrawCube(nodeWorldPos, Vector3.one * BaseCellSize * 0.95f);
                        }
                    }
                }
            if (gizmosEdge)
            {
                Gizmos.color = edgeColor;
                if (ThreeD) Gizmos.DrawWireCube(transform.position + Vector3.up * worldHeight / 2, new Vector3(worldSize.x, worldHeight, worldSize.y));
                else Gizmos.DrawWireCube(transform.position, new Vector3(worldSize.x, worldSize.y, 0));
            }
        }
    }
    #endregion
}

public struct PathRequest
{
    public readonly Vector3 start;
    public readonly Vector3 goal;
    public readonly Seeker seeker;
    public readonly Vector2Int unitSize;
    public readonly OnPathDelegate pathCallback;

    public PathRequest(Vector3 start, Vector3 goal, Seeker seeker, Vector2Int unitSize, OnPathDelegate callback)
    {
        this.start = start;
        this.goal = goal;
        this.seeker = seeker;
        this.unitSize = unitSize;
        this.pathCallback = callback;
    }
}