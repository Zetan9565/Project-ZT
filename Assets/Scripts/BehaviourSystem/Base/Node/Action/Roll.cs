using ZetanStudio.BehaviourTree;

[NodeDescription("角色翻滚一次")]
public class Roll : Action
{
    public override bool IsValid => true;

    private CharacterController2D controller;

    protected override void OnAwake()
    {
        controller = gameObject.GetComponentInParent<CharacterController2D>();
    }

    protected override NodeStates OnUpdate()
    {
        controller.Roll();
        return NodeStates.Success;
    }
}