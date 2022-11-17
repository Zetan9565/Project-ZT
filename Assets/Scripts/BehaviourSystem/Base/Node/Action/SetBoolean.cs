namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("布尔变量赋值结点：设置行为树布尔型共享变量的值")]
    public class SetBoolean : Action
    {

        [Label("全局变量")]
        public bool global;
        [Label("变量名称"), HideIf("global", true), NameOfVariable(typeof(SharedBool))]
        public SharedString varName;
        [Label("全局变量名称"), HideIf("global", false), NameOfVariable(typeof(SharedBool), true)]
        public SharedString gvarName;
        [Label("变量值")]
        public SharedBool value;

        public override bool IsValid => (!global && !string.IsNullOrEmpty(varName) || global && !string.IsNullOrEmpty(gvarName)) && (value?.IsValid ?? false);

        protected override NodeStates OnUpdate()
        {
            if (global)
            {
                GlobalVariables global;
                if (!Tree.IsInstance) global = Utility.Editor.LoadAsset<GlobalVariables>();
                else global = BehaviourTreeManager.Instance.GlobalVariables;
                global.SetVariable<bool>(gvarName, value);
            }
            else if (Tree.SetVariable<bool>(varName, value)) return NodeStates.Success;
            return NodeStates.Failure;
        }
    }
}