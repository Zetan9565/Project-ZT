using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 等待结点：等待一定时间后，才向上反馈评估成功，期间持续向上反馈评估正进行
    /// </summary>
    [NodeDescription("等待结点：等待一定时间后，才向上反馈评估成功，期间持续向上反馈评估正进行")]
    public class Wait : Action
    {
        [DisplayName("等待随机时长")]
        public SharedBool randomWait = false;
        [DisplayName("最小随机时长"), HideIf_BT("randomWait.value", false)]
        public SharedFloat randomWaitMin = 1;
        [DisplayName("最大随机时长"), HideIf_BT("randomWait.value", false)]
        public SharedFloat randomWaitMax = 1;
        [DisplayName("等待时长(秒)"), HideIf_BT("randomWait.value", true)]
        public SharedFloat duration = 1;

        private float waitTime;
        private float startTime;
        private float pauseTime;

        public override bool IsValid
        {
            get
            {
                return randomWait != null && randomWait.IsValid
                       && (!randomWait || randomWait && randomWaitMin != null && randomWaitMin.IsValid)
                       && (!randomWait || randomWait && randomWaitMax != null && randomWaitMax.IsValid)
                       && (randomWait || !randomWait && duration != null && duration.IsValid);
            }
        }

        protected override void OnStart()
        {
            startTime = Time.time;
            if (randomWait.Value) waitTime = Random.Range(randomWaitMin.Value, randomWaitMax.Value);
            else waitTime = duration.Value;
        }

        protected override void OnPause(bool paused)
        {
            if (paused) pauseTime = Time.time;
            else startTime += Time.time - pauseTime;
        }

        protected override NodeStates OnUpdate()
        {
            if (Time.time - startTime > waitTime)
                return NodeStates.Success;
            return NodeStates.Running;
        }

        protected override void OnReset()
        {
            duration = 1;
            randomWait = false;
            randomWaitMin = 1;
            randomWaitMax = 1;
        }
    }
}