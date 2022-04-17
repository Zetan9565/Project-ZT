public class CharacterDeathState : CharacterAbnormalState
{
    public CharacterDeathState(CharacterStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterAbnormalStates.Dead);
    }

    public override bool CanTransitTo<T>(Transition transition)
    {
        var type = typeof(T);
        if (type == GetType()) return true;
        else if (typeof(CharacterNormalState).IsAssignableFrom(type) && transition is BoolTransition b && b.name == "relive" && b.value) return true;
        return false;
    }
}