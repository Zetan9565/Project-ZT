using UnityEngine;
using System.Collections;
using System;

public class GatherBehaviour : StateMachineBehaviour
{
    public Action enterCallback;
    public Action exitCallback;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        enterCallback?.Invoke();
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        exitCallback?.Invoke();
    }
}
