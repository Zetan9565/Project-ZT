using UnityEngine;

namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("距离判断：判断参照物离目标是否在指定距离内")]
    public class WithinDistance2D : Conditional
    {
        [Label("距离")]
        public SharedFloat distance = 5.0f;
        [Label("使用点")]
        public bool usePoint;
        [Label("目标点"), HideIf("usePoint", false)]
        public SharedVector3 point;
        [Label("目标对象"), HideIf("usePoint", true)]
        public SharedGameObject target;
        [Label("检查视线")]
        public SharedBool lineOfSight;
        [Label("障碍检测层"), HideIf("lineOfSight", false)]
        public LayerMask obstacleLayer;
        [Label("眼睛位置偏移"), HideIf("lineOfSight", false)]
        public SharedVector3 eyesOffset;

        public override bool IsValid => distance != null && (usePoint && point != null && point.IsValid || !usePoint && target != null && target.IsValid)
            && lineOfSight != null && lineOfSight.IsValid && (!lineOfSight || lineOfSight && eyesOffset != null && eyesOffset.IsValid);

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