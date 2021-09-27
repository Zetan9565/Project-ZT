namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 条件结点：判断设置的条件是否符合并向上反馈相应评估结果，可选是否持续进行评估。与Action的区别：可用于中止Composite的评估。注意：OnUpdate( )功能已完整，派生时谨慎覆写
    /// </summary>
    public abstract class Conditional : Node
    {
        protected virtual bool ShouldKeepRunning() { return false; }

        public abstract bool CheckCondition();

        protected override NodeStates OnUpdate()
        {
            if (ShouldKeepRunning()) return NodeStates.Running;
            if (CheckCondition()) return NodeStates.Success;
            else return NodeStates.Failure;
        }
    }
}