using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 等待结点：等待一定时间后，才向上反馈评估成功，期间持续向上反馈评估正进行
    /// </summary>
    [NodeDescription("等待结点：等待一定时间后，才向上反馈评估成功，期间持续向上反馈评估正进行")]
    public class Wait : Action
    {
        [DisplayName("等待时长(秒)")]
        public SharedFloat duration = 1;
        [DisplayName("等待随机时长")]
        public SharedBoolean randomWait = false;
        [DisplayName("最小随机时长")]
        public SharedFloat randomWaitMin = 1;
        [DisplayName("最大随机时长")]
        public SharedFloat randomWaitMax = 1;

        private float waitTime;
        private float startTime;
        private float pauseTime;

        public override bool IsValid
        {
            get
            {
                return duration != null && randomWait != null && randomWaitMin != null && randomWaitMax != null;
            }
        }

        protected override void OnStart()
        {
            startTime = Time.time;
            if (randomWait.Value) waitTime = Random.Range(randomWaitMin.Value, randomWaitMax.Value);
            else waitTime = duration.Value;
        }

        protected override void OnPaused(bool paused)
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