using System.Linq;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("带标签的距离判断：判断参照物离带标签的目标是否在指定距离内")]
    public class TagWithinDistance2D : Conditional
    {
        [Label("距离")]
        public SharedFloat distance;
        [Label("标签"), Tag]
        public SharedString tag;
        [Label("可视检测层")]
        public LayerMask sightLayer = 1 << 2;
        [Label("检查视线")]
        public SharedBool lineOfSight;
        [Label("障碍检测曾"), HideIf("lineOfSight", false)]
        public LayerMask obstacleLayer;
        [Label("眼睛位置偏移"), HideIf("lineOfSight", false)]
        public SharedVector3 eyesOffset;
        [Label("寄存器")]
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