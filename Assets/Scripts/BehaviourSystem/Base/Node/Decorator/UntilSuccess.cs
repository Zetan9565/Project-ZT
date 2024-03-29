namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 重复直到成功结点：如果子结点没有评估成功，则会持续评估此结点
    /// </summary>
    [Description("重复直到成功结点：如果子结点没有评估成功，则会持续评估此结点")]
    public class UntilSuccess : Decorator
    {
        protected override NodeStates OnUpdate()
        {
            if (!child) return NodeStates.Failure;
            return child.Evaluate() switch
            {
                NodeStates.Success => NodeStates.Success,
                _ => NodeStates.Running,
            };
        }
    }
}