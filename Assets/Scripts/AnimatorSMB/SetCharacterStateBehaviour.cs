using UnityEngine;

public class SetCharacterStateBehaviour : StateMachineBehaviour
{
    [Header("处于此状态时")]
    [SerializeField, Range(0, 1)]
    private float normalizedTime;
    [SerializeField]
    private CharacterStates state = CharacterStates.Normal;
    [SerializeField]
    private int subState = 0;

    [Header("退出此状态时")]
    [SerializeField, Range(0, 1)]
    private float exitNormalizedTime = 0.9f;
    [SerializeField]
    private CharacterStates exitState = CharacterStates.Normal;
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

    private void SetState(CharacterStates main, int sub)
    {
        switch (main)
        {
            case CharacterStates.Normal:
                character.SetState(main, (CharacterNormalStates)sub);
                break;
            case CharacterStates.Abnormal:
                character.SetState(main, (CharacterAbnormalStates)sub);
                break;
            case CharacterStates.Gather:
                character.SetState(main, (CharacterGatherStates)sub);
                break;
            case CharacterStates.Attack:
                character.SetState(main, (CharacterAttackStates)sub);
                break;
            case CharacterStates.Busy:
                character.SetState(main, (CharacterBusyStates)sub);
                break;
            default:
                break;
        }
    }
}