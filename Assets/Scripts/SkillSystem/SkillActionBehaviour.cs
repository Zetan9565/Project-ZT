using UnityEngine;
using System;

public class SkillActionBehaviour : StateMachineBehaviour
{
    public SkillInfomation parentSkill;
    public int actionIndex;

    [HideInInspector]
    public SkillAction runtimeAction;

    public Action<SkillActionBehaviour> enterCallback;
    public Action<float> updateCallback;
    public Action<SkillActionBehaviour> exitCallback;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enterCallback?.Invoke(this);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        updateCallback?.Invoke(stateInfo.normalizedTime);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        exitCallback?.Invoke(this);
    }
}
