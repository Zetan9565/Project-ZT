namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("并行完成选择器：同时评估所有子结点，当任意子结点评估出结果时，向上反馈相应的评估结果；若子结点还在进行评估，则向上反馈评估正进行")]
    public class ParallelComplete : Composite
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
                            InactivateFrom(i);
                            return NodeStates.Success;
                        case NodeStates.Failure:
                            InactivateFrom(i);
                            return NodeStates.Failure;
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