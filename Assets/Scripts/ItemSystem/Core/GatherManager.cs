using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/采集理器")]
public class GatherManager : SingletonMonoBehaviour<GatherManager>
{
    [SerializeField]
    private string animaName;
    private int animaNameHash;

    [SerializeField]
    private float lootInvaildDistance = 2.0f;
    private Gathering doneAgent;

    public Gathering Gathering { get; private set; }

    public bool GatherAble { get; private set; }
    public bool IsGathering { get; private set; }

    public void Init()
    {
        animaNameHash = Animator.StringToHash(animaName);
        var gatherBehaviours = PlayerManager.Instance.PlayerController.Animator.GetBehaviours<GatherBehaviour>();
        foreach (var gb in gatherBehaviours)
        {
            gb.enterCallback = GatherStart;
        }
    }

    public void Cancel()
    {
        GatherAble = false;
        if (Gathering) Gathering.FinishInteraction();
        Gathering = null;
        if (IsGathering) ProgressBar.Instance.Cancel();
    }

    public bool Gather(Gathering gatherAgent)
    {
        if (IsGathering)
        {
            MessageManager.Instance.New("请等待上一个采集完成");
            return false;
        }
        //PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, (int)GatherAgent.GatheringInfo.GatherType);
        Gathering = gatherAgent;
        GatherAble = true;
        GatherStart();
        return true;
    }

    private void GatherStart()
    {
        doneAgent = null;
        if (!IsGathering)
            NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.GatheringStateChange, true);
        IsGathering = true;
        ProgressBar.Instance.New(Gathering.GatheringInfo.GatherTime, GatherDone, GatherCancel, "采集中");
    }

    private void GatherDone()
    {
        PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, -1);
        if (!Gathering) return;
        Gathering.FinishInteraction();
        Gathering.GatherSuccess();
        if (IsGathering)
            NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.GatheringStateChange, false);
        IsGathering = false;
        doneAgent = Gathering;
        StartCoroutine(UpdateDistance());
    }

    private void GatherCancel()
    {
        PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, -1);
        if (Gathering) Gathering.FinishInteraction();
        if (IsGathering)
            NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.GatheringStateChange, false);
        IsGathering = false;
    }

    private IEnumerator UpdateDistance()
    {
        while (doneAgent)
        {
            if (Vector3.Distance(PlayerManager.Instance.PlayerTransform.position, doneAgent.transform.position) >= lootInvaildDistance)
            {
                doneAgent = null;
                yield break;
            }
            yield return null;
        }
    }
}
