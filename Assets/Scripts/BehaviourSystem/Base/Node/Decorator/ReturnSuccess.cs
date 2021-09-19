namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 直接成功结点：不论子结点评估结果如何，都向上反馈评估成功
    /// </summary>
    [NodeDescription("直接成功结点：不论子结点评估结果如何，都向上反馈评估成功")]
    public class ReturnSuccess : Decorator
    {
        protected override NodeStates OnUpdate()
        {
            child.Evaluate();
            return NodeStates.Success;
        }
    }
}