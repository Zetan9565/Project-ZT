using UnityEngine;

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

    public CharacterRollState(CharacterStateMachine stateMachine) : base(stateMachine) { }

    protected override void OnEnter()
    {
        base.OnEnter();

        hasRollStart = false;
        direction = control.ValidMoveInput;
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
        base.OnExit();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (hasRollStart)
            motion.SetVelocity(direction * rollSpeed);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        bool isRolling = animator.CurrentState.IsTag(CharacterAnimaTags.Roll);
        float normalizedTime = animator.CurrentState.normalizedTime;
        if (!hasRollStart && isRolling && normalizedTime >= startTime)
        {
            hasRollStart = true;
            control.UseActionInputs();
        }
        if (hasRollStart)
        {
            control.UseActionInputs();
            if (normalizedTime >= canMoveStartTime && normalizedTime < canMoveEndTime && (control.MoveInput.x != 0 || control.MoveInput.y != 0))
            {
                Machine.SetCurrentState<CharacterMoveState>();
            }
            else if (!isRolling || normalizedTime >= endTime)
            {
                motion.SetVelocity(Vector2.zero);
                Machine.SetCurrentState<CharacterIdleState>();
            }
            else rollSpeed = speedCurve.Evaluate(NormalizedTime(normalizedTime));
        }
    }

    private float NormalizedTime(float normalizedTime)
    {
        return (normalizedTime - startTime) / duration;
    }
}