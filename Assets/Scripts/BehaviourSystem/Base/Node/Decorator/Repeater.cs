namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 重复器：以一定或不限次数反复评估子结点
    /// </summary>
    [Description("重复器：以一定或不限次数反复评估子结点")]
    public class Repeater : Decorator
    {
        [Label("失败时停止")]
        public bool stopOnFailure;
        [Label("重复次数")]
        public SharedInt count = 0;

        private int times;

        protected override NodeStates OnUpdate()
        {
            switch (child.Evaluate())
            {
                case NodeStates.Failure:
                    if (stopOnFailure)
                        return NodeStates.Failure;
                    break;
            }
            times++;
            if (count > 0 && times >= count)
            {
                return NodeStates.Success;
            }
            else return NodeStates.Running;
        }
    }
}