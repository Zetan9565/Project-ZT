namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("评估选择器：每次都重头开始评估子结点，若当前子结点还在进行评估，则向上反馈评估正进行；若评估成功，则向上反馈评估成功；若评估失败，则转至下一个子结点")]
    public class SelectorEvaluator : Composite
    {
        protected override NodeStates OnUpdate()
        {
            for (int i = currentChildIndex; i < children.Count; i++)
            {
                switch (children[i].Evaluate())
                {
                    case NodeStates.Success:
                        InactivateFrom(i);
                        return NodeStates.Success;
                    case NodeStates.Running:
                        InactivateFrom(i);
                        return NodeStates.Running;
                }
            }
            return NodeStates.Failure;
        }

        protected override void OnEnd()
        {
            currentChildIndex = 0;
        }
    }
}