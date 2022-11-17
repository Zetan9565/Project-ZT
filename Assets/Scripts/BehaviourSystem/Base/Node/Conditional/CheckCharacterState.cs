using ZetanStudio;
using ZetanStudio.BehaviourTree;
using ZetanStudio.BehaviourTree.Nodes;

[Description("检查角色状态：检查角色的主状态是否是指定状态，可选检查子状态")]
public class CheckCharacterState : Conditional
{
    [Label("状态")]
    public CharacterStates mainState;
    [Label("检查子状态")]
    public bool checkSubState;
    [Label("子状态"), HideIf("checkSubState", false), SubState("mainState")]
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
                CharacterStates.Normal => character.GetState(out var main, out var sub) && main == mainState && (CharacterNormalStates)sub == (CharacterNormalStates)subState,
                CharacterStates.Abnormal => character.GetState(out var main, out var sub) && main == mainState && (CharacterAbnormalStates)sub == (CharacterAbnormalStates)subState,
                CharacterStates.Gather => character.GetState(out var main, out var sub) && main == mainState && (CharacterGatherStates)sub == (CharacterGatherStates)subState,
                CharacterStates.Attack => character.GetState(out var main, out var sub) && main == mainState && (CharacterAttackStates)sub == (CharacterAttackStates)subState,
                CharacterStates.Busy => character.GetState(out var main, out var sub) && main == mainState && (CharacterBusyStates)sub == (CharacterBusyStates)subState,
                _ => false,
            };
        }
        else return character.GetMainState(out var main) && main == mainState;
    }
}