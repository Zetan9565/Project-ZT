namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 随机选择器：当任意子结点评估成功时，向上反馈评估成功；若所有子结点都失败时，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("随机选择器：当任意子结点评估成功时，向上反馈评估成功；若所有子结点都失败时，向上反馈评估失败；若任意子结点还在评估，则向上反馈评估正进行")]
    public class RandomSelector : Composite
    {
        protected override NodeStates OnUpdate()
        {
            foreach (var child in children)
            {
                if (child.IsValid)
                    switch (child.Evaluate())
                    {
                        case NodeStates.Inactive:
                            continue;
                        case NodeStates.Success:
                            return NodeStates.Success;
                        case NodeStates.Failure:
                            continue;
                        case NodeStates.Running:
                            return NodeStates.Running;
                    }
            }
            return NodeStates.Failure;
        }
    }
}