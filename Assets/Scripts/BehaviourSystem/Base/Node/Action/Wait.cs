using UnityEngine;

namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 等待结点：等待一定时间后，才向上反馈评估成功，期间持续向上反馈评估正进行
    /// </summary>
    [Description("等待结点：等待一定时间后，才向上反馈评估成功，期间持续向上反馈评估正进行")]
    public class Wait : Action
    {
        [Label("等待随机时长")]
        public SharedBool randomWait = false;
        [Label("最小随机时长"), HideIf("randomWait", false)]
        public SharedFloat randomWaitMin = 1;
        [Label("最大随机时长"), HideIf("randomWait", false)]
        public SharedFloat randomWaitMax = 1;
        [Label("等待时长(秒)"), HideIf("randomWait", true)]
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