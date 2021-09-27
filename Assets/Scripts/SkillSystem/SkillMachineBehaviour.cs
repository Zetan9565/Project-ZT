using UnityEngine;
using System;

public class SkillMachineBehaviour : StateMachineBehaviour
{
    public SkillInformation skillInfo;

    public Action<string> enterCallback;
    public Action<string> updateCallback;
    public Action<string> exitCallback;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enterCallback?.Invoke(skillInfo.Name);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        updateCallback?.Invoke(skillInfo.Name);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enterCallback?.Invoke(skillInfo.Name);
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        base.OnStateMachineEnter(animator, stateMachinePathHash);
    }
}
