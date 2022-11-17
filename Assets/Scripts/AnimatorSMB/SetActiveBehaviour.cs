using UnityEngine;
using ZetanStudio;

public class SetActiveBehaviour : StateMachineBehaviour
{
    public int index;
    public string _name;
    public bool value;
    public float startTime;
    public float endTime;

    private GameObjectArray array;
    private bool activeBef;
    private bool isExit;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        array = animator.GetComponentInParent<GameObjectArray>();
        activeBef = array.GetActive(index);
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash && stateInfo.normalizedTime > startTime && stateInfo.normalizedTime < endTime)
            array.SetActive(index, value);
        isExit = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!isExit && animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
        {
            if (stateInfo.normalizedTime > startTime && stateInfo.normalizedTime < endTime)
                array.SetActive(index, value);
            else if (stateInfo.normalizedTime >= endTime)
            {
                isExit = true;
                array.SetActive(index, activeBef);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!isExit) array.SetActive(index, activeBef);
    }
}
