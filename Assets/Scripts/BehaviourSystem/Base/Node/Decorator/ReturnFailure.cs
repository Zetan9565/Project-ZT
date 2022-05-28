namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 直接失败结点：不论子结点评估结果如何，都向上反馈评估失败
    /// </summary>
    [Description("直接失败结点：不论子结点评估结果如何，都向上反馈评估失败")]
    public class ReturnFailure : Decorator
    {
        protected override NodeStates OnUpdate()
        {
            child.Evaluate();
            return NodeStates.Failure;
        }
    }
}