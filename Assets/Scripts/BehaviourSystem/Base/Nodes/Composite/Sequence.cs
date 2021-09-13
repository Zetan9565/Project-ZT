using System.Collections.Generic;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 序列器：当没有子结点在进行评估时，向上反馈评估成功；若当前子结点评估失败时，向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("序列器：当没有子结点在进行评估时，向上反馈评估成功；若当前子结点评估失败时，向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行")]
    public class Sequence : Composite
    {
        protected override NodeStates OnUpdate()
        {
            switch (currentChild.Evaluate())
            {
                case NodeStates.Success:
                    if (currentIndex + 1 >= children.Count) //执行到这一步，还没有失败的，说明所有子结点都评估成功了
                        return NodeStates.Success;
                    else
                    {
                        currentIndex++;
                        currentChild = children[currentIndex];
                        while (!currentChild.IsValid)
                        {
                            currentChild = children[currentIndex++];
                        }
                        return NodeStates.Running;
                    }
                case NodeStates.Failure:
                    return NodeStates.Failure;
                default:
                    return NodeStates.Running;
            }
        }
    }
}