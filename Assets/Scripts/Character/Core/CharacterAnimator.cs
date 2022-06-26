using UnityEngine;

[AddComponentMenu("Zetan Studio/角色动画代理")]
[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    public bool flip;

    public AnimatorStateInfo CurrentState => GetCurrentAnimatorStateInfo();

    public AnimatorStateInfo PreviousState { get; private set; }

    private Animator animator;

    public Animator GetAnimatorComponent()
    {
        return animator;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var b in animator.GetBehaviours<CallbackBehaviour>())
        {
            b.Init(OnStateEnter, OnStateUpdate, OnStateExit);
        }
    }

    private void LateUpdate()
    {
        PreviousState = CurrentState;
    }

    private void OnStateEnter(AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
    private void OnStateUpdate(AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetFloat(CharacterAnimaParams.Normalize, Mathf.Repeat(stateInfo.normalizedTime, 1.0f));
    }
    private void OnStateExit(AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    public void SetAnimaState(int state, int substate)
    {
        SetAnimaMainState(state);
        SetAnimaSubState(substate);
    }

    public void SetAnimaMainState(int state)
    {
        SetInteger(CharacterAnimaParams.State, state);
    }

    public void SetAnimaSubState(int state)
    {
        SetInteger(CharacterAnimaParams.SubState, state);
    }

    public void SetAnimaCombatState(bool value)
    {
        SetBool(CharacterAnimaParams.Combat, value);
    }

    public void PlayMoveAnima(Vector2 direction)
    {
        SetFloat(CharacterAnimaParams.DirectionX, direction.x);
        SetFloat(CharacterAnimaParams.DirectionY, direction.y);
        if (flip)
            if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
    }

    public void PlayIdleAnima(Vector2 direction)
    {

    }

    public void SetDesiredSpeed(Vector2 input)
    {
        animator.SetFloat(CharacterAnimaParams.DesiredSpeed, input.sqrMagnitude);
    }

    public void PlayRollAnima(Vector2 direction)
    {
        ResetAttackAnima();
        ResetTrigger(CharacterAnimaParams.Flash);
        SetTrigger(CharacterAnimaParams.Roll);
        if (flip)
            if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
    }

    public void PlayFlashAnima(Vector2 direction)
    {
        ResetAttackAnima();
        ResetTrigger(CharacterAnimaParams.Roll);
        SetTrigger(CharacterAnimaParams.Flash);
        if (flip)
            if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
    }

    public void PlayHurtAnima(Vector2 direction)
    {
        ResetAttackAnima();
        ResetTrigger(CharacterAnimaParams.Roll);
        ResetTrigger(CharacterAnimaParams.Flash);
        SetFloat(CharacterAnimaParams.HurtDirX, direction.x);
        SetFloat(CharacterAnimaParams.HurtDirY, direction.y);
        SetTrigger(CharacterAnimaParams.GetHurt);
    }

    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0)
    {
        if (animator.runtimeAnimatorController) return animator.GetCurrentAnimatorStateInfo(layerIndex);
        else return default;
    }
    public void PlayAttackAnima()
    {
        ResetAttackAnima();
        ResetTrigger(CharacterAnimaParams.Roll);
        ResetTrigger(CharacterAnimaParams.Flash);
        SetTrigger(CharacterAnimaParams.Attack);
    }
    public void ResetAttackAnima()
    {
        ResetTrigger(CharacterAnimaParams.Attack);
    }

    private bool StateEquals(AnimatorStateInfo left, AnimatorStateInfo right)
    {
        return left.fullPathHash == right.fullPathHash;
    }

    #region Animator部分方法覆写
    public void SetInteger(string id, int value)
    {
        animator.SetInteger(id, value);
    }
    public void SetInteger(int id, int value)
    {
        animator.SetInteger(id, value);
    }

    public void SetFloat(string id, float value)
    {
        animator.SetFloat(id, value);
    }
    public void SetFloat(int id, float value)
    {
        animator.SetFloat(id, value);
    }

    public void SetBool(string id, bool value)
    {
        animator.SetBool(id, value);
    }
    public void SetBool(int id, bool value)
    {
        animator.SetBool(id, value);
    }

    public void SetTrigger(string id)
    {
        animator.SetTrigger(id);
    }
    public void SetTrigger(int id)
    {
        animator.SetTrigger(id);
    }

    public void ResetTrigger(string id)
    {
        animator.ResetTrigger(id);
    }
    public void ResetTrigger(int id)
    {
        animator.ResetTrigger(id);
    }
    #endregion
}