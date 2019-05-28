using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/AI/A*寻路管理器")]
public class AStarManager : MonoBehaviour
{
    private static AStarManager instance;
    public static AStarManager Instance
    {
        get
        {
            if (!instance || instance.gameObject)
                instance = FindObjectOfType<AStarManager>();
            return instance;
        }
    }

    #region Gizmos相关
    [SerializeField]
    private bool gizmosEdge = true;
    [SerializeField]
    private Color edgeColor = Color.white;

    [SerializeField]
    private bool gizmosGrid = true;
    [SerializeField]
    private Color gridColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.15f);

    [SerializeField]
    private bool gizmosCast = true;
    [SerializeField]
    private Color castColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.1f);
    #endregion

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

    [SerializeField]
    private float worldHeight = 20.0f;

    [SerializeField, Tooltip("以单元格尺寸倍数为单位，至少是 1 倍")]
    private int maxUnitHeight = 2;

    [SerializeField]
    private LayerMask groundLayer = ~0;

    [SerializeField, Tooltip("以单元格倍数为单位，至少是 1 倍")]
    private int[] unitSizes;

    [SerializeField]
    private LayerMask unwalkableLayer = ~0;
    public LayerMask UnwalkableLayer
    {
        get
        {
            return unwalkableLayer;
        }
    }

    [SerializeField, Tooltip("以单元格倍数为单位"), Range(0.25f, 0.5f)]
    private float castRadiusMultiple = 0.5f;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("方形[精度较低]", "球形[性能较低]")]
#endif
    private CastCheckType castCheckType = CastCheckType.Box;

    #region 实时变量
    private readonly Dictionary<int, AStar> AStars = new Dictionary<int, AStar>();

    private readonly Queue<PathResult> results = new Queue<PathResult>();
    #endregion

    #region 网格相关
    private void CreateAStars()
    {
        foreach (int unitSize in unitSizes)
            CreateAStar(unitSize);
    }

    private void CreateAStar(int unitSize)
    {
        if (unitSize < 1 || AStars.ContainsKey(unitSize)) return;
        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();

        Vector3 axisOrigin;
        if (!ThreeD)
            axisOrigin = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0)
                - Vector3.right * (worldSize.x / 2) - Vector3.up * (worldSize.y / 2);
        else
            axisOrigin = new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z))
                - Vector3.right * (worldSize.x / 2) - Vector3.forward * (worldSize.y / 2);

        AStar AStar = new AStar(worldSize, BaseCellSize * unitSize, castCheckType, castRadiusMultiple, UnwalkableLayer, groundLayer,
                               ThreeD, worldHeight, maxUnitHeight);
        AStar.CreateGrid(axisOrigin);
        AStars.Add(unitSize, AStar);

        //stopwatch.Stop();
        //Debug.Log("为规格为 " + unitSize + " 的单位建立寻路基础，耗时 " + stopwatch.ElapsedMilliseconds + "ms");
    }

    public void UpdateAStars()
    {
        foreach (AStar AStar in AStars.Values)
        {
            AStar.RefreshGrid();
        }
    }

    public void UpdateAStars(Vector3 fromPoint, Vector3 toPoint)
    {
        foreach (AStar AStar in AStars.Values)
        {
            AStar.RefreshGrid(fromPoint, toPoint);
        }
    }

    public void UpdateAStar(int unitSize)
    {
        if (unitSize < 1 || !AStars.ContainsKey(unitSize)) return;
        AStars[unitSize].RefreshGrid();
    }

    public void UpdateAStar(Vector3 fromPoint, Vector3 toPoint, int unitSize)
    {
        if (unitSize < 1 || !AStars.ContainsKey(unitSize)) return;
        AStars[unitSize].RefreshGrid(fromPoint, toPoint);
    }

    public AStarNode WorldPointToNode(Vector3 position, int unitSize)
    {
        if (unitSize < 1) return null;
        if (!AStars.ContainsKey(unitSize))
        {
            CreateAStar(unitSize);
        }
        return AStars[unitSize].WorldPointToNode(position);
    }

    public bool WorldPointWalkable(Vector3 point, int unitSize)
    {
        if (unitSize < 1) return false;
        if (!AStars.ContainsKey(unitSize))
        {
            CreateAStar(unitSize);
        }
        return AStars[unitSize].WorldPointWalkable(point);
    }
    #endregion

    #region 路径相关
    public bool TryGetPath(Vector3 startPos, Vector3 goalPos, int unitSize = 1)
    {
        if (unitSize < 1)
        {
            return false;
        }
        if (!AStars.ContainsKey(unitSize))
        {
            CreateAStar(unitSize);
        }
        return AStars[unitSize].TryGetPath(startPos, goalPos);
    }

    public bool TryGetPath(Vector3 startPos, Vector3 goalPos, out IEnumerable<Vector3> pathResult, int unitSize = 1)
    {
        if (unitSize < 1)
        {
            pathResult = null;
            return false;
        }
        if (!AStars.ContainsKey(unitSize))
        {
            CreateAStar(unitSize);
        }
        return AStars[unitSize].TryGetPath(startPos, goalPos, out pathResult);
    }

    public void RequestPath(PathRequest request)
    {
        if (!AStars.ContainsKey(request.unitSize))
        {
            CreateAStar(request.unitSize);
        }
        lock (AStars[request.unitSize])
            ((ThreadStart)delegate
                    {
                        AStars[request.unitSize].FindPath(request, GetResult);
                    }).Invoke();
    }

    public void GetResult(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);
        }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        CreateAStars();
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int resultsCount = results.Count;
            lock (results)
                for (int i = 0; i < resultsCount; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.waypoints, result.findSuccessfully);
                }
        }
    }

    private void OnDrawGizmos()
    {
        if (gizmosGrid)
        {
            if (AStars != null && AStars.ContainsKey(1))
            {
                AStarNode[,] Grid = AStars[1].Grid;
                for (int x = 0; x < AStars[1].GridSize.x; x++)
                    for (int y = 0; y < AStars[1].GridSize.y; y++)
                    {
                        Gizmos.color = gridColor;
                        if (!Grid[x, y].walkable)
                        {
                            Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, gridColor.a);
                            Gizmos.DrawCube(Grid[x, y], ThreeD ? Vector3.one : new Vector3(1, 1, 0) * BaseCellSize * 0.95f);
                        }
                        else if (!ThreeD) Gizmos.DrawWireCube(Grid[x, y], new Vector3(1, 1, 0) * BaseCellSize);
                        else Gizmos.DrawCube(Grid[x, y], Vector3.one * BaseCellSize * 0.95f);
                        if (gizmosCast)
                        {
                            Gizmos.color = castColor;
                            if (castCheckType == CastCheckType.Sphere) Gizmos.DrawWireSphere(Grid[x, y], BaseCellSize * castRadiusMultiple);
                            else Gizmos.DrawWireCube(Grid[x, y], Vector3.one * BaseCellSize * castRadiusMultiple * 2);
                        }
                    }
            }
            else
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
                        Gizmos.color = gridColor;
                        if (!ThreeD)
                        {
                            nodeWorldPos = axisOrigin + Vector3.right * (x + 0.5f) * BaseCellSize + Vector3.up * (y + 0.5f) * BaseCellSize;
                            Gizmos.DrawWireCube(nodeWorldPos, new Vector3(1, 1, 0) * BaseCellSize);
                        }
                        else
                        {
                            nodeWorldPos = axisOrigin + Vector3.right * (x + 0.5f) * BaseCellSize + Vector3.forward * (y + 0.5f) * BaseCellSize;
                            float height;
                            if (Physics.Raycast(nodeWorldPos + Vector3.up * (worldHeight + 0.01f), Vector3.down, out RaycastHit hit, worldHeight + 0.01f, groundLayer))
                                height = hit.point.y;
                            else height = worldHeight + 0.01f;
                            if (height > worldHeight) Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, gridColor.a);
                            nodeWorldPos += Vector3.up * height;
                            Gizmos.DrawCube(nodeWorldPos, Vector3.one * BaseCellSize * 0.95f);
                        }
                        if (gizmosCast)
                        {
                            Gizmos.color = castColor;
                            if (castCheckType == CastCheckType.Sphere) Gizmos.DrawWireSphere(nodeWorldPos, BaseCellSize * castRadiusMultiple);
                            else Gizmos.DrawWireCube(nodeWorldPos, Vector3.one * BaseCellSize * castRadiusMultiple * 2);
                        }
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
    #endregion
}
public class AStar
{
    public AStar(Vector2 worldSize, float cellSize, CastCheckType castCheckType, float castRadiusMultiple, LayerMask unwalkableLayer, LayerMask groundLayer, bool threeD, float worldHeight, float maxUnitHeight)
    {
        this.worldSize = worldSize;
        this.cellSize = cellSize;
        this.castCheckType = castCheckType;
        this.castRadiusMultiple = castRadiusMultiple;
        this.unwalkableLayer = unwalkableLayer;
        this.groundLayer = groundLayer;
        this.threeD = threeD;
        this.worldHeight = worldHeight;
        this.maxUnitHeight = maxUnitHeight;
    }

    private readonly bool threeD;
    private readonly Vector2 worldSize;
    private readonly float worldHeight;
    private readonly LayerMask groundLayer;
    private readonly float maxUnitHeight;

    private readonly float cellSize;
    public Vector2Int GridSize { get; private set; }
    public AStarNode[,] Grid { get; private set; }

    private readonly LayerMask unwalkableLayer;
    private readonly CastCheckType castCheckType;
    private readonly float castRadiusMultiple;

    #region 网格相关
    public void CreateGrid(Vector3 axisOrigin)
    {
        GridSize = Vector2Int.RoundToInt(worldSize / cellSize);
        Grid = new AStarNode[Mathf.RoundToInt(GridSize.x), Mathf.RoundToInt(GridSize.y)];
        int row = Mathf.Min(GridSize.x, GridSize.y);//量小的做行，貌似稍微提升遍历性能
        int col = Mathf.Max(GridSize.x, GridSize.y);
        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                CreateNode(axisOrigin, i, j);
        RefreshGrid();
    }

    private void CreateNode(Vector3 axisOrigin, int gridX, int gridY)
    {
        Vector3 nodeWorldPos;
        if (!threeD)
        {
            nodeWorldPos = axisOrigin + Vector3.right * (gridX + 0.5f) * cellSize + Vector3.up * (gridY + 0.5f) * cellSize;
        }
        else
        {
            nodeWorldPos = axisOrigin + Vector3.right * (gridX + 0.5f) * cellSize + Vector3.forward * (gridY + 0.5f) * cellSize;
            float height;
            if (Physics.Raycast(nodeWorldPos + Vector3.up * (worldHeight + 0.01f), Vector3.down, out RaycastHit hit, worldHeight + 0.01f, groundLayer))
                height = hit.point.y;
            else height = worldHeight + 0.01f;
            nodeWorldPos += Vector3.up * height;
        }
        Grid[gridX, gridY] = new AStarNode(nodeWorldPos, gridX, gridY, threeD ? nodeWorldPos.y : 0);
    }

    public void RefreshGrid()
    {
        if (Grid == null) return;
        int row = Mathf.Min(GridSize.x, GridSize.y);
        int col = Mathf.Max(GridSize.x, GridSize.y);
        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                CheckNodeWalkable(Grid[i, j]);
        CalculateConnections();
    }

    public void RefreshGrid(Vector3 fromPoint, Vector3 toPoint)
    {
        if (Grid == null) return;
        AStarNode fromNode = WorldPointToNode(fromPoint);
        AStarNode toNode = WorldPointToNode(toPoint);
        if (fromNode == toNode)
        {
            CheckNodeWalkable(fromNode);
            return;
        }
        AStarNode min = fromNode.gridPosition.x <= toNode.gridPosition.x && fromNode.gridPosition.y <= toNode.gridPosition.y ? fromNode : toNode;
        AStarNode max = fromNode.gridPosition.x > toNode.gridPosition.x && fromNode.gridPosition.y > toNode.gridPosition.y ? fromNode : toNode;
        fromNode = min;
        toNode = max;
        //Debug.Log(string.Format("From {0} to {1}", fromNode.GridPosition, toNode.GridPosition));
        if (toNode.gridPosition.x - fromNode.gridPosition.x <= toNode.gridPosition.y - fromNode.gridPosition.y)
            for (int i = fromNode.gridPosition.x; i <= toNode.gridPosition.x; i++)
                for (int j = fromNode.gridPosition.y; j <= toNode.gridPosition.y; j++)
                    CheckNodeWalkable(Grid[i, j]);
        else for (int i = fromNode.gridPosition.y; i <= toNode.gridPosition.y; i++)
                for (int j = fromNode.gridPosition.x; j <= toNode.gridPosition.x; j++)
                    CheckNodeWalkable(Grid[i, j]);
        CalculateConnections();
    }

    public AStarNode WorldPointToNode(Vector3 position)
    {
        if (Grid == null || (threeD && position.y > worldHeight)) return null;
        int gX = Mathf.RoundToInt((GridSize.x - 1) * Mathf.Clamp01((position.x + worldSize.x / 2) / worldSize.x));
        //gX怎么算：首先利用坐标系原点的 x 坐标来修正该点的 x 轴坐标，然后除以世界宽度，获得该点的 x 坐标在网格坐标系上所处的区域，用不大于 1 分数来表示，
        //然后获得相同区域的网格的 x 坐标即 gX，举个例子：
        //假设 x 坐标为 -2，而世界起点的 x 坐标为 -24 即实际坐标原点 x 坐标 0 减去世界宽度的一半，则修正 x 坐标为 x + 24 = 22，这就是它在A*坐标系上虚拟的修正了的位置 x'， 
        //以上得知世界宽度为48，那么 22 / 48 = 11/24，说明 x' 在世界宽度轴 11/24 的位置上，所以，该位置相应的格子的 x 坐标也是网格宽度轴的11/24，
        //假设网格宽度为也为48，则 gX = 48 * 11/24 = 22，看似正确，其实，假设上面算得的 x' 是48，那么 48 * 48/48 = 48，而网格坐标最多到47，因为数组从0开始，
        //所以这时就会发生越界错误，反而用 gX = (48 - 1) * 48/48 = 47 * 1 = 47 来代替就对了，回过头来，x' 是 22，则 47 * 11/24 = 21.54 ≈ 22，位于 x' 轴数起第 23 个格子上，
        //假设算出的 x' 是 30，则 gX = 47 * 15/24 = 29.38 ≈ 29，完全符合逻辑，为什么不用 48 算完再减去 1 ？如果 x' 是 0， 48 * 0/48 - 1 = -1，又越界了
        int gY;
        if (!threeD) gY = Mathf.RoundToInt((GridSize.y - 1) * Mathf.Clamp01((position.y + worldSize.y / 2) / worldSize.y));
        else gY = Mathf.RoundToInt((GridSize.y - 1) * Mathf.Clamp01((position.z + worldSize.y / 2) / worldSize.y));
        return Grid[gX, gY];
    }

    private bool CheckNodeWalkable(AStarNode node)
    {
        if (!node) return false;
        if (!threeD)
        {
            RaycastHit2D[] hit2Ds = new RaycastHit2D[0];
            switch (castCheckType)
            {
                case CastCheckType.Box:
                    hit2Ds = Physics2D.BoxCastAll(node.worldPosition,
                        Vector2.one * cellSize * castRadiusMultiple * 2, 0, Vector2.zero, Mathf.Infinity, unwalkableLayer);
                    break;
                case CastCheckType.Sphere:
                    hit2Ds = Physics2D.CircleCastAll(node.worldPosition,
                        cellSize * castRadiusMultiple, Vector2.zero, Mathf.Infinity, unwalkableLayer);
                    break;
            }
            node.walkable = hit2Ds.Length < 1 || hit2Ds.Where(h => !h.collider.isTrigger && h.collider.tag != "Player").Count() < 1;
            return node.walkable;
        }
        else
        {
            RaycastHit[] hits = new RaycastHit[0];
            switch (castCheckType)
            {
                case CastCheckType.Box:
                    hits = Physics.BoxCastAll(node.worldPosition, Vector3.one * cellSize * castRadiusMultiple,
                        Vector3.up, Quaternion.identity, (maxUnitHeight - 1) * cellSize, unwalkableLayer, QueryTriggerInteraction.Ignore);
                    break;
                case CastCheckType.Sphere:
                    hits = Physics.SphereCastAll(node.worldPosition, cellSize * castRadiusMultiple,
                        Vector3.up, (maxUnitHeight - 1) * cellSize, unwalkableLayer, QueryTriggerInteraction.Ignore);
                    break;
            }
            node.walkable = node.Height < worldHeight && (hits.Length < 1 || hits.Where(h => h.collider.tag != "Player").Count() < 1);
            return node.walkable;
        }
    }

    public bool WorldPointWalkable(Vector3 point)
    {
        return CheckNodeWalkable(WorldPointToNode(point));
    }

    private AStarNode GetClosestSurroundingNode(AStarNode node, AStarNode closestTo, int ringCount = 1)
    {
        var neighbours = GetSurroundingNodes(node, ringCount);
        if (ringCount >= Mathf.Max(GridSize.x, GridSize.y)) return null;//突破递归
        AStarNode closest = neighbours.FirstOrDefault(x => x.walkable);
        if (closest)
            using (var neighbourEnum = neighbours.GetEnumerator())
            {
                while (neighbourEnum.MoveNext())
                    if (neighbourEnum.Current == closest) break;
                while (neighbourEnum.MoveNext())
                {
                    AStarNode neighbour = neighbourEnum.Current;
                    if (Vector3.Distance(closestTo.worldPosition, neighbour.worldPosition) < Vector3.Distance(closestTo.worldPosition, closest.worldPosition))
                        if (neighbour.walkable) closest = neighbour;
                }
            }
        if (!closest) return GetClosestSurroundingNode(node, closestTo, ringCount + 1);
        else return closest;
    }

    private List<AStarNode> GetSurroundingNodes(AStarNode node, int ringCount/*圈数*/)
    {
        List<AStarNode> neighbours = new List<AStarNode>();
        if (node != null && ringCount > 0)
        {
            int neiborX;
            int neiborY;
            for (int x = -ringCount; x <= ringCount; x++)
                for (int y = -ringCount; y <= ringCount; y++)
                {
                    if (Mathf.Abs(x) < ringCount && Mathf.Abs(y) < ringCount) continue;//对于圈内的结点，总有其x和y都小于圈数，所以依此跳过

                    neiborX = node.gridPosition.x + x;
                    neiborY = node.gridPosition.y + y;

                    if (neiborX >= 0 && neiborX < GridSize.x && neiborY >= 0 && neiborY < GridSize.y)
                        neighbours.Add(Grid[neiborX, neiborY]);
                }
        }
        return neighbours;
    }

    private List<AStarNode> GetReachableNeighbours(AStarNode node, HashSet<AStarNode> walkableCheckExceptions = null)
    {
        List<AStarNode> neighbours = new List<AStarNode>();
        if (node != null)
        {
            int neiborX;
            int neiborY;
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    neiborX = node.gridPosition.x + x;
                    neiborY = node.gridPosition.y + y;

                    if (neiborX >= 0 && neiborX < GridSize.x && neiborY >= 0 && neiborY < GridSize.y)
                        if (Reachable(Grid[neiborX, neiborY]))
                            neighbours.Add(Grid[neiborX, neiborY]);
                }
        }
        return neighbours;

        bool Reachable(AStarNode neighbour)
        {
            if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(neighbour))
            {
                CheckNodeWalkable(neighbour);
                if (walkableCheckExceptions != null)
                    walkableCheckExceptions.Add(neighbour);
            }
#if true
            if (neighbour.gridPosition.x == node.gridPosition.x || neighbour.gridPosition.y == node.gridPosition.y) return neighbour.walkable;
            else return CanGoStraight(node, neighbour);
#endif
#if false
            if (neighbour.GridPosition.x == node.GridPosition.x || neighbour.GridPosition.y == node.GridPosition.y) return neighbour.Walkable;
            if (!neighbour.Walkable) return false;
            if (neighbour.GridPosition.x > node.GridPosition.x && neighbour.GridPosition.y > node.GridPosition.y)//右上角
            {
                int leftX = node.GridPosition.x;
                int leftY = neighbour.GridPosition.y;
                AStarNode leftNode = Grid[leftX, leftY];
                if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(leftNode))
                {
                    CheckNodeWalkable(leftNode);
                    if (walkableCheckExceptions != null)
                        walkableCheckExceptions.Add(leftNode);
                }
                if (leftNode.Walkable)
                {
                    int downX = neighbour.GridPosition.x;
                    int downY = node.GridPosition.y;
                    AStarNode downNode = Grid[downX, downY];
                    if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(downNode))
                    {
                        CheckNodeWalkable(downNode);
                        if (walkableCheckExceptions != null)
                            walkableCheckExceptions.Add(downNode);
                    }
                    if (!downNode.Walkable) return false;
                    else return true;
                }
                else return false;
            }
            else if (neighbour.GridPosition.x > node.GridPosition.x && neighbour.GridPosition.y < node.GridPosition.y)//右下角
            {
                int leftX = node.GridPosition.x;
                int leftY = neighbour.GridPosition.y;
                AStarNode leftNode = Grid[leftX, leftY];
                if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(leftNode))
                {
                    CheckNodeWalkable(leftNode);
                    if (walkableCheckExceptions != null)
                        walkableCheckExceptions.Add(leftNode);
                }
                if (leftNode.Walkable)
                {
                    int upX = neighbour.GridPosition.x;
                    int upY = node.GridPosition.y;
                    AStarNode upNode = Grid[upX, upY];
                    if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(upNode))
                    {
                        CheckNodeWalkable(upNode);
                        if (walkableCheckExceptions != null)
                            walkableCheckExceptions.Add(upNode);
                    }
                    if (!upNode.Walkable) return false;
                    else return true;
                }
                else return false;
            }
            else if (neighbour.GridPosition.x < node.GridPosition.x && neighbour.GridPosition.y > node.GridPosition.y)//左上角
            {
                int rightX = node.GridPosition.x;
                int rightY = neighbour.GridPosition.y;
                AStarNode rightNode = Grid[rightX, rightY];
                if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(rightNode))
                {
                    CheckNodeWalkable(rightNode);
                    if (walkableCheckExceptions != null)
                        walkableCheckExceptions.Add(rightNode);
                }
                if (rightNode.Walkable)
                {
                    int downX = neighbour.GridPosition.x;
                    int downY = node.GridPosition.y;
                    AStarNode downNode = Grid[downX, downY];
                    if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(downNode))
                    {
                        CheckNodeWalkable(downNode);
                        if (walkableCheckExceptions != null)
                            walkableCheckExceptions.Add(downNode);
                    }
                    if (!downNode.Walkable) return false;
                    else return true;
                }
                else return false;
            }
            else if (neighbour.GridPosition.x < node.GridPosition.x && neighbour.GridPosition.y < node.GridPosition.y)//左下角
            {
                int rightX = node.GridPosition.x;
                int rightY = neighbour.GridPosition.y;
                AStarNode rightNode = Grid[rightX, rightY];
                if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(rightNode))
                {
                    CheckNodeWalkable(rightNode);
                    if (walkableCheckExceptions != null)
                        walkableCheckExceptions.Add(rightNode);
                }
                if (rightNode.Walkable)
                {
                    int upX = neighbour.GridPosition.x;
                    int upY = node.GridPosition.y;
                    AStarNode upNode = Grid[upX, upY];
                    if (walkableCheckExceptions == null || !walkableCheckExceptions.Contains(upNode))
                    {
                        CheckNodeWalkable(upNode);
                        if (walkableCheckExceptions != null)
                            walkableCheckExceptions.Add(upNode);
                    }
                    if (!upNode.Walkable) return false;
                    else return true;
                }
                else return false;
            }
            else return true;
#endif
        }
    }

    private bool CanGoStraight(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dis = Vector3.Distance(from, to);
        if (!threeD)
        {
            RaycastHit2D[] hit2Ds = new RaycastHit2D[0];
            switch (castCheckType)
            {
                case CastCheckType.Box:
                    hit2Ds = Physics2D.BoxCastAll(from, Vector2.one * cellSize * castRadiusMultiple * 2, 0, dir, dis, unwalkableLayer);
                    break;
                case CastCheckType.Sphere:
                    hit2Ds = Physics2D.CircleCastAll(from, cellSize * castRadiusMultiple, dir, dis, unwalkableLayer);
                    break;
            }
            return hit2Ds.Where(h => h.collider && !h.collider.isTrigger && h.collider.tag != "Player").Count() < 1;
        }
        else
        {
            RaycastHit[] hits = new RaycastHit[0];
            switch (castCheckType)
            {
                case CastCheckType.Box:
                    hits = Physics.BoxCastAll(from, Vector3.one * cellSize * castRadiusMultiple + Vector3.up * (maxUnitHeight - 1) * cellSize * castRadiusMultiple,
                                              dir, Quaternion.identity, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                    break;
                case CastCheckType.Sphere:
                    if (maxUnitHeight == 1)
                        hits = Physics.SphereCastAll(from, cellSize * castRadiusMultiple, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                    else hits = Physics.CapsuleCastAll(from, from + Vector3.up * (maxUnitHeight - 1) * cellSize, cellSize * castRadiusMultiple,
                                                       dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
                    break;
            }
            return hits.Where(h => h.collider && h.collider.tag != "Player").Count() < 1;
        }
    }

    private AStarNode GetEffectiveGoalByDiagonal(AStarNode startNode, AStarNode goalNode)
    {
        AStarNode newGoalNode = null;
        int startNodeNodeX = startNode.gridPosition.x, startNodeY = startNode.gridPosition.y;
        int goalNodeX = goalNode.gridPosition.x, goalNodeY = goalNode.gridPosition.y;
        int xDistn = Mathf.Abs(goalNode.gridPosition.x - startNode.gridPosition.x);
        int yDistn = Mathf.Abs(goalNode.gridPosition.y - startNode.gridPosition.y);
        float deltaX, deltaY;
        if (xDistn >= yDistn)
        {
            deltaX = 1;
            deltaY = yDistn / (xDistn * 1.0f);
        }
        else
        {
            deltaY = 1;
            deltaX = xDistn / (yDistn * 1.0f);
        }

        bool CheckNodeReachable(float fX, float fY)
        {
            int gX = Mathf.RoundToInt(fX);
            int gY = Mathf.RoundToInt(fY);
            if (Grid[gX, gY].CanReachTo(startNode))
            {
                newGoalNode = Grid[gX, gY];
                return true;
            }
            else return false;
        }

        if (startNodeNodeX >= goalNodeX && startNodeY >= goalNodeY)//起点位于终点右上角
            for (float x = goalNodeX + deltaX, y = goalNodeY + deltaY; x <= startNodeNodeX && y <= startNodeY; x += deltaX, y += deltaY)
            {
                if (CheckNodeReachable(x, y)) break;
            }
        else if (startNodeNodeX >= goalNodeX && startNodeY <= goalNodeY)//起点位于终点右下角
            for (float x = goalNodeX + deltaX, y = goalNodeY - deltaY; x <= startNodeNodeX && y >= startNodeY; x += deltaX, y -= deltaY)
            {
                if (CheckNodeReachable(x, y)) break;
            }
        else if (startNodeNodeX <= goalNodeX && startNodeY >= goalNodeY)//起点位于终点左上角
            for (float x = goalNodeX - deltaX, y = goalNodeY + deltaY; x >= startNodeNodeX && y <= startNodeY; x -= deltaX, y += deltaY)
            {
                if (CheckNodeReachable(x, y)) break;
            }
        else if (startNodeNodeX <= goalNodeX && startNodeY <= goalNodeY)//起点位于终点左下角
            for (float x = goalNodeX - deltaX, y = goalNodeY - deltaY; x >= startNodeNodeX && y >= startNodeY; x -= deltaX, y -= deltaY)
            {
                if (CheckNodeReachable(x, y)) break;
            }
        return newGoalNode;
    }

    private AStarNode GetEffectiveGoalByRing(AStarNode startNode, AStarNode goalNode, int ringCount = 1)
    {
        var neighbours = GetSurroundingNodes(goalNode, ringCount);
        if (ringCount >= Mathf.Max(GridSize.x, GridSize.y)) return null;//突破递归
        AStarNode newGoalNode = neighbours.FirstOrDefault(x => x.CanReachTo(startNode));
        if (newGoalNode)
            using (var neighbourEnum = neighbours.GetEnumerator())
            {
                while (neighbourEnum.MoveNext())
                    if (neighbourEnum.Current == newGoalNode) break;
                while (neighbourEnum.MoveNext())
                {
                    AStarNode neighbour = neighbourEnum.Current;
                    if (Vector3.Distance(goalNode.worldPosition, neighbour.worldPosition) < Vector3.Distance(goalNode.worldPosition, newGoalNode.worldPosition))
                        if (neighbour.CanReachTo(startNode)) newGoalNode = neighbour;
                }
            }
        if (!newGoalNode) return GetEffectiveGoalByRing(startNode, goalNode, ringCount + 1);
        else return newGoalNode;
    }
    #endregion

    #region 路径相关
    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        AStarNode startNode = WorldPointToNode(request.start);
        AStarNode goalNode = WorldPointToNode(request.goal);
        if (startNode == null || goalNode == null)
        {
            stopwatch.Stop();
            callback(new PathResult(null, false, request.callback));
            return;
        }

        IEnumerable<Vector3> pathResult = null;
        bool findSuccessfully = false;

        if (!goalNode.walkable)
        {
            goalNode = GetClosestSurroundingNode(goalNode, startNode);
            if (goalNode == default)
            {
                stopwatch.Stop();
                callback(new PathResult(null, false, request.callback));
                //Debug.Log("找不到合适的终点" + request.goal);
                return;
            }
        }
        if (!startNode.walkable)
        {
            startNode = GetClosestSurroundingNode(startNode, goalNode);
            if (startNode == default)
            {
                stopwatch.Stop();
                callback(new PathResult(null, false, request.callback));
                //Debug.Log("找不到合适的起点" + request.start);
                return;
            }
        }
        if (startNode == goalNode)
        {
            stopwatch.Stop();
            callback(new PathResult(null, false, request.callback));
            //Debug.Log("起始相同");
            return;
        }
        if (!startNode.CanReachTo(goalNode))
        {
            goalNode = GetEffectiveGoalByRing(startNode, goalNode);
            if (!goalNode)
            {
                stopwatch.Stop();
                //Debug.Log("检测到目的地不可到达");
                callback(new PathResult(pathResult, false, request.callback));
                return;
            }
        }

        //Debug.Log("起点网格: " + startNode.GridPosition);
        //Debug.Log("终点网格: " + goalNode.GridPosition);

        if (CanGoStraight(startNode, goalNode))
        {
            findSuccessfully = true;
            pathResult = new Vector3[] { goalNode };
            stopwatch.Stop();
            //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过直走找到路径");
        }

        if (!findSuccessfully)
        {
            Heap<AStarNode> openList = new Heap<AStarNode>(GridSize.x * GridSize.y);
            HashSet<AStarNode> closedList = new HashSet<AStarNode>();
            HashSet<AStarNode> checkedNodes = new HashSet<AStarNode>();
            openList.Add(startNode);
            while (openList.Count > 0)
            {
                AStarNode current = openList.RemoveRoot();
                if (current == default || current == null)
                {
                    callback(new PathResult(null, false, request.callback));
                    return;
                }
                closedList.Add(current);

                if (current == goalNode)
                {
                    findSuccessfully = true;
                    stopwatch.Stop();
                    //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过搜索 " + closedList.Count + " 个结点找到路径");
                    break;
                }

                using (var nodeEnum = GetReachableNeighbours(current, checkedNodes).GetEnumerator())
                    while (nodeEnum.MoveNext())
                    {
                        AStarNode neighbour = nodeEnum.Current;
                        if (!neighbour.walkable || closedList.Contains(neighbour)) continue;
                        int costStartToNeighbour = current.GCost + current.CalculateHCostTo(neighbour);
                        if (costStartToNeighbour < neighbour.GCost || !openList.Contains(neighbour))
                        {
                            neighbour.GCost = costStartToNeighbour;
                            neighbour.HCost = neighbour.CalculateHCostTo(goalNode);
                            neighbour.parent = current;
                            if (!openList.Contains(neighbour))
                                openList.Add(neighbour);
                        }
                    }
            }
            if (!findSuccessfully)
            {
                stopwatch.Stop();
                //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，搜索 " + closedList.Count + " 个结点后找不到路径");
            }
            else
            {
                stopwatch.Start();
                goalNode.HeapIndex = -1;
                startNode.HeapIndex = -1;
                AStarNode pathNode = goalNode;
                List<AStarNode> path = new List<AStarNode>();
                while (pathNode != startNode)
                {
                    path.Add(pathNode);
                    AStarNode temp = pathNode;
                    pathNode = pathNode.parent;
                    temp.parent = null;
                    temp.HeapIndex = -1;
                }
                pathResult = GetWaypoints(path);
                stopwatch.Stop();
                //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过搜索 " + closedList.Count + " 个结点后取得实际路径");
            }
        }
        callback(new PathResult(pathResult, findSuccessfully, request.callback));
    }

    public bool TryGetPath(Vector3 start, Vector3 goal, out IEnumerable<Vector3> pathResult)
    {
        pathResult = null;
        AStarNode startNode = WorldPointToNode(start);
        AStarNode goalNode = WorldPointToNode(goal);
        if (startNode == null || goalNode == null) return false;

        if (!goalNode.walkable)
        {
            goalNode = GetClosestSurroundingNode(goalNode, startNode);
            if (!goalNode) return false;
        }
        if (!startNode.walkable)
        {
            startNode = GetClosestSurroundingNode(startNode, goalNode);
            if (!startNode) return false;
        }
        if (startNode == goalNode) return false;
        if (!startNode.CanReachTo(goalNode))
        {
            goalNode = GetEffectiveGoalByRing(startNode, goalNode);
            if (!goalNode) return false;
        }
        if (CanGoStraight(startNode, goalNode))
        {
            pathResult = new Vector3[] { goalNode };
            return true;
        }

        bool findSuccessfully = false;
        Heap<AStarNode> openList = new Heap<AStarNode>(GridSize.x * GridSize.y);
        HashSet<AStarNode> closedList = new HashSet<AStarNode>();
        HashSet<AStarNode> checkedNodes = new HashSet<AStarNode>();
        openList.Add(startNode);
        AStarNode current;
        while (openList.Count > 0)
        {
            current = openList.RemoveRoot();
            if (current == default || current == null) return false;
            closedList.Add(current);

            if (current == goalNode)
            {
                findSuccessfully = true;
                break;
            }

            using (var nodeEnum = GetReachableNeighbours(current, checkedNodes).GetEnumerator())
                while (nodeEnum.MoveNext())
                {
                    AStarNode neighbour = nodeEnum.Current;
                    if (!neighbour.walkable || closedList.Contains(neighbour)) continue;
                    int disToNeighbour = current.GCost + current.CalculateHCostTo(neighbour);
                    if (disToNeighbour < neighbour.GCost || !openList.Contains(neighbour))
                    {
                        neighbour.GCost = disToNeighbour;
                        neighbour.HCost = neighbour.CalculateHCostTo(goalNode);
                        neighbour.parent = current;
                        if (!openList.Contains(neighbour)) openList.Add(neighbour);
                    }
                }
        }
        if (findSuccessfully)
        {
            goalNode.HeapIndex = -1;
            startNode.HeapIndex = -1;
            AStarNode pathNode = goalNode;
            List<AStarNode> path = new List<AStarNode>();
            while (pathNode != startNode)
            {
                path.Add(pathNode);
                AStarNode temp = pathNode;
                pathNode = pathNode.parent;
                temp.parent = null;
                temp.HeapIndex = -1;
            }
            pathResult = GetWaypoints(path);
        }
        return findSuccessfully;
    }

    public bool TryGetPath(Vector3 start, Vector3 goal)
    {
        AStarNode startNode = WorldPointToNode(start);
        AStarNode goalNode = WorldPointToNode(goal);
        if (startNode == null || goalNode == null) return false;

        if (!goalNode.walkable)
        {
            goalNode = GetClosestSurroundingNode(goalNode, startNode);
            if (!goalNode) return false;
        }
        if (!startNode.walkable)
        {
            startNode = GetClosestSurroundingNode(startNode, goalNode);
            if (!startNode) return false;
        }
        if (startNode == goalNode) return false;
        if (!startNode.CanReachTo(goalNode))
        {
            goalNode = GetEffectiveGoalByRing(startNode, goalNode);
            if (!goalNode) return false;
        }
        if (CanGoStraight(startNode, goalNode))
        {
            return true;
        }

        bool findSuccessfully = false;
        Heap<AStarNode> openList = new Heap<AStarNode>(GridSize.x * GridSize.y);
        HashSet<AStarNode> closedList = new HashSet<AStarNode>();
        HashSet<AStarNode> checkedNodes = new HashSet<AStarNode>();
        openList.Add(startNode);
        AStarNode current;
        while (openList.Count > 0)
        {
            current = openList.RemoveRoot();
            if (current == default || current == null) return false;
            closedList.Add(current);

            if (current == goalNode)
            {
                findSuccessfully = true;
                break;
            }

            using (var nodeEnum = GetReachableNeighbours(current, checkedNodes).GetEnumerator())
                while (nodeEnum.MoveNext())
                {
                    AStarNode neighbour = nodeEnum.Current;
                    if (!neighbour.walkable || closedList.Contains(neighbour)) continue;
                    int disToNeighbour = current.GCost + current.CalculateHCostTo(neighbour);
                    if (disToNeighbour < neighbour.GCost || !openList.Contains(neighbour))
                    {
                        neighbour.GCost = disToNeighbour;
                        neighbour.HCost = neighbour.CalculateHCostTo(goalNode);
                        neighbour.parent = current;
                        if (!openList.Contains(neighbour)) openList.Add(neighbour);
                    }
                }
        }
        if (findSuccessfully)
        {
            goalNode.HeapIndex = -1;
            startNode.HeapIndex = -1;
            AStarNode pathNode = goalNode;
            List<AStarNode> path = new List<AStarNode>();
            while (pathNode != startNode)
            {
                path.Add(pathNode);
                AStarNode temp = pathNode;
                pathNode = pathNode.parent;
                temp.parent = null;
                temp.HeapIndex = -1;
            }
        }
        return findSuccessfully;
    }

    private List<Vector3> GetWaypoints(List<AStarNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        if (path.Count < 1) return waypoints;
        PathToWaypoints();
        StraightenPath();
        waypoints.Reverse();
        return waypoints;

        void PathToWaypoints(bool simplify = true)
        {
            if (simplify)
            {
                Vector2 oldDir = Vector3.zero;
                for (int i = 1; i < path.Count; i++)
                {
                    Vector2 newDir = path[i - 1].gridPosition - path[i].gridPosition;
                    if (newDir != oldDir)//方向不一样时才使用前面的点
                        waypoints.Add(path[i - 1]);
                    else if (i == path.Count - 1) waypoints.Add(path[i]);//即使方向一样，也强制把起点也加进去
                    oldDir = newDir;
                }
            }
            else foreach (AStarNode node in path)
                    waypoints.Add(node);
        }

        void StraightenPath()
        {
            if (waypoints.Count < 1) return;
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            List<Vector3> toRemove = new List<Vector3>();
            Vector3 from = waypoints[0];
            for (int i = 2; i < waypoints.Count; i++)
                if (CanGoStraight(from, waypoints[i]))
                    toRemove.Add(waypoints[i - 1]);
                else from = waypoints[i - 1];
            foreach (Vector3 point in toRemove)
                waypoints.Remove(point);
            //stopwatch.Stop();
            //Debug.Log("耗时 " + stopwatch.Elapsed.TotalMilliseconds + "ms完成路径直化");
        }
    }
    #endregion

    #region 四邻域连通检测算法
    private readonly Dictionary<int, HashSet<AStarNode>> Connections = new Dictionary<int, HashSet<AStarNode>>();

    private void CalculateConnections()
    {
        Connections.Clear();//重置连通域字典

        int[,] gridData = new int[GridSize.x, GridSize.y];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                if (Grid[x, y].walkable) gridData[x, y] = 1;//大于0表示可行走
                else gridData[x, y] = 0;//0表示有障碍

        int label = 1;
        for (int y = 0; y < GridSize.y; y++)//从下往上扫
        {
            for (int x = 0; x < GridSize.x; x++)//从左往右扫
            {
                //若该数据的标记不为0，即该数据表示的位置没有障碍
                if (gridData[x, y] != 0)
                {
                    int labelNeedToChange = 0;//记录需要更改的标记
                    if (y == 0)//第一行，只用看当前数据点的左边
                    {
                        //若该点是第一行第一个，前面已判断不为0，直接标上标记
                        if (x == 0)
                        {
                            gridData[x, y] = label;
                            label++;
                        }
                        else if (gridData[x - 1, y] != 0)//若该点的左侧数据的标记不为0，那么当前数据的标记标为左侧的标记，表示同属一个连通域
                            gridData[x, y] = gridData[x - 1, y];
                        else//否则，标上新标记
                        {
                            gridData[x, y] = label;
                            label++;
                        }
                    }
                    else if (x == 0)//网格最左边，不可能出现与左侧形成衔接的情况
                    {
                        if (gridData[x, y - 1] != 0) gridData[x, y] = gridData[x, y - 1]; //若下方数据不为0，则当前数据标上下方标记的标记
                        else
                        {
                            gridData[x, y] = label;
                            label++;
                        }
                    }
                    else if (gridData[x, y - 1] != 0)//若下方标记不为0
                    {
                        gridData[x, y] = gridData[x, y - 1];//则用下方标记来标记当前数据
                        if (gridData[x - 1, y] != 0) labelNeedToChange = gridData[x - 1, y]; //若左方数据不为0，则被左方标记所标记的数据都要更改
                    }
                    else if (gridData[x - 1, y] != 0)//若左侧不为0
                        gridData[x, y] = gridData[x - 1, y];//则用左侧标记来标记当前数据
                    else
                    {
                        gridData[x, y] = label;
                        label++;
                    }

                    if (!Connections.ContainsKey(gridData[x, y])) Connections.Add(gridData[x, y], new HashSet<AStarNode>());
                    Connections[gridData[x, y]].Add(Grid[x, y]);
                    //将对应网格结点的连通域标记标为当前标记，操作可选，若不操作，则在寻路检测可达性时使用下面的ACanReachB()
                    Grid[x, y].connectionLabel = gridData[x, y];

                    //如果有需要更改的标记，且其与当前标记不同
                    if (labelNeedToChange > 0 && labelNeedToChange != gridData[x, y])
                    {
                        foreach (AStarNode node in Connections[labelNeedToChange])//把对应连通域合并到当前连通域
                        {
                            gridData[node.gridPosition.x, node.gridPosition.y] = gridData[x, y];
                            Connections[gridData[x, y]].Add(node);
                            node.connectionLabel = gridData[x, y];//操作可选
                        }
                        Connections[labelNeedToChange].Clear();
                        Connections.Remove(labelNeedToChange);
                    }
                }
            }
        }
    }

    private bool ACanReachB(AStarNode nodeA, AStarNode nodeB)
    {
        if (!nodeA || !nodeB) return false;
        var first = Connections.FirstOrDefault(x => x.Value.Contains(nodeA));
        if (default(KeyValuePair<string, AStarNode>).Equals(first)) return false;
        var second = Connections.FirstOrDefault(x => x.Value.Contains(nodeB));
        if (default(KeyValuePair<string, AStarNode>).Equals(second)) return false;
        return first.Key == second.Key;
    }
    #endregion

    public static implicit operator bool(AStar self)
    {
        return self != null;
    }
}

public class AStarNode : IHeapItem<AStarNode>
{
    public readonly Vector3 worldPosition;
    public readonly Vector2Int gridPosition;
    public bool walkable;

    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get { return GCost + HCost; } }
    public float Height { get; private set; }

    public AStarNode parent;

    public int connectionLabel;

    public int HeapIndex { get; set; }

    public AStarNode(Vector3 position, int gridX, int gridY, float height)
    {
        worldPosition = position;
        gridPosition = new Vector2Int(gridX, gridY);
        Height = height;
        walkable = true;
    }

    public int CalculateHCostTo(AStarNode other)
    {
        //使用曼哈顿距离
        int disX = Mathf.Abs(gridPosition.x - other.gridPosition.x);
        int disY = Mathf.Abs(gridPosition.y - other.gridPosition.y);

        if (disX > disY)
            return 14 * disY + 10 * (disX - disY) + Mathf.RoundToInt(Mathf.Abs(Height - other.Height));
        else return 14 * disX + 10 * (disY - disX) + Mathf.RoundToInt(Mathf.Abs(Height - other.Height));
    }

    public bool CanReachTo(AStarNode other)
    {
        return connectionLabel > 0 && connectionLabel == other.connectionLabel;
    }

    public int CompareTo(AStarNode other)
    {
        if (FCost < other.FCost || (FCost == other.FCost && HCost < other.HCost) || (FCost == other.FCost && HCost == other.HCost && Height < other.Height))
            return -1;
        else if (FCost == other.FCost && HCost == other.HCost && Height == Height) return 0;
        else return 1;
    }

    public static implicit operator Vector3(AStarNode self)
    {
        return self.worldPosition;
    }

    public static implicit operator Vector2(AStarNode self)
    {
        return self.worldPosition;
    }

    public static implicit operator bool(AStarNode self)
    {
        return self != null;
    }
}

public struct PathRequest
{
    public readonly Vector3 start;
    public readonly Vector3 goal;
    public readonly int unitSize;
    public readonly Action<IEnumerable<Vector3>, bool> callback;

    public PathRequest(Vector3 start, Vector3 goal, int unitSize, Action<IEnumerable<Vector3>, bool> callback)
    {
        this.start = start;
        this.goal = goal;
        this.unitSize = unitSize;
        this.callback = callback;
    }
}

public struct PathResult
{
    public readonly IEnumerable<Vector3> waypoints;
    public readonly bool findSuccessfully;
    public readonly Action<IEnumerable<Vector3>, bool> callback;

    public PathResult(IEnumerable<Vector3> waypoints, bool findSuccessfully, Action<IEnumerable<Vector3>, bool> callback)
    {
        this.waypoints = waypoints;
        this.findSuccessfully = findSuccessfully;
        this.callback = callback;
    }
}

public delegate void PathFoundListner(IEnumerable<Vector3> waypoints, bool findSuccessfully);

public enum CastCheckType
{
    Box,
    Sphere
}