namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 条件结点：判断设置的条件是否符合并向上反馈相应评估结果，可选是否持续进行评估。与<see cref="Action"/>的区别：可用于中止<see cref="Composite"/>的评估。
    /// </summary>
    public abstract class Conditional : Node
    {
        private bool latestState;

        public abstract bool CheckCondition();

        protected sealed override NodeStates OnUpdate()
        {
            latestState = CheckCondition();
            if (latestState) return NodeStates.Success;
            else return NodeStates.Failure;
        }

        public bool CheckConditionalAbort()
        {
            return latestState != CheckCondition();//发生变化才会引发Abort
        }
    }
}