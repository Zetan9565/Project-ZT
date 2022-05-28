namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 逆变器：当子结点评估失败时，向上反馈评估成功；若子结点评估成功，则向上反馈评估失败；若子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [Description("逆变器：当子结点评估失败时，向上反馈评估成功；若子结点评估成功，则向上反馈评估失败；若子结点还在评估，则向上反馈评估正进行")]
    public class Inverter : Decorator
    {
        protected override NodeStates OnUpdate()
        {
            return child.Evaluate() switch
            {
                NodeStates.Success => NodeStates.Failure,
                NodeStates.Failure => NodeStates.Success,
                NodeStates.Running => NodeStates.Running,
                _ => NodeStates.Success,
            };
        }
    }
}