using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

//[BurstCompile]
public unsafe struct PathFindingJob : IJob
{
    private readonly float3 start;
    private readonly float3 goal;

    private readonly float2 worldSize;
    private readonly bool threeD;
    private readonly float worldHeight;
    private readonly int2 gridSize;

    [DeallocateOnJobCompletion]
    private AStarGrid Grid;
    /// <summary>
    /// 用于缓存Open结点，无需Dispose()
    /// </summary>
    [DeallocateOnJobCompletion]
    private UnmanagedHeap<AStarNodeStruct> openList;
    /// <summary>
    /// 用于缓存Closed结点，无需Dispose()
    /// </summary>
    [DeallocateOnJobCompletion]
    private AStarNodeSet closedList;

    #region 需要手动Dispose()
    /// <summary>
    /// 用于缓存结点集合网格位置，需在Job外Dispose()
    /// </summary>
    private NativeList<int2> gridPosCache;
    /// <summary>
    /// 用于存储最终结果，需在Job外Dispose()
    /// </summary>
    [WriteOnly] public NativeList<AStarNodeStruct> crudePathResult;
    #endregion

    //private int openCount;
    //private int maxSize;
    private AStarNodeStruct defaultNode;

    public PathFindingJob(float3 start, float3 destination, float2 worldSize, bool threeD, float worldHeight, int2 gridSize,
        AStarGrid grid, UnmanagedHeap<AStarNodeStruct> openList, AStarNodeSet closedList,
        NativeList<int2> neighboursCache,
        NativeList<AStarNodeStruct> crudePathResult)
    {
        this.start = start;
        this.goal = destination;
        this.worldSize = worldSize;
        this.threeD = threeD;
        this.worldHeight = worldHeight;
        this.gridSize = gridSize;
        Grid = grid;
        this.openList = openList;
        this.closedList = closedList;
        this.gridPosCache = neighboursCache;
        this.crudePathResult = crudePathResult;
        //openCount = 0;
        //maxSize = gridSize.x * gridSize.y;
        defaultNode = AStarNodeStruct.Defalut;
    }

    public void Execute()
    {
        //FindPath(start, goal);
    }

    public AStarNodeStruct* WorldPointToNode(float3 position)
    {
        if (threeD && position.y > worldHeight) return null;
        int gX = Mathf.RoundToInt((gridSize.x - 1) * Mathf.Clamp01((position.x + worldSize.x / 2) / worldSize.x));
        int gY;
        if (!threeD) gY = Mathf.RoundToInt((gridSize.y - 1) * Mathf.Clamp01((position.y + worldSize.y / 2) / worldSize.y));
        else gY = Mathf.RoundToInt((gridSize.y - 1) * Mathf.Clamp01((position.z + worldSize.y / 2) / worldSize.y));
        return Grid[gX, gY];
    }

    private void GetSurroundingNodes(AStarNodeStruct* node, int ringCount/*圈数*/)
    {
        gridPosCache.Clear();
        if (!node->gridPosition.Equals(defaultNode.gridPosition) && ringCount > 0)
        {
            int neiborX;
            int neiborY;
            for (int x = -ringCount; x <= ringCount; x++)
                for (int y = -ringCount; y <= ringCount; y++)
                {
                    if (Mathf.Abs(x) < ringCount && Mathf.Abs(y) < ringCount) continue;//对于圈内的结点，总有其x和y都小于圈数，所以依此跳过

                    neiborX = node->gridPosition.x + x;
                    neiborY = node->gridPosition.y + y;

                    if (neiborX >= 0 && neiborX < gridSize.x && neiborY >= 0 && neiborY < gridSize.y)
                        gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                }
        }
    }
    private AStarNodeStruct* GetClosestSurroundingNode(AStarNodeStruct* node, AStarNodeStruct* closestTo, int ringCount = 1)
    {
        if (ringCount >= math.max(gridSize.x, gridSize.y)) return null;//突破递归
        gridPosCache.Clear();
        GetSurroundingNodes(node, ringCount);
        AStarNodeStruct* closest = null;
        if (gridPosCache.Length > 0)
            closest = Grid[gridPosCache[0]];
        for (int i = 0; i < gridPosCache.Length; i++)
        {
            AStarNodeStruct* neighbour = Grid[gridPosCache[i]];
            if (math.distance(closestTo->worldPosition, neighbour->worldPosition) < math.distance(closestTo->worldPosition, closest->worldPosition))
                if (neighbour->walkable) closest = neighbour;
        }
        if (closest == null) return GetClosestSurroundingNode(node, closestTo, ringCount + 1);
        else return closest;
    }

    private void GetReachableNeighbours(AStarNodeStruct* node)
    {
        gridPosCache.Clear();
        int neiborX;
        int neiborY;
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                neiborX = node->gridPosition.x + x;
                neiborY = node->gridPosition.y + y;

                if (neiborX >= 0 && neiborX < gridSize.x && neiborY >= 0 && neiborY < gridSize.y)
                {
                    AStarNodeStruct* neighbour = Grid[neiborX, neiborY];
                    if (neighbour->gridPosition.x == node->gridPosition.x || neighbour->gridPosition.y == node->gridPosition.y)
                    {
                        if (!neighbour->walkable) gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                    }
                    if (!neighbour->walkable) continue;
                    if (neighbour->gridPosition.x > node->gridPosition.x && neighbour->gridPosition.y > node->gridPosition.y)//右上角
                    {
                        int leftX = node->gridPosition.x;
                        int leftY = neighbour->gridPosition.y;
                        AStarNodeStruct* leftNode = Grid[leftX, leftY];
                        if (leftNode->walkable)
                        {
                            int downX = neighbour->gridPosition.x;
                            int downY = node->gridPosition.y;
                            AStarNodeStruct* downNode = Grid[downX, downY];
                            if (!downNode->walkable) continue;
                            else gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                        }
                        else continue;
                    }
                    else if (neighbour->gridPosition.x > node->gridPosition.x && neighbour->gridPosition.y < node->gridPosition.y)//右下角
                    {
                        int leftX = node->gridPosition.x;
                        int leftY = neighbour->gridPosition.y;
                        AStarNodeStruct* leftNode = Grid[leftX, leftY];
                        if (leftNode->walkable)
                        {
                            int upX = neighbour->gridPosition.x;
                            int upY = node->gridPosition.y;
                            AStarNodeStruct* upNode = Grid[upX, upY];
                            if (!upNode->walkable) continue;
                            else gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                        }
                        else continue;
                    }
                    else if (neighbour->gridPosition.x < node->gridPosition.x && neighbour->gridPosition.y > node->gridPosition.y)//左上角
                    {
                        int rightX = node->gridPosition.x;
                        int rightY = neighbour->gridPosition.y;
                        AStarNodeStruct* rightNode = Grid[rightX, rightY];
                        if (rightNode->walkable)
                        {
                            int downX = neighbour->gridPosition.x;
                            int downY = node->gridPosition.y;
                            AStarNodeStruct* downNode = Grid[downX, downY];
                            if (!downNode->walkable) continue;
                            else gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                        }
                        else continue;
                    }
                    else if (neighbour->gridPosition.x < node->gridPosition.x && neighbour->gridPosition.y < node->gridPosition.y)//左下角
                    {
                        int rightX = node->gridPosition.x;
                        int rightY = neighbour->gridPosition.y;
                        AStarNodeStruct* rightNode = Grid[rightX, rightY];
                        if (rightNode->walkable)
                        {
                            int upX = neighbour->gridPosition.x;
                            int upY = node->gridPosition.y;
                            AStarNodeStruct* upNode = Grid[upX, upY];
                            if (!upNode->walkable) continue;
                            else gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                        }
                        else continue;
                    }
                    else gridPosCache.Add(Grid[neiborX, neiborY]->gridPosition);
                }
            }
    }

    private AStarNodeStruct* GetEffectiveGoalByRing(AStarNodeStruct* startNode, AStarNodeStruct* goalNode, int ringCount = 1)
    {
        if (ringCount >= Mathf.Max(gridSize.x, gridSize.y)) return null;//突破递归
        gridPosCache.Clear();
        GetSurroundingNodes(goalNode, ringCount);
        AStarNodeStruct* newGoalNode = null;
        if (gridPosCache.Length > 0)
            newGoalNode = Grid[gridPosCache[0]];
        for (int i = 0; i < gridPosCache.Length; i++)
        {
            AStarNodeStruct* neighbour = Grid[gridPosCache[i]];
            if (math.distance(goalNode->worldPosition, neighbour->worldPosition) < math.distance(goalNode->worldPosition, newGoalNode->worldPosition))
                if ((*neighbour).CanReachTo(ref *startNode)) newGoalNode = neighbour;
        }
        if (newGoalNode == null) return GetEffectiveGoalByRing(startNode, goalNode, ringCount + 1);
        else return newGoalNode;
    }

    private bool CanGoStraight(AStarNodeStruct* from, AStarNodeStruct* to)
    {
        //Debug.Log(string.Format("from {0} to {1}", from.gridPosition, to.gridPosition));
        int startNodeNodeX = from->gridPosition.x, startNodeY = from->gridPosition.y;
        int goalNodeX = to->gridPosition.x, goalNodeY = to->gridPosition.y;
        int xDistn = Mathf.Abs(to->gridPosition.x - from->gridPosition.x);
        int yDistn = Mathf.Abs(to->gridPosition.y - from->gridPosition.y);
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
        //Debug.Log("deltaX: " + deltaX + ", deltaY: " + deltaY);
        if (startNodeNodeX >= goalNodeX && startNodeY >= goalNodeY)//起点位于终点右上角
        {
            for (float x = goalNodeX + deltaX, y = goalNodeY + deltaY; x <= startNodeNodeX && y <= startNodeY; x += deltaX, y += deltaY)
            {
                int gX = Mathf.RoundToInt(x);
                int gY = Mathf.RoundToInt(y);
                if (!Grid[gX, gY]->walkable)
                {
                    //Debug.Log("右上角");
                    return false;
                }
            }
        }
        else if (startNodeNodeX >= goalNodeX && startNodeY <= goalNodeY)//起点位于终点右下角
        {
            for (float x = goalNodeX + deltaX, y = goalNodeY - deltaY; x <= startNodeNodeX && y >= startNodeY; x += deltaX, y -= deltaY)
            {
                int gX = Mathf.RoundToInt(x);
                int gY = Mathf.RoundToInt(y);
                if (!Grid[gX, gY]->walkable)
                {
                    //Debug.Log("右下角");
                    return false;
                }
            }
        }
        else if (startNodeNodeX <= goalNodeX && startNodeY >= goalNodeY)//起点位于终点左上角
        {
            for (float x = goalNodeX - deltaX, y = goalNodeY + deltaY; x >= startNodeNodeX && y <= startNodeY; x -= deltaX, y += deltaY)
            {
                int gX = Mathf.RoundToInt(x);
                int gY = Mathf.RoundToInt(y);
                if (!Grid[gX, gY]->walkable)
                {
                    //Debug.Log("左上角");
                    return false;
                }
            }
        }
        else if (startNodeNodeX <= goalNodeX && startNodeY <= goalNodeY)//起点位于终点左下角
        {
            for (float x = goalNodeX - deltaX, y = goalNodeY - deltaY; x >= startNodeNodeX && y >= startNodeY; x -= deltaX, y -= deltaY)
            {
                int gX = Mathf.RoundToInt(x);
                int gY = Mathf.RoundToInt(y);
                if (!Grid[gX, gY]->walkable)
                {
                    //Debug.Log("左下角");
                    return false;
                }
            }
        }
        return true;
    }

    private unsafe void FindPath(float3 start, float3 goal)
    {
        AStarNodeStruct* startNode = WorldPointToNode(start);
        AStarNodeStruct* goalNode = WorldPointToNode(goal);

        crudePathResult.Clear();
        bool findSuccessfully = false;

        if (!goalNode->walkable)
        {
            goalNode = GetClosestSurroundingNode(goalNode, startNode);
            if (goalNode->IsInvalid)
            {
                //Debug.Log("终点无效");
                return;
            }
        }

        if (!startNode->walkable)
        {
            startNode = GetClosestSurroundingNode(startNode, goalNode);
            if (startNode->IsInvalid)
            {
                //Debug.Log("起点无效");
                return;
            }
        }
        if (NodeEqual(startNode, goalNode))
        {
            return;
        }
        if (!(*startNode).CanReachTo(ref *goalNode))
        {
            goalNode = GetEffectiveGoalByRing(startNode, goalNode);
            if (goalNode->IsInvalid)
            {
                //Debug.Log("终点无效");
                return;
            }
        }

        if (CanGoStraight(startNode, goalNode))
        {
            crudePathResult.Add(*goalNode);
            //Debug.Log("直走");
            return;
        }

        if (!findSuccessfully)
        {
            //openCount = 0;
            //maxSize = gridSize.x * gridSize.y;
            openList.Clear();
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                AStarNodeStruct* current = openList.RemoveRoot();
                closedList.Add(current);
                if (NodeEqual(current, goalNode))
                {
                    findSuccessfully = true;
                    //openList.Clear();
                    //Debug.Log("搜索 " + closedList.Count + " 个结点后找到路径");
                    break;
                }
                //Debug.Log("current: " + current->gridPosition + ", F::" + current->FCost + ", H::" + current->HCost + ", G::" + current->GCost);
                GetReachableNeighbours(current);
                for (int i = 0; i < gridPosCache.Length; i++)
                {
                    AStarNodeStruct* neighbour = Grid[gridPosCache[i]];
                    if (!neighbour->walkable || closedList.Contains(neighbour))
                    {
                        //if (neighbour->walkable) Debug.Log(neighbour->SetIndex + ": " + neighbour->gridPosition);
                        //Debug.Log(closedList.Contains(ref neighbour));
                        continue;
                    }
                    int costStartToNeighbour = current->GCost + current->CalculateHCostTo(*neighbour);
                    if (costStartToNeighbour < neighbour->GCost || !openList.Contains(neighbour))
                    {
                        neighbour->GCost = costStartToNeighbour;
                        neighbour->HCost = neighbour->CalculateHCostTo(*goalNode);
                        neighbour->parent = current->gridPosition;
                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                            //Debug.Log("Add: " + neighbour->gridPosition + ", F::" + neighbour->FCost + ", H::" + neighbour->HCost + ", G::" + neighbour->GCost);
                        }
                    }
                }
                /*Debug.Log("closed: " + closedList.Count);
                Debug.Log("open: " + openCount);*/
            }
        }
        if (findSuccessfully)
        {
            GetResult(startNode, goalNode);
        }
    }
    private void GetResult(AStarNodeStruct* startNode, AStarNodeStruct* goalNode)
    {
        crudePathResult.Clear();
        AStarNodeStruct* pathNode = goalNode;
        while (!pathNode->gridPosition.Equals(startNode->gridPosition))
        {
            crudePathResult.Add(*pathNode);
            pathNode = Grid[pathNode->parent.x, pathNode->parent.y];
        }
    }

    private bool NodeEqual(AStarNodeStruct* nodeA, AStarNodeStruct* nodeB)
    {
        return nodeA->gridPosition.Equals(nodeB->gridPosition);
    }

    //#region OpenList操作
    //void AddToOpen(AStarNodeStruct node)
    //{
    //    if (openCount >= maxSize) return;
    //    node.HeapIndex = openCount;
    //    Grid.TryCover(node);
    //    openList[openCount] = new AStarNodeData(ref node);
    //    SortUpFrom(openList[openCount]);
    //    openCount++;
    //}

    //AStarNodeStruct RemoveRoot()
    //{
    //    if (openCount < 1) return defaultNode;
    //    AStarNodeStruct root = Grid[openList[0].gridPosition];
    //    root.HeapIndex = -1;
    //    Grid.TryCover(root);

    //    openCount--;
    //    var temp = openList[openCount];
    //    temp.HeapIndex = 0;
    //    openList[0] = temp;

    //    var node = Grid[temp.gridPosition];
    //    node.HeapIndex = temp.HeapIndex;
    //    Grid.TryCover(node);

    //    SortDownFrom(openList[0]);

    //    return root;
    //}

    //bool OpenContains(AStarNodeStruct node)
    //{
    //    if (node.HeapIndex < 0 || node.HeapIndex > openList.Length - 1) return false;
    //    return Equals(openList[node.HeapIndex].gridPosition, node.gridPosition);
    //}

    //void Swap(ref AStarNodeData data1, ref AStarNodeData data2)
    //{
    //    int item1Index = data1.HeapIndex;
    //    int item2Index = data2.HeapIndex;

    //    data1.HeapIndex = item2Index;
    //    AStarNodeStruct node1 = Grid[data1.gridPosition];
    //    node1.HeapIndex = data1.HeapIndex;
    //    Grid.TryCover(node1);

    //    data2.HeapIndex = item1Index;

    //    AStarNodeStruct node2 = Grid[data2.gridPosition];
    //    node2.HeapIndex = data2.HeapIndex;
    //    Grid.TryCover(node2);

    //    openList[item1Index] = data2;
    //    openList[item2Index] = data1;
    //}

    //void SortUpFrom(AStarNodeData data)
    //{
    //    int parentIndex = (data.HeapIndex - 1) / 2;
    //    if (parentIndex >= 0)
    //        while (true)
    //        {
    //            AStarNodeData parent = openList[parentIndex];
    //            if (Grid[data.gridPosition].CompareTo(Grid[parent.gridPosition]) < 0)
    //            {
    //                Swap(ref data, ref parent);
    //            }
    //            else break;
    //            parentIndex = (data.HeapIndex - 1) / 2;
    //            if (parentIndex < 0) break;
    //        }
    //}

    //void SortDownFrom(AStarNodeData data)
    //{
    //    while (true)
    //    {
    //        int leftChildIndex = data.HeapIndex * 2 + 1;
    //        int rightChildIndex = data.HeapIndex * 2 + 2;
    //        if (leftChildIndex < openCount)
    //        {
    //            int swapIndex = leftChildIndex;
    //            if (rightChildIndex < openCount && Grid[data.gridPosition].CompareTo(Grid[openList[rightChildIndex].gridPosition]) > 0)
    //                swapIndex = rightChildIndex;
    //            if (Grid[data.gridPosition].CompareTo(Grid[openList[swapIndex].gridPosition]) > 0)
    //            {
    //                var item2 = openList[swapIndex];
    //                Swap(ref data, ref item2);
    //            }
    //            else break;
    //        }
    //        else break;
    //    }
    //}

    //void UpdateOpen()
    //{
    //    SortDownFrom(openList[0]);
    //    SortUpFrom(openList[openCount - 1]);
    //}
    //#endregion
}

public struct AStarNodeData
{
    public int2 gridPosition;
    public int HeapIndex;
    public int SetIndex;

    public AStarNodeData(ref AStarNodeStruct node)
    {
        gridPosition = node.gridPosition;
        HeapIndex = node.HeapIndex;
        SetIndex = node.SetIndex;
    }
}

public struct AStarNodeStruct : IHeapItem<AStarNodeStruct>, ISetItem<AStarNodeStruct>
{
    public readonly float3 worldPosition;
    public readonly int2 gridPosition;
    public bool walkable;

    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get { return GCost + HCost; } }
    public float Height { get; private set; }

    public int2 parent;

    public int connectionLabel;

    public int HeapIndex { get; set; }
    public int SetIndex { get; set; }
    public bool IsInvalid => gridPosition.x < 0 || gridPosition.y < 0;

    public AStarNodeStruct(float3 position, int gridX, int gridY, float height, bool walkable)
    {
        worldPosition = position;
        gridPosition = new int2(gridX, gridY);
        Height = height;
        this.walkable = walkable;
        parent = new int2(-1, -1);
        connectionLabel = -1;
        GCost = 0;
        HCost = 0;
        HeapIndex = -1;
        SetIndex = -1;
    }

    public AStarNodeStruct(float3 position, int gridX, int gridY, float height, bool walkable, int hCost, int gCost, int2 parent, int connectionLabel, int heapIndex) : this(position, gridX, gridY, height, walkable)
    {
        HCost = hCost;
        GCost = gCost;
        this.parent = parent;
        this.connectionLabel = connectionLabel;
        HeapIndex = heapIndex;
    }

    public int CalculateHCostTo(AStarNodeStruct other)
    {
        //使用曼哈顿距离
        int disX = math.abs(gridPosition.x - other.gridPosition.x);
        int disY = math.abs(gridPosition.y - other.gridPosition.y);
        if (disX > disY)
            return 14 * disY + 10 * (disX - disY) + Mathf.RoundToInt(math.abs(Height - other.Height));
        else return 14 * disX + 10 * (disY - disX) + Mathf.RoundToInt(math.abs(Height - other.Height));
    }

    public bool CanReachTo(ref AStarNodeStruct other)
    {
        return connectionLabel > 0 && connectionLabel == other.connectionLabel;
    }

    public int CompareTo(AStarNodeStruct other)
    {
        if (FCost < other.FCost || (FCost == other.FCost && HCost < other.HCost) || (FCost == other.FCost && HCost == other.HCost && Height < other.Height))
            return -1;
        else if (FCost == other.FCost && HCost == other.HCost && Height == other.Height) return 0;
        else return 1;
    }

    public static AStarNodeStruct Defalut
    {
        get => new AStarNodeStruct(float3.zero, -1, -1, 0, false);
    }

    public static implicit operator float3(AStarNodeStruct self)
    {
        return self.worldPosition;
    }

    public static implicit operator float2(AStarNodeStruct self)
    {
        return new float2(self.worldPosition.x, self.worldPosition.y);
    }
}

public unsafe struct AStarNodeSet : IDisposable
{
    [NativeDisableUnsafePtrRestriction]
    private AStarNodeStruct** nodes;//可以看成AStarNodeStruct* *nodes，即一维指针数组 (node*)[]
    private int maxSize;

    public int Count { get; private set; }

    [NativeDisableUnsafePtrRestriction]
    private IntPtr mainPtr;
    [NativeDisableUnsafePtrRestriction]
    private NativeArray<IntPtr> subPtrs;

    public AStarNodeSet(int size, Allocator allocator)
    {
        mainPtr = Marshal.AllocHGlobal(sizeof(AStarNodeStruct*) * size);
        nodes = (AStarNodeStruct**)mainPtr;
        subPtrs = new NativeArray<IntPtr>(size, allocator);
        for (int i = 0; i < size; i++)
        {
            var ptr = Marshal.AllocHGlobal(sizeof(AStarNodeStruct));
            subPtrs[i] = ptr;
            nodes[i] = (AStarNodeStruct*)ptr;
        }
        maxSize = size;
        Count = 0;
    }

    public void Add(AStarNodeStruct* nodePtr)
    {
        if (Count >= maxSize) return;
        nodePtr->HeapIndex = Count;
        nodes[Count] = nodePtr;
        Count++;
    }

    public bool Contains(AStarNodeStruct* node)
    {
        if (node->HeapIndex < 0 || node->HeapIndex > Count - 1) return false;
        return Equals(nodes[node->HeapIndex]->gridPosition, node->gridPosition);
    }

    public void Dispose()
    {
        foreach (IntPtr p in subPtrs)
        {
            Marshal.FreeHGlobal(p);
        }
        subPtrs.Dispose();
        Marshal.FreeHGlobal(mainPtr);
    }
}

public interface ISetItem<T> where T : struct
{
    int SetIndex { get; set; }
}

public interface IHasIndex<T> where T : struct
{
    int Index { get; set; }
}

//public struct AStarStruct : IDisposable
//{
//    public AStarStruct(float2 worldSize, bool threeD, float worldHeight, int2 gridSize)
//    {
//        this.worldSize = worldSize;
//        GridSize = gridSize;
//        Grid = new AStarGrid(GridSize.x, GridSize.y);
//        this.threeD = threeD;
//        this.worldHeight = worldHeight;
//    }

//    private readonly float2 worldSize;
//    private readonly bool threeD;
//    private readonly float worldHeight;

//    public int2 GridSize { get; private set; }
//    public AStarGrid Grid { get; set; }

//    #region 网格相关
//    public AStarNodeStruct WorldPointToNode(float3 position)
//    {
//        if (threeD && position.y > worldHeight) return AStarNodeStruct.Defalut;
//        int gX = Mathf.RoundToInt((GridSize.x - 1) * Mathf.Clamp01((position.x + worldSize.x / 2) / worldSize.x));
//        int gY;
//        if (!threeD) gY = Mathf.RoundToInt((GridSize.y - 1) * Mathf.Clamp01((position.y + worldSize.y / 2) / worldSize.y));
//        else gY = Mathf.RoundToInt((GridSize.y - 1) * Mathf.Clamp01((position.z + worldSize.y / 2) / worldSize.y));
//        return Grid[gX, gY];
//    }

//    private AStarNodeStruct GetClosestSurroundingNode(AStarNodeStruct node, AStarNodeStruct closestTo, int ringCount = 1)
//    {
//        AStarNodeStruct closest = AStarNodeStruct.Defalut;
//        using (var neighbours = GetSurroundingNodes(node, ringCount))
//        {
//            if (ringCount >= math.max(GridSize.x, GridSize.y)) return AStarNodeStruct.Defalut;//突破递归
//            for (int i = 0; i < neighbours.Length; i++)
//            {
//                AStarNodeStruct neighbour = neighbours[i];
//                if (math.distance(closestTo.worldPosition, neighbour.worldPosition) < math.distance(closestTo.worldPosition, closest.worldPosition))
//                    if (neighbour.walkable) closest = neighbour;
//            }
//        }
//        if (closest == AStarNodeStruct.Defalut) return GetClosestSurroundingNode(node, closestTo, ringCount + 1);
//        else return closest;
//    }

//    private NativeList<AStarNodeStruct> GetSurroundingNodes(AStarNodeStruct node, int ringCount/*圈数*/)
//    {
//        NativeList<AStarNodeStruct> neighbours = new NativeList<AStarNodeStruct>(Allocator.Temp);
//        if (node != AStarNodeStruct.Defalut && ringCount > 0)
//        {
//            int neiborX;
//            int neiborY;
//            for (int x = -ringCount; x <= ringCount; x++)
//                for (int y = -ringCount; y <= ringCount; y++)
//                {
//                    if (Mathf.Abs(x) < ringCount && Mathf.Abs(y) < ringCount) continue;//对于圈内的结点，总有其x和y都小于圈数，所以依此跳过

//                    neiborX = node.gridPosition.x + x;
//                    neiborY = node.gridPosition.y + y;

//                    if (neiborX >= 0 && neiborX < GridSize.x && neiborY >= 0 && neiborY < GridSize.y)
//                        neighbours.Add(Grid[neiborX, neiborY]);
//                }
//        }
//        return neighbours;
//    }

//    public NativeList<AStarNodeStruct> GetReachableNeighbours(AStarNodeStruct node)
//    {
//        NativeList<AStarNodeStruct> neighbours = new NativeList<AStarNodeStruct>(Allocator.Temp);
//        int neiborX;
//        int neiborY;
//        for (int x = -1; x <= 1; x++)
//            for (int y = -1; y <= 1; y++)
//            {
//                if (x == 0 && y == 0) continue;

//                neiborX = node.gridPosition.x + x;
//                neiborY = node.gridPosition.y + y;

//                if (neiborX >= 0 && neiborX < GridSize.x && neiborY >= 0 && neiborY < GridSize.y)
//                {
//                    AStarNodeStruct neighbour = Grid[neiborX, neiborY];
//                    if (neighbour.gridPosition.x == node.gridPosition.x || neighbour.gridPosition.y == node.gridPosition.y)
//                    {
//                        if (!neighbour.walkable) neighbours.Add(Grid[neiborX, neiborY]);
//                    }
//                    if (!neighbour.walkable) continue;
//                    if (neighbour.gridPosition.x > node.gridPosition.x && neighbour.gridPosition.y > node.gridPosition.y)//右上角
//                    {
//                        int leftX = node.gridPosition.x;
//                        int leftY = neighbour.gridPosition.y;
//                        AStarNodeStruct leftNode = Grid[leftX, leftY];
//                        if (leftNode.walkable)
//                        {
//                            int downX = neighbour.gridPosition.x;
//                            int downY = node.gridPosition.y;
//                            AStarNodeStruct downNode = Grid[downX, downY];
//                            if (!downNode.walkable) continue;
//                            else neighbours.Add(Grid[neiborX, neiborY]);
//                        }
//                        else continue;
//                    }
//                    else if (neighbour.gridPosition.x > node.gridPosition.x && neighbour.gridPosition.y < node.gridPosition.y)//右下角
//                    {
//                        int leftX = node.gridPosition.x;
//                        int leftY = neighbour.gridPosition.y;
//                        AStarNodeStruct leftNode = Grid[leftX, leftY];
//                        if (leftNode.walkable)
//                        {
//                            int upX = neighbour.gridPosition.x;
//                            int upY = node.gridPosition.y;
//                            AStarNodeStruct upNode = Grid[upX, upY];
//                            if (!upNode.walkable) continue;
//                            else neighbours.Add(Grid[neiborX, neiborY]);
//                        }
//                        else continue;
//                    }
//                    else if (neighbour.gridPosition.x < node.gridPosition.x && neighbour.gridPosition.y > node.gridPosition.y)//左上角
//                    {
//                        int rightX = node.gridPosition.x;
//                        int rightY = neighbour.gridPosition.y;
//                        AStarNodeStruct rightNode = Grid[rightX, rightY];
//                        if (rightNode.walkable)
//                        {
//                            int downX = neighbour.gridPosition.x;
//                            int downY = node.gridPosition.y;
//                            AStarNodeStruct downNode = Grid[downX, downY];
//                            if (!downNode.walkable) continue;
//                            else neighbours.Add(Grid[neiborX, neiborY]);
//                        }
//                        else continue;
//                    }
//                    else if (neighbour.gridPosition.x < node.gridPosition.x && neighbour.gridPosition.y < node.gridPosition.y)//左下角
//                    {
//                        int rightX = node.gridPosition.x;
//                        int rightY = neighbour.gridPosition.y;
//                        AStarNodeStruct rightNode = Grid[rightX, rightY];
//                        if (rightNode.walkable)
//                        {
//                            int upX = neighbour.gridPosition.x;
//                            int upY = node.gridPosition.y;
//                            AStarNodeStruct upNode = Grid[upX, upY];
//                            if (!upNode.walkable) continue;
//                            else neighbours.Add(Grid[neiborX, neiborY]);
//                        }
//                        else continue;
//                    }
//                    else neighbours.Add(Grid[neiborX, neiborY]);
//                }
//            }
//        return neighbours;
//    }

//    private bool CanGoStraight(ref AStarNodeStruct from, ref AStarNodeStruct to)
//    {
//        //Debug.Log(string.Format("from {0} to {1}", from.gridPosition, to.gridPosition));
//        int startNodeNodeX = from.gridPosition.x, startNodeY = from.gridPosition.y;
//        int goalNodeX = to.gridPosition.x, goalNodeY = to.gridPosition.y;
//        int xDistn = Mathf.Abs(to.gridPosition.x - from.gridPosition.x);
//        int yDistn = Mathf.Abs(to.gridPosition.y - from.gridPosition.y);
//        float deltaX, deltaY;
//        if (xDistn >= yDistn)
//        {
//            deltaX = 1;
//            deltaY = yDistn / (xDistn * 1.0f);
//        }
//        else
//        {
//            deltaY = 1;
//            deltaX = xDistn / (yDistn * 1.0f);
//        }
//        //Debug.Log("deltaX: " + deltaX + ", deltaY: " + deltaY);
//        if (startNodeNodeX >= goalNodeX && startNodeY >= goalNodeY)//起点位于终点右上角
//        {
//            for (float x = goalNodeX + deltaX, y = goalNodeY + deltaY; x <= startNodeNodeX && y <= startNodeY; x += deltaX, y += deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (!Grid[gX, gY].walkable)
//                {
//                    //Debug.Log("右上角");
//                    return false;
//                }
//            }
//        }
//        else if (startNodeNodeX >= goalNodeX && startNodeY <= goalNodeY)//起点位于终点右下角
//        {
//            for (float x = goalNodeX + deltaX, y = goalNodeY - deltaY; x <= startNodeNodeX && y >= startNodeY; x += deltaX, y -= deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (!Grid[gX, gY].walkable)
//                {
//                    //Debug.Log("右下角");
//                    return false;
//                }
//            }
//        }
//        else if (startNodeNodeX <= goalNodeX && startNodeY >= goalNodeY)//起点位于终点左上角
//        {
//            for (float x = goalNodeX - deltaX, y = goalNodeY + deltaY; x >= startNodeNodeX && y <= startNodeY; x -= deltaX, y += deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (!Grid[gX, gY].walkable)
//                {
//                    //Debug.Log("左上角");
//                    return false;
//                }
//            }
//        }
//        else if (startNodeNodeX <= goalNodeX && startNodeY <= goalNodeY)//起点位于终点左下角
//        {
//            for (float x = goalNodeX - deltaX, y = goalNodeY - deltaY; x >= startNodeNodeX && y >= startNodeY; x -= deltaX, y -= deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (!Grid[gX, gY].walkable)
//                {
//                    //Debug.Log("左下角");
//                    return false;
//                }
//            }
//        }
//        return true;
//    }

//    private AStarNodeStruct GetEffectiveGoalByDiagonal(AStarNodeStruct startNode, AStarNodeStruct goalNode)
//    {
//        AStarNodeStruct newGoalNode = AStarNodeStruct.Defalut;
//        int startNodeNodeX = startNode.gridPosition.x, startNodeY = startNode.gridPosition.y;
//        int goalNodeX = goalNode.gridPosition.x, goalNodeY = goalNode.gridPosition.y;
//        int xDistn = Mathf.Abs(goalNode.gridPosition.x - startNode.gridPosition.x);
//        int yDistn = Mathf.Abs(goalNode.gridPosition.y - startNode.gridPosition.y);
//        float deltaX, deltaY;
//        if (xDistn >= yDistn)
//        {
//            deltaX = 1;
//            deltaY = yDistn / (xDistn * 1.0f);
//        }
//        else
//        {
//            deltaY = 1;
//            deltaX = xDistn / (yDistn * 1.0f);
//        }

//        if (startNodeNodeX >= goalNodeX && startNodeY >= goalNodeY)//起点位于终点右上角
//            for (float x = goalNodeX + deltaX, y = goalNodeY + deltaY; x <= startNodeNodeX && y <= startNodeY; x += deltaX, y += deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (Grid[gX, gY].CanReachTo(ref startNode))
//                {
//                    newGoalNode = Grid[gX, gY];
//                    break;
//                }
//            }
//        else if (startNodeNodeX >= goalNodeX && startNodeY <= goalNodeY)//起点位于终点右下角
//            for (float x = goalNodeX + deltaX, y = goalNodeY - deltaY; x <= startNodeNodeX && y >= startNodeY; x += deltaX, y -= deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (Grid[gX, gY].CanReachTo(ref startNode))
//                {
//                    newGoalNode = Grid[gX, gY];
//                    break;
//                }
//            }
//        else if (startNodeNodeX <= goalNodeX && startNodeY >= goalNodeY)//起点位于终点左上角
//            for (float x = goalNodeX - deltaX, y = goalNodeY + deltaY; x >= startNodeNodeX && y <= startNodeY; x -= deltaX, y += deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (Grid[gX, gY].CanReachTo(ref startNode))
//                {
//                    newGoalNode = Grid[gX, gY];
//                    break;
//                }
//            }
//        else if (startNodeNodeX <= goalNodeX && startNodeY <= goalNodeY)//起点位于终点左下角
//            for (float x = goalNodeX - deltaX, y = goalNodeY - deltaY; x >= startNodeNodeX && y >= startNodeY; x -= deltaX, y -= deltaY)
//            {
//                int gX = Mathf.RoundToInt(x);
//                int gY = Mathf.RoundToInt(y);
//                if (Grid[gX, gY].CanReachTo(ref startNode))
//                {
//                    newGoalNode = Grid[gX, gY];
//                    break;
//                }
//            }
//        return newGoalNode;
//    }

//    private AStarNodeStruct GetEffectiveGoalByRing(AStarNodeStruct startNode, AStarNodeStruct goalNode, int ringCount = 1)
//    {
//        if (ringCount >= Mathf.Max(GridSize.x, GridSize.y)) return AStarNodeStruct.Defalut;//突破递归
//        AStarNodeStruct newGoalNode = AStarNodeStruct.Defalut;
//        using (var neighbours = GetSurroundingNodes(goalNode, ringCount))
//        {
//            for (int i = 0; i < neighbours.Length; i++)
//            {
//                AStarNodeStruct neighbour = neighbours[i];
//                if (math.distance(goalNode.worldPosition, neighbour.worldPosition) < math.distance(goalNode.worldPosition, newGoalNode.worldPosition))
//                    if (neighbour.CanReachTo(ref startNode)) newGoalNode = neighbour;
//            }
//        }
//        if (newGoalNode == AStarNodeStruct.Defalut) return GetEffectiveGoalByRing(startNode, goalNode, ringCount + 1);
//        else return newGoalNode;
//    }
//    #endregion

//    #region 路径相关
//    //public void FindPath(PathRequestf request)
//    //{
//    //    AStarNodeStruct startNode = WorldPointToNode(request.start);
//    //    AStarNodeStruct goalNode = WorldPointToNode(request.goal);

//    //    NativeList<float3> pathResult = new NativeList<float3>(Allocator.Temp);
//    //    bool findSuccessfully = false;

//    //    if (!goalNode.walkable)
//    //    {
//    //        goalNode = GetClosestSurroundingNode(goalNode, startNode);
//    //        if (goalNode == default)
//    //        {
//    //            request.callback(default, false);
//    //            //Debug.Log("找不到合适的终点" + request.goal);
//    //            pathResult.Dispose();
//    //            return;
//    //        }
//    //    }
//    //    if (!startNode.walkable)
//    //    {
//    //        startNode = GetClosestSurroundingNode(startNode, goalNode);
//    //        if (startNode == AStarNodeStruct.Defalut)
//    //        {
//    //            request.callback(default, false);
//    //            //Debug.Log("找不到合适的起点" + request.start);
//    //            pathResult.Dispose();
//    //            return;
//    //        }
//    //    }
//    //    if (startNode == goalNode)
//    //    {
//    //        request.callback(default, false);
//    //        //Debug.Log("起始相同");
//    //        pathResult.Dispose();
//    //        return;
//    //    }
//    //    if (!startNode.CanReachTo(ref goalNode))
//    //    {
//    //        goalNode = GetEffectiveGoalByRing(startNode, goalNode);
//    //        if (goalNode == AStarNodeStruct.Defalut)
//    //        {
//    //            //Debug.Log("检测到目的地不可到达");
//    //            request.callback(default, false);
//    //            pathResult.Dispose();
//    //            return;
//    //        }
//    //    }

//    //    //Debug.Log("起点网格: " + startNode.GridPosition);
//    //    //Debug.Log("终点网格: " + goalNode.GridPosition);

//    //    if (CanGoStraight(ref startNode, ref goalNode))
//    //    {
//    //        findSuccessfully = true;
//    //        pathResult = new NativeList<float3>(Allocator.Temp);
//    //        pathResult.Add(goalNode);
//    //        //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过直走找到路径");
//    //        pathResult.Dispose();
//    //        request.callback(pathResult, true);
//    //    }

//    //    if (!findSuccessfully)
//    //    {
//    //        StructHeap<AStarNodeStruct> openList = new StructHeap<AStarNodeStruct>(GridSize.x * GridSize.y);
//    //        HashSet<AStarNodeStruct> closedList = new HashSet<AStarNodeStruct>();
//    //        openList.Add(ref startNode);
//    //        while (openList.Count > 0)
//    //        {
//    //            AStarNodeStruct current = openList.RemoveRoot();
//    //            if (current.Equals(default))
//    //            {
//    //                request.callback(pathResult, false);
//    //                pathResult.Dispose();
//    //                return;
//    //            }
//    //            closedList.Add(current);

//    //            if (current.Equals(goalNode))
//    //            {
//    //                findSuccessfully = true;
//    //                //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过搜索 " + closedList.Count + " 个结点找到路径");
//    //                break;
//    //            }
//    //            using (var neighbours = GetReachableNeighbours(current))
//    //                for (int i = 0; i < neighbours.Length; i++)
//    //                {
//    //                    AStarNodeStruct neighbour = neighbours[i];
//    //                    if (!neighbour.walkable || closedList.Contains(neighbour)) continue;
//    //                    int costStartToNeighbour = current.GCost + current.CalculateHCostTo(neighbour);
//    //                    if (costStartToNeighbour < neighbour.GCost || !openList.Contains(neighbour))
//    //                    {
//    //                        neighbour.GCost = costStartToNeighbour;
//    //                        neighbour.HCost = neighbour.CalculateHCostTo(goalNode);
//    //                        neighbour.parent = current.gridPosition;
//    //                        if (!openList.Contains(neighbour))
//    //                            openList.Add(ref neighbour);
//    //                        Grid.CoverOrInsert(neighbour.gridPosition.x, neighbour.gridPosition.y, neighbour);
//    //                    }
//    //                }
//    //        }
//    //        if (!findSuccessfully)
//    //        {
//    //            //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，搜索 " + closedList.Count + " 个结点后找不到路径");
//    //        }
//    //        else
//    //        {
//    //            goalNode.HeapIndex = -1;
//    //            startNode.HeapIndex = -1;
//    //            AStarNodeStruct pathNode = goalNode;
//    //            NativeList<AStarNodeStruct> path = new NativeList<AStarNodeStruct>(Allocator.TempJob);
//    //            while (pathNode.Equals(startNode))
//    //            {
//    //                path.Add(pathNode);
//    //                AStarNodeStruct temp = pathNode;
//    //                pathNode = Grid[pathNode.parent.x, pathNode.parent.y];
//    //                temp.parent = new int2(-1, -1);
//    //                temp.HeapIndex = -1;
//    //            }
//    //            pathResult = GetWaypoints(path);
//    //            //Debug.Log("耗时 " + stopwatch.ElapsedMilliseconds + "ms，通过搜索 " + closedList.Count + " 个结点后取得实际路径");
//    //        }
//    //    }
//    //    request.callback(pathResult, true);
//    //    pathResult.Dispose();
//    //}

//    public NativeList<float3> FindPath(float3 start, float3 goal)
//    {
//        AStarNodeStruct startNode = WorldPointToNode(start);
//        AStarNodeStruct goalNode = WorldPointToNode(goal);

//        NativeList<float3> pathResult = new NativeList<float3>(Allocator.Temp);
//        bool findSuccessfully = false;

//        if (!goalNode.walkable)
//        {
//            goalNode = GetClosestSurroundingNode(goalNode, startNode);
//            if (goalNode == AStarNodeStruct.Defalut)
//            {
//                Debug.Log("找不到合适的终点" + goal);
//                return pathResult;
//            }
//        }

//        if (!startNode.walkable)
//        {
//            startNode = GetClosestSurroundingNode(startNode, goalNode);
//            if (startNode == AStarNodeStruct.Defalut)
//            {
//                Debug.Log("找不到合适的起点" + start);
//                return pathResult;
//            }
//        }
//        if (startNode == goalNode)
//        {
//            Debug.Log("起始相同");
//            return pathResult;
//        }
//        if (!startNode.CanReachTo(ref goalNode))
//        {
//            goalNode = GetEffectiveGoalByRing(startNode, goalNode);
//            if (goalNode == AStarNodeStruct.Defalut)
//            {
//                Debug.Log("检测到目的地不可到达");
//                return pathResult;
//            }
//        }

//        //Debug.Log("起点网格: " + startNode.GridPosition);
//        //Debug.Log("终点网格: " + goalNode.GridPosition);

//        if (CanGoStraight(ref startNode, ref goalNode))
//        {
//            pathResult = new NativeList<float3>(1, Allocator.Temp);
//            pathResult.Add(goalNode);
//            Debug.Log("通过直走找到路径");
//            return pathResult;
//        }

//        if (!findSuccessfully)
//        {
//            NativeArray<AStarNodeData> openList = new NativeArray<AStarNodeData>(GridSize.x * GridSize.y, Allocator.Temp);
//            int openCount = 0;
//            int maxSize = GridSize.x * GridSize.y;

//            #region 往open加入起点
//            {
//                if (openCount < maxSize)
//                {
//                    startNode.HeapIndex = openCount;
//                    Grid.CoverOrInsert(startNode.gridPosition.x, startNode.gridPosition.y, startNode);
//                    openList[openCount] = new AStarNodeData(startNode);

//                    #region 向上排序
//                    {
//                        var item = openList[openCount];

//                        int parentIndex = (item.HeapIndex - 1) / 2;
//                        if (parentIndex >= 0)
//                            while (true)
//                            {
//                                AStarNodeData parent = openList[parentIndex];
//                                if (item.ToNode(Grid).CompareTo(parent.ToNode(Grid)) < 0)
//                                {
//                                    int item1Index = item.HeapIndex;
//                                    int item2Index = parent.HeapIndex;

//                                    item.HeapIndex = item2Index;
//                                    var node1 = item.ToNode(Grid);
//                                    node1.HeapIndex = item.HeapIndex;
//                                    Grid.CoverOrInsert(item.gridPosition.x, item.gridPosition.y, node1);

//                                    parent.HeapIndex = item1Index;

//                                    var node2 = parent.ToNode(Grid);
//                                    node2.HeapIndex = parent.HeapIndex;
//                                    Grid.CoverOrInsert(parent.gridPosition.x, parent.gridPosition.y, node2);

//                                    openList[item1Index] = parent;
//                                    openList[item2Index] = item;
//                                }
//                                else break;
//                                parentIndex = (item.HeapIndex - 1) / 2;
//                                if (parentIndex < 0) break;
//                            }
//                    }
//                    #endregion

//                    openCount++;
//                }
//            }
//            #endregion

//            using (AStarNodeSet closedList = new AStarNodeSet(GridSize.x * GridSize.y))
//            {
//                while (openCount > 0)
//                {
//                    AStarNodeStruct current;

//                    #region 获取最小结点
//                    {
//                        if (openCount < 1) current = AStarNodeStruct.Defalut;
//                        AStarNodeStruct root = openList[0].ToNode(Grid);
//                        root.HeapIndex = -1;
//                        Grid.CoverOrInsert(root.gridPosition.x, root.gridPosition.y, root);

//                        openCount--;
//                        var temp = openList[openCount];
//                        temp.HeapIndex = 0;
//                        openList[0] = temp;

//                        var node = temp.ToNode(Grid);
//                        node.HeapIndex = temp.HeapIndex;
//                        Grid.CoverOrInsert(temp.gridPosition.x, temp.gridPosition.y, node);

//                        #region 向下排序
//                        {
//                            var item = openList[0];
//                            while (true)
//                            {
//                                int leftChildIndex = item.HeapIndex * 2 + 1;
//                                int rightChildIndex = item.HeapIndex * 2 + 2;
//                                if (leftChildIndex < openCount)
//                                {
//                                    int swapIndex = leftChildIndex;
//                                    if (rightChildIndex < openCount && openList[leftChildIndex].ToNode(Grid).CompareTo(openList[rightChildIndex].ToNode(Grid)) > 0)
//                                        swapIndex = rightChildIndex;
//                                    if (item.ToNode(Grid).CompareTo(openList[swapIndex].ToNode(Grid)) > 0)
//                                    {
//                                        var item2 = openList[swapIndex];

//                                        int item1Index = item.HeapIndex;
//                                        int item2Index = item2.HeapIndex;

//                                        item.HeapIndex = item2Index;
//                                        var node1 = item.ToNode(Grid);
//                                        node1.HeapIndex = item.HeapIndex;
//                                        Grid.CoverOrInsert(item.gridPosition.x, item.gridPosition.y, node1);

//                                        item2.HeapIndex = item1Index;

//                                        var node2 = item2.ToNode(Grid);
//                                        node2.HeapIndex = item2.HeapIndex;
//                                        Grid.CoverOrInsert(item2.gridPosition.x, item2.gridPosition.y, node2);

//                                        openList[item1Index] = item2;
//                                        openList[item2Index] = item;
//                                    }
//                                    else break;
//                                }
//                                else break;
//                            }
//                        }
//                        #endregion

//                        current = root;
//                    }
//                    #endregion

//                    closedList.Add(ref current);
//                    Grid.CoverOrInsert(current.gridPosition.x, current.gridPosition.y, current);//因为是值类型，需要把数据写回覆盖

//                    if (Equals(current.gridPosition, goalNode.gridPosition))
//                    {
//                        findSuccessfully = true;
//                        Debug.Log("搜索 " + closedList.Count + " 个结点后找到路径");
//                        break;
//                    }
//                    using (var neighbours = GetReachableNeighbours(current))
//                        for (int i = 0; i < neighbours.Length; i++)
//                        {
//                            AStarNodeStruct neighbour = neighbours[i];
//                            if (!neighbour.walkable || closedList.Contains(ref neighbour))
//                            {
//                                //if (neighbour.walkable) Debug.Log(neighbour.SetIndex + ": " + neighbour.gridPosition);
//                                continue;
//                            }
//                            int costStartToNeighbour = current.GCost + current.CalculateHCostTo(neighbour);
//                            if (costStartToNeighbour < neighbour.GCost || !OpenContains(ref neighbour))
//                            {
//                                neighbour.GCost = costStartToNeighbour;
//                                neighbour.HCost = neighbour.CalculateHCostTo(goalNode);
//                                neighbour.parent = current.gridPosition;
//                                Grid.CoverOrInsert(neighbour.gridPosition.x, neighbour.gridPosition.y, neighbour);
//                                if (!OpenContains(ref neighbour))
//                                {
//                                    #region 往open加入neighbour
//                                    if (openCount < maxSize)
//                                    {
//                                        neighbour.HeapIndex = openCount;
//                                        Grid.CoverOrInsert(neighbour.gridPosition.x, neighbour.gridPosition.y, neighbour);
//                                        openList[openCount] = new AStarNodeData(neighbour);

//                                        #region 向上排序
//                                        var item = openList[openCount];
//                                        int parentIndex = (item.HeapIndex - 1) / 2;
//                                        if (parentIndex >= 0)
//                                            while (true)
//                                            {
//                                                AStarNodeData parent = openList[parentIndex];
//                                                if (item.ToNode(Grid).CompareTo(parent.ToNode(Grid)) < 0)
//                                                {
//                                                    int item1Index = item.HeapIndex;
//                                                    int item2Index = parent.HeapIndex;

//                                                    item.HeapIndex = item2Index;
//                                                    var node1 = item.ToNode(Grid);
//                                                    node1.HeapIndex = item.HeapIndex;
//                                                    Grid.CoverOrInsert(item.gridPosition.x, item.gridPosition.y, node1);

//                                                    parent.HeapIndex = item1Index;

//                                                    var node2 = parent.ToNode(Grid);
//                                                    node2.HeapIndex = parent.HeapIndex;
//                                                    Grid.CoverOrInsert(parent.gridPosition.x, parent.gridPosition.y, node2);

//                                                    openList[item1Index] = parent;
//                                                    openList[item2Index] = item;
//                                                }
//                                                else break;
//                                                parentIndex = (item.HeapIndex - 1) / 2;
//                                                if (parentIndex < 0) break;
//                                            }
//                                        #endregion

//                                        openCount++;
//                                    }
//                                    #endregion
//                                }
//                                else
//                                {
//                                    #region 向下排序
//                                    /*SortDown*/
//                                    var itemDown = openList[0];
//                                    while (true)
//                                    {
//                                        int leftChildIndex = itemDown.HeapIndex * 2 + 1;
//                                        int rightChildIndex = itemDown.HeapIndex * 2 + 2;
//                                        if (leftChildIndex < openCount)
//                                        {
//                                            int swapIndex = leftChildIndex;
//                                            if (rightChildIndex < openCount && openList[leftChildIndex].ToNode(Grid).CompareTo(openList[rightChildIndex].ToNode(Grid)) > 0)
//                                                swapIndex = rightChildIndex;
//                                            if (itemDown.ToNode(Grid).CompareTo(openList[swapIndex].ToNode(Grid)) > 0)
//                                            {
//                                                var item2 = openList[swapIndex];

//                                                int item1Index = itemDown.HeapIndex;
//                                                int item2Index = item2.HeapIndex;

//                                                itemDown.HeapIndex = item2Index;
//                                                var node1 = itemDown.ToNode(Grid);
//                                                node1.HeapIndex = itemDown.HeapIndex;
//                                                Grid.CoverOrInsert(itemDown.gridPosition.x, itemDown.gridPosition.y, node1);

//                                                item2.HeapIndex = item1Index;

//                                                var node2 = item2.ToNode(Grid);
//                                                node2.HeapIndex = item2.HeapIndex;
//                                                Grid.CoverOrInsert(item2.gridPosition.x, item2.gridPosition.y, node2);

//                                                openList[item1Index] = item2;
//                                                openList[item2Index] = itemDown;
//                                            }
//                                            else break;
//                                        }
//                                        else break;
//                                    }
//                                    #endregion

//                                    #region 向上排序
//                                    /*SortUp*/
//                                    var itemUp = openList[openCount - 1];
//                                    int parentIndex = (itemUp.HeapIndex - 1) / 2;
//                                    if (parentIndex >= 0)
//                                        while (true)
//                                        {
//                                            AStarNodeData parent = openList[parentIndex];
//                                            if (itemUp.ToNode(Grid).CompareTo(parent.ToNode(Grid)) < 0)
//                                            {
//                                                int item1Index = itemUp.HeapIndex;
//                                                int item2Index = parent.HeapIndex;

//                                                itemUp.HeapIndex = item2Index;
//                                                var node1 = itemUp.ToNode(Grid);
//                                                node1.HeapIndex = itemUp.HeapIndex;
//                                                Grid.CoverOrInsert(itemUp.gridPosition.x, itemUp.gridPosition.y, node1);

//                                                parent.HeapIndex = item1Index;

//                                                var node2 = parent.ToNode(Grid);
//                                                node2.HeapIndex = parent.HeapIndex;
//                                                Grid.CoverOrInsert(parent.gridPosition.x, parent.gridPosition.y, node2);

//                                                openList[item1Index] = parent;
//                                                openList[item2Index] = itemUp;
//                                            }
//                                            else break;
//                                            parentIndex = (itemUp.HeapIndex - 1) / 2;
//                                            if (parentIndex < 0) break;
//                                        }
//                                    #endregion
//                                }
//                            }
//                        }
//                }
//            }
//            if (findSuccessfully)
//            {
//                AStarNodeStruct pathNode = goalNode;
//                using (NativeList<AStarNodeStruct> path = new NativeList<AStarNodeStruct>(Allocator.Temp))
//                {
//                    while (!Equals(pathNode.gridPosition, startNode.gridPosition))
//                    {
//                        path.Add(pathNode);
//                        pathNode = Grid[pathNode.parent.x, pathNode.parent.y];
//                    }
//                    pathResult = GetWaypoints(path);
//                }
//            }
//            openList.Dispose();

//            bool OpenContains(ref AStarNodeStruct item)
//            {
//                if (item.HeapIndex < 0 || item.HeapIndex > openList.Length - 1) return false;
//                return Equals(openList[item.HeapIndex].gridPosition, item.gridPosition);
//            }
//        }
//        return pathResult;
//    }

//    private NativeList<float3> GetWaypoints(NativeList<AStarNodeStruct> path)
//    {
//        NativeList<float3> waypoints = new NativeList<float3>(Allocator.Temp);
//        if (path.Length < 1)
//        {
//            return waypoints;
//        }

//        PathToWaypoints();

//        Reverse();

//        path.Dispose();

//        return waypoints;

//        void Reverse()
//        {
//            using (NativeList<float3> reversed = new NativeList<float3>(waypoints.Length, Allocator.Temp))
//            {
//                for (int i = waypoints.Length - 1; i > 0; i--)
//                {
//                    reversed.Add(waypoints[i]);
//                }
//                waypoints = reversed;
//            }
//        }

//        void PathToWaypoints(bool simplify = true)
//        {
//            if (simplify)
//            {
//                float2 oldDir = float2.zero;
//                for (int i = 1; i < path.Length; i++)
//                {
//                    float2 newDir = path[i - 1].gridPosition - path[i].gridPosition;
//                    if (!newDir.Equals(oldDir))//方向不一样时才使用前面的点
//                        waypoints.Add(path[i - 1]);
//                    else if (i == path.Length - 1) waypoints.Add(path[i]);//即使方向一样，也强制把起点也加进去
//                    oldDir = newDir;
//                }
//            }
//            else
//            {
//                for (int i = 0; i < path.Length; i++)
//                {
//                    waypoints.Add(path[i]);
//                }
//            }
//        }
//    }
//    #endregion


//    public void Dispose()
//    {
//        Grid.Dispose();
//    }
//}

//public struct AStarGrid : IDisposable
//{
//    private NativeArray<AStarNodeStruct> nodes;
//    private int2 size;
//    private int currentCount;

//    unsafe private AStarGrid(int sizeX, int sizeY)
//    {
//        size = new int2(sizeX, sizeY);
//        nodes = new NativeArray<AStarNodeStruct>(sizeX * sizeY, Allocator.TempJob);
//        currentCount = 0;
//    }

//    unsafe public AStarGrid(AStarNodeStruct[,] grid)
//    {
//        int sizeX = grid.GetLength(0);
//        int sizeY = grid.GetLength(1);
//        size = new int2(sizeX, sizeY);
//        nodes = new NativeArray<AStarNodeStruct>(sizeX * sizeY, Allocator.TempJob);
//        currentCount = 0;
//        for (int i = 0; i < sizeX; i++)
//            for (int j = 0; j < sizeY; j++)
//                Append(grid[i, j]);
//    }

//    unsafe public AStarNodeStruct this[int index1, int index2]
//    {
//        get
//        {
//            int index = IndexOf(index1, index2);
//            return nodes[index];
//        }
//    }

//    public AStarNodeStruct this[int2 index]
//    {
//        get
//        {
//            int nindex = IndexOf(index.x, index.y);
//            return nodes[nindex];
//        }
//    }

//    public bool TryCover(AStarNodeStruct value)
//    {
//        int nindex = value.Index;
//        if (nindex >= 0)
//        {
//            nodes[nindex] = value;
//            return true;
//        }
//        else return false;
//    }

//    private int IndexOf(int index1, int index2)
//    {
//        int tempIndex = -1;
//        for (int i = 0; i < size.x; i++)
//        {
//            for (int j = 0; j < size.y; j++)
//            {
//                tempIndex++;
//                if (i == index1 && j == index2)
//                    return tempIndex;
//            }
//        }
//        return -1;
//    }

//    public void Append(AStarNodeStruct value)
//    {
//        if (currentCount >= nodes.Length) return;
//        value.Index = currentCount;
//        nodes[currentCount] = value;
//        currentCount++;
//    }

//    public void Dispose()
//    {
//        nodes.Dispose();
//    }

//    public static implicit operator AStarGrid(AStarNodeStruct[,] grid)
//    {
//        int sizeX = grid.GetLength(0);
//        int sizeY = grid.GetLength(1);
//        AStarGrid starGrid = new AStarGrid(sizeX, sizeY);
//        for (int i = 0; i < sizeX; i++)
//            for (int j = 0; j < sizeY; j++)
//                starGrid.Append(grid[i, j]);
//        return starGrid;
//    }

//    public static implicit operator AStarGrid(AStarNode[,] grid)
//    {
//        int sizeX = grid.GetLength(0);
//        int sizeY = grid.GetLength(1);
//        AStarGrid starGrid = new AStarGrid(sizeX, sizeY);
//        for (int i = 0; i < sizeX; i++)
//            for (int j = 0; j < sizeY; j++)
//                starGrid.Append(grid[i, j].Structed);
//        return starGrid;
//    }
//}


public unsafe struct AStarGrid : IDisposable
{
    [NativeDisableUnsafePtrRestriction]
    public unsafe readonly AStarNodeStruct*** nodes;//可以看成AStarNodeStruct** *nodes，即二维指针数组 (node*)[,]
    public int2 size;

    [NativeDisableUnsafePtrRestriction]
    private IntPtr mainPtr;
    [NativeDisableUnsafePtrRestriction]
    private NativeArray<IntPtr> subPtrs1;

    public AStarGrid(int sizeX, int sizeY, Allocator allocator)
    {
        size = new int2(sizeX, sizeY);
        //分配***级指针空间
        mainPtr = Marshal.AllocHGlobal(sizeof(AStarNodeStruct**) * sizeX);
        nodes = (AStarNodeStruct***)mainPtr;
        //分配**级指针空间
        subPtrs1 = new NativeArray<IntPtr>(sizeX, allocator);
        //指向二维[指针数组]第一维的指针，一共有sizeX个，每一个能够容纳一个指向二维[指针数组]第二维的指针
        for (int i = 0; i < sizeX; i++)
        {
            //指向二维[指针数组]第二维的指针，每一个一维元素有sizeY个这种指针，每一个能容纳一个指向AStarNodeStruct的指针
            var ptr1 = Marshal.AllocHGlobal(sizeof(AStarNodeStruct*) * sizeY);
            subPtrs1[i] = ptr1;
            nodes[i] = (AStarNodeStruct**)ptr1;
        }
    }

    public AStarNodeStruct* this[int index1, int index2]
    {
        get
        {
            return nodes[index1][index2];
        }
    }

    public AStarNodeStruct* this[int2 dindex]
    {
        get
        {
            return nodes[dindex.x][dindex.y];
        }
    }

    public void Dispose()
    {
        foreach (IntPtr p in subPtrs1)
        {
            Marshal.FreeHGlobal(p);//释放**级指针
        }
        subPtrs1.Dispose();

        Marshal.FreeHGlobal(mainPtr);//释放***级指针
    }

    public static AStarGrid GetAStarGrid(ref AStarNodeStruct[,] grid, Allocator allocator)
    {
        int sizeX = grid.GetLength(0);
        int sizeY = grid.GetLength(1);
        AStarGrid starGrid = new AStarGrid(sizeX, sizeY, allocator);
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            {
                var node = grid[x, y];
                starGrid.nodes[x][y] = &node;
            }
        return starGrid;
    }

    public static AStarGrid GetAStarGrid(AStarNode[,] grid, Allocator allocator)
    {
        int sizeX = grid.GetLength(0);
        int sizeY = grid.GetLength(1);
        AStarGrid starGrid = new AStarGrid(sizeX, sizeY, allocator);
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            {
                var node = grid[x, y].Structed;
                starGrid.nodes[x][y] = &node;
            }
        return starGrid;
    }
}

public unsafe struct AStarNodeHeap : IDisposable
{
    [NativeDisableUnsafePtrRestriction]
    private AStarNodeStruct** nodes;//可以看成AStarNodeStruct* *nodes，即一维指针数组 (node*)[]
    private int maxSize;

    public int Count { get; private set; }

    [NativeDisableUnsafePtrRestriction]
    private IntPtr mainPtr;
    [NativeDisableUnsafePtrRestriction]
    private NativeArray<IntPtr> subPtrs;

    public AStarNodeHeap(int size, Allocator allocator)
    {
        mainPtr = Marshal.AllocHGlobal(sizeof(AStarNodeStruct*) * size);
        nodes = (AStarNodeStruct**)mainPtr;
        subPtrs = new NativeArray<IntPtr>(size, allocator);
        for (int i = 0; i < size; i++)
        {
            var ptr = Marshal.AllocHGlobal(sizeof(AStarNodeStruct));
            subPtrs[i] = ptr;
            nodes[i] = (AStarNodeStruct*)ptr;
        }
        maxSize = size;
        Count = 0;
    }

    public void Add(AStarNodeStruct* nodePtr)
    {
        if (Count >= maxSize) return;
        nodePtr->HeapIndex = Count;
        nodes[Count] = nodePtr;
        Count++;
        SortUp(nodes[Count]);
    }

    public AStarNodeStruct* RemoveRoot()
    {
        if (Count < 1) return null;
        AStarNodeStruct* root = nodes[0];
        root->HeapIndex = -1;
        Count--;
        nodes[0] = nodes[Count];
        nodes[0]->HeapIndex = 0;
        SortDown(nodes[0]);
        return root;
    }

    public bool Contains(AStarNodeStruct* node)
    {
        if (node->HeapIndex < 0 || node->HeapIndex > Count - 1) return false;
        return Equals(nodes[node->HeapIndex]->gridPosition, node->gridPosition);
    }

    private void SortUp(AStarNodeStruct* nodePtr)
    {
        int parentIndex = (nodePtr->HeapIndex - 1) / 2;
        if (parentIndex < 0) return;
        while (true)
        {
            AStarNodeStruct* parent = nodes[parentIndex];
            if ((*nodePtr).CompareTo(*parent) < 0)//当前结点比他的父节点还小，进行交换
            {
                Swap(nodePtr, parent);
            }
            else break;
            parentIndex = (nodePtr->HeapIndex - 1) / 2;
            if (parentIndex < 0) return;
        }
    }

    private void SortDown(AStarNodeStruct* nodePtr)
    {
        while (true)
        {
            int leftChildIndex = nodePtr->HeapIndex * 2 + 1;
            int rightChildIndex = nodePtr->HeapIndex * 2 + 2;
            if (leftChildIndex < Count)
            {
                int swapIndex = leftChildIndex;//假设需要交换左子结点
                if (rightChildIndex < Count && (*nodes[rightChildIndex]).CompareTo(*nodes[leftChildIndex]) < 0)
                    swapIndex = rightChildIndex;//如果右子结点比左子结点小，那么可能需要交换该右子结点
                if ((*nodePtr).CompareTo(*nodes[swapIndex]) > 0)//当前结点比假设需要交换的结点大，假设成立，进行交换
                    Swap(nodePtr, nodes[swapIndex]);
                else return;
            }
            else return;
        }
    }

    public void Update()
    {
        if (Count < 1) return;
        SortDown(nodes[0]);
        SortUp(nodes[Count - 1]);
    }

    private void Swap(AStarNodeStruct* nodePtr1, AStarNodeStruct* nodePtr2)
    {
        if (!Contains(nodePtr1) || !Contains(nodePtr2)) return;
        nodes[nodePtr1->HeapIndex] = nodePtr2;
        nodes[nodePtr2->HeapIndex] = nodePtr1;
        int item1Index = nodePtr1->HeapIndex;
        nodePtr1->HeapIndex = nodePtr2->HeapIndex;
        nodePtr2->HeapIndex = item1Index;
    }

    public void Dispose()
    {
        foreach (IntPtr p in subPtrs)
        {
            Marshal.FreeHGlobal(p);
        }
        subPtrs.Dispose();
        Marshal.FreeHGlobal(mainPtr);
    }
}

public unsafe struct UnmanagedHeap<T> : IDisposable where T : unmanaged, IHeapItem<T>
{
    [NativeDisableUnsafePtrRestriction]
    private readonly T** items;//可以看成AStarNodeStruct* *nodes，即一维指针数组 (node*)[]
    private readonly int maxSize;

    public int Count { get; private set; }

    [NativeDisableUnsafePtrRestriction]
    private readonly IntPtr mainPtr;

    public UnmanagedHeap(int size)
    {
        mainPtr = Marshal.AllocHGlobal(sizeof(T*) * size);
        items = (T**)mainPtr;
        maxSize = size;
        Count = 0;
    }

    public void Add(T* itemPtr)
    {
        if (Count >= maxSize) return;
        itemPtr->HeapIndex = Count;
        items[Count] = itemPtr;
        Count++;
        SortUp(itemPtr);
    }

    public T* RemoveRoot()
    {
        if (Count < 1) return null;
        T* root = items[0];
        root->HeapIndex = -1;
        Count--;
        if (Count > 0)
        {
            items[0] = items[Count];
            items[0]->HeapIndex = 0;
            SortDown(items[0]);
        }
        return root;
    }

    public bool Contains(T* itemPtr)
    {
        if (itemPtr->HeapIndex < 0 || itemPtr->HeapIndex > Count - 1) return false;
        return items[itemPtr->HeapIndex] == itemPtr;
    }

    private void SortUp(T* itemPtr)
    {
        int parentIndex = (itemPtr->HeapIndex - 1) / 2;
        while (true)
        {
            T* parent = items[parentIndex];
            if (parent == itemPtr) return;
            if ((*itemPtr).CompareTo(*parent) < 0)//当前结点比他的父节点还小，进行交换
            {
                if (!Swap(itemPtr, parent))
                    return;
            }
            else return;
            parentIndex = (itemPtr->HeapIndex - 1) / 2;
        }
    }

    private void SortDown(T* itemPtr)
    {
        while (true)
        {
            int leftChildIndex = itemPtr->HeapIndex * 2 + 1;
            int rightChildIndex = itemPtr->HeapIndex * 2 + 2;
            if (leftChildIndex < Count)
            {
                int swapIndex = leftChildIndex;//假设需要交换左子结点
                if (rightChildIndex < Count && (*items[rightChildIndex]).CompareTo(*items[leftChildIndex]) < 0)
                    swapIndex = rightChildIndex;//如果右子结点比左子结点小，那么可能需要交换该右子结点
                if ((*itemPtr).CompareTo(*items[swapIndex]) > 0)//当前结点比假设需要交换的结点大，假设成立，进行交换
                {
                    if (!Swap(itemPtr, items[swapIndex]))//交换不成功则退出，防止死循环
                        return;
                }
                else return;
            }
            else return;
        }
    }

    public void Update()
    {
        if (Count < 1) return;
        SortDown(items[0]);
        SortUp(items[Count - 1]);
    }

    private bool Swap(T* itemPtr1, T* itemPtr2)
    {
        if (!Contains(itemPtr1) || !Contains(itemPtr2)) return false;

        int item1Index = itemPtr1->HeapIndex;
        int item2Index = itemPtr2->HeapIndex;

        itemPtr1->HeapIndex = item2Index;
        itemPtr2->HeapIndex = item1Index;

        items[item1Index] = itemPtr2;
        items[item2Index] = itemPtr1;

        return true;
    }

    public void Clear()
    {
        Count = 0;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(mainPtr);
    }
}