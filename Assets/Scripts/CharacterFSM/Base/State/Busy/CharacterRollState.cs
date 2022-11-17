using UnityEngine;
using ZetanStudio.CharacterSystem;

public class CharacterRollState : CharacterBusyState
{
    private AnimationCurve speedCurve;
    private Vector2 direction;
    private bool hasRollStart;
    private float rollSpeed;
    private float startTime;
    private float endTime;
    private float duration;
    private float canMoveStartTime;
    private float canMoveEndTime;
    private const string moveInAdvance = "move in advance";
    private const string rollComplete = "roll complete";

    public CharacterRollState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();
        Character.SetSubState(CharacterBusyStates.Roll);
        hasRollStart = false;
        control.ReadValue(CharacterInputNames.Instance.Direction, out direction);
        direction.Normalize();
        speedCurve = Machine.Params.RollSpeedCurve;
        startTime = Machine.Params.RollEffectedTime.x;
        endTime = Machine.Params.RollEffectedTime.y;
        duration = endTime - startTime;
        canMoveStartTime = Machine.Params.RollCanMoveTime.x;
        canMoveEndTime = Machine.Params.RollCanMoveTime.y;
        rollSpeed = speedCurve.Evaluate(0);
        animator.PlayRollAnima(direction);
    }

    protected override void OnExit()
    {
        control.ResetTrigger(CharacterInputNames.Instance.Roll);
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (hasRollStart) motion.SetVelocity(direction * rollSpeed);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        bool isRolling = animator.CurrentState.IsTag(CharacterAnimaTags.Roll);
        float normalizedTime = animator.CurrentState.normalizedTime;
        if (!hasRollStart && isRolling && normalizedTime >= startTime) hasRollStart = true;
        if (hasRollStart)
        {
            control.ResetTrigger(CharacterInputNames.Instance.Roll);
            if (CanMove(normalizedTime))
            {
                Machine.SetCurrentState<CharacterMoveState>(new StringTransition(moveInAdvance));
            }
            else if (!isRolling || normalizedTime >= endTime)
            {
                motion.SetVelocity(Vector2.zero);
                Machine.SetCurrentState<CharacterIdleState>(new StringTransition(rollComplete));
            }
            else rollSpeed = speedCurve.Evaluate(NormalizedTime(normalizedTime));
        }
    }

    private bool CanMove(float normalizedTime)
    {
        return normalizedTime >= canMoveStartTime && normalizedTime < canMoveEndTime
                        && control.ReadValue(CharacterInputNames.Instance.Move, out Vector2 move) && (move.x != 0 || move.y != 0);
    }

    private float NormalizedTime(float normalizedTime)
    {
        return (normalizedTime - startTime) / duration;
    }

    public override bool CanTransitTo<T>(Transition transition) => transition is StringTransition s
                                                                   && (s.value == moveInAdvance && IsState<CharacterMoveState, T>() || s.value == rollComplete && IsState<CharacterIdleState, T>())
                                                                   || IsState<CharacterDeathState, T>();
}