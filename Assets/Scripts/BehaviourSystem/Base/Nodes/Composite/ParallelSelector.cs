namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 并行选择器：当任意子结点评估成功时，向上反馈评估成功；若所有子结点都失败时，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("并行选择器：当任意子结点评估成功时，向上反馈评估成功；若所有子结点都评估失败，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行")]
    public class ParallelSelector : Composite
    {
        protected override NodeStates OnUpdate()
        {
            for (int i = currentIndex; i < children.Count; i++)
            {
                Node child = children[i];
                if (child.IsValid && child.State != NodeStates.Failure)
                    switch (child.Evaluate())
                    {
                        case NodeStates.Success:
                            return NodeStates.Success;
                        case NodeStates.Running:
                            return NodeStates.Running;
                    }
            }
            return NodeStates.Failure;
        }
    }
}