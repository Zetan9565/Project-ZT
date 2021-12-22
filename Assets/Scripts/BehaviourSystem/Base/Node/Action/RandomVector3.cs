using System.Collections;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class RandomVector3 : Action
    {
        [DisplayName("范围而不是边界")]
        public bool useRange = true;
        [DisplayName("范围半径"), HideIf_BT("useRange", false)]
        public SharedFloat range = 10f;
        [DisplayName("盲区半径"), HideIf_BT("useRange", false), Tooltip("不会在此范围内取点")]
        public SharedFloat blindRange = 0f;
        [DisplayName("边界最小值"), HideIf_BT("useRange", true)]
        public SharedVector3 boundMin;
        [DisplayName("边界最大值"), HideIf_BT("useRange", true)]
        public SharedVector3 boundMax;
        [DisplayName("结果寄存器")]
        public SharedVector3 register;

        public override bool IsValid => useRange && range != null && range.IsValid && blindRange != null && blindRange.IsValid
            || !useRange && boundMin != null && boundMax != null && boundMin.IsValid && boundMax.IsValid;

        protected override NodeStates OnUpdate()
        {
            if (useRange) register.Value = new Vector3(
                transform.position.x + blindRange + Random.Range(-range + blindRange, range - blindRange),
                transform.position.y + blindRange + Random.Range(-range + blindRange, range - blindRange));
            else register.Value = new Vector3(Random.Range(boundMin.Value.x, boundMax.Value.x), Random.Range(boundMin.Value.y, boundMax.Value.y));
            return NodeStates.Success;
        }

        public override void OnDrawGizmosSelected()
        {
            if (useRange && transform) Gizmos.DrawWireSphere(transform.position, range);
            else
            {
                Gizmos.DrawWireCube(ZetanUtility.CenterBetween(boundMin, boundMax), ZetanUtility.SizeBetween(boundMin, boundMax));
            }
        }
    }
}