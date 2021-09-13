namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 并行序列器：当没有子结点在进行评估时，向上反馈评估成功；若任意子结点评估失败时，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("并行序列器：当没有子结点在进行评估时，向上反馈评估成功；若任意子结点评估失败，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行")]
    public class Parallel : Composite
    {
        protected override NodeStates OnUpdate()
        {
            bool hasChildRunning = false;
            for (int i = currentIndex; i < children.Count; i++)
            {
                Node child = children[i];
                if (child.IsValid && child.State != NodeStates.Success)
                    switch (child.Evaluate())
                    {
                        case NodeStates.Failure:
                            return NodeStates.Failure;
                        case NodeStates.Running:
                            hasChildRunning = true;
                            break;
                    }
            }
            return hasChildRunning ? NodeStates.Running : NodeStates.Success;
        }
    }
}