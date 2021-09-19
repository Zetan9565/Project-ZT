using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("距离判断结点：判断参照物离目标是否在指定距离内")]
    public class WithinDistance : Conditional
    {
        [DisplayName("距离")]
        public SharedFloat distance;
        [DisplayName("使用点")]
        public bool usePoint;
        [DisplayName("目标点"), HideIf("usePoint", false)]
        public SharedVector3 point;
        [DisplayName("目标对象"), HideIf("usePoint", true)]
        public SharedGameObject target;
        [DisplayName("以自身作参照")]
        public bool useThis = true;
        [DisplayName("参照物"), HideIf("useThis", true)]
        public SharedGameObject contrast;

        public override bool IsValid => true;

        protected override bool ShouldKeepRunning()
        {
            return !usePoint && (target == null || target.Value == null) || !useThis && (contrast == null || contrast.Value == null);
        }

        public override bool CheckCondition()
        {
            if (usePoint) return Vector3.Distance(point, transform.position) <= distance;
            if (useThis) return Vector3.Distance(target.Value.transform.position, transform.position) <= distance;
            return Vector3.Distance(target.Value.transform.position, contrast.Value.transform.position) <= distance;
        }
    }
}