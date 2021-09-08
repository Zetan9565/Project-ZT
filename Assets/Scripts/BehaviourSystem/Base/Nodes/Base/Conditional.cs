namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 条件结点：根据设置的条件判断是否符合并向上反馈相应评估结果，可选持续进行评估。注意：OnUpdate( )功能已完整，派生时谨慎覆写
    /// </summary>
    public abstract class Conditional : Node
    {
        protected abstract bool ShouldKeepRunning();

        protected abstract bool CheckCondition();

        protected override NodeStates OnUpdate()
        {
            if (ShouldKeepRunning()) return NodeStates.Running;
            if (CheckCondition()) return NodeStates.Success;
            else return NodeStates.Failure;
        }
    }
}