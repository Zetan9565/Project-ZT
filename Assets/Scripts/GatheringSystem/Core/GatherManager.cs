using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/采集理器")]
public class GatherManager : SingletonMonoBehaviour<GatherManager>
{
    [SerializeField]
    private string animaName;
    private int animaNameHash;

    [SerializeField]
    private float lootInvaildDistance = 2.0f;
    private Resource doneAgent;

    public Resource Resource { get; private set; }

    public bool GatherAble { get; private set; }
    public bool IsGathering { get; private set; }

    public void Init()
    {
        animaNameHash = Animator.StringToHash(animaName);
        //var gatherBehaviours = PlayerManager.Instance.Controller.Animator.Animator.GetBehaviours<GatherBehaviour>();
        //foreach (var gb in gatherBehaviours)
        //{
        //    gb.enterCallback = GatherStart;
        //}
    }

    public void Cancel()
    {
        GatherAble = false;
        FinishInteraction();
        Resource = null;
        if (IsGathering) ProgressBar.Instance.Cancel();
    }

    protected void FinishInteraction()
    {
        InteractionPanel.Instance.ShowOrHidePanelBy(Resource, true);
    }

    public bool Gather(Resource gatherAgent)
    {
        if (!PlayerManager.Instance.CheckIsNormalWithAlert())
            return false;
        if (IsGathering)
        {
            MessageManager.Instance.New("请等待上一个采集完成");
            return false;
        }
        //PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, (int)GatherAgent.GatheringInfo.GatherType);
        Resource = gatherAgent;
        GatherAble = true;
        GatherStart();
        return true;
    }

    private void GatherStart()
    {
        doneAgent = null;
        if (!IsGathering)
            NotifyCenter.PostNotify(NotifyCenter.CommonKeys.GatheringStateChanged, true);
        IsGathering = true;
        ProgressBar.Instance.New(Resource.ResourceInfo.GatherTime, GatherDone, GatherCancel, "采集中");
    }

    private void GatherDone()
    {
        //PlayerManager.Instance.Controller.Animator.SetInteger(animaNameHash, -1);
        if (!Resource) return;
        FinishInteraction();
        Resource.GatherSuccess();
        if (IsGathering)
            NotifyCenter.PostNotify(NotifyCenter.CommonKeys.GatheringStateChanged, false);
        IsGathering = false;
        doneAgent = Resource;
        StartCoroutine(UpdateDistance());
    }

    private void GatherCancel()
    {
        //PlayerManager.Instance.Controller.Animator.SetInteger(animaNameHash, -1);
        FinishInteraction();
        if (IsGathering)
            NotifyCenter.PostNotify(NotifyCenter.CommonKeys.GatheringStateChanged, false);
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
