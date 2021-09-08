using System;
using UnityEngine;

public class CallbackBehaviour : StateMachineBehaviour
{
    private Action<AnimatorStateInfo, int> enterCallback;
    private Action<AnimatorStateInfo, int> updateCallback;
    private Action<AnimatorStateInfo, int> exitCallback;

    public void Init(Action<AnimatorStateInfo, int> enterCallback, Action<AnimatorStateInfo, int> updateCallback, Action<AnimatorStateInfo, int> exitCallback)
    {
        this.enterCallback = enterCallback;
        this.updateCallback = updateCallback;
        this.exitCallback = exitCallback;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enterCallback?.Invoke(stateInfo, layerIndex);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        updateCallback?.Invoke(stateInfo, layerIndex);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        exitCallback?.Invoke(stateInfo, layerIndex);
    }
}
