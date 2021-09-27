using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("随机选择器：按随机顺序逐个评估子结点，若当前子结点评估成功，则向上反馈评估成功；若所有子结点都评估失败向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行")]
    public class RandomSelector : Composite
    {
        private readonly List<int> childIndexList = new List<int>();
        private readonly Stack<int> childrenExecutionOrder = new Stack<int>();

        protected override void OnStart()
        {
            childIndexList.Clear();
            for (int i = 0; i < children.Count; ++i)
            {
                childIndexList.Add(i);
            }
            CalculateOrder();
            currentChildIndex = childrenExecutionOrder.Pop();
            currentChild = children[currentChildIndex];
        }

        protected override NodeStates OnUpdate()
        {
            switch (currentChild.Evaluate())
            {
                case NodeStates.Success:
                    InactivateFrom(currentChildIndex);
                    return NodeStates.Success;
                case NodeStates.Failure:
                    if (childrenExecutionOrder.Count <= 0) //能够到达这一步，说明前面没有一个成功的，所以评估失败
                        return NodeStates.Failure;
                    else
                    {
                        currentChildIndex = childrenExecutionOrder.Pop();
                        currentChild = children[currentChildIndex];
                        InactivateFrom(currentChildIndex);
                        return NodeStates.Running;
                    }
                case NodeStates.Inactive:
                case NodeStates.Running:
                    InactivateFrom(currentChildIndex);
                    return NodeStates.Running;
            }
            return NodeStates.Failure;
        }

        private void CalculateOrder()
        {
            for (int i = childIndexList.Count; i > 0; --i)
            {
                int j = Random.Range(0, i);
                int index = childIndexList[j];
                childrenExecutionOrder.Push(index);
                childIndexList[j] = childIndexList[i - 1];
                childIndexList[i - 1] = index;
            }
        }
    }
}