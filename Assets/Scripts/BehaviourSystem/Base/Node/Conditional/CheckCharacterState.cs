using ZetanStudio.BehaviourTree;

public class CheckCharacterState : Conditional
{
    [DisplayName("状态")]
    public CharacterState mainState;
    [DisplayName("检查子状态")]
    public bool checkSubState;
    [DisplayName("子状态"), HideIf("checkSubState", false), SubState("mainState")]
    public int subState;

    private Character character;

    public override bool IsValid => true;

    protected override void OnStart()
    {
        character = gameObject.GetComponentInParent<Character>();
    }

    public override bool CheckCondition()
    {
        if (!character) return false;
        if (checkSubState)
        {
            return mainState switch
            {
                CharacterState.Normal => character.GetState(out var main, out var sub) && main == mainState && (CharacterNormalState)sub == (CharacterNormalState)subState,
                CharacterState.Abnormal => character.GetState(out var main, out var sub) && main == mainState && (CharacterAbnormalState)sub == (CharacterAbnormalState)subState,
                CharacterState.Gather => character.GetState(out var main, out var sub) && main == mainState && (CharacterGatherState)sub == (CharacterGatherState)subState,
                CharacterState.Attack => character.GetState(out var main, out var sub) && main == mainState && (CharacterAttackState)sub == (CharacterAttackState)subState,
                CharacterState.Busy => character.GetState(out var main, out var sub) && main == mainState && (CharacterBusyState)sub == (CharacterBusyState)subState,
                _ => false,
            };
        }
        else return character.GetMainState(out var main) && main == mainState;
    }
}