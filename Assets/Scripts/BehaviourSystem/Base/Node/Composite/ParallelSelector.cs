namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 并行选择器：当任意子结点评估成功时，向上反馈评估成功；若所有子结点都失败，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("并行选择器：当任意子结点评估成功时，向上反馈评估成功；若所有子结点都评估失败，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行")]
    public class ParallelSelector : Composite
    {
        protected override NodeStates OnUpdate()
        {
            bool hasChildRunning = false;
            for (int i = currentChildIndex; i < children.Count; i++)
            {
                Node child = children[i];
                if (child.IsValid)
                    switch (child.Evaluate())
                    {
                        case NodeStates.Success:
                            return NodeStates.Success;
                        case NodeStates.Running:
                            hasChildRunning = true;
                            break;
                    }
            }
            return hasChildRunning ? NodeStates.Running : NodeStates.Failure;
        }

        protected override void OnEnd()
        {
            currentChildIndex = 0;
        }
    }
}