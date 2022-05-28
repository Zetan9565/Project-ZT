namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 重复直到失败结点：如果子结点没有评估失败，则会持续评估此结点
    /// </summary>
    [Description("重复直到失败结点：如果子结点没有评估失败，则会持续评估此结点")]
    public class UntilFailure : Decorator
    {
        protected override NodeStates OnUpdate()
        {
            if (!child) return NodeStates.Failure;
            return child.Evaluate() switch
            {
                NodeStates.Failure => NodeStates.Success,
                _ => NodeStates.Running,
            };
        }
    }
}