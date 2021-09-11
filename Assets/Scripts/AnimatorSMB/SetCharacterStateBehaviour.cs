using UnityEngine;

public class SetCharacterStateBehaviour : StateMachineBehaviour
{
    [Header("处于此状态时")]
    [SerializeField, Range(0, 1)]
    private float normalizedTime;
    [SerializeField]
    private CharacterState state = CharacterState.Normal;
    [SerializeField]
    private int subState = 0;

    [Header("退出此状态时")]
    [SerializeField, Range(0, 1)]
    private float exitNormalizedTime = 0.9f;
    [SerializeField]
    private CharacterState exitState = CharacterState.Normal;
    [SerializeField]
    private int exitSubState = 0;

    private Character character;
    private bool isExit;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character = animator.GetComponentInParent<Character>();
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash && normalizedTime <= stateInfo.normalizedTime)
            SetState(state, subState);
        isExit = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!isExit && animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
        {
            if (normalizedTime <= stateInfo.normalizedTime && stateInfo.normalizedTime < exitNormalizedTime)
                SetState(state, subState);
            else if (stateInfo.normalizedTime >= exitNormalizedTime)
            {
                isExit = true;
                SetState(exitState, exitSubState);
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!isExit) SetState(exitState, exitSubState);
    }

    private void SetState(CharacterState main, int sub)
    {
        switch (main)
        {
            case CharacterState.Normal:
                character.SetState(main, (CharacterNormalState)sub);
                break;
            case CharacterState.Abnormal:
                character.SetState(main, (CharacterAbnormalState)sub);
                break;
            case CharacterState.Gather:
                character.SetState(main, (CharacterGatherState)sub);
                break;
            case CharacterState.Attack:
                character.SetState(main, (CharacterAttackState)sub);
                break;
            case CharacterState.Busy:
                character.SetState(main, (CharacterBusyState)sub);
                break;
            default:
                break;
        }
    }
}