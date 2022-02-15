namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 选择器：当当前子结点评估成功时，向上反馈评估成功；若所有子结点都失败时，向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("选择器：若当前子结点评估成功，向上反馈评估成功；若所有子结点都失败，向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行")]
    public class Selector : Composite
    {
        protected override NodeStates OnUpdate()
        {
            switch (currentChild?.Evaluate())
            {
                case NodeStates.Success:
                    InactivateFrom(currentChildIndex);
                    return NodeStates.Success;
                case NodeStates.Failure:
                    currentChildIndex++;
                    if (currentChildIndex >= children.Count) //能够到达这一步，说明前面没有一个成功的，所以评估失败
                        return NodeStates.Failure;
                    else
                    {
                        HandlingCurrentChild();
                        InactivateFrom(currentChildIndex);
                        return NodeStates.Running;
                    }
                case NodeStates.Inactive:
                case NodeStates.Running:
                    InactivateFrom(currentChildIndex);
                    return NodeStates.Running;
            }
            return NodeStates.Failure;
        }
    }
}