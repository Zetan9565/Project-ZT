using UnityEngine;

public class SetContoller2DSpeedBehaviour : StateMachineBehaviour
{
    public float startTime = 0;
    public float endTime = 1;

    public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0, 30), new Keyframe(1, 0));

    private CharacterController2D contoller;
    private float duration;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        duration = endTime - startTime;
        contoller = animator.GetComponentInParent<CharacterController2D>();
        if (stateInfo.normalizedTime >= startTime && stateInfo.normalizedTime <= endTime)
            contoller.funcSpeed = speedCurve.Evaluate(Normalize(stateInfo));
    }

    private float Normalize(AnimatorStateInfo stateInfo)
    {
        return (stateInfo.normalizedTime - startTime) / duration;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= startTime && stateInfo.normalizedTime <= endTime)
            contoller.funcSpeed = speedCurve.Evaluate(Normalize(stateInfo));
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= startTime && stateInfo.normalizedTime <= endTime)
            contoller.funcSpeed = speedCurve.Evaluate(Normalize(stateInfo));
    }
}