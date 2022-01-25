using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("可否看见目标：根据指定目标是否在视野范围内向上反馈相应的评估结果")]
    public class CanSeeObject2D : Conditional
    {
        [DisplayName("面部朝向")]
        public SharedVector3 direction;
        [DisplayName("眼睛偏移")]
        public SharedVector3 eyesOffset;
        [DisplayName("视角")]
        public SharedFloat fieldOfView = 90f;
        [DisplayName("视距")]
        public SharedFloat distance = 10f;
        [DisplayName("目标")]
        public SharedGameObject target;
        public LayerMask obstacleLayer;

        public override bool IsValid => direction != null && direction.IsValid && eyesOffset != null && eyesOffset.IsValid
            && fieldOfView != null && fieldOfView.IsValid && distance != null && distance.IsValid && target != null && target.Value != null;

        public override bool CheckCondition()
        {
            if (Vector2.SqrMagnitude(Target() - Eyes()) <= distance * distance)
            {
                if (Vector2.Angle(direction.Value, Target() - Eyes()) <= fieldOfView / 2)
                {
                    var hit = Physics2D.Linecast(Eyes(), Target(), obstacleLayer);
                    if (!hit.collider || hit.collider.gameObject == target)
                        return true;
                }
            }
            return false;
        }

        private Vector3 Eyes()
        {
            return Target() - transform.position + eyesOffset;
        }

        public Vector3 Target()
        {
            return target.Value.transform.position;
        }

        public override void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (transform) ZetanUtility.DrawGizmosSector(Eyes(), direction, distance, fieldOfView);
#endif
        }
    }
}