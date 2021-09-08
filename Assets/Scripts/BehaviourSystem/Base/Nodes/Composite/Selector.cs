namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 选择器：当当前子结点评估成功时，向上反馈评估成功；若所有子结点都失败时，向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行
    /// </summary>
    [NodeDescription("选择器：当当前子结点评估成功时，向上反馈评估成功；若所有子结点都失败时，向上反馈评估失败；若当前子结点还在评估，则向上反馈评估正进行")]
    public class Selector : Composite
    {
        private int currentIndex;
        private Node currentChild;

        protected override void OnStart()
        {
            currentIndex = 0;
            if (children.Count > 0)
            {
                currentChild = children[currentIndex];
                while (!currentChild.IsValid)
                {
                    currentChild = children[currentIndex++];
                }
            }
        }

        protected override NodeStates OnUpdate()
        {
            switch (currentChild.Evaluate())
            {
                case NodeStates.Success:
                    return NodeStates.Success;
                case NodeStates.Failure:
                    currentIndex++;
                    if (currentIndex >= children.Count) //能够到达这一步，说明前面没有一个成功的，所以评估失败
                        return NodeStates.Failure;
                    else
                    {
                        currentChild = children[currentIndex];
                        while (!currentChild.IsValid)
                        {
                            currentChild = children[currentIndex++];
                        }
                        return NodeStates.Running;
                    }
                case NodeStates.Inactive:
                case NodeStates.Running:
                default:
                    return NodeStates.Running;
            }
        }
    }
}