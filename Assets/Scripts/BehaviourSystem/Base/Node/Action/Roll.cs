using ZetanStudio.BehaviourTree;

[NodeDescription("角色翻滚一次")]
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
        controller.SetRollInput(true);
        return NodeStates.Success;
    }
}