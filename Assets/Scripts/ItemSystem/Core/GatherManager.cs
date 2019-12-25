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
    private GatherAgent doneAgent;

    public GatherAgent GatherAgent { get; private set; }

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

    public void CanGather(GatherAgent gatherAgent)
    {
        if (!gatherAgent || doneAgent) return;
        GatherAgent = gatherAgent;
        GatherAble = true;
        UIManager.Instance.EnableInteractive(true, gatherAgent.GatheringInfo.name);
    }

    public void CannotGather()
    {
        GatherAble = false;
        GatherAgent = null;
        UIManager.Instance.EnableInteractive(false);
        if (IsGathering) ProgressBar.Instance.Cancel();
    }

    public void TryGather()
    {
        //PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, (int)GatherAgent.GatheringInfo.GatherType);
        GatherStart();
    }

    private void GatherStart()
    {
        ProgressBar.Instance.NewProgress(GatherAgent.GatheringInfo.GatherTime, GatherDone, GatherCancel, "采集中");
        IsGathering = true;
        UIManager.Instance.EnableInteractive(false);
        doneAgent = null;
    }

    private void GatherDone()
    {
        PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, -1);
        GatherAgent.GatherSuccess();
        IsGathering = false;
        doneAgent = GatherAgent;
        StartCoroutine(UpdateDistance());
    }

    private void GatherCancel()
    {
        PlayerManager.Instance.PlayerController.Animator.SetInteger(animaNameHash, -1);
        IsGathering = false;
    }

    private IEnumerator UpdateDistance()
    {
        while (doneAgent)
        {
            if (Vector3.Distance(PlayerManager.Instance.PlayerController.transform.position, doneAgent.transform.position) >= lootInvaildDistance)
            {
                doneAgent = null;
                yield break;
            }
            yield return null;
        }
    }
}
