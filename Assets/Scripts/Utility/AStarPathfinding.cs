//namespace Zetan Studio.Pathfinding
//{
//    using Unity.Mathematics;
//    using Unity.Jobs;
//    using Unity.Burst;
//    using Unity.Collections;
//    using UnityEngine;
//    using System.Collections.Generic;
//    using System;

//    [BurstCompile]
//    public struct FindPathJob : IJob
//    {
//        private int2 startGridPos;
//        private int2 goalGridPos;

//        private int2 gridSize;

//        private NativeArray<GridNodeStruct> gridNodes;

//        private NativeList<int2> pathResult;

//        public FindPathJob(int2 startGPos, int2 goalGPos, int2 gridSize, NativeArray<GridNodeStruct> gridNodes, NativeList<int2> pathResult)
//        {
//            startGridPos = startGPos;
//            goalGridPos = goalGPos;
//            this.gridSize = gridSize;
//            this.gridNodes = gridNodes;
//            this.pathResult = pathResult;
//        }

//        public void Execute()
//        {
//            FindPath();
//        }

//        public void FindPath()
//        {
//            NativeArray<GridNodeStruct> gridNodes = new NativeArray<GridNodeStruct>(this.gridNodes, Allocator.Temp);

//            var goalNode = gridNodes[CalculateIndex(goalGridPos.x, goalGridPos.y)];

//            //Debug.Log("Start");
//            for (int x = 0; x < gridSize.x; x++)
//            {
//                for (int y = 0; y < gridSize.y; y++)
//                {
//                    GridNodeStruct node = gridNodes[CalculateIndex(x, y)];
//                    node.gCost = int.MaxValue;
//                    node.hCost = node.CalculateHCostTo(goalNode);
//                    node.CalculateFCost();

//                    node.parentIndex = -1;
//                    node.open = false;
//                    node.closed = false;

//                    gridNodes[node.arrayIndex] = node;
//                }
//            }

//            var startNode = gridNodes[CalculateIndex(startGridPos.x, startGridPos.y)];
//            startNode.gCost = 0;
//            gridNodes[startNode.arrayIndex] = startNode;

//            goalNode = gridNodes[CalculateIndex(goalGridPos.x, goalGridPos.y)];

//            if (!goalNode.walkable)
//            {
//                int newGoalNodeIndex = GetClosestSurroundingNode(goalNode, startNode, gridNodes);
//                if (newGoalNodeIndex < 0)
//                {
//                    //Debug.Log("未找到合适的终点1");
//                    gridNodes.Dispose();
//                    return;
//                }
//                else goalNode = gridNodes[newGoalNodeIndex];
//            }
//            if (!startNode.walkable)
//            {
//                int newStartNodeIndex = GetClosestSurroundingNode(startNode, goalNode, gridNodes);
//                if (newStartNodeIndex < 0)
//                {
//                    //Debug.Log("未找到合适的起点");
//                    gridNodes.Dispose();
//                    return;
//                }
//                else startNode = gridNodes[newStartNodeIndex];
//            }
//            if (startNode.Equals(goalNode))
//            {
//                //Debug.Log("起始相同");
//                gridNodes.Dispose();
//                return;
//            }
//            if (!startNode.CanReachTo(goalNode))
//            {
//                int newGoalNodeIndex = GetValidGoalByRing(startNode, goalNode, gridNodes);
//                if (newGoalNodeIndex < 0)
//                {
//                    //Debug.Log("未找到合适的终点2");
//                    gridNodes.Dispose();
//                    return;
//                }
//                else goalNode = gridNodes[newGoalNodeIndex];
//            }

//            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
//            openList.Add(startNode.arrayIndex);

//            while (openList.Length > 0)
//            {
//                int currentNodeIndex = GetNextNodeIndex(openList, gridNodes);
//                if (currentNodeIndex < 0) break;
//                GridNodeStruct currentNode = gridNodes[currentNodeIndex];
//                //Debug.Log(currentNodeIndex);
//                if (currentNodeIndex == goalNode.arrayIndex)
//                {
//                    //Debug.Log("找到了");
//                    NativeList<GridNodeStruct> path = new NativeList<GridNodeStruct>(Allocator.Temp);
//                    path.Add(goalNode);

//                    GridNodeStruct temp = gridNodes[goalNode.arrayIndex];
//                    while (temp.parentIndex != -1)
//                    {
//                        GridNodeStruct parent = gridNodes[temp.parentIndex];
//                        path.Add(parent);
//                        temp = parent;
//                    }
//                    int2 oldDir = int2.zero;
//                    for (int i = 1; i < path.Length; i++)
//                    {
//                        int2 newDir = path[i - 1].GridPosition - path[i].GridPosition;
//                        if (newDir.x != oldDir.x || newDir.y != oldDir.y)//方向不一样时才使用前面的点
//                            pathResult.Add(path[i - 1].GridPosition);
//                        else if (i == path.Length - 1) pathResult.Add(path[i].GridPosition);//即使方向一样，也强制把起点也加进去
//                        oldDir = newDir;
//                    }
//                    path.Dispose();
//                    break;
//                }

//                for (int i = 0; i < openList.Length; i++)
//                {
//                    if (openList[i] == currentNodeIndex)
//                    {
//                        //Debug.Log("Remove " + new int2(currentNode.x, currentNode.y));
//                        openList.RemoveAtSwapBack(i);
//                        break;
//                    }
//                }

//                currentNode.open = false;
//                currentNode.closed = true;
//                gridNodes[currentNodeIndex] = currentNode;

//                var neighbours = GetReachableNeighbours(currentNode, gridNodes);

//                for (int i = 0; i < neighbours.Length; i++)
//                {
//                    int neighbourIndex = neighbours[i];

//                    GridNodeStruct neighbourNode = gridNodes[neighbourIndex];
//                    if (!neighbourNode.closed)
//                        if (neighbourNode.walkable)
//                        {
//                            int newCost = currentNode.gCost + currentNode.CalculateHCostTo(neighbourNode);
//                            if (newCost < neighbourNode.gCost)
//                            {
//                                neighbourNode.parentIndex = currentNodeIndex;
//                                neighbourNode.gCost = newCost;
//                                neighbourNode.CalculateFCost(); ;

//                                if (!neighbourNode.open)
//                                {
//                                    openList.Add(neighbourIndex);
//                                    neighbourNode.open = true;
//                                }

//                                gridNodes[neighbourIndex] = neighbourNode;
//                            }
//                        }
//                }
//                neighbours.Dispose();
//            }
//            openList.Dispose();
//            gridNodes.Dispose();
//        }

//        private int CalculateIndex(int x, int y)
//        {
//            return x + y * gridSize.x;
//        }

//        private int GetNextNodeIndex(NativeList<int> openList, NativeArray<GridNodeStruct> gridNodes)
//        {
//            if (openList.Length < 1 || gridNodes.Length < 1) return -1;
//            GridNodeStruct result = gridNodes[openList[0]];
//            for (int i = 1; i < openList.Length; i++)
//            {
//                GridNodeStruct temp = gridNodes[openList[i]];
//                if (temp.CompareTo(result) < 0)
//                    result = temp;
//            }
//            return result.arrayIndex;
//        }

//        private int GetClosestSurroundingNode(GridNodeStruct node, GridNodeStruct closestTo, NativeArray<GridNodeStruct> gridNodes, int ringCount = 1)
//        {
//            var neighbours = GetSurroundingNodes(node, ringCount);
//            if (ringCount >= math.max(gridSize.x, gridSize.y)) return -1;//突破递归
//            int closestIndex = -1;
//            var neighbourEnum = neighbours.GetEnumerator();
//            while (neighbourEnum.MoveNext())
//                if (gridNodes[neighbourEnum.Current].walkable)
//                {
//                    closestIndex = neighbourEnum.Current;
//                    break;
//                }
//            while (neighbourEnum.MoveNext())
//            {
//                GridNodeStruct closestNode = gridNodes[closestIndex];
//                GridNodeStruct neighbour = gridNodes[neighbourEnum.Current];
//                if (math.distancesq(closestNode.worldPosition, neighbour.worldPosition) < math.distancesq(closestTo.worldPosition, closestNode.worldPosition))
//                    if (neighbour.walkable) closestIndex = neighbour.arrayIndex;
//            }
//            neighbourEnum.Dispose();
//            neighbours.Dispose();
//            if (closestIndex == -1) return GetClosestSurroundingNode(node, closestTo, gridNodes, ringCount + 1);
//            else return closestIndex;
//        }

//        private NativeList<int> GetSurroundingNodes(GridNodeStruct node, int ringCount/*圈数*/)
//        {
//            NativeList<int> neighbours = new NativeList<int>(Allocator.Temp);
//            if (ringCount > 0)
//            {
//                int neiborX;
//                int neiborY;
//                for (int x = -ringCount; x <= ringCount; x++)
//                    for (int y = -ringCount; y <= ringCount; y++)
//                    {
//                        if (math.abs(x) < ringCount && math.abs(y) < ringCount) continue;//对于圈内的结点，总有其x和y都小于圈数，所以依此跳过

//                        neiborX = node.x + x;
//                        neiborY = node.y + y;

//                        if (neiborX >= 0 && neiborX < gridSize.x && neiborY >= 0 && neiborY < gridSize.y)
//                            neighbours.Add(CalculateIndex(neiborX, neiborY));
//                    }
//            }
//            return neighbours;
//        }

//        private NativeList<int> GetReachableNeighbours(GridNodeStruct node, NativeArray<GridNodeStruct> gridNodes)
//        {
//            NativeList<int> neighbours = new NativeList<int>(Allocator.Temp);
//            for (int x = -1; x <= 1; x++)
//                for (int y = -1; y <= 1; y++)
//                {
//                    if (x == 0 && y == 0) continue;

//                    int neiborX = node.x + x;
//                    int neiborY = node.y + y;

//                    if (neiborX >= 0 && neiborX < gridSize.x && neiborY >= 0 && neiborY < gridSize.y)
//                    {
//                        GridNodeStruct neighbour = gridNodes[CalculateIndex(neiborX, neiborY)];
//                        if (NeighbourReachable(node, neighbour, gridNodes))
//                            neighbours.Add(neighbour.arrayIndex);
//                    }
//                }
//            return neighbours;
//        }

//        private bool NeighbourReachable(GridNodeStruct node, GridNodeStruct neighbour, NativeArray<GridNodeStruct> gridNodes)
//        {
//            if (neighbour.x == node.x || neighbour.y == node.y) return neighbour.walkable;
//            if (!neighbour.walkable) return false;
//            if (neighbour.x > node.x && neighbour.y > node.y)//右上角
//            {
//                int leftX = node.x;
//                int leftY = neighbour.y;
//                var leftNode = gridNodes[CalculateIndex(leftX, leftY)];
//                if (leftNode.walkable)
//                {
//                    int downX = neighbour.x;
//                    int downY = node.y;
//                    var downNode = gridNodes[CalculateIndex(downX, downY)];
//                    if (!downNode.walkable) return false;
//                    else return true;
//                }
//                else return false;
//            }
//            else if (neighbour.x > node.x && neighbour.y < node.y)//右下角
//            {
//                int leftX = node.x;
//                int leftY = neighbour.y;
//                var leftNode = gridNodes[CalculateIndex(leftX, leftY)];
//                if (leftNode.walkable)
//                {
//                    int upX = neighbour.x;
//                    int upY = node.y;
//                    var upNode = gridNodes[CalculateIndex(upX, upY)];
//                    if (!upNode.walkable) return false;
//                    else return true;
//                }
//                else return false;
//            }
//            else if (neighbour.x < node.x && neighbour.y > node.y)//左上角
//            {
//                int rightX = node.x;
//                int rightY = neighbour.y;
//                var rightNode = gridNodes[CalculateIndex(rightX, rightY)];
//                if (rightNode.walkable)
//                {
//                    int downX = neighbour.x;
//                    int downY = node.y;
//                    var downNode = gridNodes[CalculateIndex(downX, downY)];
//                    if (!downNode.walkable) return false;
//                    else return true;
//                }
//                else return false;
//            }
//            else if (neighbour.x < node.x && neighbour.y < node.y)//左下角
//            {
//                int rightX = node.x;
//                int rightY = neighbour.y;
//                var rightNode = gridNodes[CalculateIndex(rightX, rightY)];
//                if (rightNode.walkable)
//                {
//                    int upX = neighbour.x;
//                    int upY = node.y;
//                    var upNode = gridNodes[CalculateIndex(upX, upY)];
//                    if (!upNode.walkable) return false;
//                    else return true;
//                }
//                else return false;
//            }
//            else return true;
//        }

//        private int GetValidGoalByRing(GridNodeStruct startNode, GridNodeStruct goalNode, NativeArray<GridNodeStruct> gridNodes, int ringCount = 1)
//        {
//            var neighbours = GetSurroundingNodes(goalNode, ringCount);
//            if (neighbours.Length < 1 || ringCount >= math.max(gridSize.x, gridSize.y)) return -1;//突破递归
//            int newGoalNodeIndex = -1;
//            GridNodeStruct newGoalNode = gridNodes[neighbours[0]];

//            for (int i = 0; i < neighbours.Length; i++)
//            {
//                GridNodeStruct neighbour = gridNodes[neighbours[i]];
//                if (math.distancesq(goalNode.worldPosition, neighbour.worldPosition) <= math.distancesq(goalNode.worldPosition, newGoalNode.worldPosition))
//                    if (neighbour.CanReachTo(startNode))
//                    {
//                        newGoalNodeIndex = neighbour.arrayIndex;
//                        newGoalNode = gridNodes[newGoalNodeIndex];
//                    }
//            }

//            neighbours.Dispose();
//            if (newGoalNodeIndex == -1) return GetValidGoalByRing(startNode, goalNode, gridNodes, ringCount + 1);
//            else return newGoalNodeIndex;
//        }
//    }

//    public struct GridNodeStruct
//    {
//        public int x;
//        public int y;
//        public int2 GridPosition => new int2(x, y);
//        public float3 worldPosition;
//        public bool walkable;

//        public int gCost;
//        public int hCost;
//        public int fCost;
//        public float height;

//        public int arrayIndex;

//        public int parentIndex;

//        public int connectionLabel;

//        public bool open;
//        public bool closed;

//        public GridNodeStruct(AStarNode node)
//        {
//            worldPosition = node.worldPosition;
//            x = node.gridPosition.x;
//            y = node.gridPosition.y;
//            walkable = node.walkable;
//            gCost = int.MaxValue;
//            hCost = int.MaxValue;
//            fCost = int.MaxValue;
//            height = node.Height;
//            arrayIndex = -1;
//            parentIndex = -1;
//            connectionLabel = node.connectionLabel;
//            open = false;
//            closed = false;
//        }

//        /*public GridNodeStruct(Vector3 position, int gridX, int gridY, float height, int connectionLabel)
//        {
//            worldPosition = position;
//            gridPosition = new int2(gridX, gridY);
//            this.height = height;
//            walkable = true;
//            gCost = int.MaxValue;
//            hCost = 0;
//            fCost = gCost + hCost;
//            this.connectionLabel = connectionLabel;
//            parentIndex = -1;
//            HeapIndex = -1;
//        }*/

//        public void CalculateFCost()
//        {
//            fCost = gCost + hCost;
//        }

//        public int CalculateHCostTo(GridNodeStruct other)
//        {
//            //使用曼哈顿距离
//            int disX = Mathf.Abs(x - other.x);
//            int disY = Mathf.Abs(y - other.y);

//            if (disX > disY)
//                return 14 * disY + 10 * (disX - disY) + Mathf.RoundToInt(Mathf.Abs(height - other.height));
//            else return 14 * disX + 10 * (disY - disX) + Mathf.RoundToInt(Mathf.Abs(height - other.height));
//        }

//        public bool CanReachTo(GridNodeStruct other)
//        {
//            return connectionLabel > 0 && connectionLabel == other.connectionLabel;
//        }

//        public int CompareTo(GridNodeStruct other)
//        {
//            if (fCost < other.fCost || (fCost == other.fCost && hCost < other.hCost) || (fCost == other.fCost && hCost == other.hCost && height < other.height))
//                return -1;
//            else if (fCost == other.fCost && hCost == other.hCost && height == other.height) return 0;
//            else return 1;
//        }

//        public bool Equals(GridNodeStruct other)
//        {
//            return x == other.x && y == other.y
//                && worldPosition.Equals(other.worldPosition)
//                && walkable == other.walkable
//                && gCost == other.gCost
//                && hCost == other.hCost
//                && fCost == other.fCost
//                && height == other.height
//                && arrayIndex == other.arrayIndex
//                && parentIndex == other.parentIndex
//                && connectionLabel == other.connectionLabel
//                && closed == other.closed;
//        }
//    }

//    public class AStar : IDisposable
//    {
//        /// <summary>
//        /// 新建基于3D空间的AStar
//        /// </summary>
//        /// <param name="worldSize">世界边界大小</param>
//        /// <param name="cellSize">网格边长</param>
//        /// <param name="cellHeight">网格高度（几倍的网格边长？）</param>
//        /// <param name="castCheckType">碰撞检测类型</param>
//        /// <param name="castRadiusMultiple">碰撞检测范围（几倍的网格边长？）</param>
//        /// <param name="unwalkableLayer">不可达检测层</param>
//        /// <param name="groundLayer">地面检测层</param>
//        /// <param name="worldHeight">世界高度</param>
//        public AStar(Vector3 axisOrigin, Vector2 worldSize, float cellSize, int cellHeight, CastCheckType castCheckType, float castRadiusMultiple, LayerMask unwalkableLayer, LayerMask groundLayer, float worldHeight)
//        {
//            this.axisOrigin = axisOrigin;
//            this.worldSize = worldSize;
//            this.cellSize = cellSize;
//            this.cellHeight = cellHeight;
//            this.castCheckType = castCheckType;
//            this.castRadiusMultiple = castRadiusMultiple;
//            this.unwalkableLayer = unwalkableLayer;
//            this.groundLayer = groundLayer;
//            this.worldHeight = worldHeight;
//            threeD = true;
//        }

//        /// <summary>
//        /// 新建基于2D空间的AStar
//        /// </summary>
//        /// <param name="worldSize">世界边界大小</param>
//        /// <param name="cellSize">网格边长</param>
//        /// <param name="castCheckType">碰撞检测类型</param>
//        /// <param name="castRadiusMultiple">碰撞检测范围（几倍的网格边长？）</param>
//        /// <param name="unwalkableLayer">不可达检测层</param>
//        public AStar(Vector3 axisOrigin, Vector2 worldSize, float cellSize, CastCheckType castCheckType, float castRadiusMultiple, LayerMask unwalkableLayer)
//        {
//            this.axisOrigin = axisOrigin;
//            this.worldSize = worldSize;
//            this.cellSize = cellSize;
//            this.castCheckType = castCheckType;
//            this.castRadiusMultiple = castRadiusMultiple;
//            this.unwalkableLayer = unwalkableLayer;
//            threeD = false;
//        }

//        private readonly bool threeD;
//        private readonly Vector3 axisOrigin;
//        private readonly Vector2 worldSize;
//        private readonly float worldHeight;
//        private readonly LayerMask groundLayer;

//        private readonly float cellSize;
//        /// <summary>
//        /// cellSize的倍数
//        /// </summary>
//        private readonly int cellHeight;

//        private Vector2Int gridSize;
//        public AStarNode[,] Grid { get; private set; }

//        private NativeArray<GridNodeStruct> gridNodes;

//        private readonly LayerMask unwalkableLayer;
//        private readonly CastCheckType castCheckType;
//        private readonly float castRadiusMultiple;

//        #region 网格相关
//        public void CreateGrid()
//        {
//            gridSize = Vector2Int.RoundToInt(worldSize / cellSize);
//            Grid = new AStarNode[Mathf.RoundToInt(gridSize.x), Mathf.RoundToInt(gridSize.y)];
//            int row = Mathf.Min(gridSize.x, gridSize.y);//量小的做行，貌似稍微提升遍历性能
//            int col = Mathf.Max(gridSize.x, gridSize.y);
//            for (int i = 0; i < row; i++)
//                for (int j = 0; j < col; j++)
//                    CreateNode(i, j);
//            RefreshGrid();
//            if (gridNodes.IsCreated) gridNodes.Dispose();
//            gridNodes = new NativeArray<GridNodeStruct>(gridSize.x * gridSize.y, Allocator.TempJob);
//            for (int x = 0; x < gridSize.x; x++)
//            {
//                for (int y = 0; y < gridSize.y; y++)
//                {
//                    GridNodeStruct node = new GridNodeStruct(Grid[x, y])
//                    {
//                        arrayIndex = x + y * gridSize.x,
//                        gCost = int.MaxValue
//                    };
//                    gridNodes[node.arrayIndex] = node;
//                }
//            }
//        }

//        private void CreateNode(int gridX, int gridY)
//        {
//            Vector3 nodeWorldPos;
//            if (!threeD)
//            {
//                nodeWorldPos = axisOrigin + Vector3.right * (gridX + 0.5f) * cellSize + Vector3.up * (gridY + 0.5f) * cellSize;
//            }
//            else
//            {
//                nodeWorldPos = axisOrigin + Vector3.right * (gridX + 0.5f) * cellSize + Vector3.forward * (gridY + 0.5f) * cellSize;
//                float height;
//                if (Physics.Raycast(nodeWorldPos + Vector3.up * (worldHeight + 0.01f), Vector3.down, out RaycastHit hit, worldHeight + 0.01f, groundLayer))
//                    height = hit.point.y;
//                else height = worldHeight + 0.01f;
//                nodeWorldPos += Vector3.up * height;
//            }
//            Grid[gridX, gridY] = new AStarNode(nodeWorldPos, gridX, gridY, threeD ? nodeWorldPos.y : 0);
//        }

//        public void RefreshGrid()
//        {
//            if (Grid == null) return;
//            int row = Mathf.Min(gridSize.x, gridSize.y);
//            int col = Mathf.Max(gridSize.x, gridSize.y);
//            for (int i = 0; i < row; i++)
//                for (int j = 0; j < col; j++)
//                    CheckNodeWalkable(Grid[i, j]);
//            CalculateConnections();
//            UpdataGridStruct();
//        }

//        public void RefreshGrid(Vector3 fromPoint, Vector3 toPoint)
//        {
//            if (Grid == null) return;
//            AStarNode fromNode = WorldPointToNode(fromPoint);
//            AStarNode toNode = WorldPointToNode(toPoint);
//            if (fromNode == toNode)
//            {
//                CheckNodeWalkable(fromNode);
//                return;
//            }
//            AStarNode min = fromNode.gridPosition.x <= toNode.gridPosition.x && fromNode.gridPosition.y <= toNode.gridPosition.y ? fromNode : toNode;
//            AStarNode max = fromNode.gridPosition.x > toNode.gridPosition.x && fromNode.gridPosition.y > toNode.gridPosition.y ? fromNode : toNode;
//            fromNode = min;
//            toNode = max;
//            //Debug.Log(string.Format("From {0} to {1}", fromNode.GridPosition, toNode.GridPosition));
//            if (toNode.gridPosition.x - fromNode.gridPosition.x <= toNode.gridPosition.y - fromNode.gridPosition.y)
//                for (int i = fromNode.gridPosition.x; i <= toNode.gridPosition.x; i++)
//                    for (int j = fromNode.gridPosition.y; j <= toNode.gridPosition.y; j++)
//                        CheckNodeWalkable(Grid[i, j]);
//            else for (int i = fromNode.gridPosition.y; i <= toNode.gridPosition.y; i++)
//                    for (int j = fromNode.gridPosition.x; j <= toNode.gridPosition.x; j++)
//                        CheckNodeWalkable(Grid[i, j]);
//            CalculateConnections();
//            UpdataGridStruct();
//        }

//        private void UpdataGridStruct()
//        {
//            if (gridNodes.IsCreated && gridNodes.Length >= gridSize.x * gridSize.y)
//            {
//                for (int x = 0; x < gridSize.x; x++)
//                {
//                    for (int y = 0; y < gridSize.y; y++)
//                    {
//                        var node = gridNodes[x + y * gridSize.x];
//                        node.walkable = Grid[x, y].walkable;
//                        node.connectionLabel = Grid[x, y].connectionLabel;
//                        gridNodes[node.arrayIndex] = node;
//                    }
//                }
//            }
//        }

//        public AStarNode WorldPointToNode(Vector3 position)
//        {
//            if (Grid == null || (threeD && position.y > worldHeight)) return null;
//            int gX = Mathf.RoundToInt((gridSize.x - 1) * Mathf.Clamp01((position.x + worldSize.x * 0.5f) / worldSize.x));
//            //gX怎么算：首先利用坐标系原点的 x 坐标来修正该点的 x 轴坐标，然后除以世界宽度，获得该点的 x 坐标在网格坐标系上所处的区域，用不大于 1 分数来表示，
//            //然后获得相同区域的网格的 x 坐标即 gX，举个例子：
//            //假设 x 坐标为 -2，而世界起点的 x 坐标为 -24 即实际坐标原点 x 坐标 0 减去世界宽度的一半，则修正 x 坐标为 x + 24 = 22，这就是它在A*坐标系上虚拟的修正了的位置 x'， 
//            //以上得知世界宽度为48，那么 22 / 48 = 11/24，说明 x' 在世界宽度轴 11/24 的位置上，所以，该位置相应的格子的 x 坐标也是网格宽度轴的11/24，
//            //假设网格宽度为也为48，则 gX = 48 * 11/24 = 22，看似正确，其实，假设上面算得的 x' 是48，那么 48 * 48/48 = 48，而网格坐标最多到47，因为数组从0开始，
//            //所以这时就会发生越界错误，反而用 gX = (48 - 1) * 48/48 = 47 * 1 = 47 来代替就对了，回过头来，x' 是 22，则 47 * 11/24 = 21.54 ≈ 22，位于 x' 轴数起第 23 个格子上，
//            //假设算出的 x' 是 30，则 gX = 47 * 15/24 = 29.38 ≈ 29，完全符合逻辑，为什么不用 48 算完再减去 1 ？如果 x' 是 0， 48 * 0/48 - 1 = -1，又越界了
//            int gY;
//            if (!threeD) gY = Mathf.RoundToInt((gridSize.y - 1) * Mathf.Clamp01((position.y + worldSize.y * 0.5f) / worldSize.y));
//            else gY = Mathf.RoundToInt((gridSize.y - 1) * Mathf.Clamp01((position.z + worldSize.y * 0.5f) / worldSize.y));
//            return Grid[gX, gY];
//        }

//        public bool PointInsideGrid(Vector3 position)
//        {
//            Vector2 xy = new Vector3(position.x, threeD ? position.z : position.y);
//            Vector2 axisxy = new Vector2(axisOrigin.x, threeD ? axisOrigin.z : axisOrigin.y);
//            return xy.x > axisxy.x && xy.x < axisxy.x + worldSize.x && xy.y > axisxy.y && xy.y < axisxy.y + worldSize.y;
//        }

//        private bool CheckNodeWalkable(AStarNode node)
//        {
//            if (!node) return false;
//            if (!threeD)
//            {
//                switch (castCheckType)
//                {
//                    case CastCheckType.Box:
//                        node.walkable = Physics2D.OverlapBox(node.worldPosition, Vector2.one * cellSize * castRadiusMultiple * 2, 0, unwalkableLayer) == null;
//                        break;
//                    case CastCheckType.Capsule:
//                        node.walkable = Physics2D.OverlapCircle(node.worldPosition, cellSize * castRadiusMultiple, unwalkableLayer) == null;
//                        break;
//                    case CastCheckType.Ray:
//                        node.walkable = Physics2D.OverlapPoint(node.worldPosition, unwalkableLayer) == null;
//                        break;
//                }
//            }
//            else
//            {
//                switch (castCheckType)
//                {
//                    case CastCheckType.Box:
//                        node.walkable = !Physics.CheckBox(node.worldPosition, Vector3.one * cellSize * castRadiusMultiple, Quaternion.identity,
//                            unwalkableLayer, QueryTriggerInteraction.Ignore);
//                        break;
//                    case CastCheckType.Capsule:
//                        node.walkable = !Physics.CheckCapsule(node.worldPosition, node.worldPosition + Vector3.up * cellHeight, cellSize * castRadiusMultiple,
//                            unwalkableLayer, QueryTriggerInteraction.Ignore);
//                        break;
//                    case CastCheckType.Ray:
//                        node.walkable = !Physics.Raycast(node.worldPosition + Vector3.up * (cellSize * (cellHeight - 1) + cellSize * 0.5f + 0.01f), Vector3.down,
//                            cellSize * cellHeight, unwalkableLayer, QueryTriggerInteraction.Ignore) &&
//                            !Physics.Raycast(node.worldPosition + Vector3.up * (cellSize * (cellHeight - 1) + cellSize * 0.5f + 0.01f), Vector3.up,
//                            cellSize * cellHeight, unwalkableLayer, QueryTriggerInteraction.Ignore);
//                        break;
//                }
//            }
//            return node.walkable;
//        }

//        public bool WorldPointWalkable(Vector3 point)
//        {
//            return CheckNodeWalkable(WorldPointToNode(point));
//        }

//        private bool CanGoStraight(Vector3 from, Vector3 to)
//        {
//            Vector3 dir = (to - from).normalized;
//            float dis = Vector3.Distance(from, to);
//            float checkRadius = cellSize * castRadiusMultiple;
//            float radiusMultiple = 1;
//            if (castCheckType == CastCheckType.Box)//根据角度确定两个端点的偏移量
//            {
//                float x, y, angle;
//                if (!threeD)
//                {
//                    if (from.x < to.x)
//                        angle = Vector2.Angle(Vector2.right.normalized, dir);
//                    else
//                        angle = Vector2.Angle(Vector2.left.normalized, dir);
//                }
//                else
//                {
//                    if (from.x < to.x)
//                        angle = Vector3.Angle(Vector3.right.normalized, new Vector3(dir.x, 0, dir.z));
//                    else
//                        angle = Vector3.Angle(Vector3.left.normalized, new Vector3(dir.x, 0, dir.z));
//                }
//                if (angle < 45)
//                {
//                    x = 1;
//                    y = Mathf.Tan(angle * Mathf.Deg2Rad);
//                }
//                else if (angle == 90)
//                {
//                    x = 0;
//                    y = 1;
//                }
//                else
//                {
//                    y = 1;
//                    x = 1 / Mathf.Tan(angle * Mathf.Deg2Rad);
//                }
//                radiusMultiple = Mathf.Sqrt(x * x + y * y);
//            }
//            if (!threeD)
//            {
//                bool hit = Physics2D.Raycast(from, dir, dis, unwalkableLayer);
//                if (!hit)//射不中，则进行第二次检测
//                {
//                    float x1 = -dir.y / dir.x;
//                    Vector3 point1 = from + new Vector3(x1, 1).normalized * checkRadius * radiusMultiple;
//                    bool hit1 = Physics2D.Raycast(point1, dir, dis, unwalkableLayer);
//                    if (!hit1)//射不中，进行第三次检测
//                    {
//                        float x2 = dir.y / dir.x;
//                        Vector3 point2 = from + new Vector3(x2, -1).normalized * checkRadius * radiusMultiple;
//                        bool hit2 = Physics2D.Raycast(point2, dir, dis, unwalkableLayer);
//                        if (!hit2) return true;
//                        else return false;
//                    }
//                    else return false;
//                }
//                else return false;
//            }
//            else
//            {
//                bool hit = Physics.Raycast(from, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
//                if (!hit)
//                {
//                    float x1 = -dir.z / dir.x;
//                    Vector3 point1 = from + new Vector3(x1, 0, 1).normalized * checkRadius * radiusMultiple;//左边
//                    bool hit1 = Physics.Raycast(point1, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
//                    if (!hit1)
//                    {
//                        float x2 = dir.z / dir.x;
//                        Vector3 point2 = from + new Vector3(x2, 0, -1).normalized * checkRadius * radiusMultiple;//右边
//                        bool hit2 = Physics.Raycast(point2, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
//                        if (!hit2)
//                        {
//                            float x3 = -dir.y / dir.x;
//                            Vector3 point3 = from + new Vector3(x3, 1, 0).normalized * checkRadius;//底部
//                            bool hit3 = Physics.Raycast(point3, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
//                            if (!hit3)
//                            {
//                                for (int i = 1; i <= cellHeight; i++)//上部
//                                {
//                                    float x4 = -dir.y / dir.x;
//                                    Vector3 point4 = from + new Vector3(x4, -1, 0).normalized * (checkRadius * (1 + 2 * (i - 1)));
//                                    bool hit4 = Physics.Raycast(point4, dir, dis, unwalkableLayer, QueryTriggerInteraction.Ignore);
//                                    if (hit4) return false;
//                                }
//                                return true;
//                            }
//                            else return false;
//                        }
//                        else return false;
//                    }
//                    else return false;
//                }
//                else return false;
//            }
//        }
//        #endregion

//        #region 路径相关
//        public void FindPath(PathRequest request)
//        {
//            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
//            stopwatch.Start();
//            AStarNode startNode = WorldPointToNode(request.start);
//            AStarNode goalNode = WorldPointToNode(request.goal);
//            if (startNode == null || goalNode == null)
//            {
//                stopwatch.Stop();
//                request.callback?.Invoke(null);
//                return;
//            }
//            if (CanGoStraight(startNode, goalNode))
//            {
//                stopwatch.Stop();
//                //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过直走找到路径");
//                request.callback?.Invoke(new Vector3[] { goalNode });
//                return;
//            }
//            NativeList<int2> result = new NativeList<int2>(Allocator.TempJob);
//            FindPathJob findPathJob = new FindPathJob(new int2(startNode.gridPosition.x, startNode.gridPosition.y),
//                new int2(goalNode.gridPosition.x, goalNode.gridPosition.y), new int2(gridSize.x, gridSize.y), gridNodes, result);
//            var jobH = findPathJob.Schedule();
//            jobH.Complete();
//            List<Vector3> pathResult = new List<Vector3>();
//            foreach (var point in result)
//                pathResult.Add(Grid[point.x, point.y].worldPosition);
//            result.Dispose();
//            request.callback?.Invoke(SimplifyPath(pathResult));
//            stopwatch.Stop();
//            if (pathResult.Count > 0) Debug.Log("耗时：" + stopwatch.Elapsed.TotalMilliseconds.ToString("F4") + "ms 找到路径。");
//        }

//        private List<Vector3> SimplifyPath(List<Vector3> waypoints)
//        {
//            if (waypoints.Count < 1) return waypoints;
//            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
//            //stopwatch.Start();
//            List<Vector3> toRemove = new List<Vector3>();
//            Vector3 from = waypoints[0];
//            for (int i = 2; i < waypoints.Count; i++)
//                if (CanGoStraight(from, waypoints[i]))
//                    toRemove.Add(waypoints[i - 1]);
//                else from = waypoints[i - 1];
//            foreach (Vector3 point in toRemove)
//                waypoints.Remove(point);
//            waypoints.Reverse();
//            //stopwatch.Stop();
//            //Debug.Log("耗时 " + stopwatch.Elapsed.TotalMilliseconds + "ms完成路径简化");

//            return waypoints;
//        }
//        #endregion

//        #region 四邻域连通检测算法
//        private readonly Dictionary<int, HashSet<AStarNode>> Connections = new Dictionary<int, HashSet<AStarNode>>();

//        private void CalculateConnections()
//        {
//            Connections.Clear();//重置连通域字典

//            int[,] gridData = new int[gridSize.x, gridSize.y];

//            for (int x = 0; x < gridSize.x; x++)
//                for (int y = 0; y < gridSize.y; y++)
//                    if (Grid[x, y].walkable) gridData[x, y] = 1;//大于0表示可行走
//                    else gridData[x, y] = 0;//0表示有障碍

//            int label = 1;
//            for (int y = 0; y < gridSize.y; y++)//从下往上扫
//            {
//                for (int x = 0; x < gridSize.x; x++)//从左往右扫
//                {
//                    //若该数据的标记不为0，即该数据表示的位置没有障碍
//                    if (gridData[x, y] != 0)
//                    {
//                        int labelNeedToChange = 0;//记录需要更改的标记
//                        if (y == 0)//第一行，只用看当前数据点的左边
//                        {
//                            //若该点是第一行第一个，前面已判断不为0，直接标上标记
//                            if (x == 0)
//                            {
//                                gridData[x, y] = label;
//                                label++;
//                            }
//                            else if (gridData[x - 1, y] != 0)//若该点的左侧数据的标记不为0，那么当前数据的标记标为左侧的标记，表示同属一个连通域
//                                gridData[x, y] = gridData[x - 1, y];
//                            else//否则，标上新标记
//                            {
//                                gridData[x, y] = label;
//                                label++;
//                            }
//                        }
//                        else if (x == 0)//网格最左边，不可能出现与左侧形成衔接的情况
//                        {
//                            if (gridData[x, y - 1] != 0) gridData[x, y] = gridData[x, y - 1]; //若下方数据不为0，则当前数据标上下方标记的标记
//                            else
//                            {
//                                gridData[x, y] = label;
//                                label++;
//                            }
//                        }
//                        else if (gridData[x, y - 1] != 0)//若下方标记不为0
//                        {
//                            gridData[x, y] = gridData[x, y - 1];//则用下方标记来标记当前数据
//                            if (gridData[x - 1, y] != 0) labelNeedToChange = gridData[x - 1, y]; //若左方数据不为0，则被左方标记所标记的数据都要更改
//                        }
//                        else if (gridData[x - 1, y] != 0)//若左侧不为0
//                            gridData[x, y] = gridData[x - 1, y];//则用左侧标记来标记当前数据
//                        else
//                        {
//                            gridData[x, y] = label;
//                            label++;
//                        }

//                        if (!Connections.ContainsKey(gridData[x, y])) Connections.Add(gridData[x, y], new HashSet<AStarNode>());
//                        Connections[gridData[x, y]].Add(Grid[x, y]);
//                        //将对应网格结点的连通域标记标为当前标记，操作可选，若不操作，则在寻路检测可达性时使用下面的ACanReachB()
//                        Grid[x, y].connectionLabel = gridData[x, y];

//                        //如果有需要更改的标记，且其与当前标记不同
//                        if (labelNeedToChange > 0 && labelNeedToChange != gridData[x, y])
//                        {
//                            foreach (AStarNode node in Connections[labelNeedToChange])//把对应连通域合并到当前连通域
//                            {
//                                gridData[node.gridPosition.x, node.gridPosition.y] = gridData[x, y];
//                                Connections[gridData[x, y]].Add(node);
//                                node.connectionLabel = gridData[x, y];//操作可选
//                            }
//                            Connections[labelNeedToChange].Clear();
//                            Connections.Remove(labelNeedToChange);
//                        }
//                    }
//                }
//            }
//        }
//        #endregion

//        public void Dispose()
//        {
//            gridNodes.Dispose();
//        }

//        public static implicit operator bool(AStar self)
//        {
//            return self != null;
//        }
//    }

//    public class AStarNode
//    {
//        public readonly Vector3 worldPosition;
//        public readonly Vector2Int gridPosition;
//        public bool walkable;

//        public float Height { get; private set; }

//        public int connectionLabel;

//        public AStarNode(Vector3 position, int gridX, int gridY, float height)
//        {
//            worldPosition = position;
//            gridPosition = new Vector2Int(gridX, gridY);
//            Height = height;
//            walkable = true;
//        }

//        public int CalculateHCostTo(AStarNode other)
//        {
//            //使用曼哈顿距离
//            int disX = Mathf.Abs(gridPosition.x - other.gridPosition.x);
//            int disY = Mathf.Abs(gridPosition.y - other.gridPosition.y);

//            if (disX > disY)
//                return 14 * disY + 10 * (disX - disY) + Mathf.RoundToInt(Mathf.Abs(Height - other.Height));
//            else return 14 * disX + 10 * (disY - disX) + Mathf.RoundToInt(Mathf.Abs(Height - other.Height));
//        }

//        public bool CanReachTo(AStarNode other)
//        {
//            return connectionLabel > 0 && connectionLabel == other.connectionLabel;
//        }

//        public static implicit operator Vector3(AStarNode self)
//        {
//            return self.worldPosition;
//        }

//        public static implicit operator Vector2(AStarNode self)
//        {
//            return self.worldPosition;
//        }

//        public static implicit operator bool(AStarNode self)
//        {
//            return self != null;
//        }
//    }

//    public struct PathRequest
//    {
//        public readonly Vector3 start;
//        public readonly Vector3 goal;
//        public readonly Vector2Int unitSize;
//        public readonly Action<IEnumerable<Vector3>> callback;

//        public PathRequest(Vector3 start, Vector3 goal, Vector2Int unitSize, Action<IEnumerable<Vector3>> callback)
//        {
//            this.start = start;
//            this.goal = goal;
//            this.unitSize = unitSize;
//            this.callback = callback;
//        }
//    }

//    public enum CastCheckType
//    {
//        Box,
//        Capsule,
//        Ray
//    }
//}