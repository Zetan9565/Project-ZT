using ZetanStudio.BehaviourTree;
using ZetanStudio.BehaviourTree.Nodes;

[Group("Movement"), Description("角色翻滚一次")]
public class Roll : Action
{
    public override bool IsValid => true;

    private CharacterControlInput controller;

    protected override void OnAwake()
    {
        controller = gameObject.GetComponentInParent<CharacterControlInput>();
    }

    protected override NodeStates OnUpdate()
    {
        controller.SetTrigger(CharacterInputNames.Instance.Roll);
        return NodeStates.Success;
    }
}