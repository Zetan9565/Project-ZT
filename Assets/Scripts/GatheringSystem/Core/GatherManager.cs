using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.GatheringSystem
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Zetan Studio/管理器/采集理器")]
    public static class GatherManager
    {
        //[SerializeField]
        //private string animaName;
        //private int animaNameHash;

        private static Resource doneAgent;

        public static Resource Resource { get; private set; }

        public static bool GatherAble { get; private set; }
        public static bool IsGathering { get; private set; }

        //public void Init()
        //{
        //    //animaNameHash = Animator.StringToHash(animaName);
        //    //var gatherBehaviours = PlayerManager.Instance.Controller.Animator.Animator.GetBehaviours<GatherBehaviour>();
        //    //foreach (var gb in gatherBehaviours)
        //    //{
        //    //    gb.enterCallback = GatherStart;
        //    //}
        //}

        public static void Cancel()
        {
            GatherAble = false;
            FinishInteraction();
            Resource = null;
            if (IsGathering) ProgressBar.Instance.Cancel();
        }

        private static void FinishInteraction()
        {
            InteractionPanel.Instance.ShowOrHidePanelBy(Resource, true);
        }

        public static bool Gather(Resource gatherAgent)
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

        private static void GatherStart()
        {
            doneAgent = null;
            if (!IsGathering)
                NotifyCenter.PostNotify(GatheringStateChanged, true);
            IsGathering = true;
            ProgressBar.Instance.New(Resource.ResourceInfo.GatherTime, GatherDone, GatherCancel, "采集中");
            PlayerManager.Instance.Player.SetMachineState<PlayerGatheringState>();
        }

        private static void GatherDone()
        {
            //PlayerManager.Instance.Controller.Animator.SetInteger(animaNameHash, -1);
            if (!Resource) return;
            FinishInteraction();
            Resource.GatherSuccess();
            if (IsGathering)
                NotifyCenter.PostNotify(GatheringStateChanged, false);
            IsGathering = false;
            doneAgent = Resource;
            if (coroutine != null) EmptyMonoBehaviour.Singleton.StopCoroutine(coroutine);
            coroutine = EmptyMonoBehaviour.Singleton.StartCoroutine(UpdateDistance());

            static IEnumerator UpdateDistance()
            {
                while (doneAgent)
                {
                    if (Vector3.Distance(PlayerManager.Instance.PlayerTransform.position, doneAgent.transform.position) >= MiscSettings.Instance.LootInvaildDistance)
                    {
                        doneAgent = null;
                        yield break;
                    }
                    yield return null;
                }
            }
        }

        private static Coroutine coroutine;

        private static void GatherCancel()
        {
            //PlayerManager.Instance.Controller.Animator.SetInteger(animaNameHash, -1);
            FinishInteraction();
            if (IsGathering)
                NotifyCenter.PostNotify(GatheringStateChanged, false);
            IsGathering = false;
        }

        public const string GatheringStateChanged = "GatheringStateChanged";
    }
}