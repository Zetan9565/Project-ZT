using System.Linq;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("带标签的距离判断：判断参照物离带标签的目标是否在指定距离内")]
    public class TagWithinDistance2D : Conditional
    {
        [DisplayName("距离")]
        public SharedFloat distance;
        [DisplayName("标签"), Tag_BT]
        public SharedString tag;
        public LayerMask sightLayer = 1 << 2;
        [DisplayName("检查视线")]
        public SharedBool lineOfSight;
        [HideIf_BT("lineOfSight.value", false)]
        public LayerMask obstacleLayer;
        [DisplayName("眼睛位置偏移"), HideIf_BT("lineOfSight.value", false)]
        public SharedVector3 eyesOffset;
        [DisplayName("寄存器")]
        public SharedGameObject register;

        public override bool IsValid => distance != null && tag != null && distance.IsValid && tag.IsValid;

        public override bool CheckCondition()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, distance, sightLayer);
            if (colliders == null) return false;
            Collider2D find;
            if (lineOfSight)
            {
                find = colliders.FirstOrDefault(c =>
                {
                    if (c.CompareTag(tag))
                    {
                        var hit = Physics2D.Linecast(c.transform.position, transform.position + eyesOffset, obstacleLayer);
                        if (!hit.collider || hit.collider.CompareTag(tag)) return true;
                        else return false;
                    }
                    else return false;
                });
            }
            else find = colliders.FirstOrDefault(c => c.CompareTag(tag));
            if (find)
            {
                register.Value = find.gameObject;
                return true;
            }
            else
            {
                register.Value = null;
                return false;
            }
        }

        public override void OnDrawGizmosSelected()
        {
            if (transform) ZetanUtility.DrawGizmosCircle(transform.position, distance);
        }
    }
}