namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 重复器：以一定或不限次数反复评估子结点
    /// </summary>
    [NodeDescription("重复器：以一定或不限次数反复评估子结点")]
    public class Repeater : Decorator
    {
        public int count;
        public bool stopOnFailure;

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