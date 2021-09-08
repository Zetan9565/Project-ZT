namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 重复直到失败结点：如果子结点没有评估失败，则会持续评估此结点
    /// </summary>
    [NodeDescription("重复直到失败结点：如果子结点没有评估失败，则会持续评估此结点")]
    public class UntilFailure : Decorator
    {
        protected override NodeStates OnUpdate()
        {
            return child.Evaluate() switch
            {
                NodeStates.Failure => NodeStates.Success,
                _ => NodeStates.Running,
            };
        }
    }
}