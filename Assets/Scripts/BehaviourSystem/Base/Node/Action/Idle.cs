namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("待机结点：无有效动作，等待被打断")]
    public class Idle : Action
    {
        public override bool IsValid => true;

        protected override NodeStates OnUpdate()
        {
            return NodeStates.Running;
        }
    }
}