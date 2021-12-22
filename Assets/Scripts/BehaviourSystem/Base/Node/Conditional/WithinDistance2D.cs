using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("距离判断：判断参照物离目标是否在指定距离内")]
    public class WithinDistance2D : Conditional
    {
        [DisplayName("距离")]
        public SharedFloat distance;
        [DisplayName("使用点")]
        public bool usePoint;
        [DisplayName("目标点"), HideIf_BT("usePoint", false)]
        public SharedVector3 point;
        [DisplayName("目标对象"), HideIf_BT("usePoint", true)]
        public SharedGameObject target;
        [DisplayName("检查视线")]
        public SharedBool lineOfSight;
        [HideIf_BT("lineOfSight.value", false)]
        public LayerMask obstacleLayer;
        [DisplayName("眼睛位置偏移"), HideIf_BT("lineOfSight.value", false)]
        public SharedVector3 eyesOffset;

        public override bool IsValid => distance != null && (usePoint && point != null && point.IsValid || !usePoint && target != null && target.IsValid)
            && lineOfSight != null && lineOfSight.IsValid && (!lineOfSight || lineOfSight && eyesOffset != null && eyesOffset.IsValid);

        protected override bool ShouldKeepRunning()
        {
            return !usePoint && target.Value == null;
        }

        public override bool CheckCondition()
        {
            if (!lineOfSight) return (Target() - transform.position).sqrMagnitude <= distance * distance;
            else if ((Target() - transform.position).sqrMagnitude <= distance * distance)
            {
                var hit = Physics2D.Linecast(Target(), transform.position + eyesOffset, obstacleLayer);
                if (!hit.collider || hit.collider.gameObject == target) return true;
                else return false;
            }
            else return false;
        }

        private Vector3 Target()
        {
            if (usePoint) return point.Value;
            else return target.Value.transform.position;
        }

        public override void OnDrawGizmosSelected()
        {
            if (transform) ZetanUtility.DrawGizmosCircle(transform.position, distance);
        }
    }
}