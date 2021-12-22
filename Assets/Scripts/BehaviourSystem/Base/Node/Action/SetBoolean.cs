namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("布尔变量赋值结点：设置行为树布尔型共享变量的值")]
    public class SetBoolean : Action
    {
        [DisplayName("变量名称"), NameOfVariable(typeof(SharedBool))]
        public string varName;
        [DisplayName("变量值")]
        public SharedBool value;

        public override bool IsValid => !string.IsNullOrEmpty(varName) && value != null && value.IsValid;

        protected override NodeStates OnUpdate()
        {
            if (Owner.SetVariable<bool>(varName, value)) return NodeStates.Success;
            else return NodeStates.Failure;
        }
    }
}